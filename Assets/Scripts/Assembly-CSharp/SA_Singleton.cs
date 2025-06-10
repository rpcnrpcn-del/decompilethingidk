using System;
using UnityEngine;

public abstract class SA_Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	private static bool applicationIsQuitting;

	[Obsolete("instance is deprectaed, plase use Instance instaed")]
	public static T instance
	{
		get
		{
			return Instance;
		}
	}

	public static T Instance
	{
		get
		{
			if (applicationIsQuitting)
			{
				return (T)null;
			}
			if (_instance == null)
			{
				_instance = UnityEngine.Object.FindObjectOfType(typeof(T)) as T;
				if (_instance == null)
				{
					_instance = new GameObject().AddComponent<T>();
					_instance.gameObject.name = _instance.GetType().Name;
				}
			}
			return _instance;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return !IsDestroyed;
		}
	}

	public static bool IsDestroyed
	{
		get
		{
			if (_instance == null)
			{
				return true;
			}
			return false;
		}
	}

	protected virtual void OnDestroy()
	{
		_instance = (T)null;
		applicationIsQuitting = true;
	}

	protected virtual void OnApplicationQuit()
	{
		_instance = (T)null;
		applicationIsQuitting = true;
	}
}
