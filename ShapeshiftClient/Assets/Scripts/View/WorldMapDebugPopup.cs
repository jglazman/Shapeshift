//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public class WorldMapDebugPopup : GenericPopupView
	{
		public void OnClick_ResetProgress()
		{
			var levelNodes = FindObjectsOfType<LevelNodeButton>();
			foreach (var node in levelNodes)
			{
				var levelData = Database.Load<LevelData>(node.LevelIndex);
				levelData.Value = default;
				levelData.Value.isUnlocked = node.LevelIndex == 1;
				Database.Save(levelData);
				
				node.Refresh();
			}

			OnClick_Close();
		}
		
		public void OnClick_UnlockAllLevels()
		{
			var levelNodes = FindObjectsOfType<LevelNodeButton>();
			foreach (var node in levelNodes)
			{
				var levelData = Database.Load<LevelData>(node.LevelIndex);
				levelData.Value.isUnlocked = true;
				Database.Save(levelData);
				
				node.Refresh();
			}

			OnClick_Close();
		}
	}
}
