using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Glazman.Shapeshift
{
	public class EditorTools
	{
		[MenuItem("Tools/Clear PlayerPrefs")]
		public static void ClearPlayerPrefs()
		{
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
			
			Debug.LogWarning("[Tools] Cleared PlayerPrefs.");
		}
	}
}
