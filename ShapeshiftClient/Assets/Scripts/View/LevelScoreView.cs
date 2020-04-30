//
// Copyright (c) 2020 Jeremy Glazman
//

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Glazman.Shapeshift
{
	public class LevelScoreView : MonoBehaviour
	{
		private static readonly int AddPointsTrigger = Animator.StringToHash("AddPoints");

		[SerializeField] private Slider _slider = null;
		[SerializeField] private RectTransform _sliderTransform = null;
		[SerializeField] private RectTransform _goal1Transform = null;
		[SerializeField] private RectTransform _goal2Transform = null;
		[SerializeField] private GameObject _goal1Completed = null;
		[SerializeField] private GameObject _goal2Completed = null;
		[SerializeField] private GameObject _goal3Completed = null;
		[SerializeField] private TextMeshProUGUI _pointsText = null;
		[SerializeField] private TextMeshProUGUI _movesText = null;
		[SerializeField] private TextMeshProUGUI _levelText = null;
		[SerializeField] private Animator _scoreAnimator = null;
		[SerializeField] private float _lerpDuration = 1f;
		[SerializeField] private float _goalCompletedOffset = 5f;

		private LevelConfig _levelConfig;
		
		private float _showingScore;
		private float _desiredScore;
		private float _lerpTime;
		private bool _isGoal1Completed;
		private bool _isGoal2Completed;
		private bool _isGoal3Completed;

		public void SetGoals(int levelIndex, LevelConfig levelConfig)
		{
			_levelConfig = levelConfig;
			
			// TODO: support other goal and challenge types
			
			//var sliderSize = _sliderTransform.sizeDelta;
			var sliderSize = _sliderTransform.rect;
			_goal1Transform.anchoredPosition = new Vector2(sliderSize.width * ((float)levelConfig.goal1 / (float)levelConfig.goal3) + _goalCompletedOffset, _goal1Transform.anchoredPosition.y);
			_goal2Transform.anchoredPosition = new Vector2(sliderSize.width * ((float)levelConfig.goal2 / (float)levelConfig.goal3) + _goalCompletedOffset, _goal2Transform.anchoredPosition.y);

			_slider.value = 0;
			_pointsText.text = "0";
			_movesText.text = levelConfig.challengeValue.ToString();
			_levelText.text = levelIndex.ToString();
		}
		
		private void Update()
		{
			if (_showingScore < _desiredScore)
			{
				_lerpTime += Time.deltaTime;
				float lerp = _lerpTime / _lerpDuration;
				_showingScore = Mathf.Lerp(_showingScore, _desiredScore, lerp);
				
				_pointsText.text = $"{_showingScore:n0}";
				_slider.value = (float)_showingScore / (float)_levelConfig.goal3;

				if (!_isGoal1Completed && _showingScore >= _levelConfig.goal1)
				{
					_isGoal1Completed = true;
					_goal1Completed.SetActive(true);
				}

				if (!_isGoal2Completed && _showingScore >= _levelConfig.goal2)
				{
					_isGoal2Completed = true;
					_goal2Completed.SetActive(true);
				}

				if (!_isGoal3Completed && _showingScore >= _levelConfig.goal3)
				{
					_isGoal3Completed = true;
					_goal3Completed.SetActive(true);
				}

				if (_showingScore >= _desiredScore)
				{
					PlayAnimation(false);
				}
			}
		}


		public void SetPoints(int points)
		{
			_desiredScore = points;
			_lerpTime = 0f;

			PlayAnimation(true);
		}

		public void SetMoves(int moves)
		{
			_movesText.text = moves.ToString();
		}

		private void PlayAnimation(bool play)
		{
			_scoreAnimator.SetBool(AddPointsTrigger, play);
		}
	}
}
