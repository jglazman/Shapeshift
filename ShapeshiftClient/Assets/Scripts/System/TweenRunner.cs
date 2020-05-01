//
// Copyright (c) 2020 Jeremy Glazman
//

using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	[RequireComponent(typeof(DontDestroyOnLoad))]
	public class TweenRunner : MonoBehaviour
	{
		private static TweenRunner Instance = null;

		[SerializeField] private TweenConfig[] _tweenConfigs = null;

		private void Awake()
		{
			Assert.IsTrue(Instance == null, "[TweenRunner] TweenRunner is a singleton");

			Instance = this;
			
			Tween.Initialize(_tweenConfigs);
		}

		private void OnDestroy()
		{
			Instance = null;
		}

		private void Update()
		{
			Tween.Update(Time.deltaTime);
		}
	}
}
