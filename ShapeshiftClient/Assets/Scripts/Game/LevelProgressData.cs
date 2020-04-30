//
// Copyright (c) 2020 Jeremy Glazman
//

using System;

namespace Glazman.Shapeshift
{
	[Serializable]
	public struct LevelProgressData
	{
		public int stars;
		public int points;
		public int moves;
		public bool isUnlocked;


		public static void UnlockLevel(int levelIndex)
		{
			var nextLevelData = Database.Load<LevelProgressData>(levelIndex);
			nextLevelData.Value.isUnlocked = true;
			Database.Save(nextLevelData);
		}
	}
}
