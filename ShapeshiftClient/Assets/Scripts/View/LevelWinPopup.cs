//
// Copyright (c) 2020 Jeremy Glazman
//

using TMPro;
using UnityEngine;

namespace Glazman.Shapeshift
{
	public class LevelWinPopup : GenericPopupView
	{
		private const string STARS_KEY = "Stars";
		
		private static readonly int StarsInteger = Animator.StringToHash(STARS_KEY);
		
		[SerializeField] private AnimEventReceiver _animEventReceiver = null;
		[SerializeField] private Animator _starsAnimator = null;
		[SerializeField] private Animation _tallyAnimation = null;
		[SerializeField] private TextMeshProUGUI _scoreText = null;
		[SerializeField] private TextMeshProUGUI _movesText = null;
		[SerializeField] private TextMeshProUGUI _winText = null;
		[SerializeField] private GameObject _bestScore = null;
		[SerializeField] private GameObject _bestMoves = null;


		private int _numStars;
		
		private void Awake()
		{
			_animEventReceiver.OnAnimEvent = OnAnimEvent;
		}

		private void OnAnimEvent(Animator animator, AnimEventPayload animEvent)
		{
			if (animEvent.key == STARS_KEY && animEvent.value == _numStars)
			{
				// TODO: this was a quick hack to splice some animations together
				float delay = _numStars == 3 ? 1.3f : _numStars == 2 ? 0.7f : 0.6f;
				CoroutineRunner.WaitSecondsThenRun(delay, () =>
				{
					animator.StopPlayback();
					animator.enabled = false;
					_tallyAnimation.Play();
				});
			}
		}

		public void ShowScore(Level.LevelWinEvent winEvent, LevelConfig levelConfig)
		{
			_numStars = winEvent.Stars;
			_scoreText.text = $"{winEvent.Points:n0}";
			_movesText.text = $"{winEvent.Moves}";
			_bestScore.SetActive(winEvent.Points >= winEvent.BestPoints);
			_bestMoves.SetActive(winEvent.Moves < winEvent.BestMoves || (winEvent.Moves == winEvent.BestMoves && winEvent.Moves < levelConfig.challengeValue ));

			_starsAnimator.SetInteger(StarsInteger, winEvent.Stars);

			if (winEvent.Stars == 1)
				_winText.text = "NO MOVES LEFT!";
			else if (winEvent.Stars == 2)
				_winText.text = "AMAZING!";
			else
				_winText.text = "YOU WIN!";
		}
		
		
		public void OnClick_Ok()
		{
			Game.Notify(new Game.GoToWorldMapMessage());
		}
	}
}
