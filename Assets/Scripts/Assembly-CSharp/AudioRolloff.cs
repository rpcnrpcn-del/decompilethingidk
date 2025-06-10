using System;
using UnityEngine;

[Serializable]
public class AudioRolloff
{
	public bool IsActive;

	public AudioRolloffMode rolloffMode;

	public float minDistance = 2f;

	public float maxDistance = 100f;

	[Tooltip("Only used if the rolloffMode is custom, this is the red volumetric curve.")]
	public AnimationCurve customCurve;

	public AudioRolloff(AudioSource audioSource)
	{
		Get(audioSource);
		IsActive = true;
	}

	public void Get(AudioSource audioSource)
	{
		rolloffMode = audioSource.rolloffMode;
		minDistance = audioSource.minDistance;
		maxDistance = audioSource.maxDistance;
		if (rolloffMode == AudioRolloffMode.Custom)
		{
			customCurve = audioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
		}
	}

	public void TrySet(AudioSource audioSource)
	{
		if (IsActive)
		{
			audioSource.rolloffMode = rolloffMode;
			audioSource.minDistance = minDistance;
			audioSource.maxDistance = maxDistance;
			if (rolloffMode == AudioRolloffMode.Custom)
			{
				audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, customCurve);
			}
		}
	}
}
