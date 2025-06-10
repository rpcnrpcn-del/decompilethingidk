using UnityEngine;

public class AudioStreamPlayer
{
	private const int maxPlayLagMs = 100;

	private int maxPlayLagSamples;

	private int playDelaySamples;

	private int frameSize;

	private int frameSamples;

	private int streamSamplePos;

	private int streamSamplePosAvg;

	private AudioSource source;

	private string logPrefix;

	private bool debugInfo;

	private int sourceTimeSamplesPrev;

	private int playLoopCount;

	public int CurrentBufferLag { get; private set; }

	public AudioSource AudioSource
	{
		get
		{
			return source;
		}
	}

	private int playSamplePos
	{
		get
		{
			return (source.clip != null) ? (playLoopCount * source.clip.samples + source.timeSamples) : 0;
		}
		set
		{
			if (source.clip != null)
			{
				int num = value % source.clip.samples;
				if (num < 0)
				{
					num += source.clip.samples;
				}
				source.timeSamples = num;
				playLoopCount = value / source.clip.samples;
				sourceTimeSamplesPrev = source.timeSamples;
			}
		}
	}

	public bool IsPlaying
	{
		get
		{
			return source.isPlaying;
		}
	}

	public bool IsStarted
	{
		get
		{
			return source.clip != null;
		}
	}

	public AudioStreamPlayer(AudioSource audioSource, string logPrefix, bool debugInfo)
	{
		source = audioSource;
		this.logPrefix = logPrefix;
		this.debugInfo = debugInfo;
	}

	internal void Start(int frequency, int channels, int frameSamples, int playDelayMs)
	{
		int lengthSamples = (100 + playDelayMs) * frequency / 1000 + frameSamples + frequency;
		this.frameSamples = frameSamples;
		frameSize = frameSamples * channels;
		maxPlayLagSamples = 100 * frequency / 1000 + this.frameSamples;
		playDelaySamples = playDelayMs * frequency / 1000 + this.frameSamples;
		CurrentBufferLag = playDelaySamples;
		streamSamplePosAvg = playDelaySamples;
		source.loop = true;
		source.clip = AudioClip.Create("AudioStreamPlayer", lengthSamples, channels, frequency, false);
		streamSamplePos = 0;
		playSamplePos = 0;
		source.Play();
		source.Pause();
	}

	public void Update()
	{
		if (!(source.clip != null))
		{
			return;
		}
		if (source.isPlaying)
		{
			if (source.timeSamples < sourceTimeSamplesPrev)
			{
				playLoopCount++;
			}
			sourceTimeSamplesPrev = source.timeSamples;
		}
		int num = playSamplePos;
		CurrentBufferLag = (CurrentBufferLag * 39 + (streamSamplePos - num)) / 40;
		streamSamplePosAvg = num + CurrentBufferLag;
		if (streamSamplePosAvg > streamSamplePos)
		{
			streamSamplePosAvg = streamSamplePos;
		}
		if (num < streamSamplePos - playDelaySamples && !source.isPlaying)
		{
			source.UnPause();
		}
		if (num > streamSamplePos - frameSamples && source.isPlaying)
		{
			if (debugInfo)
			{
				Debug.LogWarningFormat("{0} player overrun: {1}/{2}({3}) = {4}", logPrefix, num, streamSamplePos, streamSamplePosAvg, streamSamplePos - num);
			}
			source.Pause();
			num = (playSamplePos = streamSamplePos - playDelaySamples);
			CurrentBufferLag = playDelaySamples;
		}
		if (!source.isPlaying)
		{
			return;
		}
		int num3 = streamSamplePos - playDelaySamples - maxPlayLagSamples;
		if (num < num3)
		{
			if (debugInfo)
			{
				Debug.LogWarningFormat("{0} player underrun: {1}/{2}({3}) = {4}", logPrefix, num, streamSamplePos, streamSamplePosAvg, streamSamplePos - num);
			}
			num = streamSamplePos - playDelaySamples;
			playSamplePos = num;
			CurrentBufferLag = playDelaySamples;
		}
	}

	internal void OnAudioFrame(float[] frame)
	{
		if (frame.Length != 0)
		{
			if (frame.Length != frameSize)
			{
				Debug.LogErrorFormat("{0} Audio frames are not of  size: {1} != {2}", logPrefix, frame.Length, frameSize);
			}
			else
			{
				source.clip.SetData(frame, streamSamplePos % source.clip.samples);
				streamSamplePos += frame.Length / source.clip.channels;
			}
		}
	}

	public void Stop()
	{
		source.Stop();
		source.clip = null;
	}

	public void Pause()
	{
		if ((bool)source)
		{
			source.Pause();
		}
	}
}
