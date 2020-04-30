//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;

namespace Glazman.Shapeshift
{
	public class SceneTransitionStateBehaviour : StateMachineBehaviour
	{
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			var transitioner = animator.GetComponent<SceneTransitioner>();
			if (transitioner != null)
				SceneController.UnregisterTransitioner(transitioner);
			else
				Logger.LogError("Animator is missing a SceneTransitioner");
		}
	}
}
