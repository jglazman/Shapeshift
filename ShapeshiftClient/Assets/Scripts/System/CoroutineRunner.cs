//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	public class CoroutineRunner : MonoBehaviour
	{
		private static CoroutineRunner _instance = null;

		private void Awake()
		{
			Assert.IsTrue(_instance == null, "[CoroutineRunner] CoroutineRunner is a singleton");

			_instance = this;
		}

		private void OnDestroy()
		{
			_instance = null;
		}


		public static void Run(IEnumerator coroutine)
		{
			Assert.IsTrue(_instance != null, "[CoroutineRunner] CoroutineRunner instance is missing");

			_instance.StartCoroutine(coroutine);
		}

		
		public static void WaitSecondsThenRun(float delay, Action action)
		{
			Assert.IsTrue(_instance != null, "[CoroutineRunner] CoroutineRunner instance is missing");

			_instance.StartCoroutine(Coroutine_WaitSecondsThenRun(delay, action));
		}
		
		private static IEnumerator Coroutine_WaitSecondsThenRun(float delay, Action action)
		{
			yield return new WaitForSeconds(delay);

			action?.Invoke();
		}
		
		public static void WaitSecondsThenRun(float delay, IEnumerator coroutine)
		{
			Assert.IsTrue(_instance != null, "[CoroutineRunner] CoroutineRunner instance is missing");

			_instance.StartCoroutine(Coroutine_WaitSecondsThenRun(delay, coroutine));
		}

		private static IEnumerator Coroutine_WaitSecondsThenRun(float delay, IEnumerator coroutine)
		{
			yield return new WaitForSeconds(delay);

			yield return coroutine;
		}
		
		
		public static void WaitFramesThenRun(int frames, IEnumerator coroutine)
		{
			Assert.IsTrue(_instance != null, "[CoroutineRunner] CoroutineRunner instance is missing");

			_instance.StartCoroutine(Coroutine_WaitFramesThenRun(frames, coroutine));
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
