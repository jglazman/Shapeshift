//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Glazman.Shapeshift
{
	[Serializable]
	public struct WorldMapScrollData
	{
		public Vector2 scrollOffset;
	}
	
	public class WorldMapView : MonoBehaviour, ISceneTransitioner
	{
		[SerializeField] private ScrollRect _scrollRect = null;

		
		private void Awake()
		{
			SceneController.RegisterTransitioner(this);
		}

		private void OnDestroy()
		{
			SceneController.UnregisterTransitioner(this);
		}

		
		public void Notify(TransitionState state)
		{
			switch (state)
			{
				case TransitionState.Intro:
					_scrollRect.content.anchoredPosition = Database.Load<WorldMapScrollData>(0).Value.scrollOffset;
					break;
				
				case TransitionState.Outro:
					var data = Database.Load<WorldMapScrollData>(0);
					data.Value.scrollOffset = _scrollRect.content.anchoredPosition;
					Database.Save(data);
					
					SceneController.UnregisterTransitioner(this);
					break;
			}
		}
		

		
		public void OnClick_OpenDebugMenu()
		{
			PopupViewController.Open<WorldMapDebugPopup>();
		}
	}
}
