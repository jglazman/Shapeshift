//
// Copyright (c) 2020 Jeremy Glazman
//

using TMPro;
using UnityEngine;

namespace Glazman.Shapeshift
{
	public class GridItemFX : GridBaseView
	{
		[SerializeField] private TextMeshProUGUI _pointsText;

		private float _timeElapsed;
		private float _duration;


		protected override string GetSpriteResourceName()
		{
			return $"GridItem-{Type}";
		}
		
		public void Show(int points, float seconds)
		{
			_pointsText.text = points.ToString();
			_duration = seconds;
			_timeElapsed = 0f;
		}
		

		private void Update()
		{
			if (!Tween.IsPaused)
			{
				_timeElapsed += Time.deltaTime;
				if (_timeElapsed > _duration)
				{
					PrefabPool.Return(this);
				}
			}
		}
	}
}
