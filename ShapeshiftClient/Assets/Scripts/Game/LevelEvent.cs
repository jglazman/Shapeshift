//
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
			Lose,
			MatchSuccess,
			MatchRejected,
			ItemsMatched,
			ItemsFallIntoPlace,
			ItemCreated,
			ItemMoved,
			ItemDestroyed
		}

		public abstract class Event
		{
			public abstract EventType EventType { get; }
			
			//public readonly Payload Payload = new Payload();
		}

		public class LoadLevelEvent : Event
		{
			// public enum Fields
			// {
			// 	Undefined = 0,
			// 	LevelIndex
			// }

			public override EventType EventType { get { return EventType.LoadLevel; } }

			/// <summary>the level that was loaded</summary>
			public int LevelIndex { get; }

			/// <summary>the static configuration for the loaded level</summary>
			public LevelConfig LevelConfig { get; }

			/// <summary>the initial state of the loaded level</summary>
			public List<GridNodeState> InitialGridState { get; }

			public LoadLevelEvent(int levelIndex, LevelConfig levelConfig, GridNodeState[,] gridState)
			{
				// Payload.SetField((int)Fields.LevelIndex, levelIndex);
				
				LevelIndex = levelIndex;
				LevelConfig = levelConfig;

				if (gridState != null)
				{
					InitialGridState = new List<GridNodeState>();
					foreach (var item in gridState)
						InitialGridState.Add(item);
				}
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
		
		public class MatchSuccessEvent : Event
		{
			public override EventType EventType { get { return EventType.MatchSuccess; } }
		}
		
		public class MatchRejectedEvent : Event
		{
			public override EventType EventType { get { return EventType.MatchRejected; } }
		}
		
		public class ItemsMatchedEvent : Event
		{
			public override EventType EventType { get { return EventType.ItemsMatched; } }

			public List<ItemDestroyedEvent> MatchedItems { get; }

			public ItemsMatchedEvent(List<GridNodeState> nodes)
			{
				MatchedItems = new List<ItemDestroyedEvent>(nodes.Count);

				foreach (var node in nodes)
				{
					var destroyedItem = new ItemDestroyedEvent(node.index, node.itemType);
					MatchedItems.Add(destroyedItem);
				}
			}
		}

		public class ItemsFallIntoPlaceEvent : Event
		{
			public override EventType EventType { get { return EventType.ItemsFallIntoPlace; } }

			public List<ItemMovedEvent> MovedItems = new List<ItemMovedEvent>();
			public List<ItemCreatedEvent> CreatedItems = new List<ItemCreatedEvent>();
		}

		public class ItemCreatedEvent : Event
		{
			public override EventType EventType { get { return EventType.ItemCreated; } }
			
			public GridIndex Index { get; }
			public int ItemType { get; }

			public ItemCreatedEvent(GridIndex index, int itemType)
			{
				ItemType = itemType;
				Index = index;
			}
		}
		
		public class ItemMovedEvent : Event
		{
			public override EventType EventType { get { return EventType.ItemMoved; } }

			public GridIndex SourceIndex { get; }
			public GridIndex DestIndex { get; }
			public int ItemType { get; }

			public ItemMovedEvent(GridIndex source, GridIndex destination, int itemType)
			{
				SourceIndex = source;
				DestIndex = destination;
				ItemType = itemType;
			}
		}
		
		public class ItemDestroyedEvent : Event
		{
			public override EventType EventType { get { return EventType.ItemDestroyed; } }

			public GridIndex Index { get; }
			public int ItemType { get; }

			public ItemDestroyedEvent(GridIndex index, int itemType)
			{
				ItemType = itemType;
				Index = index;
			}
		}
	}
}
