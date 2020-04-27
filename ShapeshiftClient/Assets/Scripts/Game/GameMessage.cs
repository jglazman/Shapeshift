//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	public static partial class Game
	{
		// WARNING: these are serialized properties. do not change their values.
		public enum MessageType
		{
			Undefined = 0,
			GoToWorldMap = 1,
			GoToLevel = 2
		}


		public abstract class Message
		{
			public abstract MessageType MessageType { get; }
		}

		public class GoToWorldMapMessage : Message
		{
			public override MessageType MessageType { get { return MessageType.GoToWorldMap; } }
		}
		
		public class GoToLevelMessage : Message
		{
			public override MessageType MessageType { get { return MessageType.GoToLevel; } }
			
			public int LevelIndex { get; }

			public GoToLevelMessage(int levelIndex)
			{
				LevelIndex = levelIndex;
			}
		}
	}
}
