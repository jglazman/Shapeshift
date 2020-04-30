//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using UnityEngine;

namespace Glazman.Shapeshift
{
	public enum AnimEventType
	{
		Undefined = 0,
		OnStateEnter,
		OnStateExit
	}
	
	[Serializable]
	public struct AnimEventPayload
	{
		public AnimEventType eventType;
		public string key;
		public int value;
	}
	
	public class AnimEventReceiver : MonoBehaviour
	{
		public Action<AnimEventPayload> OnAnimEvent;


		public void SendEvent(AnimEventPayload animEvent)
		{
			OnAnimEvent?.Invoke(animEvent);
		}
	}
}
