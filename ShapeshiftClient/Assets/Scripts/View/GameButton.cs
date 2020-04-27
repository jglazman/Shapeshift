//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	/// <summary>UI button that controls the game flow.</summary>
	public class GameButton : MonoBehaviour
	{
		[SerializeField] private GameMessageType _messageType = GameMessageType.Undefined;

		
		private void Awake()
		{
			Assert.IsTrue(_messageType != GameMessageType.Undefined, $"[GameButton] message type is undefined: {Utilities.GetPathToGameObjectInScene(gameObject)}");
		}
		
		public void OnClick_SendMessage()
		{
			Game.Notify(new NavigationMessage(_messageType));
		}
	}
}
