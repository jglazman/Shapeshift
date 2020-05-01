//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;

namespace Glazman.Shapeshift
{
	
	public static class Utilities
	{
		public static string GetPathToGameObjectInScene(GameObject go)
		{
			if (go == null)
				return "<null>";
			
			return GetPathToGameObjectInScene(go.transform);
		}
		
		public static string GetPathToGameObjectInScene(Transform t)
		{
			if (t == null)
				return "<null>";
			
			string fullName = $"/{t.name}";
			
			Transform parent = t.parent;
			while (parent != null)
			{
				fullName = $"/{parent.name}{fullName}";
				parent = parent.parent;
			}
			
			return fullName;
		}

		public static GameObject GetRootGameObject(GameObject go)
		{
			Transform t = go.transform;

			while (t.parent != null)
				t = t.parent;
			
			return t.gameObject;
		}
	}
}
