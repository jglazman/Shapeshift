//
// Copyright (c) 2020 Jeremy Glazman
//

using System;

namespace Glazman.Shapeshift
{
	// WARNING: these are serialized properties. do not change their values.
	public enum GameOptionType
	{
		Undefined = 0,
		Sound = 1,
		Music = 2
	}

	[Serializable]
	public struct SettingsData
	{
		public int optionValue;

		public bool ToggledOn
		{
			get { return optionValue != 0; }
			set { optionValue = value ? 1 : 0; }
		}
	}
}
