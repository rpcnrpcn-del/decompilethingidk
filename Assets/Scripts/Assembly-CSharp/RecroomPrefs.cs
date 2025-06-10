using System;
using System.Collections;
using System.Collections.Generic;
using RecNet;
using UnityEngine;

public static class RecroomPrefs
{
	public delegate void DownloadPreferencesCallback(string error);

	public class Preference
	{
		public string value;

		public bool isDirty;

		public Preference(string v, bool d)
		{
			value = v;
			isDirty = d;
		}
	}

	private static bool downloadedFromRecnet = false;

	private static bool savePreferencesToRecnetInProgress = false;

	private static bool saveRequested = false;

	private static Dictionary<string, Preference> preferences = new Dictionary<string, Preference>();

	public static int CurrentDay
	{
		get
		{
			return DateTime.Today.DayOfYear;
		}
	}

	private static void CheckReady()
	{
		if (!downloadedFromRecnet)
		{
			//throw new RecnetPreferencesMissingException();
		}
	}

	public static IEnumerator DowloadLocalPlayerPreferences(DownloadPreferencesCallback callback)
	{
		string error = null;
		yield return Settings.DowloadLocalPlayerSettings(delegate(string e, List<Setting> newPreferences)
		{
			error = e;
			if (string.IsNullOrEmpty(e))
			{
				foreach (Setting newPreference in newPreferences)
				{
					SetPreference(newPreference.Key, newPreference.Value, false);
				}
			}
		});
		if (string.IsNullOrEmpty(error))
		{
			downloadedFromRecnet = true;
			string value = PlayerPrefs.GetString("google_analytics_clientid_pref_key");
			if (!string.IsNullOrEmpty(value))
			{
				SetString("google_analytics_clientid_pref_key", value);
			}
			Save();
		}
		callback(error);
	}

	private static IEnumerator RunSavePreferencesToRecnet()
	{
		CheckReady();
		savePreferencesToRecnetInProgress = true;
		bool dirtyFound = false;
		do
		{
			dirtyFound = false;
			string[] preferencesKeys = new string[preferences.Keys.Count];
			preferences.Keys.CopyTo(preferencesKeys, 0);
			saveRequested = false;
			string[] array = preferencesKeys;
			foreach (string key in array)
			{
				Preference original = preferences[key];
				if (!original.isDirty)
				{
					continue;
				}
				string originalValue = original.value;
				yield return Settings.StoreLocalPlayerSetting(new Setting
				{
					Key = key,
					Value = originalValue
				}, delegate(string e)
				{
					if (string.IsNullOrEmpty(e) && original.value == originalValue)
					{
						original.isDirty = false;
					}
				});
				dirtyFound = true;
			}
		}
		while (dirtyFound || saveRequested);
		savePreferencesToRecnetInProgress = false;
	}

	public static void Save()
	{
		saveRequested = true;
		if (!savePreferencesToRecnetInProgress)
		{
			SingletonMonoBehaviour<SettingsManager>.Instance.StartCoroutine(RunSavePreferencesToRecnet());
		}
	}

	public static void DeleteKey(string key)
	{
		CheckReady();
		SetPreference(key, null);
	}

	public static bool HasKey(string key)
	{
		CheckReady();
		return preferences.ContainsKey(key);
	}

	public static int GetInt(string key, int defaultValue)
	{
		if (HasKey(key))
		{
			return int.Parse(preferences[key].value);
		}
		return defaultValue;
	}

	public static float GetFloat(string key, float defaultValue)
	{
		if (HasKey(key))
		{
			return float.Parse(preferences[key].value);
		}
		return defaultValue;
	}

	public static string GetString(string key, string defaultValue = "")
	{
		if (HasKey(key))
		{
			return preferences[key].value;
		}
		return defaultValue;
	}

	public static void SetInt(string key, int value)
	{
		CheckReady();
		SetPreference(key, value.ToString());
	}

	public static void SetFloat(string key, float value)
	{
		CheckReady();
		SetPreference(key, value.ToString());
	}

	public static void SetString(string key, string value)
	{
		CheckReady();
		SetPreference(key, value);
	}

	private static void SetPreference(string key, string value, bool isDirty = true)
	{
		if (preferences.ContainsKey(key))
		{
			if (preferences[key].value != value)
			{
				preferences[key].value = value;
				preferences[key].isDirty = isDirty;
			}
		}
		else if (value != null)
		{
			preferences.Add(key, new Preference(value.ToString(), isDirty));
		}
	}
}
