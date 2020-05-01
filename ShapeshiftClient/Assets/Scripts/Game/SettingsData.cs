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
		Music = 2,
		Animation = 3
	}

	[Serializable]
	public struct SettingsData : IDefaultData
	{
		public int optionValue;

		public bool ToggledOn
		{
			get { return optionValue != 0; }
			set { optionValue = value ? 1 : 0; }
		}

		public void Reset(string ident)
		{
			if (int.TryParse(ident, out var optionType))
			{
				switch ((GameOptionType)optionType)
				{
					case GameOptionType.Sound:
					case GameOptionType.Music:
						optionValue = 1;	// enable sound and music by default
						break;
					
					case GameOptionType.Animation:
						optionValue = 2;	// TODO: default option values should be data-driven
						break;
				}
			}
		}
	}
}
