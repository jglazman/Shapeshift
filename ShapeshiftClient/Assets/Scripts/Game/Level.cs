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
					var matchEvents = _levelState.TryMatchItems((command as SubmitMatchCommand)?.SelectedItems);
					
					BroadcastEvents(matchEvents);
				} break;

				case CommandType.ShuffleGrid:
				{
					var swappedEvent = _levelState.ShuffleGridItems();
					
					BroadcastEvent(swappedEvent);
				} break;
				
				case CommandType.Debug_Win:
				{
					BroadcastEvent(_levelState.Debug_WinLevel());
				} break;

				case CommandType.Debug_Lose:
				{
					BroadcastEvent(_levelState.Debug_LoseLevel());
				} break;
			}
		}

		public static void Pause()
		{
			Tween.Pause();
		}

		public static void Unpause()
		{
			Tween.Unpause();
		}
	}
}
