//
// Copyright (c) 2020 Jeremy Glazman
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Glazman.Shapeshift
{
	public static class Level
	{
		public enum EventType
		{
			Undefined = 0,
			Win,
			Lose
		}

		public class Event
		{
			public EventType eventType;

			public Event(EventType type)
			{
				eventType = type;
			}
		}
		
		public delegate void LevelEventDelegate(Event levelEvent);

		private static event LevelEventDelegate _eventListeners;
		
		public static void ListenForLevelEvents(LevelEventDelegate listener)
		{
			_eventListeners += listener;
		}

		public static void StopListeningForLevelEvents(LevelEventDelegate listener)
		{
			_eventListeners -= listener;
		}

		private static void BroadcastEvent(Event levelEvent)
		{
			_eventListeners?.Invoke(levelEvent);
		}
		
		
		public enum CommandType
		{
			Undefined = 0,
			Win,
			Lose
		}

		public class Command
		{
			public CommandType commandType { get; }

			public Command(CommandType type)
			{
				commandType = type;
			}
		}

		public static void ExecuteCommand(Command command)
		{
			switch (command.commandType)
			{
				case CommandType.Win:
				{
					var levelData = Database.Load<LevelData>(_levelIndex);
					levelData.Value.stars = 1;
					levelData.Value.score = 999;

					// TODO: designer control over unlock sequence
					var nextLevelData = Database.Load<LevelData>(_levelIndex + 1);
					nextLevelData.Value.isUnlocked = true;
					Database.Save(nextLevelData);

					BroadcastEvent(new Event(EventType.Win));
				} break;

				case CommandType.Lose:
				{
					BroadcastEvent(new Event(EventType.Lose));
				} break;
			}
		}

		private static int _levelIndex = 0;
		public static void LoadLevel(int levelIndex)
		{
			_levelIndex = levelIndex;
		}

		public static void Pause()
		{
			Debug.LogWarning("TODO: pause the level");
		}

		public static void Unpause()
		{
			Debug.LogWarning("TODO: unpause the level");
		}
	}
}
