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
			string fullName = $"/{go.name}";
			
			Transform parent = go.transform.parent;
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
