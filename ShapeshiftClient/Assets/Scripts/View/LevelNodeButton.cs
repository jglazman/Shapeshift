//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public class LevelNodeButton : MonoBehaviour
	{
		[SerializeField] private int _levelIndex = 0;
		[SerializeField] private GameObject _lockedButton = null;
		[SerializeField] private GameObject _unlockedButton = null;

		public int LevelIndex { get { return _levelIndex; } }
		
		public bool IsUnlocked => Database.Load<LevelData>(_levelIndex).Value.isUnlocked;
		
		private void Awake()
		{
			Assert.IsTrue(_levelIndex > 0, $"[LevelNodeButton] level index is undefined: {Utilities.GetPathToGameObjectInScene(gameObject)}");
			Assert.IsTrue(_lockedButton != null && _unlockedButton != null, $"[ToggleButton] toggle is missing a reference: {Utilities.GetPathToGameObjectInScene(gameObject)}");
			
			// TODO: hack
			if (_levelIndex == 1)
			{
				var levelData = Database.Load<LevelData>(_levelIndex);
				levelData.Value.isUnlocked = true;
				Database.Save(levelData);
			}
		}
		
		private void Start()
		{
			Refresh();
		}

		public void Refresh()
		{
			bool unlocked = IsUnlocked;
			_lockedButton.SetActive(!unlocked);
			_unlockedButton.SetActive(unlocked);
		}
		
		public void OnClick_Select()
		{
			if (IsUnlocked)
			{
				Game.Notify(new LoadLevelMessage(_levelIndex));
			}
			else
			{
				PopupViewController.Open<LevelLockedPopup>();
			}
		}
	}
}
