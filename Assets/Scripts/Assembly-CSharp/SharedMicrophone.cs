using System.Collections.Generic;
using UnityEngine;

public static class SharedMicrophone
{
	private static Dictionary<string, AudioClip> microphones = new Dictionary<string, AudioClip>();

	private static AudioClip nullMicrophone = null;

	public static AudioClip GetMicrophone(string device, int suggestedFrequency)
	{
		AudioClip value = null;
		if (device == null)
		{
			if (nullMicrophone == null)
			{
				nullMicrophone = CreateMicrophone(device, suggestedFrequency);
			}
			value = nullMicrophone;
		}
		else if (!microphones.TryGetValue(device, out value))
		{
			value = CreateMicrophone(device, suggestedFrequency);
			microphones.Add(device, value);
		}
		return value;
	}

	private static AudioClip CreateMicrophone(string device, int suggestedFrequency)
	{
		int minFreq;
		int maxFreq;
		Microphone.GetDeviceCaps(device, out minFreq, out maxFreq);
		int frequency = suggestedFrequency;
		if (suggestedFrequency < minFreq || (maxFreq != 0 && suggestedFrequency > maxFreq))
		{
			frequency = maxFreq;
		}
		return Microphone.Start(device, true, 1, frequency);
	}
}
