using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class FxManager
{
	[SerializeField]
	protected AudioClipAssets[] customAudioClips;

	[SerializeField]
	protected VFXAssets[] customVFXs;

	protected Dictionary<FxType, AudioClips> audioClipDictionary = new Dictionary<FxType, AudioClips>();

	protected Dictionary<FxType, VFXAssets> vfxDictionary = new Dictionary<FxType, VFXAssets>();

	protected Dictionary<FxType, SFXAudioSource> loopingAudioDictionary = new Dictionary<FxType, SFXAudioSource>();

	private Dictionary<FxType, float> lastFxPlayedTime = new Dictionary<FxType, float>();

	public virtual void OnAwake()
	{
		if (customAudioClips != null)
		{
			for (int i = 0; i < customAudioClips.Length; i++)
			{
				AudioClips audioClips = new AudioClips();
				audioClips.SoundEffect = customAudioClips[i].SoundEffectClip;
				audioClips.VoiceOver = customAudioClips[i].VoiceOverClip;
				AudioClips value = audioClips;
				audioClipDictionary.Add(customAudioClips[i].AudioType, value);
			}
		}
		FxType[] array = (FxType[])Enum.GetValues(typeof(FxType));
		for (int j = 0; j < array.Length; j++)
		{
			if (!audioClipDictionary.ContainsKey(array[j]))
			{
				AudioClips audioClips2 = null;
				if (DefaultAudioSettings.TryLoadDefaultAudioClips(array[j], out audioClips2))
				{
					audioClipDictionary.Add(array[j], audioClips2);
				}
			}
		}
		if (customVFXs != null)
		{
			for (int k = 0; k < customVFXs.Length; k++)
			{
				vfxDictionary.Add(customVFXs[k].VfxType, customVFXs[k]);
			}
		}
		for (int l = 0; l < array.Length; l++)
		{
			if (!vfxDictionary.ContainsKey(array[l]))
			{
				VFXAssets vfxAsset = null;
				if (DefaultVFXSettings.TryLoadDefaultVFX(array[l], out vfxAsset))
				{
					vfxDictionary.Add(array[l], vfxAsset);
				}
			}
		}
	}

	public virtual void OnDestroy()
	{
		foreach (SFXAudioSource value in loopingAudioDictionary.Values)
		{
			AudioManager.StopLoopingSFX(value);
		}
		loopingAudioDictionary.Clear();
	}

	public void PlayFX(FxType fxType, Vector3? position = null, Quaternion? rotation = null, bool forceSFX2D = false)
	{
		if (position.HasValue)
		{
			PlayVFX(fxType, position.Value, rotation);
		}
		PlaySFX(fxType, (!forceSFX2D) ? position : ((Vector3?)null));
	}

	public void PlayRateLimitedFX(FxType fxType, float minInterval)
	{
		float value = float.MinValue;
		lastFxPlayedTime.TryGetValue(fxType, out value);
		float fixedTime = Time.fixedTime;
		if (fixedTime - value >= minInterval)
		{
			PlayFX(fxType);
			lastFxPlayedTime[fxType] = fixedTime;
		}
	}

	public void PlayVFX(FxType vfxType, Vector3 position, Quaternion? rotation)
	{
		VFXAssets value = null;
		if (!vfxDictionary.TryGetValue(vfxType, out value) || value == null || !(value.ParticlePrefab != null))
		{
			return;
		}
		PooledParticle pooledParticle = ObjectPool.Instance.Acquire(value.ParticlePrefab);
		if (pooledParticle != null)
		{
			pooledParticle.transform.position = position;
			if (rotation.HasValue)
			{
				pooledParticle.transform.rotation = rotation.Value;
			}
			pooledParticle.Play();
		}
	}

	public void PlaySFX(FxType audioType, Vector3? position = null)
	{
		AudioClips value = null;
		if (!audioClipDictionary.TryGetValue(audioType, out value))
		{
			return;
		}
		if (value.SoundEffect != null)
		{
			if (position.HasValue)
			{
				AudioManager.Play3DSFX(value.SoundEffect, position.Value);
			}
			else
			{
				AudioManager.Play2DSFX(value.SoundEffect);
			}
		}
		if (value.VoiceOver != null)
		{
			AudioManager.Play2DSFX(value.VoiceOver);
		}
	}

	public void StartLoopingSFX(FxType audioType, Transform followTarget = null)
	{
		if (!loopingAudioDictionary.ContainsKey(audioType))
		{
			AudioClips value = null;
			if (audioClipDictionary.TryGetValue(audioType, out value) && value.SoundEffect != null)
			{
				SFXAudioSource sFXAudioSource = null;
				sFXAudioSource = ((!(followTarget != null)) ? AudioManager.StartLooping2DSFX(value.SoundEffect) : AudioManager.StartLooping3DSFX(value.SoundEffect, followTarget));
				loopingAudioDictionary.Add(audioType, sFXAudioSource);
			}
		}
	}

	public void StopLoopingSFX(FxType audioType)
	{
		SFXAudioSource value;
		if (loopingAudioDictionary.TryGetValue(audioType, out value))
		{
			AudioManager.StopLoopingSFX(value);
			loopingAudioDictionary.Remove(audioType);
		}
	}
}
