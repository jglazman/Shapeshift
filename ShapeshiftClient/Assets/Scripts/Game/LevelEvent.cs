//
// Copyright (c) 2020 Jeremy Glazman
//

using System.Collections.Generic;

namespace Glazman.Shapeshift
{
	public static partial class Level
	{
		public delegate void LevelEventDelegate(IEnumerable<Event> levelEvents);

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
			BroadcastEvents(new List<Event>() {levelEvent});
		}
		
		private static void BroadcastEvents(IEnumerable<Event> levelEvents)
		{
			_eventListeners?.Invoke(levelEvents);
		}
		
		
		public enum EventType
		{
			Undefined = 0,
			LoadLevel,
			Win,
			Lose,
			UpdateScore,
			MatchSuccess,
			MatchRejected,
			ItemsCreated,
			ItemsMoved,
			ItemsDestroyed
		}

		public abstract class Event
		{
			public abstract EventType EventType { get; }
			
			//public readonly Payload Payload = new Payload();

			/// <summary>Any event can generate points.</summary>
			public int EventPoints { get; protected set; }
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

			public int Stars { get; }
			public int Moves { get; }
			public int Points { get; }
			public int BestMoves { get; }
			public int BestPoints { get; }

			public LevelWinEvent(int stars, int moves, int points, int bestMoves, int bestPoints)
			{
				Stars = stars;
				Moves = moves;
				Points = points;
				BestMoves = bestMoves;
				BestPoints = bestPoints;
			}
		}

		public class LevelLoseEvent : Event
		{
			public override EventType EventType { get { return EventType.Lose; } }
		}

		public class UpdateScoreEvent : Event
		{
			public override EventType EventType { get { return EventType.UpdateScore; } }

			public int Moves { get; }
			public int MovesDelta { get; }
			public int Points { get; }
			public int PointsDelta { get; }

			public UpdateScoreEvent(int moves, int movesDelta, int points, int pointsDelta)
			{
				Moves = moves;
				MovesDelta = movesDelta;
				Points = points;
				PointsDelta = pointsDelta;
			}
		}
		
		public class MatchSuccessEvent : Event
		{
			public override EventType EventType { get { return EventType.MatchSuccess; } }
		}
		
		public class MatchRejectedEvent : Event
		{
			public override EventType EventType { get { return EventType.MatchRejected; } }
		}

		public class ItemsCreatedEvent : Event
		{
			public override EventType EventType { get { return EventType.ItemsCreated; } }

			public List<GridEventItem> CreatedItems { get; }
			
			public ItemsCreatedEvent(List<GridNodeState> createdItems)
			{
				CreatedItems = new List<GridEventItem>(createdItems.Count);

				foreach (var item in createdItems)
					CreatedItems.Add(GridEventItem.Create(item));
			}
		}
		
		public class ItemsMovedEvent : Event
		{
			public override EventType EventType { get { return EventType.ItemsMoved; } }

			public List<GridEventItem> MovedItems { get; }

			public ItemsMovedEvent(List<GridEventItem> movedItems)
			{
				MovedItems = new List<GridEventItem>(movedItems);
			}
		}

		public class ItemsDestroyedEvent : Event
		{
			public override EventType EventType { get { return EventType.ItemsDestroyed; } }

			public CauseOfDeath Reason { get; }
			public List<GridEventItem> DestroyedItems { get; }
			
			public ItemsDestroyedEvent(CauseOfDeath reason, List<GridNodeState> destroyedItems)
			{
				Reason = reason;
				DestroyedItems = new List<GridEventItem>(destroyedItems.Count);

				for (int i = 0; i < destroyedItems.Count; i++)
				{
					int points = 100 + (25 * i);	// TODO: data-driven scoring
					var item = GridEventItem.Create(destroyedItems[i], points);
					DestroyedItems.Add(item);
					EventPoints += points;	// total points
				}
			}
		}
	}
}
