//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public static class Database
	{
		public sealed class Data<T> where T: struct
		{
			public string Key { get; }

			public T Value;

			private Data(string ident)
			{
				Key = $"{typeof(T).Name}.{ident}";
			}

		
			public static Data<T> CreateData(string ident)
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
			return Load<T>(index.ToString());
		}
		
		public static Data<T> Load<T>(string ident) where T: struct
		{
			return LoadInternal<T>(ident);
		}

		private static Data<T> LoadInternal<T>(string ident) where T: struct
		{
			var data = Data<T>.CreateData(ident);
			
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
				Debug.LogError($"[{typeof(T)}] Failed to load data with key='{data.Key}': {e.Message}");
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
