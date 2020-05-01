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
	public interface IPooledObject
	{
		/// <summary>
		/// Called when the object returns to the pool.  The object should reset itself to a
		/// default state so it is ready to be used again.
		/// </summary>
		void OnReturnToPool();

		/// <summary>
		/// This is used to convert the pooled object back to its MonoBehaviour type.
		/// </summary>
		GameObject GetGameObject();
	}
	
	[RequireComponent(typeof(DontDestroyOnLoad))]
	public class PrefabPool : MonoBehaviour
	{
		protected static PrefabPool Instance { get; private set; }

		protected static Transform HiddenRoot { get; private set; }

		private void Awake()
		{
			Assert.IsNull(Instance, "[PrefabPool] There should only be one instance of PrefabPool.");

			Instance = this;
			HiddenRoot = transform;
			gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			Assert.IsTrue(Instance == this, "[PrefabPool] Destroyed a zombie.");

			HiddenRoot = null;
			Instance = null;
		}
		
		
		private static Dictionary<System.Type, Tuple<GameObject,Queue<IPooledObject>>> _pools = new Dictionary<Type, Tuple<GameObject, Queue<IPooledObject>>>();

		public static void CreatePool<T>(T prefab) where T : MonoBehaviour, IPooledObject
		{
			Assert.IsTrue(!_pools.ContainsKey(typeof(T)), $"[PrefabPool] Pool of type '{typeof(T)}' was already created.");
			
			_pools[typeof(T)] = new Tuple<GameObject, Queue<IPooledObject>>(prefab.gameObject, new Queue<IPooledObject>());
		}
		
		public static T Get<T>(Transform parent=null) where T : MonoBehaviour, IPooledObject
		{
			Assert.IsTrue(_pools.ContainsKey(typeof(T)), $"[PrefabPool] Pool of type '{typeof(T)}' does not exist.");

			var (prefab, pool) = _pools[typeof(T)];
			
			if (pool.Count == 0)
			{
				var newObject = Instantiate(prefab, parent);
				return newObject.GetComponent<T>();
			}

			var pooledObject = pool.Dequeue();
			Assert.IsNotNull(pooledObject, $"[PrefabPool] Pooled object of type '{typeof(T)}' is null");
			
			var pooledGameObject = pooledObject.GetGameObject();
			pooledGameObject.transform.SetParent(parent, false);
			return pooledGameObject.GetComponent<T>();
		}

		public static void Return<T>(T pooledObject) where T : MonoBehaviour, IPooledObject
		{
			Assert.IsTrue(_pools.ContainsKey(typeof(T)), $"[PrefabPool] Pool of type '{typeof(T)}' does not exist.");
			Assert.IsNotNull(pooledObject, "[PrefabPool] Tried to return a null object to the pool");
			
			var (prefab, pool) = _pools[typeof(T)];
			
			pooledObject.OnReturnToPool();
			pooledObject.transform.SetParent(HiddenRoot, false);
			pool.Enqueue(pooledObject);
		}

		public static void Clear<T>() where T : MonoBehaviour, IPooledObject
		{
			Assert.IsTrue(_pools.ContainsKey(typeof(T)), $"[PrefabPool] Pool of type '{typeof(T)}' does not exist.");

			var (prefab, pool) = _pools[typeof(T)];

			while (pool.Count > 0)
			{
				var obj = pool.Dequeue().GetGameObject();
				Destroy(obj);
			}
			
			_pools.Remove(typeof(T));
		}

		private static void ClearAll()
		{
			Logger.LogWarningEditor("Clear all!!!");
			
			int numChildren = HiddenRoot.childCount;
			for (int i = numChildren - 1; i >= 0; i--)
				Destroy(HiddenRoot.GetChild(i).gameObject);
			
			_pools.Clear();
		}
	}
}
