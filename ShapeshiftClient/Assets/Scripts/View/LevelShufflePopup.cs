//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public class LevelShufflePopup : GenericPopupView
	{
		public void OnClick_Confirm()
		{
			OnClick_Close();

			var levelView = FindObjectOfType<LevelView>();
			if (levelView != null)
				levelView.OnClick_ShuffleConfirmed();
		}
	}
}
