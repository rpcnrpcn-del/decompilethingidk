using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerData : Photon.MonoBehaviour
{
	public delegate void ValueChangedCallback(PlayerData sender, string key);

	private Dictionary<string, ValueChangedCallback> callbacks;

	protected override void Awake()
	{
		base.Awake();
		callbacks = new Dictionary<string, ValueChangedCallback>();
	}

	private void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
	{
		PhotonPlayer photonPlayer = playerAndUpdatedProps[0] as PhotonPlayer;
		Hashtable hashtable = playerAndUpdatedProps[1] as Hashtable;
		if (base.owner != photonPlayer)
		{
			return;
		}
		foreach (string item in hashtable.Keys.OfType<string>())
		{
			if (callbacks.ContainsKey(item))
			{
				callbacks[item](this, item);
			}
		}
	}

	public void RegisterCallback(string key, ValueChangedCallback callback)
	{
		if (callbacks.ContainsKey(key))
		{
			Dictionary<string, ValueChangedCallback> dictionary = callbacks;
			string key2 = key;
			dictionary[key2] = (ValueChangedCallback)Delegate.Combine(dictionary[key2], callback);
		}
		else
		{
			callbacks[key] = callback;
		}
	}

	public void UnregisterCallback(string key, ValueChangedCallback callback)
	{
		if (callbacks.ContainsKey(key))
		{
			Dictionary<string, ValueChangedCallback> dictionary = callbacks;
			string key2 = key;
			dictionary[key2] = (ValueChangedCallback)Delegate.Remove(dictionary[key2], callback);
		}
	}

	public bool HasData(string key)
	{
		return base.owner != null && base.owner.customProperties.ContainsKey(key);
	}

	public T GetData<T>(string key, T defaultValue)
	{
		if (base.owner != null && base.owner.customProperties.ContainsKey(key))
		{
			return (T)base.owner.customProperties[key];
		}
		return defaultValue;
	}

	public void SetData<T>(string key, T data)
	{
		if (base.owner != null)
		{
			base.owner.SetCustomProperties(key, data);
		}
	}

	public void SetData<T>(string key, T data, T expectedValue)
	{
		if (base.owner != null)
		{
			base.owner.SetCustomProperties(key, data, expectedValue);
		}
	}

	public void RemoveData(string key)
	{
		if (base.owner != null)
		{
			base.owner.SetCustomProperties<object>(key, null);
		}
	}
}
