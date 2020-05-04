//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public static partial class Game
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
			WordMap.Initialize();
			GameConfig.Initialize();
			UserInput.Initialize();
			
			SetState(State.MainMenu, null);
		}

		public static void Notify(Message message)
		{
			Assert.IsTrue(message.MessageType != MessageType.Undefined, "[Game] Tried to send an undefined message.");
			
			switch (message.MessageType)
			{
				case MessageType.GoToWorldMap:
				{
					SetState(State.WorldMap, null);
				} break;
				
				case MessageType.GoToLevel:
				{
					SetState(State.Level, () =>
					{
						int levelIndex = (message as GoToLevelMessage).LevelIndex;
						Level.ExecuteCommand(new Level.LoadLevelCommand(levelIndex));
					});
				} break;
			}
		}
		

		private static void SetState(State nextState, System.Action callback)
		{
			Logger.LogEditor($"Set state: {_state} -> {nextState}");
			
			if (nextState == _state)
			{
				Logger.LogError($"Set state to the same state: {_state}");
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
