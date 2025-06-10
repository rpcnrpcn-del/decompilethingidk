using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon;

public abstract class SynchronizedField
{
	public static void ClearAllFieldsForPlayer(PhotonPlayer player)
	{
		string prefix = FormatPlayerKey(player, string.Empty);
		ClearAllFieldsForPrefix(prefix);
	}

	public static void ClearAllFieldsForPhotonObject(MonoBehaviour component)
	{
		string prefix = FormatPhotonObjectKey(component, string.Empty);
		ClearAllFieldsForPrefix(prefix);
	}

	public static void ClearAllFieldsForMissingPlayers()
	{
		if (PhotonNetwork.room == null)
		{
			return;
		}
		HashSet<string> hashSet = new HashSet<string>();
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		foreach (PhotonPlayer photonPlayer in playerList)
		{
			hashSet.Add(photonPlayer.ID.ToString());
		}
		Hashtable hashtable = new Hashtable();
		int length = "PLAYER".Length;
		foreach (string key in PhotonNetwork.room.customProperties.Keys)
		{
			if (!key.StartsWith("PLAYER"))
			{
				continue;
			}
			string[] array = key.Split('.');
			if (array.Length <= 1 || array[0].Length <= length)
			{
				continue;
			}
			try
			{
				string item = array[0].Substring(length);
				if (!hashSet.Contains(item))
				{
					hashtable.Add(key, null);
				}
			}
			catch
			{
			}
		}
		if (hashtable.Count > 0)
		{
			PhotonNetwork.room.SetCustomProperties(hashtable);
		}
	}

	protected static string FormatPlayerKey(PhotonPlayer player, string key)
	{
		return string.Format("PLAYER{0}.{1}", player.ID, key);
	}

	protected static string FormatPhotonObjectKey(MonoBehaviour component, string key)
	{
		return string.Format("{0}.{1}", component.photonView.viewID, key);
	}

	protected static void ClearAllFieldsForPrefix(string prefix)
	{
		if (PhotonNetwork.room == null)
		{
			return;
		}
		Hashtable hashtable = new Hashtable();
		foreach (string key in PhotonNetwork.room.customProperties.Keys)
		{
			if (key.StartsWith(prefix))
			{
				hashtable.Add(key, null);
			}
		}
		if (hashtable.Count > 0)
		{
			PhotonNetwork.room.SetCustomProperties(hashtable);
		}
	}
}
public class SynchronizedField<T> : SynchronizedField
{
	private readonly MonoBehaviour component;

	private readonly string key;

	private readonly T defaultValue;

	private readonly SetterPermissionMode permissionMode;

	private readonly Action callback;

	private T cachedValue;

	public bool HasValue { get; private set; }

	public SynchronizedField(PhotonPlayer player, string key, T defaultValue, SetterPermissionMode permissionMode, Action callback = null)
		: this(SynchronizedField.FormatPlayerKey(player, key), defaultValue, permissionMode, callback)
	{
	}

	public SynchronizedField(MonoBehaviour component, string key, T defaultValue, SetterPermissionMode permissionMode, Action callback = null)
		: this(SynchronizedField.FormatPhotonObjectKey(component, key), defaultValue, permissionMode, callback)
	{
		this.component = component;
	}

	private SynchronizedField(string key, T defaultValue, SetterPermissionMode permissionMode, Action callback)
	{
		this.key = key;
		this.defaultValue = defaultValue;
		this.permissionMode = permissionMode;
		this.callback = callback;
		HasValue = PhotonNetwork.room != null && PhotonNetwork.room.customProperties.ContainsKey(key) && PhotonNetwork.room.customProperties[key] != null;
		cachedValue = ((!HasValue) ? defaultValue : ((T)PhotonNetwork.room.customProperties[key]));
		NetworkDataManager.Instance.RegisterCallback(key, OnValueChange);
	}

	~SynchronizedField()
	{
		if (NetworkDataManager.Instance != null)
		{
			NetworkDataManager.Instance.UnregisterCallback(key, OnValueChange);
		}
	}

	public T Get()
	{
		return cachedValue;
	}

	public void ForceSet(T newValue)
	{
		if (HasPermissionToSet() && PhotonNetwork.room != null && (!HasValue || !cachedValue.Equals(newValue)))
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add(key, newValue);
			Hashtable propertiesToSet = hashtable;
			PhotonNetwork.room.SetCustomProperties(propertiesToSet);
		}
	}

	public void CompareAndSwapSet(T newValue)
	{
		if (!HasValue)
		{
			ForceSet(newValue);
		}
		else if (HasPermissionToSet() && PhotonNetwork.room != null && !cachedValue.Equals(newValue))
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add(key, newValue);
			Hashtable propertiesToSet = hashtable;
			hashtable = new Hashtable();
			hashtable.Add(key, cachedValue);
			Hashtable expectedValues = hashtable;
			PhotonNetwork.room.SetCustomProperties(propertiesToSet, expectedValues);
			UpdateCachedValue(newValue);
		}
	}

	public void Clear()
	{
		if (PhotonNetwork.room != null && HasValue)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add(key, null);
			Hashtable propertiesToSet = hashtable;
			PhotonNetwork.room.SetCustomProperties(propertiesToSet);
			cachedValue = defaultValue;
			HasValue = false;
		}
	}

	private bool HasPermissionToSet()
	{
		bool isMasterClient = PhotonNetwork.isMasterClient;
		bool flag = component != null && component.hasAuthority;
		return permissionMode == SetterPermissionMode.ANYONE || (permissionMode == SetterPermissionMode.MASTER && isMasterClient) || (permissionMode == SetterPermissionMode.AUTHORITY && flag) || (permissionMode == SetterPermissionMode.MASTER_OR_AUTHORITY && (isMasterClient || flag));
	}

	private void OnValueChange(object value)
	{
		UpdateCachedValue(value);
		if (callback != null)
		{
			callback();
		}
	}

	private void UpdateCachedValue(object value)
	{
		if (value == null)
		{
			HasValue = false;
			cachedValue = defaultValue;
		}
		else
		{
			HasValue = true;
			cachedValue = (T)value;
		}
	}
}
