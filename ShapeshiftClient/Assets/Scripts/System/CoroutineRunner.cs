//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	[RequireComponent(typeof(DontDestroyOnLoad))]
	public class CoroutineRunner : MonoBehaviour
	{
		private static CoroutineRunner Instance = null;

		private void Awake()
		{
			Assert.IsTrue(Instance == null, "[CoroutineRunner] CoroutineRunner is a singleton");

			Instance = this;
		}

		private void OnDestroy()
		{
			Instance = null;
		}


		public static void Run(IEnumerator coroutine)
		{
			Assert.IsTrue(Instance != null, "[CoroutineRunner] CoroutineRunner instance is missing");

			Instance.StartCoroutine(coroutine);
		}

		
		public static void WaitSecondsThenRun(float delay, Action action)
		{
			Assert.IsTrue(Instance != null, "[CoroutineRunner] CoroutineRunner instance is missing");

			Instance.StartCoroutine(Coroutine_WaitSecondsThenRun(delay, action));
		}
		
		private static IEnumerator Coroutine_WaitSecondsThenRun(float delay, Action action)
		{
			yield return new WaitForSeconds(delay);

			action?.Invoke();
		}
		
		public static void WaitSecondsThenRun(float delay, IEnumerator coroutine)
		{
			Assert.IsTrue(Instance != null, "[CoroutineRunner] CoroutineRunner instance is missing");

			Instance.StartCoroutine(Coroutine_WaitSecondsThenRun(delay, coroutine));
		}

		private static IEnumerator Coroutine_WaitSecondsThenRun(float delay, IEnumerator coroutine)
		{
			yield return new WaitForSeconds(delay);

			yield return coroutine;
		}
		
		
		public static void WaitFramesThenRun(int frames, IEnumerator coroutine)
		{
			Assert.IsTrue(Instance != null, "[CoroutineRunner] CoroutineRunner instance is missing");

			Instance.StartCoroutine(Coroutine_WaitFramesThenRun(frames, coroutine));
		}

		private static IEnumerator Coroutine_WaitFramesThenRun(float frames, IEnumerator coroutine)
		{
			while (frames > 0)
			{
				yield return null;
				frames--;
			} 
			
			yield return coroutine;
		}
	}
}
