//
// Copyright (c) 2020 Jeremy Glazman
//

using System.Collections.Generic;

namespace Glazman.Shapeshift
{
	public class GridItemState
	{
		public uint x;
		public uint y;
		public int itemType;

		public static GridItemState CreateFromLayout(uint x, uint y, GridNodeLayout nodeLayout)
		{
			var itemState = new GridItemState()
			{
				x = x,
				y = y
			};

			// TODO: this should be data-driven
			// choose the initial item state
			if (nodeLayout.itemType == 0)
				itemState.itemType = UnityEngine.Random.Range(1, GridItemView.NumItemTypes);
			else
				itemState.itemType = nodeLayout.itemType;

			return itemState;
		}
	}
	
	
	public class LevelState
	{
		public int LevelIndex { get; }

		public List<GridItemState> gridState = null;


		public LevelState(int levelIndex)
		{
			LevelIndex = levelIndex;

			var config = Database.Load<LevelConfig>(levelIndex).Value;

			// TODO: this would normally be an error, but we're exploiting a null gridState to auto-trigger the level editor
			if (config.width == 0 || config.height == 0)
				return;
			
			gridState = new List<GridItemState>((int)(config.width * config.height));
		
			for (uint y = 0; y < config.height; y++)
				for (uint x = 0; x < config.width; x++)
					gridState.Add(GridItemState.CreateFromLayout(x, y, config.GetNodeLayout(x, y)));
		}
	}
}
