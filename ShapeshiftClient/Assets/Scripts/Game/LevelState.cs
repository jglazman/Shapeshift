﻿//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using System.Collections.Generic;
using System.Text;
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

		private GridItemDropDistribution GridItemDrops = null;
		
		
		public LevelState(int levelIndex, LevelConfig config)
		{
			LevelIndex = levelIndex;
			Config = config;
			Grid = null;

			var includedItems = GameConfig.GetAllGridItemsInCategory(config.category, config.excludeItemIds);
			GridItemDrops = new GridItemDropDistribution(config.category, includedItems);

			if (config.width > 0 && config.height > 0)
			{
				var defaultGridItem = GameConfig.GetDefaultLayoutGridItem(config.category);
				
				// initialize the grid
				Grid = new GridNodeState[config.width, config.height];
				for (int y = 0; y < config.height; y++)
				for (int x = 0; x < config.width; x++)
				{
					var layout = config.GetNodeLayout(x, y);
					string itemId = null;
					if (!string.IsNullOrEmpty(layout.itemId))
					{
						if (layout.itemId != defaultGridItem.ID)
							itemId = layout.itemId;
						else
							itemId = GridItemDrops.Next();
					}
					Grid[x, y] = new GridNodeState(x, y, layout.nodeId, itemId);
				}
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
				if (!nodeUp.GridNodeConfig.IsOpen)
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

				var gridUpdateEvents = MatchGridItems(matchedItems);
				
				// score the move
				int pointsDelta = 0;
				foreach (var gridEvent in gridUpdateEvents)
					pointsDelta += gridEvent.EventPoints;
				
				Points += pointsDelta;
				Moves++;
				
				matchEvents.Add(new Level.UpdateScoreEvent(Moves, 1, Points, pointsDelta));

				// put the match events after the score so we can see the score first
				matchEvents.AddRange(gridUpdateEvents);

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

		private bool IsValidMatch(List<GridIndex> indices, out List<Tuple<int,GridNodeState>> matchedItems)
		{
			var matchRules = GameConfig.GetMatchRules(Config.matchRules);
			
			matchedItems = null;
			
			if (indices == null)
				return false;

			if (indices.Count < matchRules.MinSelection)
				return false;

			if (matchRules.MaxSelection > matchRules.MinSelection && indices.Count > matchRules.MaxSelection)
				return false;

			int pointsMultiplier = 1;

			var matchedNodes = new List<GridNodeState>(indices.Count);
			for (int i = 0; i < indices.Count; i++)
			{
				var matchedNode = TryGetGridNodeState(indices[i]);
				if (matchedNode == null || !matchedNode.IsFilled())
					return false; // must select valid items
				
				matchedNodes.Add(matchedNode);

				pointsMultiplier *= matchedNode.GridNodeConfig.MatchPointsMatchMultiplier;
			}

			matchedItems = new List<Tuple<int,GridNodeState>>(indices.Count) {
				new Tuple<int,GridNodeState> (
					(matchRules.MatchPointsBase + matchedNodes[0].GridItemConfig.MatchPoints) * matchedNodes[0].GridNodeConfig.MatchPointsItemMultiplier * pointsMultiplier,
					matchedNodes[0]
				)
			};

			var previousItem = matchedNodes[0];
			for (int i = 1; i < matchedNodes.Count; i++)
			{
				var item = TryGetGridNodeState(indices[i]);
				var itemConfig = GameConfig.GetGridItem(item.ItemId);

				if (itemConfig.MatchType == GridItemMatchType.None)
					return false;
				
				if (itemConfig.MatchType == GridItemMatchType.Category || previousItem.GridItemConfig.MatchType == GridItemMatchType.Category)
				{
					if (itemConfig.Category != previousItem.GridItemConfig.Category)
						return false;
				}
				else if (itemConfig.MatchType == GridItemMatchType.Exact)
				{
					if (item.ItemId != previousItem.GridItemConfig.ID)
						return false;
				}
				
				if (!GridIndex.IsNeighbor(item.Index, previousItem.Index))
					return false;	// must select neighbors

				matchedItems.Add(new Tuple<int,GridNodeState> (
					(matchRules.MatchPointsBase + matchedNodes[i].GridItemConfig.MatchPoints + (matchRules.MatchPointsIncrement * i)) * matchedNodes[i].GridNodeConfig.MatchPointsItemMultiplier * pointsMultiplier,
					matchedNodes[i]
				));
				
				previousItem = item;
			}

			if (matchRules.WordCheck)
			{
				var word = new StringBuilder();
				foreach (var item in matchedItems)
					word.Append((char)GameConfig.GetGridItem(item.Item2.ItemId).MatchIndex);

				if (!WordMap.Words.FindWord(word.ToString().ToLower(), out var wordTypes))
					return false;
			}

			return true;
		}
		
		private List<Level.Event> MatchGridItems(List<Tuple<int,GridNodeState>> matchedItems)
		{
			List<Level.Event> gridUpdateEvents = new List<Level.Event>();
			
			// record the event
			var destroyedEvent = new Level.ItemsMatchedEvent(matchedItems);
			gridUpdateEvents.Add(destroyedEvent);
			
			// remove the items from the grid
			foreach (var node in matchedItems)
				node.Item2.RemoveItem();

			// fix the grid until it ain't broke no more
			bool didUpdate;
			do
			{
				didUpdate = UpdateGridState(ref gridUpdateEvents);
			} while (didUpdate);
			
			return gridUpdateEvents;
		}

		private List<Level.Event> RemoveGridItems(CauseOfDeath reason, List<GridNodeState> removedItems)
		{
			List<Level.Event> gridUpdateEvents = new List<Level.Event>();
			
			// record the event
			var destroyedEvent = new Level.ItemsDestroyedEvent(reason, removedItems);
			gridUpdateEvents.Add(destroyedEvent);
			
			// remove the items from the grid
			foreach (var node in removedItems)
				node.RemoveItem();

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
						node.SetItemId(aboveFilledNode.ItemId);
						aboveFilledNode.RemoveItem();
						
						// record the event
						var movedItem = GridEventItem.Create(node, 0, aboveFilledNode.Index);
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
						nodeLeft.SetItemId(node.ItemId);
						node.RemoveItem();
						
						// record the event
						var movedItem = GridEventItem.Create(nodeLeft, 0, node.Index);
						movedItems.Add(movedItem);
						continue;
					}
					
					// collapse right
					var nodeRight = TryGetGridNodeState(x + 1, y - 1);
					if (nodeRight?.IsEmpty() == true)
					{
						nodeRight.SetItemId(node.ItemId);
						node.RemoveItem();
						
						// record the event
						var movedItem = GridEventItem.Create(nodeRight, 0, node.Index);
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
				if (node.GridNodeConfig.IsOpen && node.IsEmpty())
				{
					node.SetItemId(GridItemDrops.Next());
					
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

				int randomIndex;
				do
				{
					randomIndex = Random.Range(0, count);
				} while (randomIndex == i);
				var node2 = filledNodes[randomIndex];

				// swap
				var temp = node2.ItemId;
				node2.SetItemId(node1.ItemId);
				node1.SetItemId(temp);
				
				// record the event
				var swappedItem = GridEventItem.Create(node1, 0, node2.Index);
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
