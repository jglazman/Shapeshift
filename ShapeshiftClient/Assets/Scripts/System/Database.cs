//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public static class Database
	{
		/// <summary>Wrapper for data object persistence.</summary>
		/// <typeparam name="T">The type of data to be persisted. Only fields that can be serialized by Unity will persist.</typeparam>
		public sealed class Data<T> where T : struct
		{
			public string Key { get; }

			public T Value;

			/// <summary>Wrapper for data object persistence.</summary>
			/// <param name="ident">A unique identifier for this data object.</param>
			private Data(string ident)
			{
				Key = GenerateKey(ident);
			}

		
			public static Data<T> Create(string ident)
			{
				return new Data<T>(ident);
			}

			public static string GenerateKey(string ident)
			{
				return $"{typeof(T).Name}.{ident}";
			}
		}


		public static bool Exists<T>(string ident) where T : struct
		{
			string dataKey = Data<T>.GenerateKey(ident);
			return PlayerPrefs.HasKey(GetPrefsKey(dataKey));
		}

		public static void Delete<T>(string ident) where T : struct
		{
			string dataKey = Data<T>.GenerateKey(ident);
			string prefsKey = GetPrefsKey(dataKey);
			if (PlayerPrefs.HasKey(prefsKey))
			{
				Logger.LogWarningEditor($"Delete data '{dataKey}': {PlayerPrefs.GetString(prefsKey)}");
				PlayerPrefs.DeleteKey(prefsKey);
				PlayerPrefs.Save();
			}
		}

		public static string Save<T>(Data<T> data) where T : struct
		{
			string json = Serialize<T>(data.Value);
			PlayerPrefs.SetString(GetPrefsKey(data.Key), json);
			PlayerPrefs.Save();
			return json;
		}

		public static Data<T> Load<T>(int index) where T : struct
		{
			return LoadInternal<T>(index.ToString());
		}
		
		public static Data<T> Load<T>(string ident) where T : struct
		{
			return LoadInternal<T>(ident);
		}

		private static Data<T> LoadInternal<T>(string ident) where T : struct
		{
			Assert.IsTrue(!string.IsNullOrEmpty(ident), $"[Database] Tried to load {typeof(T)} data without an identifier.");
		
			string dataKey = Data<T>.GenerateKey(ident);
			string prefsKey = GetPrefsKey(dataKey);
			string json = PlayerPrefs.GetString(prefsKey);
			return LoadFromJson<T>(ident, json);
		}

		private static Data<T> LoadFromJson<T>(string ident, string json) where T : struct
		{
			var data = Data<T>.Create(ident);

			try
			{
				// Assert.IsTrue(!string.IsNullOrEmpty(json), "[Database] Tried to load a null json");
				Assert.IsTrue(typeof(T).IsSerializable, $"[Database] Tried to load a non-serializable type={typeof(T)}, guid={data.Key}");

				if (!string.IsNullOrEmpty(json))
					data.Value = Deserialize<T>(json);
				// TODO: else load designer default data for this data structure?
				
				return data;
			}
			catch (System.Exception e)
			{
				Logger.LogError($"[{typeof(T)}] Failed to load data with key='{data.Key}': {e.Message}");
				return null;
			}
		}

		private static string GetPrefsKey(string guid)
		{
			return $"Glazman.Shapeshift.{guid}";
		}
		
		private static string Serialize<T>(T data)
		{
			return JsonUtility.ToJson(data);
		}

		private static T Deserialize<T>(string json)
		{
			return JsonUtility.FromJson<T>(json);
		}
	}
}
