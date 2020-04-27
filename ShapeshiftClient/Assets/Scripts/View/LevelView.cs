//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;

namespace Glazman.Shapeshift
{
	public class LevelView : MonoBehaviour
	{

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
			switch (levelEvent.eventType)
			{
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

		public void OnClick_Win()
		{
			// TODO: temp debug
			Level.ExecuteCommand(new Level.Command(Level.CommandType.Win));
		}

		public void OnClick_Lose()
		{
			// TODO: temp debug
			Level.ExecuteCommand(new Level.Command(Level.CommandType.Lose));
		}
		
	}
}
