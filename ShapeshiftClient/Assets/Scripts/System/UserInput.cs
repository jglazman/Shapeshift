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

		/// <summary>Raycast into the world to pick all active interactable objects in the scene.</summary>
		/// <param name="screenPos">The point on the screen to pick from.</param>
		/// <returns>A list of all GameObjects found, or an empty list if none were found.</returns>
		public static IEnumerable<GameObject> PickObjects(Vector2 screenPos)
		{
			Assert.IsNotNull(UnityEventSystem, "[UserInput] Failed to pick: EventSystem is missing");
			
			var results = new List<RaycastResult>();
			UnityGuiPointerEvent.position = screenPos;
			UnityEventSystem.RaycastAll(UnityGuiPointerEvent, results);

			// Logger.LogEditor($"Picked objects:\n{string.Join("\n", results.Select(r => Utilities.GetPathToGameObjectInScene(r.gameObject)))}");

			var pickedObjects = new HashSet<GameObject>();
			
			foreach (var obj in results)
			{
				var hitbox = obj.gameObject.GetComponent<Hitbox>();
				if (hitbox != null)
					pickedObjects.Add(hitbox.Target);
				else
					pickedObjects.Add(obj.gameObject);
			}

			return pickedObjects;
		}
		
		/// <summary>Raycast into the world to pick an active interactable MonoBehaviour of the given type.</summary>
		/// <param name="screenPos">The point on the screen to pick from.</param>
		/// <param name="noClipping">If true then clip through all objects to find a match, else stop at the first GameObject tagged with "ConsumeInput".</param>
		/// <typeparam name="T">The type of MonoBehaviour we are trying to pick.</typeparam>
		/// <returns>The picked MonoBehaviour, else null if none were found.</returns>
		public static T PickObject<T>(Vector2 screenPos, bool noClipping=false) where T : UnityEngine.Component
		{
			var pickedObjects = PickObjects(screenPos);

			foreach (var obj in pickedObjects)
			{
				if (!noClipping && obj.CompareTag("ConsumeInput"))
					return null;	// consumed
				
				var component = obj.GetComponent<T>();
				if (component != null)
					return component;
			}

			return null;
		}
	}
}
