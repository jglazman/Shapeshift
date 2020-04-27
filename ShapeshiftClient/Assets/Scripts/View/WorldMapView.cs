//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;

namespace Glazman.Shapeshift
{
	public class WorldMapView : MonoBehaviour
	{



		public void OnClick_OpenDebugMenu()
		{
			PopupViewController.Open<WorldMapDebugPopup>();
		}
	}
}
