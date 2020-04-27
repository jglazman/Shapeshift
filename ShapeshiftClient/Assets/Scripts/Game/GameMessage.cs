//
// Copyright (c) 2020 Jeremy Glazman
//

namespace Glazman.Shapeshift
{
	// WARNING: these are serialized properties. do not change their values.
	public enum GameMessageType
	{
		Undefined = 0,
		
		Navigate_RefugeZero = 100,
		Navigate_Settings = 101,
		Navigate_WorldMap = 102,
		Navigate_Level = 103
	}

	
	public abstract class GameMessage
	{
		public GameMessageType GameMessageType { get; private set; }

		public GameMessage(GameMessageType messageType)
		{
			GameMessageType = messageType;
		}
	}

	
	public class NavigationMessage : GameMessage
	{
		public NavigationMessage(GameMessageType messageType) : base(messageType)
		{
		}
	}

	
	public class LoadWorldMapMessage : GameMessage
	{
		public LoadWorldMapMessage() : base(GameMessageType.Navigate_WorldMap)
		{
		}
	}


	public class LoadLevelMessage : GameMessage
	{
		public int levelIndex { get; }
		

		public LoadLevelMessage(int levelIndex) : base(GameMessageType.Navigate_Level)
		{
			this.levelIndex = levelIndex;
		}
	}
}
