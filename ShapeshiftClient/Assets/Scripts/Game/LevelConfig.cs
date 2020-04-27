//
// Copyright (c) 2020 Jeremy Glazman
//

using System;

namespace Glazman.Shapeshift
{
	// WARNING: these are serialized properties. do not change their values.
	public enum TileType
	{
		Undefined = 0,
		Closed = 1,
		Open = 2
	}
	
	[Serializable]
	public struct LevelConfig
	{
		public int width;
		public int height;
		public TileType[,] layout;  // [0,0] is bottom left


		public static LevelConfig Load(int levelIndex)
		{
			var config = new LevelConfig
			{
				width = 6,
				height = 6
			};
			config.layout = new TileType[config.width,config.height];

			for (int y = 0; y < config.height; y++)
				for (int x = 0; x < config.width; x++)
				{
					config.layout[x, y] = TileType.Open;
				}

			return config;
		}
	}
}
