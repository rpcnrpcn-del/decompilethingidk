using System;
using System.IO;
using UTJ;
using UnityEngine;
using UnityEngine.VR;

public class MP4Capture : VideoCapture
{
	[SerializeField]
	private Material blitMaterial;

	private fcAPI.fcMP4Context mp4Context;

	private fcAPI.fcStream mp4OutputStream;

	private fcAPI.fcMP4Config mp4Config = fcAPI.fcMP4Config.default_value;

	private int videoId;

	private float captureStartTime;

	private string latestFilename;

	private RenderTexture scratchBuffer;

	private AudioSource localMicCaptureSource;

	private float[] micAudio;

	private float[] audioMixBuffer;

	private AudioListenerSingleton audioListener;

	public override string ExportDirectory
	{
		get
		{
			return base.ExportDirectory + "Videos\\";
		}
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		if (SingletonMonoBehaviour<SettingsManager>.Instance.H264Plugin)
		{
			audioListener = SingletonMonoBehaviour<AudioListenerSingleton>.Instance;
			if (audioListener != null)
			{
				audioListener.AudioFilter += OnMainAudioFilter;
			}
			localMicCaptureSource = base.gameObject.AddComponent<AudioSource>();
			localMicCaptureSource.clip = SharedMicrophone.GetMicrophone(PhotonVoiceNetwork.MicrophoneDevice, (int)PhotonVoiceSettings.Instance.SamplingRate);
			localMicCaptureSource.loop = true;
			fcAPI.fcMP4SetFAACPackagePath(Application.streamingAssetsPath + "/UTJ/FrameCapturer/FAAC_SelfBuild.zip");
			fcAPI.fcMP4SetFAACUnzipPath(Application.dataPath);
			fcAPI.fcSetModulePath(Application.temporaryCachePath);
			fcAPI.fcMP4DownloadCodecBegin();
		}
		else
		{
			base.enabled = false;
		}
	}

	private void OnDestroy()
	{
		if (audioListener != null)
		{
			AttachAudioListener(false);
			audioListener.AudioFilter -= OnMainAudioFilter;
		}
	}

	public override void StartCapture()
	{
		if (!SingletonMonoBehaviour<SettingsManager>.Instance.H264Plugin || !base.enabled)
		{
			MenuNotification.PlayNext("Video camera is disabled! (check SETTINGS)", 1f, null, false, base.transform.position + Vector3.up * 0.1f, 0.1f);
			return;
		}
		VRSettings.renderScale = 0.5f;
		if (!base.IsCapturing)
		{
			base.IsCapturing = true;
			captureStartTime = Time.unscaledTime;
			InitializeContext();
			InitializeScratchBuffer();
			AttachAudioListener(true);
			localMicCaptureSource.Play();
			FireCaptureStartedEvent();
		}
	}

	public override float UpdateFrame(RenderTexture frame)
	{
		float result = 0f;
		if (base.IsCapturing)
		{
			Graphics.Blit(frame, scratchBuffer, blitMaterial);
			videoId = fcAPI.fcMP4AddVideoFrameTexture(mp4Context, scratchBuffer, Time.unscaledTime, videoId);
			GL.IssuePluginEvent(fcAPI.fcGetRenderEventFunc(), videoId);
			result = (Time.unscaledTime - captureStartTime) / maxDuration;
		}
		return result;
	}

	public override void FinishCapture()
	{
		if (base.IsCapturing)
		{
			base.IsCapturing = false;
			ReleaseContext();
			localMicCaptureSource.Stop();
			AttachAudioListener(false);
			FireExportStartedEvent();
			FireExportFinishedEvent(latestFilename);
		}
		VRSettings.renderScale = 1f;
	}

	private void AttachAudioListener(bool attachToCamera)
	{
		if (audioListener != null)
		{
			audioListener.transform.parent = ((!attachToCamera) ? Camera.main.transform : RecordingCamera.transform);
			audioListener.transform.localPosition = Vector3.zero;
			audioListener.transform.localRotation = Quaternion.identity;
			audioListener.transform.localScale = Vector3.one;
		}
	}

	private void OnMainAudioFilter(float[] samples, int channels)
	{
		if (base.IsCapturing && channels == mp4Config.audio_num_channels)
		{
			if (audioMixBuffer == null || audioMixBuffer.Length != samples.Length)
			{
				audioMixBuffer = new float[samples.Length];
			}
			if (micAudio == null || micAudio.Length != samples.Length)
			{
				micAudio = new float[samples.Length];
			}
			for (int i = 0; i < samples.Length; i++)
			{
				audioMixBuffer[i] = samples[i] + micAudio[i] / 2f;
			}
			fcAPI.fcMP4AddAudioFrame(mp4Context, audioMixBuffer, audioMixBuffer.Length);
		}
	}

	private void OnAudioFilterRead(float[] samples, int channels)
	{
		if (micAudio == null || micAudio.Length != samples.Length)
		{
			micAudio = new float[samples.Length];
		}
		Array.Copy(samples, micAudio, samples.Length);
		Array.Clear(samples, 0, samples.Length);
	}

	private void InitializeContext()
	{
		mp4Config = fcAPI.fcMP4Config.default_value;
		mp4Config.audio = true;
		mp4Config.audio_bitrate = 64000;
		mp4Config.audio_sampling_rate = AudioSettings.outputSampleRate;
		mp4Config.audio_num_channels = fcAPI.fcGetNumAudioChannels();
		mp4Config.video = true;
		mp4Config.video_height = RecordingCamera.targetTexture.height;
		mp4Config.video_width = RecordingCamera.targetTexture.width;
		mp4Config.video_max_framerate = frameRate;
		mp4Config.video_bitrate = 8192000;
		mp4Context = fcAPI.fcMP4CreateContext(ref mp4Config);
		latestFilename = VideoCapture.CurrentFileName;
		mp4OutputStream = fcAPI.fcCreateFileStream(GetFullFilename(latestFilename));
		fcAPI.fcMP4AddOutputStream(mp4Context, mp4OutputStream);
	}

	private void InitializeScratchBuffer()
	{
		if (scratchBuffer != null)
		{
			if (scratchBuffer.IsCreated() && scratchBuffer.width == mp4Config.video_width && scratchBuffer.height == mp4Config.video_height)
			{
				return;
			}
			ReleaseScratchBuffer();
		}
		scratchBuffer = new RenderTexture(mp4Config.video_width, mp4Config.video_height, 0, RenderTextureFormat.ARGB32);
		scratchBuffer.wrapMode = TextureWrapMode.Repeat;
		scratchBuffer.Create();
	}

	private void ReleaseContext()
	{
		fcAPI.fcGuard(delegate
		{
			fcAPI.fcEraseDeferredCall(videoId);
			videoId = 0;
			if (mp4Context.ptr != IntPtr.Zero)
			{
				fcAPI.fcMP4DestroyContext(mp4Context);
				mp4Context.ptr = IntPtr.Zero;
			}
			if (mp4OutputStream.ptr != IntPtr.Zero)
			{
				fcAPI.fcDestroyStream(mp4OutputStream);
				mp4OutputStream.ptr = IntPtr.Zero;
			}
		});
	}

	private void ReleaseScratchBuffer()
	{
		if (scratchBuffer != null)
		{
			scratchBuffer.Release();
			scratchBuffer = null;
		}
	}

	private string GetFullFilename(string filename)
	{
		return Path.Combine(ExportDirectory, filename + ".mp4");
	}
}
