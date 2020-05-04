//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using UnityEngine;

namespace Glazman.Shapeshift
{
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

	public enum CauseOfDeath
	{
		Undefined = 0
	}

	/// <summary>The payload for grid events.</summary>
	public class GridEventItem
	{
		public GridIndex? ReferenceIndex { get; protected set; }
		public GridIndex Index { get; protected set; }
		public string ItemId { get; protected set; }
		public int Points { get; protected set; }

		public static GridEventItem Create(GridNodeState nodeState, int points=0, GridIndex? refIndex=null)
		{
			var item = new GridEventItem()
			{
				ReferenceIndex = refIndex,
				Index = nodeState.Index,
				ItemId = nodeState.ItemId,
				Points = points
			};
			return item;
		}
	}

	/// <summary>The initial setup of a grid node.</summary>
	[Serializable]
	public struct GridNodeLayout
	{
		public string nodeId;
		public string itemId;
	}

	/// <summary>The mutable state of a grid node.</summary>
	public class GridNodeState
	{
		public GridIndex Index { get; }
		public string NodeId { get; }
		public string ItemId { get; private set;  }

		public GridNodeConfig GridNodeConfig => GameConfig.GetGridNode(NodeId);
		public GridItemConfig GridItemConfig => GameConfig.GetGridItem(ItemId);

		public GridNodeState(int x, int y, string nodeId, string itemId)
		{
			Index = new GridIndex() { x = x, y = y };
			NodeId = nodeId;
			ItemId = itemId;
		}

		public bool IsEmpty()
		{
			return GridNodeConfig.IsOpen && ItemId == null;
		}

		public bool IsFilled()
		{
			return GridNodeConfig.IsOpen && ItemId != null;
		}

		public void RemoveItem()
		{
			ItemId = null;
		}

		public void SetItemId(string itemId)
		{
			ItemId = itemId;
		}
	}
}
