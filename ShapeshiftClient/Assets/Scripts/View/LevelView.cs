//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using UnityEngine;

namespace Glazman.Shapeshift
{
	public class LevelView : MonoBehaviour
	{
		[SerializeField] private TileNodeView _tileNodePrefab;
		

		private void Awake()
		{
			Level.ListenForLevelEvents(HandleLevelEvent);
		}

		private void OnDestroy()
		{
			Level.StopListeningForLevelEvents(HandleLevelEvent);
		}

		
		private void HandleLevelEvent(Level.Event levelEvent)
		{
			switch (levelEvent.EventType)
			{
				case Level.EventType.LoadLevel:
				{
					int levelIndex = levelEvent.Payload.GetInt((int)Level.LoadLevelEvent.Fields.LevelIndex);
					var config = LevelConfig.Load(levelIndex);
					Logger.LogEditor($"Load level={levelIndex}, size={config.width}x{config.height}");
				} break;

				case Level.EventType.Win:
					PopupViewController.Open<LevelWinPopup>();
					break;
				
				case Level.EventType.Lose:
					PopupViewController.Open<LevelLosePopup>();
					break;
			}
		}


		public void OnClick_Pause()
		{
			PopupViewController.Open<LevelPausePopup>();
		}

		public void OnClick_OpenDebug()
		{
			PopupViewController.Open<LevelDebugPopup>();
		}
	}
}
