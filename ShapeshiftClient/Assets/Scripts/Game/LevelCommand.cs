//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public static partial class Level
	{
		public enum CommandType
		{
			Undefined = 0,
			
			LoadLevel,
			
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

			public int levelIndex;

			public LoadLevelCommand(int levelIndex)
			{
				// Payload.SetField((int)Field.LevelIndex, levelIndex);
				this.levelIndex = levelIndex;
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
