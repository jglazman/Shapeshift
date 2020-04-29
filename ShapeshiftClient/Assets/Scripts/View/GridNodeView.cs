//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public class GridNodeView : GridBaseView
	{
		// TODO: this should be data-driven
		public static int NumNodeTypes => System.Enum.GetValues(typeof(GridNodeType)).Length;
		
		public GridNodeType NodeType => (GridNodeType)this.Type;

		protected override string GetSpriteResourceName()
		{
			return $"GridNode-{NodeType}";
		}
	}
}
