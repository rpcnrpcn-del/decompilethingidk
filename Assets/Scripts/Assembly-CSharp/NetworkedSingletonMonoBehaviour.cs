using Photon;
using UnityEngine;

public class NetworkedSingletonMonoBehaviour<T> : Photon.MonoBehaviour where T : Photon.MonoBehaviour
{
	private static T instance;

	public static T Instance
	{
		get
		{
			return instance;
		}
		protected set
		{
			if (instance == null)
			{
				instance = value;
			}
			else
			{
				Debug.LogError("Created more than one singleton of " + typeof(T).ToString() + "!");
			}
		}
	}

	public static bool IsInitialized
	{
		get
		{
			return Instance != null;
		}
	}
}
