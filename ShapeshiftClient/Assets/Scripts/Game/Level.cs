//
// Copyright (c) 2020 Jeremy Glazman
//

using System.Collections;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public static partial class Level
	{
		public static LevelState LevelState { get; private set; }
		
		
		public static void ExecuteCommand(Command command)
		{
			switch (command.CommandType)
			{
				case CommandType.LoadLevel:
				{
					int levelIndex = (command as LoadLevelCommand).levelIndex;
					LevelConfig.LoadAsync(levelIndex, (index, config) =>
					{
						// TODO: continue loading even if the config is null. this would normally be an error, but we use this to trigger the level editor.
						if (config == null)
							config = new LevelConfig();
						 
						LevelState = new LevelState(index, config);
						BroadcastEvent(new LoadLevelEvent(index, config, LevelState.gridState));
					});
				} break;
				
				case CommandType.Debug_Win:
				{
					var levelData = Database.Load<LevelData>(LevelState.LevelIndex);
					levelData.Value.stars = 1;
					levelData.Value.score = 999;

					// TODO: designer control over unlock sequence
					var nextLevelData = Database.Load<LevelData>(LevelState.LevelIndex + 1);
					nextLevelData.Value.isUnlocked = true;
					Database.Save(nextLevelData);

					BroadcastEvent(new LevelWinEvent());
				} break;

				case CommandType.Debug_Lose:
				{
					BroadcastEvent(new LevelLoseEvent());
				} break;
			}
		}


		public static void Pause()
		{
			Logger.LogWarningEditor("TODO: pause the level");
		}

		public static void Unpause()
		{
			Logger.LogWarningEditor("TODO: unpause the level");
		}
	}
}
