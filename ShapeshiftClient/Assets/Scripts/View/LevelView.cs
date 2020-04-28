//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Glazman.Shapeshift
{
	public class LevelView : MonoBehaviour
	{
		[SerializeField] private RectTransform _playfieldTransform = null;
		[SerializeField] private GridNodeView _gridNodePrefab = null;

		private Database.Data<LevelConfig> _levelConfig;
		private List<GridNodeView> _gridNodeInstances = new List<GridNodeView>();

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
			
			GridNodeView.ClearSpriteCache();
		}


		private void Update()
		{
			if (_isEditMode)
				HandleEditModeInput();
		}

		private void HandleEditModeInput()
		{
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				// tap a grid node to cycle its type
				var gridNode = UserInput.PickObject<GridNodeView>(Input.mousePosition);
				if (gridNode != null)
				{
					int tileType = (int)gridNode.NodeType;
					
					// TODO: this only works because we set the GridNodeType values to sequential integers starting at zero
					tileType++;
					if (tileType >= Enum.GetValues(typeof(GridNodeType)).Length)
						tileType = 1; // skip 0=Undefined
					
					gridNode.SetNodeType((GridNodeType)tileType);
				}
			}
		}
		
		private void HandleLevelEvent(Level.Event levelEvent)
		{
			switch (levelEvent.EventType)
			{
				case Level.EventType.LoadLevel:
				{
					int index = levelEvent.Payload.GetInt((int)Level.LoadLevelEvent.Fields.LevelIndex);
					LoadLevel(index);
				} break;

				case Level.EventType.Win:
					PopupViewController.Open<LevelWinPopup>();
					break;
				
				case Level.EventType.Lose:
					PopupViewController.Open<LevelLosePopup>();
					break;
			}
		}


		private void LoadLevel(int levelIndex)
		{
			_levelIndex = levelIndex;
			_levelConfig = Database.Load<LevelConfig>(levelIndex);

			Logger.LogEditor($"Load level={_levelIndex}, size={_levelConfig.Value.width}x{_levelConfig.Value.height}");

			// clear the playfield (only needed for the level editor)
			for (int i = 0; i < _gridNodeInstances.Count; i++)
			{
				if (_gridNodeInstances[i] != null)
					Destroy(_gridNodeInstances[i].gameObject);
			}
			_gridNodeInstances.Clear();

			// if the config is invalid then auto-open the level editor
			if (_levelConfig.Value.width == 0 || _levelConfig.Value.height == 0)
			{
				MessagePopup.ShowMessage("This level has no config. Edit mode is enabled.");
				EnableEditMode();
				return;
			}

			// calculate the playfield dimensions
			Vector3 playfieldSize = _playfieldTransform.sizeDelta;
			_tileSize = CalculateTileSize(playfieldSize, new Vector2(_levelConfig.Value.width, _levelConfig.Value.height));
			_playfieldOrigin = new Vector3(	// [0,0] = lower-left corner
				(_levelConfig.Value.width - 1) * _tileSize * -0.5f, 
				(_levelConfig.Value.height - 1) * _tileSize * -0.5f);
			
			// instantiate the level layout
			for (uint y = 0; y < _levelConfig.Value.height; y++)
				for (uint x = 0; x < _levelConfig.Value.width; x++)
				{
					var nodeType = _levelConfig.Value.GetNodeType(x, y);
					if (nodeType == GridNodeType.Undefined)
						continue;

					var pos = CalculateGridNodePosition(x, y);
					var gridNode = Instantiate(_gridNodePrefab, _playfieldTransform);
					gridNode.Configure(x, y, nodeType, pos, _tileSize);
					
					_gridNodeInstances.Add(gridNode);
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

		public void OnClick_EditMode_Save()
		{
			foreach (var gridNode in _gridNodeInstances)
				_levelConfig.Value.SetNodeType(gridNode.X, gridNode.Y, gridNode.NodeType);

			Database.Save(_levelConfig);
		}
		
		public void OnClick_EditMode_Resize()
		{
			if (!uint.TryParse(_debugInputWidth.text, out var width))
				return;
			
			if (!uint.TryParse(_debugInputHeight.text, out var height))
				return;

			LevelConfig.EditMode_ResizeLevel(width, height, ref _levelConfig.Value);
			
			Database.Save(_levelConfig);
			
			Level.ExecuteCommand(new Level.LoadLevelCommand(_levelIndex));
		}
		
		public void OnClick_ToggleEditMode()
		{
			_isEditMode = !_isEditMode;
			if (_isEditMode)
				EnableEditMode();
			else
				DisableEditMode();
		}


		private void EnableEditMode()
		{
			_isEditMode = true;

			_debugInputWidth.text = _levelConfig.Value.width.ToString();
			_debugInputHeight.text = _levelConfig.Value.height.ToString();
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
