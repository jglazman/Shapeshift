//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;

namespace Glazman.Shapeshift
{
	// WARNING: these are serialized properties. do not change their values.
	public enum GameOption
	{
		Undefined = 0,
		Sound = 1,
		Music = 2
	}
	
	public class GameSettings
	{
		private static string GetOptionPrefsKey(GameOption option)
		{
			return $"Glazman.Shapeshift.{option}";
		}
		
		public static int GetOption(GameOption option)
		{
			// TODO: designer control over default values
			return PlayerPrefs.GetInt(GetOptionPrefsKey(option), 1);
		}

		public static int SetOption(GameOption option, int value)
		{
			// TODO: designer control over possible values
			PlayerPrefs.SetInt(GetOptionPrefsKey(option), value);
			PlayerPrefs.Save();
			return value;
		}

		public static int SetOption(GameOption option, bool value)
		{
			return SetOption(option, value ? 1 : 0);
		}
	}
}
