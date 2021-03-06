﻿//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public class LevelPausePopup : GenericPopupView
	{
		public void OnClick_Ok()
		{
			OnClick_Close();
		}

		public void OnClick_QuitLevel()
		{
			Game.Notify(new Game.GoToWorldMapMessage());
		}

		public void OnClick_ToggleEditMode()
		{
			OnClick_Close();

			var levelView = FindObjectOfType<LevelView>();
			if (levelView != null)
				levelView.ToggleEditMode();
		}
		
		public void OnClick_DebugMenu()
		{
			PopupViewController.Open<LevelDebugPopup>();
		}
	}
}
