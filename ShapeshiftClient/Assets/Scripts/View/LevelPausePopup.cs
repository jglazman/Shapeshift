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


		public override void Open()
		{
			base.Open();

			Level.Pause();
		}

		public override void Close()
		{
			base.Close();
			
			Level.Unpause();
		}
	}
}
