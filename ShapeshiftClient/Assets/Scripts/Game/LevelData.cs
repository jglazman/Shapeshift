//
// Copyright (c) 2020 Jeremy Glazman
//

using System;

namespace Glazman.Shapeshift
{
	[Serializable]
	public struct LevelData
	{
		public int index;
		public int stars;
		public int score;
		public bool isUnlocked;
	}
}
