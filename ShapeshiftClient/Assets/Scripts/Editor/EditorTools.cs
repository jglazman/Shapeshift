using System.Collections;
using System.Collections.Generic;
using System.IO;
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
			
			Logger.LogWarningEditor("[Tools] Cleared PlayerPrefs.");
		}
		
		[MenuItem("Tools/Clear PersistentDataPath")]
		public static void ClearPersistentDataPath()
		{
			var files = Directory.GetFiles(Application.persistentDataPath);

			foreach (var file in files)
			{
				if (file.EndsWith(".txt"))
				{
					Logger.LogWarningEditor($"Delete file: {file}");
					File.Delete(file);
				}
			}
			
			Logger.LogWarningEditor("[Tools] Cleared PersistentDataPath.");
		}

	}
}
