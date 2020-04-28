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
		public sealed class Data<T> where T: struct
		{
			public string Key { get; }

			public T Value;

			/// <summary>Wrapper for data object persistence.</summary>
			/// <param name="ident">A unique identifier for this data object.</param>
			private Data(string ident)
			{
				Key = $"{typeof(T).Name}.{ident}";
			}

		
			public static Data<T> Create(string ident)
			{
				return new Data<T>(ident);
			}
		}
		
		
		public static void Save<T>(Data<T> data) where T: struct
		{
			PlayerPrefs.SetString(GetPrefsKey(data.Key), Serialize<T>(data.Value));
			PlayerPrefs.Save();
		}

		public static Data<T> Load<T>(int index) where T: struct
		{
			return LoadInternal<T>(index.ToString());
		}
		
		public static Data<T> Load<T>(string ident) where T: struct
		{
			return LoadInternal<T>(ident);
		}

		private static Data<T> LoadInternal<T>(string ident) where T: struct
		{
			var data = Data<T>.Create(ident);
			
			try
			{
				Assert.IsTrue(typeof(T).IsSerializable, $"[Database] Tried to load a non-serializable type={typeof(T)}, guid={data.Key}");
				
				var json = PlayerPrefs.GetString(GetPrefsKey(data.Key));
				if (!string.IsNullOrEmpty(json))
					data.Value = Deserialize<T>(json);
				
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
