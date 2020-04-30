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
		[SerializeField] private GameObject _bestScore = null;
		[SerializeField] private GameObject _bestMoves = null;


		private int _numStars;
		
		private void Awake()
		{
			_animEventReceiver.OnAnimEvent = OnAnimEvent;
		}

		private void OnAnimEvent(AnimEventPayload animEvent)
		{
			if (animEvent.key == STARS_KEY && animEvent.value == _numStars)
			{
				_tallyAnimation.Play();
			}
		}

		public void ShowScore(Level.LevelWinEvent winEvent)
		{
			_numStars = winEvent.Stars;
			_scoreText.text = $"{winEvent.Score:n0}";
			_movesText.text = $"{winEvent.Moves}";
			_bestScore.SetActive(winEvent.Score >= winEvent.BestScore);
			_bestMoves.SetActive(winEvent.Moves <= winEvent.BestMoves);

			_starsAnimator.SetInteger(StarsInteger, winEvent.Stars);
		}
		
		
		public void OnClick_Ok()
		{
			Game.Notify(new Game.GoToWorldMapMessage());
		}
	}
}
