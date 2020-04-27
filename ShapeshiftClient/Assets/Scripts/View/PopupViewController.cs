//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public static class PopupViewController
	{
		// using a List rather than a Stack so we can more easily iterate for sanity checks. performance is not an issue here.
		private static List<GenericPopupView> _viewStack = new List<GenericPopupView>();
		
		
		public static T Open<T>() where T : GenericPopupView
		{
			Assert.IsTrue(!_viewStack.Any(v => v.GetType() == typeof(T)), $"[ViewController] View is already in the stack: {typeof(T)}");

			// hide the top view
			if (_viewStack.Count > 0)
			{
				var topView = _viewStack[_viewStack.Count - 1];
				if (topView != null)
					topView.Hide();
				else
					Logger.LogError("Tried to hide the top view, but the view on the stack was null!");
			}
			
			// create the new view
			// TODO: is it too restrictive to force prefab names to match the type name? why would we ever need multiple versions of a view?
			string prefabName = typeof(T).Name;
			var prefab = Resources.Load<T>(prefabName);
			
			Assert.IsTrue(prefab != null, $"[ViewController] Prefab '{prefabName}' is missing from Resources.");
			
			var view = GameObject.Instantiate<T>(prefab);
			view.Open();
			
			// add the new view to the stack
			_viewStack.Add(view);
			
			return view;
		}
		
		public static void Close()
		{
			Assert.IsTrue(_viewStack.Count > 0, "[ViewController] Tried to close the view, but no views are open.");

			// close the top view
			var view = _viewStack[_viewStack.Count - 1];
			if (view != null)
			{
				view.Close();
				GameObject.Destroy(view.gameObject);
			}
			else
				Logger.LogError("Tried to close the view, but the view on the stack was null!");
			
			// remove the top view from the stack
			_viewStack.RemoveAt(_viewStack.Count - 1);

			// restore the next view in the stack
			if (_viewStack.Count > 0)
			{
				var topView = _viewStack[_viewStack.Count - 1];
				if (topView != null)
					topView.Show();
				else
					Logger.LogError("Tried to show the top view, but the view on the stack was null!");
			}
		}
	}
}
