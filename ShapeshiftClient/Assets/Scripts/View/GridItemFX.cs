//
// Copyright (c) 2020 Jeremy Glazman
//

using TMPro;
using UnityEngine;

namespace Glazman.Shapeshift
{
	public class GridItemFX : GridBaseView
	{
		[SerializeField] private TextMeshProUGUI _tileText = null;
		[SerializeField] private TextMeshProUGUI _pointsText = null;

		public GridItemConfig GridItemConfig => GameConfig.GetGridItem(ID);
		
		private float _timeElapsed;
		private float _duration;


		protected override string GetSpriteResourceName()
		{
			return GridItemConfig.ImageName;
		}
		
		protected override void SetId(string id, bool andEnable = false)
		{
			base.SetId(id, andEnable);

			// HACK: show letters dynamically so we don't have to generate all 26 tiles
			if (!string.IsNullOrEmpty(id) && GridItemConfig.MatchIndex >= 65)
				_tileText.text = $"{(char)GridItemConfig.MatchIndex}";
			else
				_tileText.text = null;
		}

		
		public void Show(int points, float seconds)
		{
			_pointsText.text = $"+{points}";
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
