//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public struct TileState
	{
		public int x;
		public int y;
		public TileType tileType;
		public TileItemState tileItemState;
	}

	public class TileItemState
	{
		public int itemType;
	}
	
	
	public class LevelState
	{
		public int LevelIndex { get; }
		
		private TileState[,] _tileStates;
		
		

		public LevelState(int levelIndex)
		{
			LevelIndex = levelIndex;

			var config = LevelConfig.Load(levelIndex);
			
			_tileStates = new TileState[config.width,config.height];
			
			for (int y = 0; y < config.height; y++)
				for (int x = 0; x < config.width; x++)
				{
					_tileStates[x, y] = new TileState()
					{
						x = x,
						y = y,
						tileType = config.layout[x, y],
						tileItemState = new TileItemState()
					};
				}

		}
	}
}
