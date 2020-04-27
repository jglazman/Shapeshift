//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public class PlayButton : GenericButton
	{
		public override void OnClick()
		{
			Game.Notify(new Game.GoToWorldMapMessage());
		}
	}
}
