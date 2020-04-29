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
		
		protected override string GetSpriteResourceName()
		{
			return $"GridItem-{ItemType}";
		}
	}
}
