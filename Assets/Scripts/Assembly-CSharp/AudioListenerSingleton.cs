using UnityEngine;

[RequireComponent(typeof(AudioListener))]
public class AudioListenerSingleton : SingletonMonoBehaviour<AudioListenerSingleton>
{
	public delegate void AudioFilterDelegate(float[] samples, int channels);

	public event AudioFilterDelegate AudioFilter;

	private void Awake()
	{
		SingletonMonoBehaviour<AudioListenerSingleton>.Instance = this;
	}

	private void OnAudioFilterRead(float[] samples, int channels)
	{
		AudioFilterDelegate audioFilter = this.AudioFilter;
		if (audioFilter != null)
		{
			audioFilter(samples, channels);
		}
	}
}
