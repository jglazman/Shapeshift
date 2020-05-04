//
// Copyright (c) 2020 Jeremy Glazman
//

using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public class LevelView : MonoBehaviour, ISceneTransitioner
	{
		public enum State
		{
			Uninitialized = 0,
			
			// game
			WaitingForInput,
			WaitingForResponse,
			ExecutingEvent,
			
			// level editor
			EditMode
		}
		
		private State _state = State.Uninitialized;

		[SerializeField] private RectTransform _playfieldTransform = null;
		[SerializeField] private RectTransform _playfieldRootNodes = null;
		[SerializeField] private RectTransform _playfieldRootItems = null;
		[SerializeField] private RectTransform _playfieldFxItems = null;
		[SerializeField] private GridNodeView _gridNodePrefab = null;
		[SerializeField] private GridItemView _gridItemPrefab = null;
		[SerializeField] private GridItemFX _gridItemMatchedFXPrefab = null;
		[SerializeField] private LevelScoreView _levelScoreView = null;
		[SerializeField] private float _animationSpeedMultiplier = 2f;

		private Queue<Level.Event> _pendingEvents = new Queue<Level.Event>();
		private List<GridNodeView> _gridNodeInstances = new List<GridNodeView>();
		private List<GridItemView> _gridItemInstances = new List<GridItemView>();
		private List<GridItemView> _selectedGridItems = new List<GridItemView>();
		private LevelConfig _levelConfig;
		private int _levelIndex;
		private float _tileSize;
		private Vector3 _playfieldOrigin; // [0,0] = lower-left corner
		private static bool _didCreatePools = false;
		
		private int AnimationSpeedSetting => Database.Load<SettingsData>((int)GameOptionType.Animation).Value.optionValue;

		private float GridItemSpeed => AnimationSpeedSetting == 0 ? 0 : _tileSize * (4 - AnimationSpeedSetting) * _animationSpeedMultiplier;
		
		
		public void Notify(TransitionState state)
		{
			switch (state)
			{
				case TransitionState.Loading:
					break;
				
				case TransitionState.Intro:
					break;
				
				case TransitionState.Outro:
					// return items to the pool before the scene changes. OnDestroy is too late.
					ClearGridNodeInstances();
					ClearGridItemInstances();
					SceneController.UnregisterTransitioner(this);
					break;
			}
		}
		
		private void Awake()
		{
			if (!_didCreatePools)
			{
				PrefabPool.CreatePool<GridNodeView>(_gridNodePrefab);
				PrefabPool.CreatePool<GridItemView>(_gridItemPrefab);
				PrefabPool.CreatePool<GridItemFX>(_gridItemMatchedFXPrefab);
				_didCreatePools = true;
			}
			
			SceneController.RegisterTransitioner(this);
			Level.ListenForLevelEvents(ReceiveLevelEvents);
			DisableEditMode();
		}

		private void OnDestroy()
		{
			SceneController.UnregisterTransitioner(this);
			Level.StopListeningForLevelEvents(ReceiveLevelEvents);
			SpriteResource.ClearCache();
		}
		
		private void Update()
		{
			switch (_state)
			{
				case State.Uninitialized:
				case State.WaitingForResponse:
				{
					HandlePendingEvents();
				} break;
				
				case State.WaitingForInput:
				{
					// allow events to interrupt input from the player
					if (!HandlePendingEvents())
					{
						HandleInput();
					}
				} break;

				case State.ExecutingEvent:
				{
					// wait for actions to complete
					if (_gridItemInstances.All(item => !item.IsBusy))
						SetState(State.WaitingForInput);
				} break;
				
				case State.EditMode:
				{
					if (_pendingEvents.Count > 0 && _pendingEvents.Peek().EventType == Level.EventType.LoadLevel)
						HandlePendingEvents();
					
					EditMode_HandleInput();
				} break;
			}
		}

		private void SetState(State nextState)
		{
			if (_state == State.EditMode)
			{
				// ignore all state changes while in edit mode except for a total reset
				if (nextState != State.Uninitialized)
					return;
			}
			
			if (nextState != _state)
				Logger.LogEditor($"Set state: {_state} -> {nextState}");
			
			_state = nextState;
		}

		private void SendLevelCommand(Level.Command command)
		{
			Logger.LogWarningEditor($"Send level command: {command.CommandType}");
		
			SetState(State.WaitingForResponse);
			
			Level.ExecuteCommand(command);
		}
		
		
		private void ReceiveLevelEvents(IEnumerable<Level.Event> levelEvents)
		{
			foreach (var levelEvent in levelEvents)
				_pendingEvents.Enqueue(levelEvent);
		}

		private bool HandlePendingEvents()
		{
			if (_pendingEvents.Count > 0)
			{
				SetState(State.ExecutingEvent);

				Level.Event levelEvent;
				do
				{
					levelEvent = _pendingEvents.Dequeue();
					HandleLevelEvent(levelEvent);
				} while (_pendingEvents.Count > 0 && levelEvent.EventType == Level.EventType.ItemsMoved); // pack move events together for more fluid gameplay.
				
				return true;
			}

			return false;
		}

		private void HandleLevelEvent(Level.Event levelEvent)
		{
			Logger.LogWarningEditor($"Handle level event: {levelEvent.EventType}");

			switch (levelEvent.EventType)
			{
				// TODO: keep transitioner in loading mode until we receive a LoadLevel event
				case Level.EventType.LoadLevel:
				{
					LoadLevel(levelEvent as Level.LoadLevelEvent);
				} break;

				case Level.EventType.Win:
				{
					var winPopup = PopupViewController.Open<LevelWinPopup>();
					winPopup.ShowScore(levelEvent as Level.LevelWinEvent, _levelConfig);
				} break;

				case Level.EventType.Lose:
				{
					PopupViewController.Open<LevelLosePopup>();
				} break;

				case Level.EventType.UpdateScore:
				{
					var scoreEvent = levelEvent as Level.UpdateScoreEvent;
					_levelScoreView.SetPoints(scoreEvent.Points);
					_levelScoreView.SetMoves(_levelConfig.challengeValue - scoreEvent.Moves);
				} break;
				
				case Level.EventType.MatchSuccess:
				{
					DeselectAllGridItems();
				} break;

				case Level.EventType.MatchRejected:
				{
					DeselectAllGridItems();
				} break;

				case Level.EventType.ItemsCreated:
				{
					var itemsCreatedEvent = levelEvent as Level.ItemsCreatedEvent;
					
					float speed = GridItemSpeed;
					
					foreach (var createdItem in itemsCreatedEvent.CreatedItems)
					{
						Assert.IsNull(TryGetGridItem(createdItem.Index));
						
						var gridItem = CreateGridItemView(createdItem.Index, createdItem.ItemId);
						gridItem.DoCreateAction(createdItem.ItemId, speed);
					}
				} break;

				case Level.EventType.ItemsMoved:
				{
					var itemsMovedEvent = levelEvent as Level.ItemsMovedEvent;

					float speed = GridItemSpeed;

					foreach (var movedItem in itemsMovedEvent.MovedItems)
					{
						Assert.IsNull(TryGetGridItem(movedItem.Index));
						
						var sourceItem = TryGetGridItem(movedItem.ReferenceIndex.Value);
						Assert.IsNotNull(sourceItem);
						Assert.IsTrue(sourceItem.ID == movedItem.ItemId);
						
						sourceItem.SetGridIndex(movedItem.Index.x, movedItem.Index.y);
						sourceItem.DoMoveAction(CalculateGridNodePosition(movedItem.Index), speed);
					}
				} break;
				
				case Level.EventType.ItemsSwapped:
				{
					var itemsSwappedEvent = levelEvent as Level.ItemsSwappedEvent;

					float speed = GridItemSpeed * 2f;	// speed up the swap animation

					foreach (var swappedItem in itemsSwappedEvent.SwappedItems)
					{
						var sourceItem = TryGetGridItem(swappedItem.ReferenceIndex.Value);
						Assert.IsNotNull(sourceItem);
						Assert.IsTrue(sourceItem.ID == swappedItem.ItemId);
						
						var destItem = TryGetGridItem(swappedItem.Index);
						Assert.IsNotNull(destItem);
						Assert.IsTrue(!sourceItem.IsEmpty); // TODO: we can't verify the item type of the other node with this data structure.

						sourceItem.SetGridIndex(swappedItem.Index);
						sourceItem.DoMoveAction(CalculateGridNodePosition(swappedItem.Index), speed);
						
						destItem.SetGridIndex(swappedItem.ReferenceIndex.Value);
						destItem.DoMoveAction(CalculateGridNodePosition(swappedItem.ReferenceIndex.Value), speed);
					}
				} break;
				
				case Level.EventType.ItemsDestroyed:
				{
					var itemsDestroyedEvent = levelEvent as Level.ItemsDestroyedEvent;

					foreach (var destroyedItem in itemsDestroyedEvent.DestroyedItems)
					{
						var gridItem = TryGetGridItem(destroyedItem.Index);
						Assert.IsNotNull(gridItem);
						Assert.IsTrue(!gridItem.IsEmpty);

						switch (itemsDestroyedEvent.Reason)
						{
							case CauseOfDeath.Matched:
							{
								CreateGridItemMatchedFX(gridItem, destroyedItem.Points, 5f);
							} break;
						}
						
						DestroyGridItemView(gridItem);
					}
				} break;
				
				default:
					Logger.LogError($"Unhandled level event: {levelEvent.EventType}");
					break;
			}
		}

		private void HandleInput()
		{
			if (Input.GetKey(KeyCode.Mouse0))
			{
				// select via the node because it is static in the scene. items might be falling into place.
				var selectedNode = UserInput.PickObject<GridNodeView>(Input.mousePosition);
				if (selectedNode != null && selectedNode.GridNodeConfig.IsOpen)
				{
					var gridItem = TryGetGridItem(selectedNode.Index);
					if (CanSelectGridItem(gridItem))
					{
						// add to the end of the selection
						gridItem.SetSelected(true);
						_selectedGridItems.Add(gridItem);
					}
					else if (CanDeselectGridItems(gridItem))
					{
						// deselect back to the selected node
						for (int i = _selectedGridItems.Count - 1; i >= 0; i--)
						{
							if (_selectedGridItems[i] == gridItem)
								break;
							
							_selectedGridItems[i].SetSelected(false);
							_selectedGridItems.RemoveAt(i);
						}
					}
				}
			}

			if (Input.GetKeyUp(KeyCode.Mouse0))
			{
				// submit
				if (_selectedGridItems.Count > 0)
				{
					var selectedItems = _selectedGridItems.Select(item => item.Index);
					SendLevelCommand(new Level.SubmitMatchCommand(selectedItems));
				}
			}
		}

		private bool CanSelectGridItem(GridItemView gridItem)
		{
			if (gridItem == null || gridItem.IsEmpty)
				return false;	// invalid
			
			if (_selectedGridItems.Count == 0)
				return true;	// first selection

			if (gridItem.ID != _selectedGridItems[0].ID)
				return false;	// TODO: add wildcard rules

			if (_selectedGridItems.Contains(gridItem))
				return false;	// already selected
			
			if (GridIndex.IsNeighbor(gridItem.Index, _selectedGridItems[_selectedGridItems.Count - 1].Index))
				return true;	// must neighbor the last selected item

			return false;
		}

		private bool CanDeselectGridItems(GridItemView gridItem)
		{
			if (gridItem == null)
				return false;	// invalid
			
			if (_selectedGridItems.Count < 2)
				return false;	// not enough items selected

			if (!_selectedGridItems.Contains(gridItem))
				return false;	// not selected

			if (gridItem == _selectedGridItems[_selectedGridItems.Count - 2])
				return true;	// take one step back

			if (!GridIndex.IsNeighbor(gridItem.Index, _selectedGridItems[_selectedGridItems.Count - 1].Index))
				return true;	// must NOT neighbor the last selected item to trace back further (else it's annoying)
			
			return false;
		}


		private void LoadLevel(Level.LoadLevelEvent loadLevelEvent)
		{
			_levelIndex = loadLevelEvent.LevelIndex;
			_levelConfig = loadLevelEvent.LevelConfig;
			
			Logger.LogEditor($"Load level={_levelIndex}, size={_levelConfig.width}x{_levelConfig.height}");

			_levelScoreView.SetGoals(_levelIndex, _levelConfig);
			
			// clear the playfield
			ClearGridNodeInstances();
			ClearGridItemInstances();

			// if there is no grid state then auto-open the level editor
			if (loadLevelEvent.InitialGridState == null)
			{
				MessagePopup.ShowMessage("This level is empty. Edit mode is enabled.");
				_levelConfig = LevelConfig.CreateDefaultLevel(5, 5);
				EnableEditMode();
				return;
			}

			// calculate the playfield dimensions
			Vector3 playfieldSize = _playfieldTransform.sizeDelta;
			_tileSize = CalculateTileSize(playfieldSize, new Vector2(_levelConfig.width, _levelConfig.height));
			_playfieldOrigin = new Vector3(	// [0,0] = lower-left corner
				(_levelConfig.width - 1) * _tileSize * -0.5f, 
				(_levelConfig.height - 1) * _tileSize * -0.5f);
			
			// instantiate the level layout
			for (int y = 0; y < _levelConfig.height; y++)
				for (int x = 0; x < _levelConfig.width; x++)
				{
					var nodeLayout = _levelConfig.GetNodeLayout(x, y);
					
					Assert.IsNotNull(nodeLayout.nodeId, $"Level layout is corrupted at ({x},{y})");

					var pos = CalculateGridNodePosition(x, y);
					
					// load the node from the config
					CreateGridNodeView(x, y, nodeLayout.nodeId, pos);

					// if in edit mode then load the item from the config, else load from the state
					string itemType = _state == State.EditMode ? 
						nodeLayout.itemId : 
						loadLevelEvent.InitialGridState.FirstOrDefault(item => item.Index.x == x && item.Index.y == y)?.ItemId;
					
					// if (itemType != null)	// HACK: load all items, even if they are invalid. this makes the level editor easier to use
					{
						CreateGridItemView(x, y, itemType, pos);
					}
				}
		}

		private GridNodeView CreateGridNodeView(int x, int y, string nodeId, Vector3 position)
		{
			//var gridNode = Instantiate(_gridNodePrefab, _playfieldRootNodes);
			var gridNode = PrefabPool.Get<GridNodeView>(_playfieldRootNodes);
			gridNode.Configure(x, y, nodeId, position, _tileSize);
			_gridNodeInstances.Add(gridNode);
			return gridNode;
		}

		private GridItemView CreateGridItemView(GridIndex index, string itemId)
		{
			return CreateGridItemView(index.x, index.y, itemId, CalculateGridNodePosition(index.x, index.y));
		}

		private GridItemView CreateGridItemView(int x, int y, string itemId, Vector3 position)
		{
			// var gridItem = Instantiate(_gridItemPrefab, _playfieldRootItems);
			var gridItem = PrefabPool.Get<GridItemView>(_playfieldRootItems);
			gridItem.Configure(x, y, itemId, position, _tileSize);
			_gridItemInstances.Add(gridItem);
			return gridItem;
		}

		private void DestroyGridItemView(GridItemView gridItem)
		{
			gridItem.Invalidate();
			_gridItemInstances.Remove(gridItem);
			PrefabPool.Return(gridItem);
		}

		private GridItemFX CreateGridItemMatchedFX(GridItemView gridItem, int points, float seconds)
		{
			var fx = PrefabPool.Get<GridItemFX>(_playfieldFxItems);
			fx.Configure(gridItem.Index.x, gridItem.Index.y, gridItem.ID, gridItem.transform.localPosition, _tileSize);
			fx.Show(points, seconds);
			return fx;
		}

		private Vector3 CalculateGridNodePosition(GridIndex index)
		{
			return CalculateGridNodePosition(index.x, index.y);
		}
		
		private Vector3 CalculateGridNodePosition(int x, int y)
		{
			return new Vector3(_playfieldOrigin.x + (x * _tileSize), _playfieldOrigin.y + (y * _tileSize));
		}

		private static float CalculateTileSize(Vector2 playfieldSize, Vector2 gridSize)
		{
			return Mathf.Min(playfieldSize.x / gridSize.x, playfieldSize.y / gridSize.y);
		}

		private void ClearGridNodeInstances()
		{
			for (int i = 0; i < _gridNodeInstances.Count; i++)
			{
				if (_gridNodeInstances[i] != null)
				{
					// Destroy(_gridNodeInstances[i].gameObject);
					PrefabPool.Return(_gridNodeInstances[i]);
				}
			}
			_gridNodeInstances.Clear();
		}

		private void ClearGridItemInstances()
		{
			for (int i = 0; i < _gridItemInstances.Count; i++)
			{
				if (_gridItemInstances[i] != null)
				{
					// Destroy(_gridItemInstances[i].gameObject);
					PrefabPool.Return(_gridItemInstances[i]);
				}
			}
			_gridItemInstances.Clear();
		}
		
		private void DeselectAllGridItems()
		{
			if (_selectedGridItems.Count > 0)
			{
				foreach (var item in _selectedGridItems)
					item.SetSelected(false);

				_selectedGridItems.Clear();
			}
		}

		private GridItemView TryGetGridItem(GridIndex index)
		{
			return TryGetGridItem(index.x, index.y);
		}

		private GridItemView TryGetGridItem(int x, int y)
		{
			return _gridItemInstances.FirstOrDefault(item => item.Index.x == x && item.Index.y == y);
		}

		public void OnClick_Pause()
		{
			PopupViewController.Open<LevelPausePopup>();
		}

		public void OnClick_Shuffle()
		{
			PopupViewController.Open<LevelShufflePopup>();
		}

		public void OnClick_ShuffleConfirmed()
		{
			SendLevelCommand(new Level.ShuffleGridCommand());
		}

		
		#region Level Editor
		////////////////////////////////////////////////
		[Header("Level Editor")]
		[SerializeField] private GameObject _editModePanel = null;
		[SerializeField] private TMP_InputField _editModeInputWidth = null;
		[SerializeField] private TMP_InputField _editModeInputHeight = null;
		[SerializeField] private TextMeshProUGUI _editModeTextCategory = null;
		[SerializeField] private TMP_InputField _editModeInputMaxItemTypes = null;
		[SerializeField] private TMP_InputField _editModeInputGoal1 = null;
		[SerializeField] private TMP_InputField _editModeInputGoal2 = null;
		[SerializeField] private TMP_InputField _editModeInputGoal3 = null;
		[SerializeField] private TMP_InputField _editModeInputMoves = null;

		private bool _editModeItemLayer = true;

		private void EditMode_HandleInput()
		{
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				if (_editModeItemLayer)
				{
					// tap a grid item to cycle its type
					var gridItem = UserInput.PickObject<GridItemView>(Input.mousePosition);
					if (gridItem != null)
					{
						var selectedItemConfig = gridItem.GridItemConfig;
						
						// HACK: this only works because we set the MatchIndex values to sequential integers for each Category
						int nextItemIndex = selectedItemConfig.MatchIndex + 1;
						var nextItem = GameConfig.AllGridItems.FirstOrDefault(item => item.MatchIndex == nextItemIndex && item.Category == selectedItemConfig.Category);
						if (nextItem.ID == null)
							nextItem = GameConfig.GetDefaultLayoutGridItem(selectedItemConfig.Category);	// loop around

						gridItem.EditMode_SetId(nextItem.ID);
					}
				}
				else
				{
					// tap a grid node to cycle its type
					var gridNode = UserInput.PickObject<GridNodeView>(Input.mousePosition);
					if (gridNode != null)
					{
						// HACK: this only works because we only have 2 nodes: Open and Closed
						var selectedNodeIsOpen = gridNode.GridNodeConfig.IsOpen;
						var nextNode = GameConfig.AllGridNodes.First(node => node.IsOpen != selectedNodeIsOpen);
						
						gridNode.EditMode_SetId(nextNode.ID);
						
						// update the item
						var gridItem = TryGetGridItem(gridNode.Index);
						if (gridItem != null)
						{
							if (nextNode.IsOpen)
								gridItem.EditMode_SetId(GameConfig.GetDefaultLayoutGridItem(_editModeTextCategory.text).ID);
							else
								gridItem.EditMode_SetId(null);
						}
					}
				}
			}
		}

		public void OnClick_EditMode_NextCategory()
		{
			var allCategories = GameConfig.GetAllGridItemCategories().ToArray();
			for (int i = 0; i < allCategories.Length; i++)
			{
				if (allCategories[i] == _editModeTextCategory.text)
				{
					int index = i + 1;
					if (index >= allCategories.Length)
						index = 0;

					string nextCategory = allCategories[index];
					_editModeTextCategory.text = nextCategory;
					
					_editModeInputMaxItemTypes.text = "0";

					var defaultGridItem = GameConfig.GetDefaultLayoutGridItem(nextCategory);
					foreach (var gridItem in _gridItemInstances)
					{
						if (!gridItem.IsEmpty)
							gridItem.EditMode_SetId(defaultGridItem.ID);
					}

					return;
				}
			}
		}

		public void OnClick_EditMode_ToggleLayer()
		{
			_editModeItemLayer = !_editModeItemLayer;
			
			foreach (var item in _gridItemInstances)
				item.gameObject.SetActive(_editModeItemLayer && !item.IsEmpty);
		}

		public void OnClick_EditMode_Save()
		{
			var allCategories = GameConfig.GetAllGridItemCategories();
			if (!allCategories.Contains(_editModeTextCategory.text))
			{
				MessagePopup.ShowMessage($"Invalid Category: '{_editModeTextCategory.text}'");
				return;
			}
			
			if (!int.TryParse(_editModeInputMaxItemTypes.text, out var maxItemTypes))
			{
				MessagePopup.ShowMessage($"Invalid Max Item Types: '{_editModeInputMaxItemTypes.text}'");
				return;
			}

			if (!int.TryParse(_editModeInputGoal1.text, out var goal1))
			{
				MessagePopup.ShowMessage($"Invalid Goal #1: '{_editModeInputGoal1.text}'");
				return;
			}

			if (!int.TryParse(_editModeInputGoal2.text, out var goal2))
			{
				MessagePopup.ShowMessage($"Invalid Goal #2: '{_editModeInputGoal2.text}'");
				return;
			}

			if (!int.TryParse(_editModeInputGoal3.text, out var goal3))
			{
				MessagePopup.ShowMessage($"Invalid Goal #3: '{_editModeInputGoal3.text}'");
				return;
			}

			if (!int.TryParse(_editModeInputMoves.text, out var moves) || moves < 1)
			{
				MessagePopup.ShowMessage($"Invalid Max Moves: '{_editModeInputMoves.text}'");
				return;
			}

			_levelConfig.category = _editModeTextCategory.text;
			_levelConfig.excludeItemIds = EditMode_ConvertMaxItemTypesToExcludedItemIds(_editModeTextCategory.text, Mathf.Max(0, maxItemTypes));
			_levelConfig.goalType = LevelGoalType.Points;
			_levelConfig.goal1 = Mathf.Max(0, goal1);
			_levelConfig.goal2 = Mathf.Max(0, goal2);
			_levelConfig.goal3 = Mathf.Max(0, goal3);
			_levelConfig.challengeType = LevelChallengeType.Moves;
			_levelConfig.challengeValue = moves;

			foreach (var gridNode in _gridNodeInstances)
			{
				var gridItem = TryGetGridItem(gridNode.Index);
				var nodeLayout = _levelConfig.GetNodeLayout(gridNode.Index);
				nodeLayout.nodeId = gridNode.ID;
				nodeLayout.itemId = (gridItem != null && gridNode.GridNodeConfig.IsOpen) ? gridItem.ID : null;
				_levelConfig.SetNodeLayout(gridNode.Index, nodeLayout);
			}

			LevelConfig.ExportLevelFile(_levelIndex, _levelConfig);
		}
		
		private string[] EditMode_ConvertMaxItemTypesToExcludedItemIds(string category, int maxItemTypes)
		{
			List<string> excludedItems = new List<string>();

			var allItems = GameConfig.AllGridItems
				.Where(item => item.Category == category && 
				               item.DropFrequency > 0 && 
				               item.MatchType != GridItemMatchType.None).ToArray();

			if (maxItemTypes > 0 && maxItemTypes < allItems.Length - 1) // else don't exlude anything
			{
				// TODO: let designers choose which items to exlude
				for (int i = allItems.Length - 1; i >= maxItemTypes; i--) // chop off items from the end of the list
					excludedItems.Add(allItems[i].ID);
			}

			return excludedItems.ToArray();
		}

		private int EditMode_ConvertExcludedItemIdsToMaxItemTypes(string category, string[] excludedItems)
		{
			if (excludedItems.Length == 0)
				return 0;
			
			var includedItems = GameConfig.AllGridItems
				.Where(item => item.Category == category && 
				               item.DropFrequency > 0 && 
				               item.MatchType != GridItemMatchType.None && 
				               !excludedItems.Contains(item.ID)).ToArray();

			return includedItems.Length;
		}
		
		public void OnClick_EditMode_Resize()
		{
			if (!int.TryParse(_editModeInputWidth.text, out var width))
				return;
			
			if (!int.TryParse(_editModeInputHeight.text, out var height))
				return;

			LevelConfig.ResizeLevel(width, height, ref _levelConfig);
			LevelConfig.ExportLevelFile(_levelIndex, _levelConfig);

			// reload the level
			Level.ExecuteCommand(new Level.LoadLevelCommand(_levelIndex));
		}
		
		public void ToggleEditMode()
		{
			if (_state == State.EditMode)
				DisableEditMode();
			else
				EnableEditMode();
				
			// reload the level, so we can initialize in the proper mode
			Level.ExecuteCommand(new Level.LoadLevelCommand(_levelIndex));
		}

		private void EnableEditMode()
		{
			SetState(State.EditMode);

			// init the editor controls
			_editModeInputWidth.text = _levelConfig.width.ToString();
			_editModeInputHeight.text = _levelConfig.height.ToString();
			_editModeTextCategory.text = _levelConfig.category;
			_editModeInputMaxItemTypes.text = EditMode_ConvertExcludedItemIdsToMaxItemTypes(_levelConfig.category, _levelConfig.excludeItemIds).ToString();
			_editModeInputGoal1.text = _levelConfig.goal1.ToString();
			_editModeInputGoal2.text = _levelConfig.goal2.ToString();
			_editModeInputGoal3.text = _levelConfig.goal3.ToString();
			_editModeInputMoves.text = _levelConfig.challengeValue.ToString();
			_editModePanel.SetActive(true);
		}

		private void DisableEditMode()
		{
			SetState(State.Uninitialized);
			
			// hide the controls
			_editModePanel.SetActive(false);
		}
		////////////////////////////////////////////////
		#endregion
	}
}
