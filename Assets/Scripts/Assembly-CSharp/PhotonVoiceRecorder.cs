using System.Text;
using ExitGames.Client.Photon.Voice;
using Photon;
using UnityEngine;

[RequireComponent(typeof(PhotonVoiceSpeaker))]
[DisallowMultipleComponent]
public class PhotonVoiceRecorder : Photon.MonoBehaviour
{
	private LocalVoice voice = LocalVoice.Dummy;

	private string microphoneDevice;

	public AudioClip AudioClip;

	public bool LoopAudioClip = true;

	private string log0;

	private string log1;

	public VoiceDetector VoiceDetector
	{
		get
		{
			return (!base.photonView.isMine) ? null : voice.VoiceDetector;
		}
	}

	public string MicrophoneDevice
	{
		get
		{
			return microphoneDevice;
		}
		set
		{
			if (value != null && !Microphone.devices.Contains(value))
			{
				Debug.LogError("PUNVoice: " + value + " is not a valid microphone device");
				return;
			}
			microphoneDevice = value;
			if (voice != LocalVoice.Dummy && AudioClip == null)
			{
				PhotonVoiceSettings instance = PhotonVoiceSettings.Instance;
				Application.RequestUserAuthorization(UserAuthorization.Microphone);
				string text = ((MicrophoneDevice == null) ? PhotonVoiceNetwork.MicrophoneDevice : MicrophoneDevice);
				if (PhotonVoiceSettings.Instance.DebugInfo)
				{
					Debug.LogFormat("PUNVoice: Setting recorder's microphone device to {0}", text);
				}
				MicWrapper micWrapper = new MicWrapper(text, (int)instance.SamplingRate);
				bool debugEchoMode = PhotonVoiceNetwork.Client.DebugEchoMode;
				PhotonVoiceNetwork.Client.DebugEchoMode = false;
				VoiceInfo voiceInfo = new VoiceInfo((int)instance.SamplingRate, micWrapper.Channels, (int)instance.FrameDuration, instance.Bitrate, base.photonView.viewID);
				PhotonVoiceNetwork.RemoveLocalVoice(voice);
				LocalVoice localVoice = voice;
				voice = PhotonVoiceNetwork.CreateLocalVoice(micWrapper, voiceInfo);
				voice.AudioGroup = localVoice.AudioGroup;
				voice.Transmit = localVoice.Transmit;
				voice.VoiceDetector.On = localVoice.VoiceDetector.On;
				voice.VoiceDetector.Threshold = localVoice.VoiceDetector.Threshold;
				PhotonVoiceNetwork.Client.DebugEchoMode = debugEchoMode;
			}
		}
	}

	public byte AudioGroup
	{
		get
		{
			return voice.AudioGroup;
		}
		set
		{
			voice.AudioGroup = value;
		}
	}

	public bool IsTransmitting
	{
		get
		{
			return voice.IsTransmitting;
		}
	}

	public LevelMeter LevelMeter
	{
		get
		{
			return voice.LevelMeter;
		}
	}

	public bool Transmit
	{
		get
		{
			return voice.Transmit;
		}
		set
		{
			voice.Transmit = value;
		}
	}

	public bool Detect
	{
		get
		{
			return voice.VoiceDetector.On;
		}
		set
		{
			voice.VoiceDetector.On = value;
		}
	}

	public bool VoiceDetectorCalibrating
	{
		get
		{
			return voice.VoiceDetectorCalibrating;
		}
	}

	private void Start()
	{
		if (Microphone.devices.Length < 1)
		{
			return;
		}
		if (base.photonView.isMine)
		{
			PhotonVoiceSettings instance = PhotonVoiceSettings.Instance;
			if (MicrophoneDevice == null && PhotonVoiceNetwork.MicrophoneDevice == null)
			{
				Debug.LogWarning("PUNVoice : PhotonVoiceRecorder is disabled since we do not have a microphone.");
				base.enabled = false;
				return;
			}
			Application.RequestUserAuthorization(UserAuthorization.Microphone);
			int num = 0;
			IAudioStream audioStream;
			if (AudioClip == null)
			{
				string text = ((MicrophoneDevice == null) ? PhotonVoiceNetwork.MicrophoneDevice : MicrophoneDevice);
				if (PhotonVoiceSettings.Instance.DebugInfo)
				{
					Debug.LogFormat("PUNVoice: Setting recorder's microphone device to {0}", text);
				}
				MicWrapper micWrapper = new MicWrapper(text, (int)instance.SamplingRate);
				num = micWrapper.Channels;
				audioStream = micWrapper;
			}
			else
			{
				audioStream = new AudioClipWrapper(AudioClip);
				num = AudioClip.channels;
				if (LoopAudioClip)
				{
					((AudioClipWrapper)audioStream).Loop = true;
				}
			}
			VoiceInfo voiceInfo = new VoiceInfo((int)instance.SamplingRate, num, (int)instance.FrameDuration, instance.Bitrate, base.photonView.viewID);
			voice = PhotonVoiceNetwork.CreateLocalVoice(audioStream, voiceInfo);
			VoiceDetector.On = PhotonVoiceSettings.Instance.VoiceDetection;
			VoiceDetector.Threshold = PhotonVoiceSettings.Instance.VoiceDetectionThreshold;
			if (voice != LocalVoice.Dummy)
			{
				voice.Transmit = PhotonVoiceSettings.Instance.AutoTransmit && SingletonMonoBehaviour<SettingsManager>.Instance.VoiceChat == VoiceChat.AlwaysOn;
			}
			else if (PhotonVoiceSettings.Instance.AutoTransmit)
			{
				Debug.LogWarning("PUNVoice: Cannot Transmit.");
			}
		}
		else
		{
			base.enabled = false;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (voice != LocalVoice.Dummy)
		{
			PhotonVoiceNetwork.RemoveLocalVoice(voice);
		}
	}

	private void OnEnable()
	{
		Application.RequestUserAuthorization(UserAuthorization.Microphone);
	}

	private void OnJoinedVoiceRoom()
	{
	}

	public void VoiceDetectorCalibrate(int durationMs)
	{
		if (base.photonView.isMine)
		{
			voice.VoiceDetectorCalibrate(durationMs);
		}
	}

	private string tostr<T>(T[] x, int lim = 10)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < ((x.Length >= lim) ? lim : x.Length); i++)
		{
			stringBuilder.Append("-");
			stringBuilder.Append(x[i]);
		}
		return stringBuilder.ToString();
	}

	public string ToStringFull()
	{
		int minFreq = 0;
		int maxFreq = 0;
		Microphone.GetDeviceCaps(MicrophoneDevice, out minFreq, out maxFreq);
		return string.Format("Mic '{0}': {1}..{2} Hz", MicrophoneDevice, minFreq, maxFreq);
	}
}
