﻿//
// Copyright (c) 2020 Jeremy Glazman
//

using System.Collections.Generic;

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
			public abstract EventType EventType { get; }
			
			public readonly Payload Payload = new Payload();
		}

		public class LoadLevelEvent : Event
		{
			// public enum Fields
			// {
			// 	Undefined = 0,
			// 	LevelIndex
			// }

			public override EventType EventType { get { return EventType.LoadLevel; } }

			public int levelIndex;
			public List<GridItemState> gridState;

			public LoadLevelEvent(int levelIndex, List<GridItemState> gridState)
			{
				// Payload.SetField((int)Fields.LevelIndex, levelIndex);
				this.levelIndex = levelIndex;
				
				if (gridState != null)
					this.gridState = new List<GridItemState>(gridState);
			}
		}
		
		public class LevelWinEvent : Event
		{
			public override EventType EventType { get { return EventType.Win; } }
		}

		public class LevelLoseEvent : Event
		{
			public override EventType EventType { get { return EventType.Lose; } }
		}
	}
}
