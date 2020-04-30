//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;

namespace Glazman.Shapeshift
{
	public abstract class GenericPopupView : MonoBehaviour
	{
		public void OnClick_Close()
		{
			PopupViewController.Close();
		}

		public virtual void OnOpen()
		{
			Level.Pause();
		}

		public virtual void OnClose()
		{
			Level.Unpause();
		}

		public virtual void Show()
		{
			gameObject.SetActive(true);
		}
		
		public virtual void Hide()
		{
			gameObject.SetActive(false);
		}
	}
}
