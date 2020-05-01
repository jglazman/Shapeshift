//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public abstract class SettingsWidget : MonoBehaviour
	{
		[SerializeField] private GameOptionType _gameOption = GameOptionType.Undefined;

		protected bool IsToggledOn => Database.Load<SettingsData>((int)_gameOption).Value.ToggledOn;
		
		protected void Toggle()
		{
			var data = Database.Load<SettingsData>((int)_gameOption);
			data.Value.ToggledOn = !data.Value.ToggledOn;
			Database.Save(data);
		}

		protected void SetValue(int value)
		{
			var data = Database.Load<SettingsData>((int)_gameOption);
			data.Value.optionValue = value;
			Database.Save(data);
		}

		protected int GetValue()
		{
			return Database.Load<SettingsData>((int)_gameOption).Value.optionValue;
		}

		
		protected abstract void Refresh();

		private void Awake()
		{
			Assert.IsTrue(_gameOption != GameOptionType.Undefined, $"[SettingsWidget] option is undefined: {Utilities.GetPathToGameObjectInScene(gameObject)}");
		}
		
		private void Start()
		{
			Refresh();
		}
	}
}
