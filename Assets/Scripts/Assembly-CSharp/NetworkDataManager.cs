using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon;
using UnityEngine;

[DisallowMultipleComponent]
public class NetworkDataManager : Photon.MonoBehaviour
{
	public static NetworkDataManager Instance;

	private Dictionary<string, Action<object>> networkDataCallbackMap = new Dictionary<string, Action<object>>();

	protected override void Awake()
	{
		base.Awake();
		Instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public void RegisterCallback(string key, Action<object> callback)
	{
		if (networkDataCallbackMap.ContainsKey(key))
		{
			Dictionary<string, Action<object>> dictionary = networkDataCallbackMap;
			string key2 = key;
			dictionary[key2] = (Action<object>)Delegate.Combine(dictionary[key2], callback);
		}
		else
		{
			networkDataCallbackMap[key] = callback;
		}
	}

	public void UnregisterCallback(string key, Action<object> callback)
	{
		if (networkDataCallbackMap.ContainsKey(key))
		{
			Dictionary<string, Action<object>> dictionary = networkDataCallbackMap;
			string key2 = key;
			dictionary[key2] = (Action<object>)Delegate.Remove(dictionary[key2], callback);
			if (networkDataCallbackMap[key] == null)
			{
				networkDataCallbackMap.Remove(key);
			}
		}
	}

	private void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
	{
		foreach (string item in propertiesThatChanged.Keys.OfType<string>())
		{
			Action<object> value;
			if (networkDataCallbackMap.TryGetValue(item, out value))
			{
				value(propertiesThatChanged[item]);
			}
		}
	}
}
