using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
	public class ObjectPoolEntry
	{
		public GameObject Prefab;

		[NonSerialized]
		public List<GameObject> Objects;

		public ObjectPoolEntry(GameObject prefab)
		{
			Prefab = prefab;
			Objects = new List<GameObject>();
		}
	}

	private Dictionary<string, ObjectPoolEntry> pools;

	private static ObjectPool _instance;

	private static GameObject _singleton;

	public static ObjectPool Instance
	{
		get
		{
			if (_instance == null)
			{
				_singleton = new GameObject();
				_instance = _singleton.AddComponent<ObjectPool>();
				_singleton.name = "[ObjectPool]";
			}
			return _instance;
		}
	}

	private void Awake()
	{
		if (base.gameObject != _singleton)
		{
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
		else
		{
			pools = new Dictionary<string, ObjectPoolEntry>();
		}
	}

	private GameObject InstantiateNewObject(GameObject prefab)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
		gameObject.name = prefab.name;
		gameObject.transform.parent = base.transform;
		return gameObject;
	}

	private ObjectPoolEntry FindOrCreatePool<T>(T newPrefab)
	{
		MonoBehaviour monoBehaviour = newPrefab as MonoBehaviour;
		if (monoBehaviour != null)
		{
			ObjectPoolEntry objectPoolEntry = FindPool(monoBehaviour.name);
			if (objectPoolEntry == null)
			{
				objectPoolEntry = new ObjectPoolEntry(monoBehaviour.gameObject);
				pools.Add(monoBehaviour.name, objectPoolEntry);
			}
			return objectPoolEntry;
		}
		return null;
	}

	private ObjectPoolEntry FindPool(string name)
	{
		ObjectPoolEntry value;
		pools.TryGetValue(name, out value);
		return value;
	}

	public T Acquire<T>(T prefab)
	{
		ObjectPoolEntry objectPoolEntry = FindOrCreatePool(prefab);
		if (objectPoolEntry != null)
		{
			while (objectPoolEntry.Objects.Count > 0)
			{
				GameObject gameObject = objectPoolEntry.Objects[0];
				objectPoolEntry.Objects.RemoveAt(0);
				if (gameObject != null)
				{
					gameObject.SetActive(true);
					return gameObject.GetComponent<T>();
				}
				Debug.LogError("ObjectPool '" + objectPoolEntry.Prefab.name + "' contains a null entry!");
			}
			if (objectPoolEntry.Prefab != null)
			{
				return InstantiateNewObject(objectPoolEntry.Prefab).GetComponent<T>();
			}
			Debug.LogError("ObjectPool '" + objectPoolEntry.Prefab.name + "' contains a null Prefab reference!");
		}
		return default(T);
	}

	public bool Release(MonoBehaviour obj)
	{
		ObjectPoolEntry objectPoolEntry = FindPool(obj.gameObject.name);
		if (objectPoolEntry != null)
		{
			if (!obj.gameObject.activeSelf)
			{
				Debug.LogWarningFormat("ObjectPool is attemting to double release {0}", obj.name);
				return false;
			}
			obj.gameObject.SetActive(false);
			objectPoolEntry.Objects.Add(obj.gameObject);
			obj.transform.parent = base.transform;
			return true;
		}
		UnityEngine.Object.Destroy(obj);
		return false;
	}
}
