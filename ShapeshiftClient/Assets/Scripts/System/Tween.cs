//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Glazman.Shapeshift
{
	[Serializable]
	public class TweenConfig
	{
		public string guid;
		public AnimationCurve curve;

		public static int ConvertGuidToId(string guid)
		{
			return Animator.StringToHash(guid);
		}
	}

	public static class Tween
	{
		private static Dictionary<int, TweenConfig> TweenConfigs = new Dictionary<int, TweenConfig>();

		private static List<TweenObject> _activeTweens = new List<TweenObject>();

		public static bool IsPaused { get; private set; }


		public static void Initialize(TweenConfig[] configs)
		{
			foreach (var config in configs)
			{
				int tweenId = TweenConfig.ConvertGuidToId(config.guid);
				
				Assert.IsFalse(TweenConfigs.ContainsKey(tweenId), $"[Tween] More than one TweenConfig has the same id: {config.guid} ({tweenId})");
				
				TweenConfigs[tweenId] = config;
			}
		}

		public static float Evaluate(int tweenId, float t)
		{
			Assert.IsTrue(TweenConfigs.ContainsKey(tweenId), $"[Tween] Tried to evaluate an unknown TweenType: {tweenId}");

			return TweenConfigs[tweenId].curve.Evaluate(t);
		}

		public static void Update(float deltaTime)
		{
			if (IsPaused)
				return;
			
			for (int i = _activeTweens.Count - 1; i >= 0; i--)
			{
				var tween = _activeTweens[i];

				tween.Update(deltaTime);

				if (tween.IsDone)
					_activeTweens.RemoveAt(i);
			}
		}

		public static void Run(TweenObject tweenObject)
		{
			Assert.IsTrue(!_activeTweens.Any(t => !t.IsDone && t.Transform == tweenObject.Transform), 
				$"[Tween] Tried to tween an object that's already being tweened: {Utilities.GetPathToGameObjectInScene(tweenObject.Transform)}");
			
			_activeTweens.Add(tweenObject);
		}

		public static void Pause()
		{
			IsPaused = true;
		}

		public static void Unpause()
		{
			IsPaused = false;
		}
	}


	public class TweenObject
	{
		public Transform Transform { get; }
		public int TweenId { get; }
		public float Duration { get; }
		public float StartTime { get; private set; }
		public Vector3 Origin { get; }
		public Vector3 Destination { get; }
		public float Elapsed { get; private set; }
		public bool IsLocalSpace { get; }
		public bool IsPaused { get; private set; }
		public bool IsDone { get; private set; }

		public float NormalizedTime => Mathf.Clamp(Elapsed / Duration, 0f, 1f);

		private Action<TweenObject> OnComplete = null;


		public TweenObject(int tweenId, float duration, Transform transform, Vector3 origin, Vector3 destination, bool isLocalSpace, Action<TweenObject> onComplete=null)
		{
			Transform = transform;
			TweenId = tweenId;
			Duration = duration;
			StartTime = Time.time;
			Origin = origin;
			Elapsed = 0f;
			Destination = destination;
			IsLocalSpace = isLocalSpace;	// else is world space
			OnComplete = onComplete;
		}

		/// <summary>Returns false if the tween is complete (or if it should halt for any other reason).</summary>
		public void Update(float deltaTime)
		{
			if (IsDone || IsPaused)
				return;
			
			if (Transform == null)
			{
				Done();
				return;
			}

			Elapsed += deltaTime;

			float t = Tween.Evaluate(TweenId, NormalizedTime);
			Vector3 pos = Vector3.Lerp(Origin, Destination, t);
			
			if (IsLocalSpace)
				Transform.localPosition = pos;
			else
				Transform.position = pos;

			if (Math.Abs(t - 1f) < 0.001f)
			{
				Done();
				return;
			}
		}

		public void Pause()
		{
			IsPaused = true;
		}

		public void UnPause()
		{
			IsPaused = false;
		}

		public void Cancel()
		{
			Done();
		}

		private void Done()
		{
			IsDone = true;
			OnComplete?.Invoke(this);
		}
	}
}
