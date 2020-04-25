//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;

namespace Glazman.Shapeshift
{
	public class LevelView : MonoBehaviour
	{


		public void OnClick_Pause()
		{
			PopupViewController.Open<LevelPausePopup>();
		}

		public void OnClick_Win()
		{
			// TODO: temp debug
			PopupViewController.Open<LevelWinPopup>();
		}

		public void OnClick_Lose()
		{
			// TODO: temp debug
			PopupViewController.Open<LevelLosePopup>();
		}
		
	}
}
