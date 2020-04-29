//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Glazman.Shapeshift
{
	public class LevelState
	{
		public int LevelIndex { get; }
		public int Width { get; }
		public int Height { get; }
		public GridNodeState[,] Grid { get; } // TODO: a generic "MultidimensionalList" container would be nice

		public LevelState(int levelIndex, LevelConfig config)
		{
			LevelIndex = levelIndex;
			Width = config.width;
			Height = config.height;
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
				if (nodeUp.itemType > 0)
					return nodeUp;
			}

			return null;
		}
		
		public bool IsValidMatch(List<GridIndex> indices, out List<GridNodeState> matchedItems)
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
				
				if (Mathf.Abs(item.index.x - previousItem.index.x) > 1 ||
				    Mathf.Abs(item.index.y - previousItem.index.y) > 1)
					return false;	// must select neighbors

				matchedItems.Add(item);
				
				previousItem = item;
			}

			return true;
		}
		
		
		public List<Level.Event> HandleMatchSuccess(List<GridNodeState> matchedItems)
		{
			var gridEvents = new List<Level.Event>();
					
			// take a snapshot of the items to be removed
			var itemsMatchedEvent = new Level.ItemsMatchedEvent(matchedItems);
			gridEvents.Add(itemsMatchedEvent);
			// remove the items
			foreach (var item in matchedItems)
				item.itemType = -1;

			// drop new items into place and take snapshots along the way
			var itemsFallEvent = new Level.ItemsFallIntoPlaceEvent();
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					// TODO: more advanced falling rules (e.g. move around closed nodes)
							
					// pull items down to fill the empty nodes
					var node = Grid[x, y];
					if (node.itemType <= 0)
					{
						// find an item above
						var aboveFilledNode = FindFirstFilledNodeAbove(x, y);
						if (aboveFilledNode != null)
						{
							// snapshot
							var moved = new Level.ItemMovedEvent(aboveFilledNode.index, node.index, aboveFilledNode.itemType);
							itemsFallEvent.MovedItems.Add(moved);
									
							// pull the item down
							node.itemType = aboveFilledNode.itemType;
							aboveFilledNode.itemType = -1;
						}
						else
						{
							// no items above us. create new items to fill the column.
							for (int yFill = y; yFill < Height; yFill++)
							{
								// create random new items
								var fillNode = Grid[x, yFill];
								if (fillNode.TryRandomizeItemType())
								{
									// snapshot
									var created = new Level.ItemCreatedEvent(fillNode.index, fillNode.itemType);
									itemsFallEvent.CreatedItems.Add(created);
								}
							}
						}
					}
				}
			}
			gridEvents.Add(itemsFallEvent);

			return gridEvents;
		}
	}
}
