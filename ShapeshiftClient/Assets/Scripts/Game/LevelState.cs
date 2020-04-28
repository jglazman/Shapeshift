//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public struct TileState
	{
		public uint x;
		public uint y;
		public TileNodeType nodeType;
		public TileItemState itemState;
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

			var config = Database.Load<LevelConfig>(levelIndex).Value;
			
			_tileStates = new TileState[config.width,config.height];
			
			for (uint y = 0; y < config.height; y++)
				for (uint x = 0; x < config.width; x++)
				{
					_tileStates[x, y] = new TileState()
					{
						x = x,
						y = y,
						nodeType = config.GetNodeType(x, y),
						itemState = new TileItemState()
					};
				}

		}
	}
}
