﻿//
// Copyright (c) 2020 Jeremy Glazman
//

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
					var loadLevelCommand = command as LoadLevelCommand;
					var levelIndex = loadLevelCommand.Payload.GetInt((int)LoadLevelCommand.Field.LevelIndex);
					
					LevelState = new LevelState(levelIndex);
					
					BroadcastEvent(new LoadLevelEvent(levelIndex));
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
