//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using UnityEngine;

namespace Glazman.Shapeshift
{
	[RequireComponent(typeof(UnityEngine.UI.Button))]
	public abstract class GenericButton : MonoBehaviour
	{
		public abstract void OnClick();
		
		
#if UNITY_EDITOR
		private void Reset()
		{
			// for convenience, automatically register our OnClick method
			var button = gameObject.GetComponent<UnityEngine.UI.Button>();
			if (button != null)
			{
				try
				{
					// remove the old listener, if any
					UnityEditor.Events.UnityEventTools.RemovePersistentListener(button.onClick, 0);
				}
				catch (ArgumentOutOfRangeException)
				{
					// we can't query for existing listeners to be removed, so we're stabbing blindly
				}
				finally
				{
					var action = new UnityEngine.Events.UnityAction(OnClick);
					UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, action);
				}
			}
		}
#endif
	}
}
