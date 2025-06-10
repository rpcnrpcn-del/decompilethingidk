using System;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : SingletonMonoBehaviour<SettingsManager>
{
	[SerializeField]
	private TeleportBufferSize[] teleportBufferSizes;

	private Dictionary<TeleportBuffer, float> teleportBufferSizeMap;

	private static string H264_PLUGIN_PREF = "H.264 plugin";

	private static string SHOW_NAMES_PREF = "ShowNames";

	private static string PERSONAL_BUBBLE_PREF = "PersonalBubble";

	private static string VOICE_CHAT_PREF = "VoiceChat";

	private static string TELEPORT_BUFFER_PREF = "TeleportBuffer";

	private static string ROTATE_IN_PLACE_ENABLED = "ROTATE_IN_PLACE_ENABLED";

	private static string ROTATION_INCREMENT = "ROTATION_INCREMENT";

	private static string CONTINUOUS_ROTATION_MODE = "CONTINUOUS_ROTATION_MODE";

	private static string MOTION_TELEPORT_ENABLED = "VIGNETTED_TELEPORT_ENABLED";

	private static string VOICE_FILTER_PREF = "VoiceFilter";

	private static string QUALITY_SETTINGS_PREF = "QualitySettings";

	private static string SHOW_ROOM_CENTER_PREF = "ShowRoomCenter";

	private readonly string TOTAL_SESSION_COUNT_PREF = "PlayerSessionCount";

	private static string MODERATOR_BLOCKED_TIME = "MOD_BLOCKED_TIME";

	private static string MODERATOR_BLOCKED_DURATION = "MOD_BLOCKED_DURATION";

	private Dictionary<string, int> propertyMap = new Dictionary<string, int>();

	public bool H264Plugin
	{
		get
		{
			return GetProperty(H264_PLUGIN_PREF, true);
		}
		set
		{
			SetProperty(H264_PLUGIN_PREF, value);
		}
	}

	public bool ShowNames
	{
		get
		{
			return GetProperty(SHOW_NAMES_PREF, true);
		}
		set
		{
			SetProperty(SHOW_NAMES_PREF, value);
		}
	}

	public bool PersonalBubble
	{
		get
		{
			return GetProperty(PERSONAL_BUBBLE_PREF, true);
		}
		set
		{
			SetProperty(PERSONAL_BUBBLE_PREF, value);
		}
	}

	public VoiceChat VoiceChat
	{
		get
		{
			return (VoiceChat)GetProperty(VOICE_CHAT_PREF, 2);
		}
		set
		{
			SetProperty(VOICE_CHAT_PREF, (int)value, true, value.ToString());
			if (Player.LocalPlayer != null)
			{
				Player.LocalPlayer.Mute = VoiceChat != VoiceChat.AlwaysOn;
			}
		}
	}

	public TeleportBuffer TeleportBuffer
	{
		get
		{
			return (TeleportBuffer)GetProperty(TELEPORT_BUFFER_PREF, 1);
		}
		set
		{
			SetProperty(TELEPORT_BUFFER_PREF, (int)value, true, value.ToString());
			if (Player.LocalPlayer != null)
			{
				Player.LocalPlayer.PlayerLocomotion.TeleportBuffer = TeleportBuffer;
			}
		}
	}

	public bool RotateInPlaceEnabled
	{
		get
		{
			return GetProperty(ROTATE_IN_PLACE_ENABLED, true);
		}
		set
		{
			SetProperty(ROTATE_IN_PLACE_ENABLED, value);
			if (Player.LocalPlayer != null)
			{
				Player.LocalPlayer.PlayerLocomotion.RotateInPlaceEnabled = RotateInPlaceEnabled;
			}
		}
	}

	public PlayerLocomotion.RotationIncrement RotationIncrement
	{
		get
		{
			return (PlayerLocomotion.RotationIncrement)GetProperty(ROTATION_INCREMENT, 1);
		}
		set
		{
			SetProperty(ROTATION_INCREMENT, (int)value, true, string.Empty);
			if (Player.LocalPlayer != null)
			{
				Player.LocalPlayer.PlayerLocomotion.RotateInPlaceIncrement = RotationIncrement;
			}
		}
	}

	public ContinuousRotationMode ContinuousRotationMode
	{
		get
		{
			return (ContinuousRotationMode)GetProperty(CONTINUOUS_ROTATION_MODE);
		}
		set
		{
			SetProperty(CONTINUOUS_ROTATION_MODE, (int)value, true, string.Empty);
			if (Player.LocalPlayer != null)
			{
				Player.LocalPlayer.PlayerLocomotion.ContinuousRotationMode = ContinuousRotationMode;
			}
		}
	}

	public bool MotionTeleportEnabled
	{
		get
		{
			return GetProperty(MOTION_TELEPORT_ENABLED, false);
		}
		set
		{
			SetProperty(MOTION_TELEPORT_ENABLED, value);
			if (Player.LocalPlayer != null)
			{
				Player.LocalPlayer.PlayerLocomotion.MotionTeleportEnabled = MotionTeleportEnabled;
			}
		}
	}

	public AudioManager.VOIPFilter VoiceFilter
	{
		get
		{
			return (AudioManager.VOIPFilter)GetProperty(VOICE_FILTER_PREF, 2);
		}
		set
		{
			SetProperty(VOICE_FILTER_PREF, (int)value, true, value.ToString());
			if (Player.LocalPlayer != null)
			{
				Player.LocalPlayer.VOIPFilter = VoiceFilter;
			}
		}
	}

	public RecRoomQualitySetting QualitySetting
	{
		get
		{
			return (RecRoomQualitySetting)GetProperty(QUALITY_SETTINGS_PREF, 3);
		}
		set
		{
			if (QualitySetting != value)
			{
				SetProperty(QUALITY_SETTINGS_PREF, (int)value, true, value.ToString());
				ApplyQualitySetting();
			}
		}
	}

	public bool ShowRoomCenter
	{
		get
		{
			return GetProperty(SHOW_ROOM_CENTER_PREF, false);
		}
		set
		{
			SetProperty(SHOW_ROOM_CENTER_PREF, value);
		}
	}

	public int TotalSessionCount
	{
		get
		{
			return GetProperty(TOTAL_SESSION_COUNT_PREF);
		}
		private set
		{
			SetProperty(TOTAL_SESSION_COUNT_PREF, value, true, string.Empty);
		}
	}

	private void ApplyQualitySetting()
	{
		bool flag = false;
		string text = QualitySetting.ToString();
		int index = QualitySettings.names.Length - 1;
		for (int i = 0; i < QualitySettings.names.Length; i++)
		{
			if (text == QualitySettings.names[i])
			{
				index = i;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Debug.LogError("Could not find the Quality setting named " + text);
		}
		else
		{
			QualitySettings.SetQualityLevel(index, true);
		}
	}

	private void Awake()
	{
		SingletonMonoBehaviour<SettingsManager>.Instance = this;
		teleportBufferSizeMap = new Dictionary<TeleportBuffer, float>();
		for (int i = 0; i < teleportBufferSizes.Length; i++)
		{
			if (!teleportBufferSizeMap.ContainsKey(teleportBufferSizes[i].buffer))
			{
				teleportBufferSizeMap.Add(teleportBufferSizes[i].buffer, teleportBufferSizes[i].distance);
			}
			else
			{
				Debug.LogError("TeleportBufferSize can not contain duplicate values for " + teleportBufferSizes[i].buffer);
			}
		}
		if (teleportBufferSizes.Length != Enum.GetValues(typeof(TeleportBuffer)).Length)
		{
			Debug.LogError("TeleportBufferSizes does not have correct number of values.");
		}
	}

	public void Initialize()
	{
		Debug.Log("Quality setting : " + QualitySetting);
		ApplyQualitySetting();
		int num = ++TotalSessionCount;
		AnalyticsHelper.TotalSessionCount(num);
		AnalyticsHelper.SettingsInitialized(QualitySetting, num);
	}

	public float GetTeleportBufferSize(TeleportBuffer buffer)
	{
		return teleportBufferSizeMap[buffer];
	}

	public void SetUserBlocked(int duration)
	{
		RecroomPrefs.SetString(MODERATOR_BLOCKED_TIME, DateTime.Now.ToString());
		RecroomPrefs.SetInt(MODERATOR_BLOCKED_DURATION, duration);
		RecroomPrefs.Save();
	}

	public int GetUserBlockedDurationSecondsRemaning()
	{
		if (RecroomPrefs.HasKey(MODERATOR_BLOCKED_TIME) && RecroomPrefs.HasKey(MODERATOR_BLOCKED_DURATION))
		{
			int num = RecroomPrefs.GetInt(MODERATOR_BLOCKED_DURATION, 0);
			string s = RecroomPrefs.GetString(MODERATOR_BLOCKED_TIME, string.Empty);
			DateTime result;
			if (num > 0 && DateTime.TryParse(s, out result))
			{
				int num2 = Convert.ToInt32(DateTime.Now.Subtract(result).TotalSeconds);
				return num - num2;
			}
		}
		return 0;
	}

	private int GetProperty(string propertyName, int defaultValue = 0)
	{
		if (!propertyMap.ContainsKey(propertyName))
		{
			propertyMap.Add(propertyName, RecroomPrefs.GetInt(propertyName, defaultValue));
		}
		return propertyMap[propertyName];
	}

	private bool GetProperty(string propertyName, bool defaultValue)
	{
		return GetProperty(propertyName, defaultValue ? 1 : 0) > 0;
	}

	private void SetProperty(string propertyName, bool value, bool sendAnalytics = true)
	{
		SetProperty(propertyName, value ? 1 : 0, sendAnalytics, (!value) ? "false" : "true");
	}

	private void SetProperty(string propertyName, int value, bool sendAnalytics = true, string analyticsLabel = "")
	{
		int property = GetProperty(propertyName);
		if (property != value)
		{
			propertyMap[propertyName] = value;
			RecroomPrefs.SetInt(propertyName, value);
			RecroomPrefs.Save();
			if (sendAnalytics)
			{
				AnalyticsHelper.UserSettings(propertyName, analyticsLabel, value);
			}
		}
	}
}
