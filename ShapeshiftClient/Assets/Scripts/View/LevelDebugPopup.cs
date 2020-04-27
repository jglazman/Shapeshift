//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public class LevelDebugPopup : GenericPopupView
	{
		
		
		public void OnClick_DebugWin()
		{
			OnClick_Close();

			Level.ExecuteCommand(new Level.DebugWinCommand());
		}

		public void OnClick_DebugLose()
		{
			OnClick_Close();
			
			Level.ExecuteCommand(new Level.DebugLoseCommand());
		}
	}
}
