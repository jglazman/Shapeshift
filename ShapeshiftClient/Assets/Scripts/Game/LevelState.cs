//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Glazman.Shapeshift
{
	public class LevelState
	{
		public int LevelIndex { get; }
		public LevelConfig Config { get; }
		public int Width => Config?.width ?? 0;
		public int Height => Config?.height ?? 0;
		public GridNodeState[,] Grid { get; } // TODO: a generic "MultidimensionalList" container would be nice
		public int Points { get; protected set; }
		public int Moves { get; protected set; }
		public int Result { get; protected set; } // -1 = lose, 0 = in progress, 1 = win

		public LevelState(int levelIndex, LevelConfig config)
		{
			LevelIndex = levelIndex;
			Config = config;
			Grid = null;

			if (config.width > 0 && config.height > 0)
			{
				// initialize the grid
				Grid = new GridNodeState[config.width, config.height];
				for (int y = 0; y < config.height; y++)
					for (int x = 0; x < config.width; x++)
						Grid[x, y] = GridNodeState.CreateFromLayout(x, y, config.GetNodeLayout(x, y));
			}
		}


		public bool IsInBounds(GridIndex index)
		{
			return IsInBounds(index.x, index.y);
		}
		
		public bool IsInBounds(int x, int y)
		{
			return (x >= 0 && x < Width && y >= 0 && y < Height);
		}

		public GridNodeState TryGetGridNodeState(GridIndex index)
		{
			return TryGetGridNodeState(index.x, index.y);
		}
		
		public GridNodeState TryGetGridNodeState(int x, int y)
		{
			if (IsInBounds(x, y))
				return Grid[x, y];

			return null;
		}

		private GridNodeState FindFirstFilledNodeAbove(int x, int y)
		{
			if (!IsInBounds(x, y))
				return null;	// the query begins out of bounds, we won't find anything
			
			if (y >= Height - 1)
				return null;	// already at the top
			
			for (int yUp = y + 1; yUp < Height; yUp++)
			{
				var nodeUp = Grid[x, yUp];
				if (nodeUp.nodeType == GridNodeType.Closed)
					return null; // blocked
				
				if (nodeUp.IsFilled())
					return nodeUp;
			}

			return null;
		}

		private bool CheckEndOfLevel()
		{
			Assert.IsTrue(Result == 0, "[LevelState] Checking for end of level, but the level already ended");

			return (Moves >= Config.challengeValue || Points >= Config.goal3);
			
			// TODO: other game modes
			// switch (Config.challengeType)
			// {
			// 	case LevelChallengeType.Moves:
			// 	{
			// 		return (Moves >= Config.challengeValue);
			// 	}
			//
			// 	default:
			// 	{
			// 		Logger.LogError($"Unhandled LevelChallengeType: {Config.challengeType}");
			// 		return true; // we don't know how to end the level properly, so end it immediately
			// 	}
			// }
		}

		private Level.Event GetEndOfLevelResults()
		{
			Assert.IsTrue(Result == 0, "[LevelState] Getting end of level results, but the level already ended");
			
			switch (Config.goalType)
			{
				case LevelGoalType.Points:
				{
					if (Points < Config.goal1)
					{
						Result = -1;
						
						return new Level.LevelLoseEvent();
					}
					else
					{
						Result = 1;

						var levelProgress = Database.Load<LevelProgressData>(LevelIndex);
						int bestMoves = levelProgress.Value.moves <= 0 ? Moves : Mathf.Min(Moves, levelProgress.Value.moves);
						int bestPoints = Mathf.Max(Points, levelProgress.Value.points);
						int stars = Points >= Config.goal3 ? 3 : Points >= Config.goal2 ? 2 : 1;
						
						levelProgress.Value.moves = bestMoves;
						levelProgress.Value.points = bestPoints;
						levelProgress.Value.stars = Mathf.Max(stars, levelProgress.Value.stars);
						Database.Save(levelProgress);

						LevelProgressData.UnlockLevel(LevelIndex + 1);
						
						return new Level.LevelWinEvent(stars, Moves, Points, bestMoves, bestPoints);
					}
				}

				default:
				{
					Logger.LogError($"Unhandled LevelGoalType: {Config.goalType}");
					Result = -1;
					return new Level.LevelLoseEvent(); // we don't know how to end the level properly, so end it immediately
				}
			}
		}
		
		public List<Level.Event> TryMatchItems(List<GridIndex> selectedItems)
		{
			var matchEvents = new List<Level.Event>();

			if (IsValidMatch(selectedItems, out var matchedItems))
			{
				matchEvents.Add(new Level.MatchSuccessEvent());

				// make the move
				var gridUpdateEvents = RemoveGridItems(CauseOfDeath.Matched, matchedItems);
				matchEvents.AddRange(gridUpdateEvents);
				
				// score the move
				int pointsDelta = 0;
				foreach (var gridEvent in gridUpdateEvents)
					pointsDelta += gridEvent.EventPoints;
				
				Points += pointsDelta;
				Moves++;
				
				matchEvents.Add(new Level.UpdateScoreEvent(Moves, 1, Points, pointsDelta));

				// check for end of level
				if (CheckEndOfLevel())
				{
					var endOfLevelEvent = GetEndOfLevelResults();
					if (endOfLevelEvent != null)
						matchEvents.Add(endOfLevelEvent);
				}
			}
			else
			{
				matchEvents.Add(new Level.MatchRejectedEvent());
			}

			return matchEvents;
		}

		private bool IsValidMatch(List<GridIndex> indices, out List<GridNodeState> matchedItems)
		{
			matchedItems = new List<GridNodeState>();
			
			if (indices == null || indices.Count < 3)
				return false; // must select at least 3 items

			var firstItem = TryGetGridNodeState(indices[0]);
			if (firstItem == null || firstItem.itemType <= 0)
				return false; // must select valid items
			
			matchedItems.Add(firstItem);

			var previousItem = firstItem; 
			for (int i = 1; i < indices.Count; i++)
			{
				var item = TryGetGridNodeState(indices[i]);

				if (item.itemType != firstItem.itemType)	// TODO: add wildcard rules
					return false;	// must select similar items
				
				if (!GridIndex.IsNeighbor(item.index, previousItem.index))
					return false;	// must select neighbors

				matchedItems.Add(item);
				
				previousItem = item;
			}

			return true;
		}
		
		private List<Level.Event> RemoveGridItems(CauseOfDeath reason, List<GridNodeState> itemsToRemove)
		{
			List<Level.Event> gridUpdateEvents = new List<Level.Event>();
		
			// record the event
			var destroyedEvent = new Level.ItemsDestroyedEvent(reason, itemsToRemove);
			gridUpdateEvents.Add(destroyedEvent);
			
			// remove the items from the grid
			foreach (var item in itemsToRemove)
				item.itemType = -1;

			// fix the grid until it ain't broke no more
			bool didUpdate;
			do
			{
				didUpdate = UpdateGridState(ref gridUpdateEvents);
			} while (didUpdate);
			
			return gridUpdateEvents;
		}

		private bool UpdateGridState(ref List<Level.Event> gridUpdateEvents)
		{
			bool didUpdate = PullDownGridItems(ref gridUpdateEvents);
		
			if (CollapseGridItems(ref gridUpdateEvents))
				didUpdate = true;
			
			if (DropInGridItems(ref gridUpdateEvents))
				didUpdate = true;
			
			return didUpdate;
		}

		private bool PullDownGridItems(ref List<Level.Event> gridUpdateEvents)
		{
			var movedItems = new List<GridEventItem>();
			
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					// find an empty node
					var node = Grid[x, y];
					if (!node.IsEmpty())
						continue;
					
					// find an item above
					var aboveFilledNode = FindFirstFilledNodeAbove(x, y);
					if (aboveFilledNode != null)
					{
						// pull the item down
						node.itemType = aboveFilledNode.itemType;
						aboveFilledNode.itemType = -1;
						
						// record the event
						var movedItem = GridEventItem.Create(node, 0, aboveFilledNode.index);
						movedItems.Add(movedItem);
					}
				}
			}

			if (movedItems.Count > 0)
			{
				var itemsMovedEvent = new Level.ItemsMovedEvent(movedItems);
				gridUpdateEvents.Add(itemsMovedEvent);
				return true;
			}

			return false;
		}
		
		private bool CollapseGridItems(ref List<Level.Event> gridUpdateEvents)
		{
			var movedItems = new List<GridEventItem>();
			
			for (int x = 0; x < Width; x++)
			{
				for (int y = 1; y < Height; y++)	// skip the lowest row
				{
					// find a filled node
					var node = Grid[x, y];
					if (!node.IsFilled())
						continue;
					
					// collapse left
					var nodeLeft = TryGetGridNodeState(x - 1, y - 1);
					if (nodeLeft?.IsEmpty() == true)
					{
						nodeLeft.itemType = node.itemType;
						node.itemType = -1;
						
						// record the event
						var movedItem = GridEventItem.Create(nodeLeft, 0, node.index);
						movedItems.Add(movedItem);
						continue;
					}
					
					// collapse right
					var nodeRight = TryGetGridNodeState(x + 1, y - 1);
					if (nodeRight?.IsEmpty() == true)
					{
						nodeRight.itemType = node.itemType;
						node.itemType = -1;
						
						// record the event
						var movedItem = GridEventItem.Create(nodeRight, 0, node.index);
						movedItems.Add(movedItem);
						continue;
					}
				}
			}

			if (movedItems.Count > 0)
			{
				var itemsMovedEvent = new Level.ItemsMovedEvent(movedItems);
				gridUpdateEvents.Add(itemsMovedEvent);
				return true;
			}

			return false;
		}
		
		private bool DropInGridItems(ref List<Level.Event> gridUpdateEvents)
		{
			var createdItems = new List<GridNodeState>();

			int yTop = Height - 1;
			for (int x = 0; x < Width; x++)
			{
				// create randomized items at each empty node in the top row
				var node = Grid[x, yTop];
				if (node.IsEmpty() && node.TryRandomizeItemType())
				{
					// record the event
					createdItems.Add(node);
				}
			}

			if (createdItems.Count > 0)
			{
				var itemsCreatedEvent = new Level.ItemsCreatedEvent(createdItems);
				gridUpdateEvents.Add(itemsCreatedEvent);
				return true;
			}

			return false;
		}

		public Level.ItemsSwappedEvent ShuffleGridItems()
		{
			var filledNodes = new List<GridNodeState>();
			
			for (int x = 0; x < Width; x++)
				for (int y = 0; y < Height; y++)
				{
					var node = Grid[x, y];
					if (node.IsFilled())
						filledNodes.Add(node);
				}
			
			var swappedItems = new List<GridEventItem>();

			int count = filledNodes.Count;
			for (int i = 0; i < count; i++)
			{
				var node1 = filledNodes[i];
				var node2 = filledNodes[Random.Range(0, count)];	// we could pick ourself here. that's fine.

				// swap
				var node2Type = node2.itemType;
				node2.itemType = node1.itemType;
				node1.itemType = node2Type;
				
				// record the event
				var swappedItem = GridEventItem.Create(node1, 0, node2.index);
				swappedItems.Add(swappedItem);
			}

			return new Level.ItemsSwappedEvent(swappedItems);
		}


		public Level.Event Debug_WinLevel()
		{
			Result = 1;
			
			var levelProgress = Database.Load<LevelProgressData>(LevelIndex);
			levelProgress.Value.stars = 3;
			levelProgress.Value.points = 9999;
			levelProgress.Value.moves = 99;
			Database.Save(levelProgress);

			LevelProgressData.UnlockLevel(LevelIndex + 1);

			return new Level.LevelWinEvent(3, 99, 9999, 99, 9999);
		}

		public Level.Event Debug_LoseLevel()
		{
			Result = -1;
			
			return new Level.LevelLoseEvent();
		}
	}
}
