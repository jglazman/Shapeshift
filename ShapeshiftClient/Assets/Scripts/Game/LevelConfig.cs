//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace Glazman.Shapeshift
{
	// WARNING: these are serialized properties. do not change their values.
	public enum GridNodeType
	{
		Undefined = 0,
		Closed = 1,
		Open = 2
	}

	/// <summary>
	/// The initial state of a grid node in the LevelConfig.
	/// </summary>
	[Serializable]
	public struct GridNodeLayout
	{
		public GridNodeType nodeType;
		public int itemType;	// -1 = closed, 0 = random
	}
	
	/// <summary>
	/// Static level data, everything needed to load a level as designed.
	/// </summary>
	[Serializable]
	public class LevelConfig
	{
		public uint width;
		public uint height;
		public GridNodeLayout[] layout;  // Unity can't serialize a multidimensional array, so let's emulate one


		public bool IsInBounds(uint x, uint y)
		{
			return (x < width && y < height);
		}
		
		public GridNodeLayout? TryGetNodeLayout(uint x, uint y)
		{
			if (IsInBounds(x, y)) 
				return layout[GetLinearIndex(x, y)];
			
			return null;
		}

		public GridNodeLayout GetNodeLayout(uint x, uint y)
		{
			return layout[GetLinearIndex(x, y)];
		}

		public void SetNodeLayout(uint x, uint y, GridNodeLayout nodeLayout)
		{
			Assert.IsTrue(IsInBounds(x, y), $"Tried to set out-of-bounds node type: ({x},{y})={nodeLayout.nodeType}:{nodeLayout.itemType}");

			layout[GetLinearIndex(x, y)] = nodeLayout;
		}

		private uint GetLinearIndex(uint x, uint y)
		{
			return (y * width) + x;
		}

		public static uint GetLinearIndex(uint x, uint y, uint width)
		{
			return (y * width) + x;
		}
		

		public static LevelConfig CreateDefaultLevel(uint width, uint height)
		{
			var config = new LevelConfig
			{
				width = width,
				height = height,
				layout = new GridNodeLayout[width * height]
			};

			for (uint y = 0; y < config.height; y++)
				for (uint x = 0; x < config.width; x++)
					config.SetNodeLayout(x, y, new GridNodeLayout() { nodeType=GridNodeType.Open, itemType=0 });

			return config;
		}

		public static void ResizeLevel(uint width, uint height, ref LevelConfig config)
		{
			var resizedLayout = new GridNodeLayout[width * height];
			
			for (uint y = 0; y < height; y++)
				for (uint x = 0; x < width; x++)
				{
					if (config.IsInBounds(x, y))
						resizedLayout[GetLinearIndex(x, y, width)] = config.GetNodeLayout(x, y);
					else
						resizedLayout[GetLinearIndex(x, y, width)] = new GridNodeLayout() { nodeType=GridNodeType.Open, itemType=0 };
				}

			config.width = width;
			config.height = height;
			config.layout = resizedLayout;
		}

		private static string GetLevelResourceName(int levelIndex)
		{
			return $"Level-{levelIndex}";
		}
		
		private static LevelConfig LoadFromJson(string json)
		{
			try
			{
				var config = JsonUtility.FromJson<LevelConfig>(json);
				return config;
			}
			catch (System.Exception e)
			{
				Logger.LogError($"Failed to deserialize level config json: {json}");
				return null;
			}
		}

		/// <summary>
		/// Load the static level config for the given level. The file can be in PersistentDataPath,
		/// StreamingAssets, or Resources, in that order; the first file found will be loaded.
		/// See GetLevelResourceName() for the filename format.
		/// </summary>
		/// <param name="levelIndex">The index of the level config to be loaded.</param>
		/// <param name="callback">Returns the loaded config. Can not be null.</param>
		public static void LoadAsync(int levelIndex, Action<int,LevelConfig> callback)
		{
			CoroutineRunner.Run(LoadAsync_Coroutine(levelIndex, callback));
		}
		
		private static IEnumerator LoadAsync_Coroutine(int levelIndex, Action<int,LevelConfig> callback)
		{
			string resourceName = GetLevelResourceName(levelIndex);
			string fileName = $"{resourceName}.txt";
		
			// 1. Try to load from PersistentDataPath. Files created in the the runtime level editor are here.
			string pathToFile = Path.Combine(Application.persistentDataPath, fileName);
			if (File.Exists(pathToFile))
			{
				try
				{
					using (var reader = new StreamReader(pathToFile))
					{
						string json = reader.ReadToEnd();
						var config = LoadFromJson(json);
						if (config != null)
						{
							Logger.LogEditor($"Loaded level file from disk: {pathToFile}");
							callback.Invoke(levelIndex, config);
							yield break;
						}
					}
				}
				catch (System.Exception e)
				{
					Logger.LogError($"Error while loading '{resourceName}' from PersistentDataPath '{pathToFile}': {e.Message}");
				}
			}

			// 2. Try to load from from the web (actually we're using StreamingAssets as a proxy).
			string pathToStreamingAssets = Path.Combine(Application.streamingAssetsPath, fileName);
			
#if UNITY_ANDROID && !UNITY_EDITOR
			string url = pathToStreamingAssets;
#else
			string url = $"file://{pathToStreamingAssets}";
#endif
			
			using (var uwr = UnityWebRequest.Get(url))
			{
				yield return uwr.SendWebRequest();

				if (uwr.isHttpError || uwr.isNetworkError)
				{
					if (string.IsNullOrEmpty(uwr.error) || !uwr.error.Contains("404"))	// 404 is not an error
						Logger.LogError($"Error while loading '{resourceName}' from web '{url}': {uwr.error}");
				}
				else
				{
					try
					{
						string json = uwr.downloadHandler.text;
						var config = JsonUtility.FromJson<LevelConfig>(json);
						Logger.LogEditor($"Loaded level file from web: {url}");
						callback.Invoke(levelIndex, config);
						yield break;
					}
					catch (System.Exception e)
					{
						Logger.LogError($"Error while loading '{resourceName}' from StreamingAssets '{pathToFile}': {e.Message}");
					}
				}
			}
			
			// 3. Try to load from Resources.  This is the last resort; the other methods allow for dynamically 
			// updating the level files after the game has shipped, but Resources are baked assets shipped with the build.
			var textAsset = Resources.Load<TextAsset>(resourceName);
			if (textAsset != null)
			{
				var config = LoadFromJson(textAsset.text);
				if (config != null)
				{
					Logger.LogEditor($"Loaded level file from Resources: {resourceName}");
					callback.Invoke(levelIndex, config);
					yield break;
				}
			}

			// 4. All these moments will be lost in time, like tears in rain. Time to die.
			Logger.LogError($"Failed to level file '{resourceName}': file not found.");
			callback.Invoke(levelIndex, null);
		}

		public static void ExportLevelFile(int levelIndex, LevelConfig config)
		{
			string resourceName = GetLevelResourceName(levelIndex);
			string pathToFile = Path.Combine(Application.persistentDataPath, $"{resourceName}.txt");
			
			try
			{
				string json = JsonUtility.ToJson(config);
			
				Logger.LogEditor($"[EXPORT] Level={levelIndex}, Data={json}");	// adb logcat, copy and paste ;)
				
				using (var writer = new StreamWriter(pathToFile))
				{
					writer.Write(json);
					
					Logger.LogEditor($"[EXPORT] Level '{resourceName}' was exported to file: {pathToFile}");
				}
			}
			catch (System.Exception e)
			{
				Logger.LogError($"Error while exporting '{resourceName}' to PersistentDataPath '{pathToFile}': {e.Message}");
			}
		}

	}
}
