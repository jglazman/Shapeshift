//
// Copyright (c) 2020 Jeremy Glazman
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public static partial class Level
	{
		private static LevelState _levelState;
		
		public static void ExecuteCommand(Command command)
		{
			switch (command.CommandType)
			{
				case CommandType.LoadLevel:
				{
					int levelIndex = (command as LoadLevelCommand).LevelIndex;
					LevelConfig.LoadAsync(levelIndex, (index, config) =>
					{
						// TODO: continue loading even if the config is null. this would normally be an error, but we use this to trigger the level editor.
						if (config == null)
							config = new LevelConfig();
						 
						_levelState = new LevelState(index, config);
						BroadcastEvent(new LoadLevelEvent(index, config, _levelState.Grid));
					});
				} break;

				case CommandType.SubmitMatch:
				{
					if (_levelState.IsValidMatch((command as SubmitMatchCommand)?.SelectedItems, out var matchedItems))
					{
						var events = _levelState.HandleMatchSuccess(matchedItems);
						
						BroadcastEvent(new MatchSuccessEvent());
						
						foreach (var matchEvent in events)
							BroadcastEvent(matchEvent);
					}
					else
					{
						BroadcastEvent(new MatchRejectedEvent());
					}
				} break;
				
				case CommandType.Debug_Win:
				{
					var levelData = Database.Load<LevelProgressData>(_levelState.LevelIndex);
					levelData.Value.stars = 1;
					levelData.Value.score = 999;

					// TODO: designer control over unlock sequence
					var nextLevelData = Database.Load<LevelProgressData>(_levelState.LevelIndex + 1);
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
