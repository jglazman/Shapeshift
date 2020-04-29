//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Glazman.Shapeshift
{
	public class LevelView : MonoBehaviour
	{
		[SerializeField] private RectTransform _playfieldTransform = null;
		[SerializeField] private GridNodeView _gridNodePrefab = null;
		[SerializeField] private GridItemView _gridItemPrefab = null;

		private List<GridNodeView> _gridNodeInstances = new List<GridNodeView>();
		private List<GridItemView> _gridItemInstances = new List<GridItemView>();

		private LevelConfig _levelConfig;
		
		private bool _isEditMode;
		private int _levelIndex;
		private float _tileSize;
		private Vector3 _playfieldOrigin; // [0,0] = lower-left corner


		private void Awake()
		{
			Level.ListenForLevelEvents(HandleLevelEvent);
			DisableEditMode();
		}

		private void OnDestroy()
		{
			Level.StopListeningForLevelEvents(HandleLevelEvent);
			SpriteResource.ClearCache();
		}

		private void Update()
		{
			if (_isEditMode)
				HandleEditModeInput();
		}

		
		private void HandleLevelEvent(Level.Event levelEvent)
		{
			switch (levelEvent.EventType)
			{
				// TODO: keep transitioner in loading mode until we receive a LoadLevel event
				case Level.EventType.LoadLevel:
				{
					//int levelIndex = levelEvent.Payload.GetInt((int)Level.LoadLevelEvent.Fields.LevelIndex);

					LoadLevel(levelEvent as Level.LoadLevelEvent);
				} break;

				case Level.EventType.Win:
					PopupViewController.Open<LevelWinPopup>();
					break;
				
				case Level.EventType.Lose:
					PopupViewController.Open<LevelLosePopup>();
					break;
			}
		}


		private void LoadLevel(Level.LoadLevelEvent loadLevelEvent)
		{
			_levelIndex = loadLevelEvent.levelIndex;
			_levelConfig = loadLevelEvent.levelConfig;
			
			Logger.LogEditor($"Load level={_levelIndex}, size={_levelConfig.width}x{_levelConfig.height}");

			// clear the playfield
			ClearGridNodeInstances();
			ClearGridItemInstances();

			// if there is no grid state then auto-open the level editor
			if (loadLevelEvent.gridState == null)
			{
				MessagePopup.ShowMessage("This level has no config. Edit mode is enabled.");
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
			for (uint y = 0; y < _levelConfig.height; y++)
				for (uint x = 0; x < _levelConfig.width; x++)
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
					int itemType = _isEditMode ? 
						nodeLayout.itemType : 
						loadLevelEvent.gridState.FirstOrDefault(item => item.x == x && item.y == y)?.itemType ?? -1;
					
					// if (itemType >= 0)	// HACK: load all items, even if they are invalid. this makes the level editor easier to use
					{
						var gridItem = Instantiate(_gridItemPrefab, _playfieldTransform);
						gridItem.Configure(x, y, itemType, pos, _tileSize);
						_gridItemInstances.Add(gridItem);
					}
				}
		}

		private Vector3 CalculateGridNodePosition(uint x, uint y)
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

		private GridItemView TryGetGridItem(uint x, uint y)
		{
			return _gridItemInstances.FirstOrDefault(item => item.X == x && item.Y == y);
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

						var gridItem = TryGetGridItem(gridNode.X, gridNode.Y);
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
				var nodeLayout = _levelConfig.GetNodeLayout(gridNode.X, gridNode.Y);
				
				nodeLayout.nodeType = gridNode.NodeType;
				
				var gridItem = TryGetGridItem(gridNode.X, gridNode.Y);
				if (gridItem != null)
					nodeLayout.itemType = gridItem.ItemType;
				
				_levelConfig.SetNodeLayout(gridNode.X, gridNode.Y, nodeLayout);
			}

			LevelConfig.ExportLevelFile(_levelIndex, _levelConfig);
		}
		
		public void OnClick_EditMode_Resize()
		{
			if (!uint.TryParse(_debugInputWidth.text, out var width))
				return;
			
			if (!uint.TryParse(_debugInputHeight.text, out var height))
				return;

			LevelConfig.ResizeLevel(width, height, ref _levelConfig);
			LevelConfig.ExportLevelFile(_levelIndex, _levelConfig);
			
			// reload the level
			Level.ExecuteCommand(new Level.LoadLevelCommand(_levelIndex));
		}
		
		public void ToggleEditMode()
		{
			_isEditMode = !_isEditMode;
			
			if (_isEditMode)
				EnableEditMode();
			else
				DisableEditMode();
				
			Level.ExecuteCommand(new Level.LoadLevelCommand(_levelIndex));
		}

		private void EnableEditMode()
		{
			_isEditMode = true;
			
			// init the editor controls
			_debugInputWidth.text = _levelConfig.width.ToString();
			_debugInputHeight.text = _levelConfig.height.ToString();
			_debugPanel.SetActive(true);
		}

		private void DisableEditMode()
		{
			_isEditMode = false;
			_debugPanel.SetActive(false);
		}
		////////////////////////////////////////////////
		#endregion
	}
}
