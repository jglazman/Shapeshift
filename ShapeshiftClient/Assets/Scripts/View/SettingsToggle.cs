//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public class SettingsToggle : SettingsWidget
	{
		[SerializeField] private GameObject _toggleOnButton = null;
		[SerializeField] private GameObject _toggleOffButton = null;

		protected override void Refresh()
		{
			bool on = IsToggledOn;
			_toggleOnButton.SetActive(on);
			_toggleOffButton.SetActive(!on);
		}
		
		public void OnClick_Toggle()
		{
			Toggle();
			Refresh();
		}
	}
}
