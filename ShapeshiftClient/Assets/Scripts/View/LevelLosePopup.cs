//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public class LevelLosePopup : GenericPopupView
	{
		public void OnClick_Ok()
		{
			Game.Notify(new Game.GoToWorldMapMessage());
		}
	}
}
