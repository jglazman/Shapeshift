//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using UnityEngine;
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

	/// <summary>
	/// The initial state of a grid node in the LevelConfig.
	/// </summary>
	[Serializable]
	public struct GridNodeLayout
	{
		public GridNodeType nodeType;
		public int itemType;	// -1 = closed, 0 = random
	}
	
	/// <summary>
	/// Static level data, everything needed to load a level as designed.
	/// </summary>
	[Serializable]
	public struct LevelConfig
	{
		public uint width;
		public uint height;
		public GridNodeLayout[] layout;  // Unity can't serialize a multidimensional array, so let's emulate one


		public bool IsInBounds(uint x, uint y)
		{
			return (x < width && y < height);
		}
		
		public GridNodeLayout? TryGetNodeLayout(uint x, uint y)
		{
			if (IsInBounds(x, y)) 
				return layout[GetLinearIndex(x, y)];
			
			return null;
		}

		public GridNodeLayout GetNodeLayout(uint x, uint y)
		{
			return layout[GetLinearIndex(x, y)];
		}

		public void SetNodeLayout(uint x, uint y, GridNodeLayout nodeLayout)
		{
			Assert.IsTrue(IsInBounds(x, y), $"Tried to set out-of-bounds node type: ({x},{y})={nodeLayout.nodeType}:{nodeLayout.itemType}");

			layout[GetLinearIndex(x, y)] = nodeLayout;
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
				layout = new GridNodeLayout[width * height]
			};

			for (uint y = 0; y < config.height; y++)
				for (uint x = 0; x < config.width; x++)
					config.SetNodeLayout(x, y, new GridNodeLayout() { nodeType=GridNodeType.Open, itemType=0 });

			return config;
		}

		public static void EditMode_ResizeLevel(uint width, uint height, ref LevelConfig config)
		{
			var resizedLayout = new GridNodeLayout[width * height];
			
			for (uint y = 0; y < height; y++)
				for (uint x = 0; x < width; x++)
				{
					if (config.IsInBounds(x, y))
						resizedLayout[GetLinearIndex(x, y, width)] = config.GetNodeLayout(x, y);
					else
						resizedLayout[GetLinearIndex(x, y, width)] = new GridNodeLayout() { nodeType=GridNodeType.Open, itemType=0 };
				}

			config.width = width;
			config.height = height;
			config.layout = resizedLayout;
		}
	}
}
