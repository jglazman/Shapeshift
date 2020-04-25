//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;

namespace Glazman.Shapeshift
{
	public class SceneTransitioner : MonoBehaviour
	{
		private static readonly int IntroTrigger = Animator.StringToHash("PlayIntro");
		private static readonly int OutroTrigger = Animator.StringToHash("PlayOutro");
		
		
		[SerializeField] private Animator _animator = null;
		

		private void Awake()
		{
			SceneController.RegisterTransitioner(this);
		}

		private void OnDestroy()
		{
			SceneController.UnregisterTransitioner(this);
		}


		public void Notify(TransitionState state)
		{
			switch (state)
			{
				case TransitionState.Loading:
					break;
				
				case TransitionState.Intro:
					PlayIntro();
					break;
				
				case TransitionState.Outro:
					PlayOutro();
					break;
			}
		}


		protected virtual void PlayIntro()
		{
			if (_animator != null)
				_animator.SetTrigger(IntroTrigger);
		}

		protected virtual void PlayOutro()
		{
			if (_animator != null)
				_animator.SetTrigger(OutroTrigger);
		}
	}
}
