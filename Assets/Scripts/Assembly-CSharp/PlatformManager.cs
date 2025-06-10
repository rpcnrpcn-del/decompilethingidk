using System.Collections;
using UnityEngine;

public abstract class PlatformManager : MonoBehaviour
{
	public enum PlatformType
	{
		STEAM = 0,
		OCULUS = 1
	}

	public enum HardwareType
	{
		VIVE = 0,
		OCULUS = 1
	}

	public enum TrackingMode
	{
		DEFAULT = 0,
		ONE_EIGHTY_DEGREE = 1,
		THREE_SIXTY_DEGREE = 2
	}

	public delegate void InitializeCallback(string error);

	private TrackingMode currentTrackingMode;

	private string SAVED_TRACKING_MODE_KEY = "SAVED_TRACKING_MODE";

	public static PlatformManager Instance { get; private set; }

	public ulong PlatformProfileId { get; protected set; }

	public string PlatformProfileName { get; protected set; }

	public byte[] PlatformProfileImage { get; protected set; }

	public TrackingMode CurrentTrackingMode
	{
		get
		{
			if (currentTrackingMode == TrackingMode.DEFAULT)
			{
				return TrackingMode.THREE_SIXTY_DEGREE;
			}
			return currentTrackingMode;
		}
		set
		{
			currentTrackingMode = value;
			if (currentTrackingMode != SavedTrackingMode)
			{
				SavedTrackingMode = currentTrackingMode;
			}
		}
	}

	public abstract PlatformType CurrentPlatform { get; }

	public abstract HardwareType CurrentHardwareType { get; }

	public abstract bool IgnoreVRFocus { set; }

	public abstract bool HasVRFocus { get; }

	private TrackingMode SavedTrackingMode
	{
		get
		{
			return (TrackingMode)PlayerPrefs.GetInt(SAVED_TRACKING_MODE_KEY, 0);
		}
		set
		{
			PlayerPrefs.SetInt(SAVED_TRACKING_MODE_KEY, (int)value);
		}
	}

	protected void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("There can only be one PlatformManager!");
			Object.Destroy(base.gameObject);
		}
		else
		{
			Instance = this;
			CurrentTrackingMode = SavedTrackingMode;
		}
	}

	public abstract IEnumerator Initialize(InitializeCallback callback);

	public abstract void SetRichPresenceStatus(string statusText);
}
