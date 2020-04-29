//
// Copyright (c) 2020 Jeremy Glazman
//

using System.Collections.Generic;

namespace Glazman.Shapeshift
{
	public static partial class Level
	{
		public enum CommandType
		{
			Undefined = 0,
			
			LoadLevel,
			SubmitMatch,
			
			// debug
			Debug_Win,
			Debug_Lose
		}

		public abstract class Command
		{
			public abstract CommandType CommandType { get; }
			
			public readonly Payload Payload = new Payload();
		}

		public class LoadLevelCommand : Command
		{
			// public enum Field
			// {
			// 	Undefined = 0,
			// 	LevelIndex,
			// 	GridState
			// }

			public override CommandType CommandType { get { return CommandType.LoadLevel; } }

			public int LevelIndex { get; }

			public LoadLevelCommand(int levelIndex)
			{
				// Payload.SetField((int)Field.LevelIndex, levelIndex);
				LevelIndex = levelIndex;
			}
		}

		public class SubmitMatchCommand : Command
		{
			public override CommandType CommandType { get { return CommandType.SubmitMatch; } }

			public List<GridIndex> SelectedItems { get; }

			public SubmitMatchCommand(IEnumerable<GridIndex> selectedItems)
			{
				SelectedItems = new List<GridIndex>(selectedItems);
			}
		}
		
		
		public class DebugWinCommand : Command
		{
			public override CommandType CommandType { get { return CommandType.Debug_Win; } }
		}

		public class DebugLoseCommand : Command
		{
			public override CommandType CommandType { get { return CommandType.Debug_Lose; } }
		}
	}
}
