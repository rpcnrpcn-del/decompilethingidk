using Photon;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class PhotonVoiceSpeaker : Photon.MonoBehaviour
{
	private AudioStreamPlayer player;

	public float LastRecvTime { get; private set; }

	public bool IsPlaying
	{
		get
		{
			return player.IsPlaying;
		}
	}

	public int CurrentBufferLag
	{
		get
		{
			return player.CurrentBufferLag;
		}
	}

	public bool IsVoiceLinked
	{
		get
		{
			return player != null && player.IsStarted;
		}
	}

	public bool IsVoiceDetected { get; private set; }

	public float MaxAudioSample { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		player = new AudioStreamPlayer(GetComponent<AudioSource>(), "PUNVoice: PhotonVoiceSpeaker:", PhotonVoiceSettings.Instance.DebugInfo);
		PhotonVoiceNetwork.LinkSpeakerToRemoteVoice(this);
	}

	internal void OnVoiceLinked(int frequency, int channels, int frameSamplesPerChannel, int playDelayMs)
	{
		player.Start(frequency, channels, frameSamplesPerChannel, playDelayMs);
	}

	public void Pause()
	{
		player.Pause();
	}

	internal void OnVoiceUnlinked()
	{
		player.Stop();
	}

	private void Update()
	{
		player.Update();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PhotonVoiceNetwork.UnlinkSpeakerFromRemoteVoice(this);
		player.Stop();
	}

	private void OnApplicationQuit()
	{
		player.Stop();
	}

	internal void OnAudioFrame(float[] frame)
	{
		LastRecvTime = Time.time;
		player.OnAudioFrame(frame);
		float num = 0f;
		foreach (float f in frame)
		{
			num = Mathf.Max(Mathf.Abs(f), num);
		}
		IsVoiceDetected = num > PhotonVoiceSettings.Instance.VoiceDetectionThreshold;
		MaxAudioSample = num;
	}
}
