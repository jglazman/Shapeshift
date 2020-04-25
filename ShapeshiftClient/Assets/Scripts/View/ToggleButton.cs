//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public class ToggleButton : MonoBehaviour
	{
		[SerializeField] private GameOption _gameOption = GameOption.Undefined;
		[SerializeField] private GameObject _toggleOnButton = null;
		[SerializeField] private GameObject _toggleOffButton = null;


		private bool IsToggledOn => GameSettings.GetOption(_gameOption) != 0;
		
		
		private void Awake()
		{
			Assert.IsTrue(_gameOption != GameOption.Undefined, $"[ToggleButton] toggle is undefined: {Utilities.GetPathToGameObjectInScene(gameObject)}");
			Assert.IsTrue(_toggleOnButton != null && _toggleOffButton != null, $"[ToggleButton] toggle is missing a reference: {Utilities.GetPathToGameObjectInScene(gameObject)}");
		}
		
		private void Start()
		{
			Refresh();
		}

		private void Refresh()
		{
			bool on = IsToggledOn;
			_toggleOnButton.SetActive(on);
			_toggleOffButton.SetActive(!on);
		}
		
		public void OnClick_Toggle()
		{
			GameSettings.SetOption(_gameOption, !IsToggledOn);
			Refresh();
		}
	}
}
