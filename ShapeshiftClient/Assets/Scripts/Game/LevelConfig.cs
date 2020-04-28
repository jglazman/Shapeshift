//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	// WARNING: these are serialized properties. do not change their values.
	public enum GridNodeType
	{
		Undefined = 0,
		Closed = 1,
		Open = 2
	}
	
	[Serializable]
	public struct LevelConfig
	{
		public uint width;
		public uint height;
		public GridNodeType[] layout;  // Unity can't serialize a multidimensional array, so let's emulate one


		public bool IsInBounds(uint x, uint y)
		{
			return (x < width && y < height);
		}
		
		public GridNodeType TryGetNodeType(uint x, uint y)
		{
			if (IsInBounds(x, y)) 
				return layout[GetLinearIndex(x, y)];
			
			return GridNodeType.Undefined;
		}

		public GridNodeType GetNodeType(uint x, uint y)
		{
			return layout[GetLinearIndex(x, y)];
		}

		public void SetNodeType(uint x, uint y, GridNodeType nodeType)
		{
			Assert.IsTrue(IsInBounds(x, y), $"Tried to set out-of-bounds node type: ({x},{y})={nodeType}");

			layout[GetLinearIndex(x, y)] = nodeType;
		}

		private uint GetLinearIndex(uint x, uint y)
		{
			return (y * width) + x;
		}

		public static uint GetLinearIndex(uint x, uint y, uint width)
		{
			return (y * width) + x;
		}
		

		public static LevelConfig EditMode_CreateDefaultLevel(uint width, uint height)
		{
			var config = new LevelConfig
			{
				width = width,
				height = height,
				layout = new GridNodeType[width * height]
			};

			for (uint y = 0; y < config.height; y++)
				for (uint x = 0; x < config.width; x++)
					config.SetNodeType(x, y, GridNodeType.Open);

			return config;
		}

		public static void EditMode_ResizeLevel(uint width, uint height, ref LevelConfig config)
		{
			var resizedLayout = new GridNodeType[width * height];
			
			for (uint y = 0; y < height; y++)
				for (uint x = 0; x < width; x++)
				{
					if (config.IsInBounds(x, y))
						resizedLayout[GetLinearIndex(x, y, width)] = config.GetNodeType(x, y);
					else
						resizedLayout[GetLinearIndex(x, y, width)] = GridNodeType.Open;
				}

			config.width = width;
			config.height = height;
			config.layout = resizedLayout;
		}
	}
}
