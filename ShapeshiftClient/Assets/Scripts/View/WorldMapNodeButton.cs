//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public class WorldMapNodeButton : MonoBehaviour
	{
		[SerializeField] private int _levelIndex = 0;
		[SerializeField] private GameObject _lockedButton = null;
		[SerializeField] private GameObject _unlockedButton = null;

		public int LevelIndex { get { return _levelIndex; } }
		
		public bool IsUnlocked => _levelIndex == 1 || Database.Load<LevelProgressData>(_levelIndex).Value.isUnlocked;
		
		private void Awake()
		{
			Assert.IsTrue(_levelIndex > 0, $"[WorldMapNodeButton] level index is undefined: {Utilities.GetPathToGameObjectInScene(gameObject)}");
			Assert.IsTrue(_lockedButton != null && _unlockedButton != null, $"[WorldMapNodeButton] toggle is missing a reference: {Utilities.GetPathToGameObjectInScene(gameObject)}");
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
		
		public void OnClick()
		{
			if (IsUnlocked)
			{
				Game.Notify(new Game.GoToLevelMessage(_levelIndex));
			}
			else
			{
				MessagePopup.ShowMessage("Complete earlier levels to unlock this level.");
			}
		}
	}
}
