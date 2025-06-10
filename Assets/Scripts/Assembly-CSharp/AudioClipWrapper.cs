using ExitGames.Client.Photon.Voice;
using UnityEngine;

internal class AudioClipWrapper : IAudioStream
{
	private AudioClip audioClip;

	private int readPos;

	private float startTime;

	private bool playing = true;

	public int SamplingRate
	{
		get
		{
			return audioClip.frequency;
		}
	}

	public bool Loop { get; set; }

	public AudioClipWrapper(AudioClip audioClip)
	{
		this.audioClip = audioClip;
		startTime = Time.time;
	}

	public bool GetData(float[] buffer)
	{
		if (!playing)
		{
			return false;
		}
		int num = (int)((Time.time - startTime) * (float)audioClip.frequency);
		int num2 = buffer.Length / audioClip.channels;
		if (num > readPos + num2)
		{
			audioClip.GetData(buffer, readPos);
			readPos += num2;
			if (readPos >= audioClip.samples)
			{
				if (Loop)
				{
					readPos = 0;
					startTime = Time.time;
				}
				else
				{
					playing = false;
				}
			}
			return true;
		}
		return false;
	}
}
