//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public class LevelPausePopup : GenericPopupView
	{
		public void OnClick_Ok()
		{
			PopupViewController.Close();
		}


		public override void OnOpen()
		{
			base.OnOpen();

			Level.Pause();
		}

		public override void OnClose()
		{
			base.OnClose();
			
			Level.Unpause();
		}
	}
}
