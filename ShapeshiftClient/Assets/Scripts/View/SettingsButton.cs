//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public class SettingsButton : GenericButton
	{
		public override void OnClick()
		{
			PopupViewController.Open<GameSettingsPopup>();
		}
	}
}
