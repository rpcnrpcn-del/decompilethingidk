using ExitGames.Client.Photon.Voice;
using UnityEngine;

internal class MicWrapper : IAudioStream
{
	private AudioClip mic;

	private string device;

	private int micPrevPos;

	private int micLoopCnt;

	private int readAbsPos;

	public int SamplingRate
	{
		get
		{
			return mic.frequency;
		}
	}

	public int Channels
	{
		get
		{
			return mic.channels;
		}
	}

	public MicWrapper(string device, int suggestedFrequency)
	{
		if (Microphone.devices.Length >= 1)
		{
			this.device = device;
			int minFreq;
			int maxFreq;
			Microphone.GetDeviceCaps(device, out minFreq, out maxFreq);
			int suggestedFrequency2 = suggestedFrequency;
			if (suggestedFrequency < minFreq || (maxFreq != 0 && suggestedFrequency > maxFreq))
			{
				Debug.LogWarningFormat("PUNVoice: MicWrapper does not support suggested frequency {0} (min: {1}, max: {2}). Setting to {2}", suggestedFrequency, minFreq, maxFreq);
				suggestedFrequency2 = maxFreq;
			}
			mic = SharedMicrophone.GetMicrophone(device, suggestedFrequency2);
		}
	}

	public bool GetData(float[] buffer)
	{
		int position = Microphone.GetPosition(device);
		if (position < micPrevPos)
		{
			micLoopCnt++;
		}
		micPrevPos = position;
		int num = micLoopCnt * mic.samples + position;
		int num2 = buffer.Length / mic.channels;
		int num3 = readAbsPos + num2;
		if (num3 < num)
		{
			mic.GetData(buffer, readAbsPos % mic.samples);
			readAbsPos = num3;
			return true;
		}
		return false;
	}
}
