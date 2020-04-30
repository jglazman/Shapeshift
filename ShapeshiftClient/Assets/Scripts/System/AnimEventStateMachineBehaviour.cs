//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using System.Linq;
using UnityEngine;

namespace Glazman.Shapeshift
{
	public class AnimEventStateMachineBehaviour : StateMachineBehaviour
	{
		[SerializeField] private AnimEventPayload[] _animEvents = null;
		
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			SendEvents(animator, AnimEventType.OnStateEnter);
		}

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			SendEvents(animator, AnimEventType.OnStateExit);
			
			Logger.LogEditor($"[OnStateExit] {animator.name}, {stateInfo.shortNameHash}");
		}

		private void SendEvents(Animator animator, AnimEventType eventType)
		{
			var receiver = animator.GetComponent<AnimEventReceiver>();
			if (receiver != null)
			{
				var exitEvents = _animEvents.Where(e => e.eventType == eventType);
				foreach (var exitEvent in exitEvents)
					receiver.SendEvent(exitEvent);
			}

		}
	}
}
