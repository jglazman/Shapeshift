//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.UI;

namespace Glazman.Shapeshift
{
	public class SettingsSlider : SettingsWidget
	{
		[SerializeField] private Slider _slider;

		protected override void Refresh()
		{
			_slider.value = GetValue();
		}

		
		public void OnSliderChanged()
		{
			int value = Mathf.FloorToInt(_slider.value);
			SetValue(value);
		}
	}
}
