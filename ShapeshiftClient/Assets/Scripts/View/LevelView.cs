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
		[SerializeField] private TileNodeView _tileNodePrefab = null;


		private bool _isEditMode;
		private int _levelIndex;
		private Database.Data<LevelConfig> _levelConfig;
		private List<TileNodeView> _tileNodeInstances = new List<TileNodeView>();


		private void Awake()
		{
			Level.ListenForLevelEvents(HandleLevelEvent);
			DisableEditMode();
		}

		private void OnDestroy()
		{
			Level.StopListeningForLevelEvents(HandleLevelEvent);
			
			TileNodeView.ClearSpriteCache();
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


		// private static LevelConfig LoadLevelConfig(int levelIndex)
		// {
		// 	return Database.Load<LevelConfig>(levelIndex).Value;
		// }
		//
		// private static void SaveLevelConfig(int levelIndex, LevelConfig config)
		// {
		// 	var data = Database.Load<LevelConfig>(levelIndex);
		// 	data.Value = config;
		// 	Database.Save(data);
		// }
		

		private void LoadLevel(int levelIndex)
		{
			_levelIndex = levelIndex;
			_levelConfig = Database.Load<LevelConfig>(levelIndex);

			Logger.LogEditor($"Load level={_levelIndex}, size={_levelConfig.Value.width}x{_levelConfig.Value.height}");

			// clear the playfield. only needed for the level editor.
			for (int i = 0; i < _tileNodeInstances.Count; i++)
			{
				if (_tileNodeInstances[i] != null)
					Destroy(_tileNodeInstances[i].gameObject);
			}
			_tileNodeInstances.Clear();

			if (_levelConfig.Value.width == 0 || _levelConfig.Value.height == 0)
			{
				MessagePopup.ShowMessage("This level has no config. Edit mode is enabled.");
				EnableEditMode();
				return;
			}

			float nodeSize = GetNodeSize();
			
			for (uint y = 0; y < _levelConfig.Value.height; y++)
				for (uint x = 0; x < _levelConfig.Value.width; x++)
				{
					var nodeType = _levelConfig.Value.GetNodeType(x, y);
					if (nodeType == TileNodeType.Undefined)
						continue;
					
					var tileNode = Instantiate(_tileNodePrefab, _playfieldTransform);
					tileNode.SetNodeType(nodeType);
					tileNode.SetPosition(GetNodePosition(x, y));
					tileNode.SetSize(GetNodeSize());
					
					_tileNodeInstances.Add(tileNode);
				}

		}

		private Vector3 GetNodePosition(uint x, uint y)
		{
			return new Vector3(x * 50f, y * 50f);
		}

		private float GetNodeSize()
		{
			return 50f;
		}


		public void OnClick_Pause()
		{
			PopupViewController.Open<LevelPausePopup>();
		}

		
		////////////////////////////////////////////////
		// debug
		[Header("Debug")]
		[SerializeField] private GameObject _debugPanel = null;
		[SerializeField] private InputField _debugInputWidth = null;
		[SerializeField] private InputField _debugInputHeight = null;
		
		public void OnClick_DebugResize()
		{
			// TODO: error feedback
			if (_levelConfig == null)
				return;
			
			if (!uint.TryParse(_debugInputWidth.text, out var width))
				return;
			
			if (!uint.TryParse(_debugInputHeight.text, out var height))
				return;

			LevelConfig.Debug_ResizeLevel(width, height, ref _levelConfig.Value);
			
			Database.Save(_levelConfig);
			
			Level.ExecuteCommand(new Level.LoadLevelCommand(_levelIndex));
		}
		
		public void OnClick_DebugMenu()
		{
			PopupViewController.Open<LevelDebugPopup>();
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
	}
}
