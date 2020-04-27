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
			Debug_Win,
			Debug_Lose
		}

		public abstract class Command
		{
			public abstract CommandType commandType { get; }
			
			public readonly Payload payload = new Payload();
		}

		public class LoadLevelCommand : Command
		{
			public enum Field
			{
				Undefined = 0,
				LevelIndex
			}
			
			public override CommandType commandType { get { return CommandType.LoadLevel; } }

			public LoadLevelCommand(int levelIndex)
			{
				payload.SetField((int)Field.LevelIndex, levelIndex);
			}
		}

		public class DebugWinCommand : Command
		{
			public override CommandType commandType { get { return CommandType.Debug_Win; } }
		}

		public class DebugLoseCommand : Command
		{
			public override CommandType commandType { get { return CommandType.Debug_Lose; } }
		}
	}
}
