//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	// [GameLogger]
	public static class Game
	{
		public enum State
		{
			Uninitialized,
			MainMenu,
			WorldMap,
			Level,
			Teardown
		}

		private static State _state = State.Uninitialized;
		

		public static void Initialize()
		{
			SetState(State.MainMenu, null);
		}

		public static void Notify(GameMessage message)
		{
			Assert.IsTrue(message.GameMessageType != GameMessageType.Undefined, "[Game] Tried to send an undefined message.");
			
			switch (message.GameMessageType)
			{
				case GameMessageType.Navigate_RefugeZero:
				{
					Application.OpenURL("https://www.refugezero.com");
				} break;
				
				case GameMessageType.Navigate_Settings:
				{
					PopupViewController.Open<GameSettingsPopup>();
				} break;

				case GameMessageType.Navigate_WorldMap:
				{
					SetState(State.WorldMap, null);
				} break;
				
				case GameMessageType.Navigate_Level:
				{
					SetState(State.Level, () =>
					{
						Level.LoadLevel((message as LoadLevelMessage).levelIndex);
					});
				} break;
			}
		}
		

		private static void SetState(State nextState, System.Action callback)
		{
			if (nextState == _state)
			{
				Debug.LogError($"[Game] Set state to the same state: {_state}");
				return;	// nothing to do
			}

			// clean up previous state
			switch (_state)
			{
				case State.MainMenu:
				{
					// TODO: anything?
				} break;

				case State.WorldMap:
				{
					// TODO: anything?
				} break;

				case State.Level:
				{
					// TODO: anything?
				} break;
			}

			// switch to the next state
			_state = nextState;
			
			// handle the state change
			switch (_state)
			{
				case State.MainMenu:
				{
					SceneController.LoadScene(SceneName.MainMenu, callback);
				} break;

				case State.WorldMap:
				{
					SceneController.LoadScene(SceneName.WorldMap, callback);
				} break;

				case State.Level:
				{
					SceneController.LoadScene(SceneName.Level, callback);
				} break;

				// default:
				// {
				// 	Assert.IsTrue(false, $"[Game] Unhandled state: {nextState}");
				// } break;
			}
		}
	}
}
