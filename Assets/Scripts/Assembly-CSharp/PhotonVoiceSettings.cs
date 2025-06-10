using ExitGames.Client.Photon.Voice;
using POpusCodec.Enums;
using UnityEngine;

public class PhotonVoiceSettings : MonoBehaviour
{
	public bool AutoConnect = true;

	public bool AutoDisconnect;

	public bool AutoTransmit = true;

	public SamplingRate SamplingRate = SamplingRate.Sampling24000;

	public FrameDuration FrameDuration = FrameDuration.Frame20ms;

	public int Bitrate = 30000;

	public bool VoiceDetection;

	public float VoiceDetectionThreshold = 0.01f;

	public int PlayDelayMs = 200;

	public int DebugLostPercent;

	public bool DebugInfo;

	private static PhotonVoiceSettings instance;

	private static object instanceLock = new object();

	public static PhotonVoiceSettings Instance
	{
		get
		{
			if (instance == null)
			{
				instance = PhotonVoiceNetwork.instance.gameObject.AddComponent<PhotonVoiceSettings>();
			}
			return instance;
		}
	}

	private void Awake()
	{
		lock (instanceLock)
		{
			if (instance == null)
			{
				instance = this;
			}
			else
			{
				Debug.LogError("PUNVoice: Attempt to create multiple instances of PhotonVoiceSettings");
			}
		}
	}
}
