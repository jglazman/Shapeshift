//
// Copyright (c) 2020 Jeremy Glazman
//

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Glazman.Shapeshift
{
	public enum SceneName
	{
		Bootstrap,
		MainMenu,
		Game,
		WorldMap,
		Level
	}

	public enum TransitionState
	{
		Loading,
		Intro,
		Outro
	}
	
	public static class SceneController
	{
		/// <summary>
		/// Transitioners that are active in the scene, having already played their intros.
		/// We will wait for all active transitioners to complete their outro before we complete the transition.
		/// </summary>
		private static List<SceneTransitioner> _activeTransitioners = new List<SceneTransitioner>();
		
		/// <summary>
		/// Transitioners that have loaded from the next scene and are waiting to play their intros.
		/// </summary>
		private static List<SceneTransitioner> _pendingTransitioners = new List<SceneTransitioner>();


		private static bool _isLoadingScene = false;
		
		
		public static void RegisterTransitioner(SceneTransitioner transitioner)
		{
			Debug.Log($"[SceneController] register: {Utilities.GetPathToGameObjectInScene(transitioner.gameObject)}");

			Assert.IsTrue(!_activeTransitioners.Contains(transitioner), "[SceneController] Tried to register a SceneTransitioner that was already active.");
			Assert.IsTrue(!_pendingTransitioners.Contains(transitioner), "[SceneController] Tried to register a SceneTransitioner that was already pending.");
			
			_pendingTransitioners.Add(transitioner);
		}

		public static void UnregisterTransitioner(SceneTransitioner transitioner)
		{
			Debug.Log($"[SceneController] unregister: {Utilities.GetPathToGameObjectInScene(transitioner.gameObject)}");

			Assert.IsTrue(!_pendingTransitioners.Contains(transitioner), "[SceneController] Tried to unregister a SceneTransitioner that was still pending.");
			// Assert.IsTrue(_activeTransitioners.Contains(transitioner), "[SceneController] Tried to unregister a SceneTransitioner that was not active.");
			
			_activeTransitioners.Remove(transitioner);
		}

		
		/// <summary>Transition to the given scene, then callback.</summary>
		public static void LoadScene(SceneName sceneName, System.Action callback)
		{
			Assert.IsTrue(!_isLoadingScene, $"[SceneController] Tried to load scene '{sceneName}' but a scene load is already in progress.");

			_isLoadingScene = true;
			
			CoroutineRunner.Run(Coroutine_TransitionToScene(sceneName, callback));
		}

		private static IEnumerator Coroutine_TransitionToScene(SceneName sceneName, System.Action callback)
		{
			var previousScene = SceneManager.GetActiveScene();
			
			Debug.Log($"[SceneController] transition from scene '{previousScene.name}' to '{sceneName}'");
			
			// play the outro transitions
			for (int i = 0; i < _activeTransitioners.Count; i++)
			{
				if (_activeTransitioners[i] != null)
				{
					Debug.Log($"[SceneController] notify outro: {Utilities.GetPathToGameObjectInScene(_activeTransitioners[i].gameObject)}");
					_activeTransitioners[i].Notify(TransitionState.Outro);
				}
			}
			
			Debug.Log($"[SceneController] start loading next scene '{sceneName}'");

			// load the next scene
			var task = SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Additive);
			
			Debug.Log($"[SceneController] wait for transitioners to outro: {_activeTransitioners.Count}");

			// wait for transitions to finish
			float startTime = Time.time;
			while (_activeTransitioners.Count > 0)
			{
				yield return null;

				// TODO: implement a safety net that takes into account low-end devices?
				if (Time.time - startTime > 10f)
				{
					var remaining = string.Join(", ", _activeTransitioners.Select(t => t != null ? t.name : "null"));
					Debug.LogError($"[SceneController] Timed out while waiting for SceneTransitioners to outro: {remaining}");
					break;
				}
			}
			
			Debug.Log($"[SceneController] all transitioners finished outro");
			
			// wait for the scene to finish loading
			while (!task.isDone)
				yield return null;
			
			Debug.Log($"[SceneController] next scene '{sceneName}' finished loading");

			// unload previous scene
			yield return SceneManager.UnloadSceneAsync(previousScene);

			if (_activeTransitioners.Count > 0)
			{
				var remaining = string.Join(", ", _activeTransitioners.Select(t => t != null ? t.name : "null"));
				Debug.LogError($"[SceneController] Previous SceneTransitioners did not unregister themselves: {remaining}");
			}
			
			var pending = string.Join(", ", _pendingTransitioners.Select(t => t != null ? Utilities.GetPathToGameObjectInScene(t.gameObject) : "null"));
			Debug.Log($"[SceneController] move from pending to active: {pending}");
			
			// switch transitioners
			_activeTransitioners.Clear();
			_activeTransitioners.AddRange(_pendingTransitioners);
			_pendingTransitioners.Clear();
			
			Debug.Log($"[SceneController] wait for transitioners to intro: {_activeTransitioners.Count}");

			// play the intro transitions
			for (int i = 0; i < _activeTransitioners.Count; i++)
			{
				if (_activeTransitioners[i] != null)
					_activeTransitioners[i].Notify(TransitionState.Intro);
			}
			
			Debug.Log($"[SceneController] done. scene={sceneName}");

			// done
			callback?.Invoke();
			
			// TODO: which side of the callback should this be on? do we need to chain level loads together?
			_isLoadingScene = false;
		}
	}
}
