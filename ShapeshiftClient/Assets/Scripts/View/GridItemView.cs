//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;

namespace Glazman.Shapeshift
{
	public class GridItemView : GridBaseView
	{
		// TODO: this should be data-driven
		public static int NumItemTypes => 6;

		public int ItemType => this.Type;

		public bool IsBusy { get; private set; }

		protected override string GetSpriteResourceName()
		{
			return $"GridItem-{ItemType}";
		}


		public void DoCreateAction(int itemType)
		{
			SetType(itemType, true);
		}

		public void DoMoveAction(Vector3 destination)
		{
			SetPosition(destination);
		}

		public void DoMatchAction(int points)
		{
			Invalidate();
		}
	}
}
