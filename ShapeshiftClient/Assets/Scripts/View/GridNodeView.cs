//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public class GridNodeView : GridBaseView
	{
		public GridNodeConfig GridNodeConfig => GameConfig.GetGridNode(ID);
		
		protected override string GetSpriteResourceName()
		{
			return GridNodeConfig.ImageName;
		}
	}
}
