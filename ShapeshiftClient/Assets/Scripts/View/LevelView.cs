//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Glazman.Shapeshift
{
	public class LevelView : MonoBehaviour
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
		[SerializeField] private GridNodeView _gridNodePrefab = null;
		[SerializeField] private GridItemView _gridItemPrefab = null;

		private Queue<Level.Event> _pendingEvents = new Queue<Level.Event>();
		private List<GridNodeView> _gridNodeInstances = new List<GridNodeView>();
		private List<GridItemView> _gridItemInstances = new List<GridItemView>();
		private List<GridItemView> _selectedGridItems = new List<GridItemView>();
		private LevelConfig _levelConfig;
		private int _levelIndex;
		private float _tileSize;
		private Vector3 _playfieldOrigin; // [0,0] = lower-left corner


		private void Awake()
		{
			Level.ListenForLevelEvents(ReceiveLevelEvents);
			DisableEditMode();
		}

		private void OnDestroy()
		{
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
					// idle
				} break;
				
				case State.EditMode:
				{
					if (_pendingEvents.Count > 0 && _pendingEvents.Peek().EventType == Level.EventType.LoadLevel)
						HandlePendingEvents();
					
					HandleEditModeInput();
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
				
				// while (_pendingEvents.Count > 0)	// TODO: one per frame while debugging
					HandleLevelEvent(_pendingEvents.Dequeue());

				// TODO: multi-frame handling of events (animations, etc.)
				SetState(State.WaitingForInput);

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
					PopupViewController.Open<LevelWinPopup>();
				} break;

				case Level.EventType.Lose:
				{
					PopupViewController.Open<LevelLosePopup>();
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

					foreach (var createdItem in itemsCreatedEvent.CreatedItems)
					{
						var gridItem = TryGetGridItem(createdItem.Index);
						Assert.IsNotNull(gridItem);
						Assert.IsTrue(gridItem.ItemType == -1);

						gridItem.SetType(createdItem.ItemType, true);
					}
				} break;

				case Level.EventType.ItemsMoved:
				{
					var itemsMovedEvent = levelEvent as Level.ItemsMovedEvent;

					foreach (var movedItem in itemsMovedEvent.MovedItems)
					{
						var sourceItem = TryGetGridItem(movedItem.ReferenceIndex.Value);
						Assert.IsNotNull(sourceItem);
						Assert.IsTrue(sourceItem.ItemType == movedItem.ItemType);

						var destItem = TryGetGridItem(movedItem.Index);
						Assert.IsNotNull(destItem);
						Assert.IsTrue(destItem.ItemType == -1);

						sourceItem.SetType(-1);
						destItem.SetType(movedItem.ItemType, true);
					}
				} break;
				
				case Level.EventType.ItemsDestroyed:
				{
					var itemsDestroyedEvent = levelEvent as Level.ItemsDestroyedEvent;

					foreach (var destroyedItem in itemsDestroyedEvent.DestroyedItems)
					{
						var gridItem = TryGetGridItem(destroyedItem.Index);
						Assert.IsNotNull(gridItem);
						Assert.IsTrue(gridItem.ItemType > 0);

						gridItem.SetType(-1, true);
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
				if (selectedNode != null && selectedNode.NodeType == GridNodeType.Open)
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
			if (gridItem == null || gridItem.ItemType <= 0)
				return false;	// invalid
			
			if (_selectedGridItems.Count == 0)
				return true;	// first selection

			if (gridItem.ItemType != _selectedGridItems[0].ItemType)
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

			// clear the playfield
			ClearGridNodeInstances();
			ClearGridItemInstances();

			// if there is no grid state then auto-open the level editor
			if (loadLevelEvent.InitialGridState == null)
			{
				MessagePopup.ShowMessage("This level is invalid. Edit mode is enabled.");
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
					if (nodeLayout.nodeType == GridNodeType.Undefined)
						continue;

					var pos = CalculateGridNodePosition(x, y);
					
					// load the node from the config
					var gridNode = Instantiate(_gridNodePrefab, _playfieldTransform);
					gridNode.Configure(x, y, (int)nodeLayout.nodeType, pos, _tileSize);
					_gridNodeInstances.Add(gridNode);

					// if in edit mode then load the item from the config, else load from the state
					int itemType = _state == State.EditMode ? 
						nodeLayout.itemType : 
						loadLevelEvent.InitialGridState.FirstOrDefault(item => item.index.x == x && item.index.y == y)?.itemType ?? -1;
					
					// if (itemType >= 0)	// HACK: load all items, even if they are invalid. this makes the level editor easier to use
					{
						var gridItem = Instantiate(_gridItemPrefab, _playfieldTransform);
						gridItem.Configure(x, y, itemType, pos, _tileSize);
						_gridItemInstances.Add(gridItem);
					}
				}
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
					Destroy(_gridNodeInstances[i].gameObject);
			}
			_gridNodeInstances.Clear();
		}

		private void ClearGridItemInstances()
		{
			for (int i = 0; i < _gridItemInstances.Count; i++)
			{
				if (_gridItemInstances[i] != null)
					Destroy(_gridItemInstances[i].gameObject);
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
			return _gridItemInstances.FirstOrDefault(item => item.Index.x == index.x && item.Index.y == index.y);
		}

		private GridItemView TryGetGridItem(int x, int y)
		{
			return _gridItemInstances.FirstOrDefault(item => item.Index.x == x && item.Index.y == y);
		}

		public void OnClick_Pause()
		{
			PopupViewController.Open<LevelPausePopup>();
		}

		
		#region Level Editor
		////////////////////////////////////////////////
		[Header("Debug")]
		[SerializeField] private GameObject _debugPanel = null;
		[SerializeField] private InputField _debugInputWidth = null;
		[SerializeField] private InputField _debugInputHeight = null;

		private bool _editModeItemLayer = true;

		private void HandleEditModeInput()
		{
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				if (_editModeItemLayer)
				{
					// tap a grid item to cycle its type
					var gridItem = UserInput.PickObject<GridItemView>(Input.mousePosition);
					if (gridItem != null)
					{
						int itemType = (int)gridItem.Type;
					
						// TODO: this only works because we set the GridItemType values to sequential integers starting at zero
						itemType++;
						if (itemType >= GridItemView.NumItemTypes)
							itemType = 0; // loop around to 0=random
					
						gridItem.SetType(itemType);
					}
				}
				else
				{
					// tap a grid node to cycle its type
					var gridNode = UserInput.PickObject<GridNodeView>(Input.mousePosition);
					if (gridNode != null)
					{
						int nodeType = (int)gridNode.NodeType;
					
						// TODO: this only works because we set the GridNodeType values to sequential integers starting at zero
						nodeType++;
						if (nodeType >= GridNodeView.NumNodeTypes)
							nodeType = 1; // loop around, but skip 0=Undefined
					
						gridNode.SetType(nodeType);

						var gridItem = TryGetGridItem(gridNode.Index);
						if (gridItem != null)
						{
							if (gridNode.NodeType == GridNodeType.Closed)
								gridItem.SetType(-1);
							else
								gridItem.SetType(0);
						}
					}
				}
			}
		}

		public void OnClick_EditMode_ToggleLayer()
		{
			_editModeItemLayer = !_editModeItemLayer;
			
			foreach (var item in _gridItemInstances)
				item.gameObject.SetActive(_editModeItemLayer && item.ItemType >= 0);
		}

		public void OnClick_EditMode_Save()
		{
			foreach (var gridNode in _gridNodeInstances)
			{
				var nodeLayout = _levelConfig.GetNodeLayout(gridNode.Index);
				
				nodeLayout.nodeType = gridNode.NodeType;
				
				var gridItem = TryGetGridItem(gridNode.Index);
				if (gridItem != null)
					nodeLayout.itemType = gridItem.ItemType;
				
				_levelConfig.SetNodeLayout(gridNode.Index, nodeLayout);
			}

			LevelConfig.ExportLevelFile(_levelIndex, _levelConfig);
		}
		
		public void OnClick_EditMode_Resize()
		{
			if (!int.TryParse(_debugInputWidth.text, out var width))
				return;
			
			if (!int.TryParse(_debugInputHeight.text, out var height))
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
			_debugInputWidth.text = _levelConfig.width.ToString();
			_debugInputHeight.text = _levelConfig.height.ToString();
			_debugPanel.SetActive(true);
		}

		private void DisableEditMode()
		{
			SetState(State.Uninitialized);
			
			// hide the controls
			_debugPanel.SetActive(false);
		}
		////////////////////////////////////////////////
		#endregion
	}
}
