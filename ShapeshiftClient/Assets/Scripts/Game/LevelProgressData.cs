//
// Copyright (c) 2020 Jeremy Glazman
//

using System;

namespace Glazman.Shapeshift
{
	[Serializable]
	public struct LevelProgressData
	{
		public int index;
		public int stars;
		public int score;
		public bool isUnlocked;
	}
}
