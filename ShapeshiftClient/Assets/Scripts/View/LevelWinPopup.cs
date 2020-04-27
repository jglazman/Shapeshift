//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public class LevelWinPopup : GenericPopupView
	{
		public void OnClick_Ok()
		{
			PopupViewController.Close();
		}


		public override void Close()
		{
			base.Close();
			
			Game.Notify(new LoadWorldMapMessage());
		}
	}
}
