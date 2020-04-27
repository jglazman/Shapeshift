﻿//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public static partial class Level
	{
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
	}
}