using System.Collections.Generic;
using UnityEngine;

public class GoogleAnalyticsSettings : ScriptableObject
{
	public static string VERSION_NUMBER = "3.0.2";

	[SerializeField]
	public List<GAProfile> accounts = new List<GAProfile>();

	[SerializeField]
	public List<GAPlatfromBound> platfromBounds = new List<GAPlatfromBound>();

	public bool showAdditionalParams;

	public bool showAdvancedParams;

	public bool showCParams;

	public bool showAccounts = true;

	public bool showPlatfroms;

	public bool showTestingMode;

	public string AppName = "My App";

	public string AppVersion = "0.0.1";

	public bool UseHTTPS;

	public bool StringEscaping = true;

	public bool EditorAnalytics = true;

	public bool IsDisabled;

	public bool IsTestingModeEnabled;

	public int TestingModeAccIndex;

	public bool IsRequetsCachingEnabled = true;

	public bool IsQueueTimeEnabled = true;

	public bool AutoLevelTracking = true;

	public string LevelPrefix = "Level_";

	public string LevelPostfix = string.Empty;

	public bool AutoAppQuitTracking = true;

	public bool AutoCampaignTracking;

	public bool AutoExceptionTracking = true;

	public bool AutoAppResumeTracking = true;

	public bool SubmitSystemInfoOnFirstLaunch = true;

	public bool UsePlayerSettingsForAppInfo = true;

	private const string AnalyticsSettingsAssetName = "GoogleAnalyticsSettings";

	private const string AnalyticsSettingsAssetExtension = ".asset";

	private static GoogleAnalyticsSettings instance;

	public static GoogleAnalyticsSettings Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Resources.Load("GoogleAnalyticsSettings") as GoogleAnalyticsSettings;
				if (instance == null)
				{
					instance = ScriptableObject.CreateInstance<GoogleAnalyticsSettings>();
				}
			}
			return instance;
		}
	}

	public void UpdateVersionAndName()
	{
	}

	public void AddProfile(GAProfile p)
	{
		accounts.Add(p);
	}

	public void RemoveProfile(GAProfile p)
	{
		accounts.Remove(p);
	}

	public void SetProfileIndexForPlatfrom(RuntimePlatform platfrom, int index, bool IsTesting)
	{
		foreach (GAPlatfromBound platfromBound in platfromBounds)
		{
			if (platfromBound.platfrom.Equals(platfrom))
			{
				if (IsTesting)
				{
					platfromBound.profileIndexTestMode = index;
				}
				else
				{
					platfromBound.profileIndex = index;
				}
				return;
			}
		}
		GAPlatfromBound gAPlatfromBound = new GAPlatfromBound();
		gAPlatfromBound.platfrom = platfrom;
		gAPlatfromBound.profileIndex = 0;
		gAPlatfromBound.profileIndexTestMode = 0;
		if (IsTesting)
		{
			gAPlatfromBound.profileIndexTestMode = index;
		}
		else
		{
			gAPlatfromBound.profileIndex = index;
		}
		platfromBounds.Add(gAPlatfromBound);
	}

	public int GetProfileIndexForPlatfrom(RuntimePlatform platfrom, bool IsTestMode)
	{
		foreach (GAPlatfromBound platfromBound in platfromBounds)
		{
			if (platfromBound.platfrom.Equals(platfrom))
			{
				int num = platfromBound.profileIndex;
				if (IsTestMode)
				{
					num = platfromBound.profileIndexTestMode;
				}
				if (num < accounts.Count)
				{
					return num;
				}
				return 0;
			}
		}
		return 0;
	}

	public string[] GetProfileNames()
	{
		List<string> list = new List<string>();
		foreach (GAProfile account in accounts)
		{
			list.Add(account.Name);
		}
		return list.ToArray();
	}

	public int GetProfileIndex(GAProfile p)
	{
		int num = 0;
		string[] profileNames = GetProfileNames();
		string[] array = profileNames;
		foreach (string text in array)
		{
			if (text.Equals(p.Name))
			{
				return num;
			}
			num++;
		}
		return 0;
	}

	public GAProfile GetCurentProfile()
	{
		bool isDeveloper = SessionManager.IsDeveloper;
		return GetPrfileForPlatfrom(Application.platform, isDeveloper);
	}

	public GAProfile GetPrfileForPlatfrom(RuntimePlatform platfrom, bool IsTestMode)
	{
		if (accounts.Count == 0)
		{
			return new GAProfile();
		}
		return accounts[GetProfileIndexForPlatfrom(platfrom, IsTestMode)];
	}
}
