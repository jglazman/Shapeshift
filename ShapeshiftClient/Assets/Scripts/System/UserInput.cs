//
// Copyright (c) 2020 Jeremy Glazman
//

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace Glazman.Shapeshift
{
	public static class UserInput
	{
		private static EventSystem UnityEventSystem = null;
		private static PointerEventData UnityGuiPointerEvent = null;


		public static void Initialize()
		{
			if (UnityEventSystem == null)
				UnityEventSystem = EventSystem.current;	// we have a singleton EventSystem in the Bootstrap scene
			
			Assert.IsNotNull(UnityEventSystem, "[UserInput] Failed to initialize: EventSystem is missing");
				
			if (UnityGuiPointerEvent == null)
				UnityGuiPointerEvent = new PointerEventData(UnityEventSystem);
		}

		public static IEnumerable<GameObject> PickObjects(Vector2 screenPos)
		{
			Assert.IsNotNull(UnityEventSystem, "[UserInput] Failed to pick: EventSystem is missing");
			
			var results = new List<RaycastResult>();
			UnityGuiPointerEvent.position = screenPos;
			UnityEventSystem.RaycastAll(UnityGuiPointerEvent, results);

			if (results.Count > 0)
			{
				Logger.LogEditor($"Picked objects:\n{string.Join("\n", results.Select(r => Utilities.GetPathToGameObjectInScene(r.gameObject)))}");

				return results.Select(r => r.gameObject);
			}
			
			return null;
		}
		
		public static T PickObject<T>(Vector2 screenPos) where T : UnityEngine.Component
		{
			var pickedObjects = PickObjects(screenPos);

			foreach (var obj in pickedObjects)
			{
				var component = obj.GetComponent<T>();
				if (component != null)
					return component;
			}

			return null;
		}
	}
}
