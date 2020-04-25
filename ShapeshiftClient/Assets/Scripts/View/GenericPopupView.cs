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

		public virtual void Open() { }

		public virtual void Close() { }

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
