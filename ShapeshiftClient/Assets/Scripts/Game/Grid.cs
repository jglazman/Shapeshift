//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using UnityEngine;

namespace Glazman.Shapeshift
{
	// WARNING: these are serialized properties. do not change their values.
	public enum GridNodeType
	{
		Undefined = 0,
		Closed = 1,
		Open = 2
	}

	/// <summary>The coordinates of a grid node.</summary>
	[Serializable]
	public struct GridIndex
	{
		public int x;
		public int y;

		public static bool IsNeighbor(GridIndex index1, GridIndex index2)
		{
			if (Mathf.Abs(index1.x - index2.x) > 1 || Mathf.Abs(index1.y - index2.y) > 1)
				return false;

			return true;
		}
	}

	// [Serializable]
	// public struct GridItem
	// {
	// 	public GridIndex index;
	// 	public int itemType;
	// }
	
	/// <summary>The initial setup of a grid node.</summary>
	[Serializable]
	public struct GridNodeLayout
	{
		public GridNodeType nodeType;
		public int itemType;	// -1 = closed, 0 = random
	}

	/// <summary>The mutable state of a grid node.</summary>
	public class GridNodeState
	{
		public GridIndex index { get; }
		
		public GridNodeType nodeType;
		public int itemType;

		private GridNodeState(int x, int y)
		{
			index = new GridIndex() { x = x, y = y };
		}
		
		public static GridNodeState CreateFromLayout(int x, int y, GridNodeLayout nodeLayout)
		{
			var itemState = new GridNodeState(x, y)
			{
				nodeType = nodeLayout.nodeType
			};

			// choose the initial item state
			if (nodeLayout.itemType == 0)
				itemState.itemType = GetRandomItemType();
			else
				itemState.itemType = nodeLayout.itemType;

			return itemState;
		}

		public bool TryRandomizeItemType()
		{
			if (nodeType == GridNodeType.Open)
			{
				itemType = GetRandomItemType();
				return true;
			}

			return false;
		}

		private static int GetRandomItemType()
		{
			return UnityEngine.Random.Range(1, GridItemView.NumItemTypes);
		}
	}
}
