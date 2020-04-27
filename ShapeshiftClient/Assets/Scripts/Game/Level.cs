//
// Copyright (c) 2020 Jeremy Glazman
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Glazman.Shapeshift
{
	public class Payload
	{
		private Dictionary<int, object> items = new Dictionary<int, object>();

		public void SetField(int field, object value)
		{
			items[field] = value;
		}

		public int GetInt(int field)
		{
			if (items.TryGetValue(field, out var value))
				return (int)value;

			return 0;
		}
	}
	
	
	public static class Level
	{
		public enum EventType
		{
			Undefined = 0,
			LoadLevel,
			Win,
			Lose
		}

		public abstract class Event
		{
			public abstract EventType eventType { get; }
			
			public readonly Payload payload = new Payload();
		}

		public class LoadLevelEvent : Event
		{
			public enum Fields
			{
				Undefined = 0,
				LevelIndex
			}
			
			public override EventType eventType { get { return EventType.LoadLevel; } }

			public LoadLevelEvent(int levelIndex)
			{
				payload.SetField((int)Fields.LevelIndex, levelIndex);
			}
		}
		
		public class LevelWinEvent : Event
		{
			public override EventType eventType { get { return EventType.Win; } }
		}

		public class LevelLoseEvent : Event
		{
			public override EventType eventType { get { return EventType.Lose; } }
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
			LoadLevel,
			Debug_Win,
			Debug_Lose
		}

		public abstract class Command
		{
			public abstract CommandType commandType { get; }
			
			public readonly Payload payload = new Payload();
		}

		public class LoadLevelCommand : Command
		{
			public enum Field
			{
				Undefined = 0,
				LevelIndex
			}
			
			public override CommandType commandType { get { return CommandType.LoadLevel; } }

			public LoadLevelCommand(int levelIndex)
			{
				payload.SetField((int)Field.LevelIndex, levelIndex);
			}
		}

		public class DebugWinCommand : Command
		{
			public override CommandType commandType { get { return CommandType.Debug_Win; } }
		}

		public class DebugLoseCommand : Command
		{
			public override CommandType commandType { get { return CommandType.Debug_Lose; } }
		}

		public static void ExecuteCommand(Command command)
		{
			switch (command.commandType)
			{
				case CommandType.LoadLevel:
				{
					var loadLevelCommand = command as LoadLevelCommand;
					var levelIndex = loadLevelCommand.payload.GetInt((int)LoadLevelCommand.Field.LevelIndex);
					
					LevelState = new LevelState(levelIndex);
					
					BroadcastEvent(new LoadLevelEvent(levelIndex));
				} break;
				
				case CommandType.Debug_Win:
				{
					var levelData = Database.Load<LevelData>(LevelState.LevelIndex);
					levelData.Value.stars = 1;
					levelData.Value.score = 999;

					// TODO: designer control over unlock sequence
					var nextLevelData = Database.Load<LevelData>(LevelState.LevelIndex + 1);
					nextLevelData.Value.isUnlocked = true;
					Database.Save(nextLevelData);

					BroadcastEvent(new LevelWinEvent());
				} break;

				case CommandType.Debug_Lose:
				{
					BroadcastEvent(new LevelLoseEvent());
				} break;
			}
		}



		public static LevelState LevelState { get; private set; }


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
