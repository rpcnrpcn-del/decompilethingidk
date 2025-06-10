using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : SingletonMonoBehaviour<AudioManager>
{
	private enum SFXType
	{
		_2D = 0,
		_3D = 1
	}

	public enum VOIPFilter
	{
		LowerPitch = 0,
		LowPitch = 1,
		None = 2
	}

	[Header("Global settings")]
	public AudioMixer MasterMixer;

	[Header("Other")]
	[Tooltip("The SFX volume when VR Focus has been lost. dB")]
	[Range(-80f, 20f)]
	public float SFXMixerVolumeMuted = -80f;

	[Tooltip("The SFX volume when someone is talking. dB")]
	[Range(-80f, 20f)]
	public float SFXMixerVolumeLow = -10f;

	[Tooltip("The Voice volume when in situation mode. dB")]
	[Range(-80f, 20f)]
	public float VoiceMixerVolumeLow = -15f;

	[Tooltip("The Non Coach volume when coach is talking. dB")]
	[Range(-80f, 20f)]
	public float NonCoachVolumeLow = -10f;

	[SerializeField]
	private SFXAudioSource sfx3DAudioSourcePrefab;

	[SerializeField]
	private SFXAudioSource sfx2DAudioSourcePrefab;

	[SerializeField]
	private AudioSource ambientAudioSource;

	[SerializeField]
	private AudioSource musicAudioSource;

	[Header("Tool specific audio clips")]
	public RecRoomAudioClip ToolLockedAudio;

	public RecRoomAudioClip ToolUnLockedAudio;

	private static int last2DClipNumber;

	private static int last3DClipNumber;

	private bool initialized;

	private static string MUSIC_VOLUME_PERCENT_PREF = "MUSIC_VOLUME_PERCENT_PREF";

	private float musicMixerVolumeDefault;

	private float currentMusicVolume;

	private float targetMusicVolume;

	private float _musicVolumePercentage = 1f;

	private static string VOICE_VOLUME_PERCENT_PREF = "VOICE_VOLUME_PERCENT_PREF";

	private float voiceMixerVolumeDefault;

	private float currentVoiceVolume;

	private float targetVoiceVolume;

	private float _voiceVolumePercentage = 1f;

	private static string SFX_VOLUME_PERCENT_PREF = "SFX_VOLUME_PERCENT_PREF";

	private float _sfxVolumePercentage = 1f;

	private bool sfxVolumeLowered;

	private float lastSFXVolumeLoweredTime;

	private float sfxMixerVolumeDefault;

	private float currentSFXVolume;

	private float targetSFXVolume;

	private float sfxVolumeInterpolationSpeed = 10f;

	private float nonCoachVolumeDefault;

	private float currentNonCoachVolume;

	private float targetNonCoachVolume;

	private float nonCoachVolumeInterpolationSpeed = 10f;

	public float MusicVolumePercentage
	{
		get
		{
			return _musicVolumePercentage;
		}
		set
		{
			if (_musicVolumePercentage != value)
			{
				_musicVolumePercentage = value;
				targetMusicVolume = ApplyPercentToDB(musicMixerVolumeDefault, _musicVolumePercentage);
				RecroomPrefs.SetFloat(MUSIC_VOLUME_PERCENT_PREF, _musicVolumePercentage);
			}
		}
	}

	public float VoiceVolumePercentage
	{
		get
		{
			return _voiceVolumePercentage;
		}
		set
		{
			if (_voiceVolumePercentage != value)
			{
				_voiceVolumePercentage = value;
				targetVoiceVolume = ApplyPercentToDB(voiceMixerVolumeDefault, _voiceVolumePercentage);
				RecroomPrefs.SetFloat(VOICE_VOLUME_PERCENT_PREF, _voiceVolumePercentage);
			}
		}
	}

	public float SfxVolumePercentage
	{
		get
		{
			return _sfxVolumePercentage;
		}
		set
		{
			if (_sfxVolumePercentage != value)
			{
				_sfxVolumePercentage = value;
				targetSFXVolume = ApplyPercentToDB((!sfxVolumeLowered) ? sfxMixerVolumeDefault : SFXMixerVolumeLow, SfxVolumePercentage);
				RecroomPrefs.SetFloat(SFX_VOLUME_PERCENT_PREF, _sfxVolumePercentage);
			}
		}
	}

	public bool VoiceVolumeLowered { get; set; }

	private void Awake()
	{
		SingletonMonoBehaviour<AudioManager>.Instance = this;
		ambientAudioSource.playOnAwake = false;
		ambientAudioSource.loop = true;
		musicAudioSource.playOnAwake = false;
		musicAudioSource.loop = true;
	}

	public void Initialize()
	{
		float value;
		if (MasterMixer.GetFloat("SFXVolume", out value))
		{
			sfxMixerVolumeDefault = (currentSFXVolume = (targetSFXVolume = value));
		}
		float value2;
		if (MasterMixer.GetFloat("MusicVolume", out value2))
		{
			musicMixerVolumeDefault = (currentMusicVolume = (targetMusicVolume = value2));
		}
		float value3;
		if (MasterMixer.GetFloat("VoiceVolume", out value3))
		{
			voiceMixerVolumeDefault = (currentVoiceVolume = (targetVoiceVolume = value3));
		}
		if (MasterMixer.GetFloat("NonCoachVolume", out value3))
		{
			nonCoachVolumeDefault = (currentNonCoachVolume = (targetNonCoachVolume = value3));
		}
		MusicVolumePercentage = RecroomPrefs.GetFloat(MUSIC_VOLUME_PERCENT_PREF, 1f);
		SfxVolumePercentage = RecroomPrefs.GetFloat(SFX_VOLUME_PERCENT_PREF, 1f);
		VoiceVolumePercentage = RecroomPrefs.GetFloat(VOICE_VOLUME_PERCENT_PREF, 1f);
		initialized = true;
	}

	private void Update()
	{
		if (initialized)
		{
			UpdateSFXVolume();
			UpdateNonCoachVolume();
		}
	}

	public AudioMixerGroup GetVOIPMixerOutput(VOIPFilter filter)
	{
		string subPath = "VOIP_Master";
		switch (filter)
		{
		case VOIPFilter.LowerPitch:
			subPath = "VOIP_LowerPitch";
			break;
		case VOIPFilter.LowPitch:
			subPath = "VOIP_LowPitch";
			break;
		}
		return MasterMixer.FindMatchingGroups(subPath)[0];
	}

	private void UpdateSFXVolume()
	{
		bool flag = false;
		if (PhotonNetwork.connected && PhotonNetwork.otherPlayers != null)
		{
			PhotonPlayer[] otherPlayers = PhotonNetwork.otherPlayers;
			foreach (PhotonPlayer photonPlayer in otherPlayers)
			{
				Player player = photonPlayer.TagObject as Player;
				if (player != null && player.IsTalking)
				{
					flag = true;
					break;
				}
			}
		}
		if (!PlatformManager.Instance.HasVRFocus)
		{
			targetSFXVolume = ApplyPercentToDB(SFXMixerVolumeMuted, SfxVolumePercentage);
			sfxVolumeLowered = true;
			lastSFXVolumeLoweredTime = Time.time;
		}
		else if (!sfxVolumeLowered && flag)
		{
			targetSFXVolume = ApplyPercentToDB(SFXMixerVolumeLow, SfxVolumePercentage);
			sfxVolumeLowered = true;
			lastSFXVolumeLoweredTime = Time.time;
		}
		else if (sfxVolumeLowered && Time.time - lastSFXVolumeLoweredTime > 1f)
		{
			targetSFXVolume = ApplyPercentToDB(sfxMixerVolumeDefault, SfxVolumePercentage);
			sfxVolumeLowered = false;
		}
		if (VoiceVolumeLowered)
		{
			targetVoiceVolume = ApplyPercentToDB(VoiceMixerVolumeLow, VoiceVolumePercentage);
		}
		else
		{
			targetVoiceVolume = ApplyPercentToDB(voiceMixerVolumeDefault, VoiceVolumePercentage);
		}
		if (currentSFXVolume != targetSFXVolume)
		{
			currentSFXVolume = Mathf.SmoothStep(currentSFXVolume, targetSFXVolume, Time.deltaTime * sfxVolumeInterpolationSpeed);
			MasterMixer.SetFloat("SFXVolume", currentSFXVolume);
		}
		if (currentMusicVolume != targetMusicVolume)
		{
			currentMusicVolume = Mathf.SmoothStep(currentMusicVolume, targetMusicVolume, Time.deltaTime * sfxVolumeInterpolationSpeed);
			MasterMixer.SetFloat("MusicVolume", currentMusicVolume);
		}
		if (currentVoiceVolume != targetVoiceVolume)
		{
			currentVoiceVolume = Mathf.SmoothStep(currentVoiceVolume, targetVoiceVolume, Time.deltaTime * sfxVolumeInterpolationSpeed);
			MasterMixer.SetFloat("VoiceVolume", currentVoiceVolume);
		}
	}

	private void UpdateNonCoachVolume()
	{
		bool isPlayingVO = SingletonMonoBehaviour<TutorialManager>.Instance.IsPlayingVO;
		targetNonCoachVolume = ((!isPlayingVO) ? nonCoachVolumeDefault : NonCoachVolumeLow);
		if (currentNonCoachVolume != targetNonCoachVolume)
		{
			currentNonCoachVolume = Mathf.SmoothStep(currentNonCoachVolume, targetNonCoachVolume, Time.deltaTime * nonCoachVolumeInterpolationSpeed);
			MasterMixer.SetFloat("NonCoachVolume", currentNonCoachVolume);
		}
	}

	private float ApplyPercentToDB(float volume, float percent)
	{
		float num = volume + 80f;
		return num * Mathf.Log10(percent.ReMapRange(new Vector2(0f, 1f), new Vector2(1f, 10f))) - 80f;
	}

	public IEnumerator RunChangeAudioVolume(AudioSource audio, float from, float to, float duration)
	{
		audio.volume = from;
		float timer = 0f;
		while (timer <= duration)
		{
			timer += Time.deltaTime;
			float alpha = Mathf.Clamp01(timer / duration);
			audio.volume = Mathf.Lerp(from, to, alpha);
			yield return null;
		}
	}

	public static SFXAudioSource Play2DSFX(RecRoomAudioClip clip)
	{
		if (clip == null)
		{
			return null;
		}
		return Play2DSFX(clip.audioClip, SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.position, clip.volume, clip.pitchVariation);
	}

	public static SFXAudioSource Play2DSFX(AudioClip clip, Vector3 point, float volume = 1f, float pitchVariation = 0f)
	{
		return PlaySFX(SFXType._2D, clip, point, volume, pitchVariation, null);
	}

	public static SFXAudioSource Play3DSFX(RecRoomAudioClip clip, Vector3 point, AudioRolloff customRolloff = null)
	{
		if (clip == null)
		{
			return null;
		}
		return Play3DSFX(clip.audioClip, point, clip.volume, clip.pitchVariation, customRolloff);
	}

	public static SFXAudioSource Play3DSFX(RecRoomAudioClip recClip, Transform followTarget, AudioRolloff customRolloff = null)
	{
		if (recClip == null || followTarget == null)
		{
			return null;
		}
		SFXAudioSource sFXAudioSource = PlaySFX(SFXType._3D, recClip.audioClip, followTarget.position, recClip.volume, recClip.pitchVariation, customRolloff);
		SFXAudioSource component = sFXAudioSource.GetComponent<SFXAudioSource>();
		if (component != null)
		{
			component.FollowTarget = followTarget;
		}
		return sFXAudioSource;
	}

	public static SFXAudioSource Play3DSFX(AudioClip clip, Vector3 point, float volume = 1f, float pitchVariation = 0f, AudioRolloff customRolloff = null)
	{
		return PlaySFX(SFXType._3D, clip, point, volume, pitchVariation, customRolloff);
	}

	public static SFXAudioSource PlayRandom2DSFX(RecRoomAudioClip[] clips, Vector3 point, bool allowRepeat = false)
	{
		if (clips == null || clips.Length == 0)
		{
			Debug.Log("Warning: SFX array is null or empty...");
			return null;
		}
		int num = (last2DClipNumber = GetRandomClipNumber(clips.Length, last2DClipNumber, allowRepeat));
		return PlaySFX(SFXType._2D, clips[num].audioClip, point, clips[num].volume, clips[num].pitchVariation, null);
	}

	public static SFXAudioSource PlayRandom3DSFX(RecRoomAudioClip[] clips, Vector3 point, bool allowRepeat = false)
	{
		if (clips == null || clips.Length == 0)
		{
			Debug.Log("Warning: SFX array is null or empty...");
			return null;
		}
		int num = (last3DClipNumber = GetRandomClipNumber(clips.Length, last3DClipNumber, allowRepeat));
		return PlaySFX(SFXType._3D, clips[num].audioClip, point, clips[num].volume, clips[num].pitchVariation, null);
	}

	public static SFXAudioSource PlayRandom3DSFX(RecRoomAudioClip[] clips, Transform followTarget, AudioRolloff customRolloff = null, bool allowRepeat = false)
	{
		if (clips == null || clips.Length == 0)
		{
			Debug.Log("Warning: SFX array is null or empty...");
			return null;
		}
		return Play3DSFX(clips[last3DClipNumber = GetRandomClipNumber(clips.Length, last3DClipNumber, allowRepeat)], followTarget, customRolloff);
	}

	public static SFXAudioSource StartLooping3DSFX(RecRoomAudioClip clip, Transform followTarget, AudioRolloff customRolloff = null)
	{
		SFXAudioSource sFXAudioSource = Play3DSFX(clip, followTarget, customRolloff);
		sFXAudioSource.Loop();
		return sFXAudioSource;
	}

	public static SFXAudioSource StartLooping2DSFX(RecRoomAudioClip clip)
	{
		SFXAudioSource sFXAudioSource = Play2DSFX(clip);
		sFXAudioSource.Loop();
		return sFXAudioSource;
	}

	public static void StopLoopingSFX(SFXAudioSource audioSource)
	{
		if (audioSource != null)
		{
			audioSource.Stop();
			audioSource.Release();
		}
	}

	public static void PlayAmbientAudio(RecRoomAudioClip ambientAudio)
	{
		if (ambientAudio != null && !(SingletonMonoBehaviour<AudioManager>.Instance == null))
		{
			AudioSource audioSource = SingletonMonoBehaviour<AudioManager>.Instance.ambientAudioSource;
			if (audioSource.isPlaying)
			{
				audioSource.Stop();
			}
			audioSource.volume = ambientAudio.volume;
			audioSource.clip = ambientAudio.audioClip;
			audioSource.Play();
		}
	}

	public static void StopAmbientAudio()
	{
		if (SingletonMonoBehaviour<AudioManager>.Instance != null && SingletonMonoBehaviour<AudioManager>.Instance.ambientAudioSource.isPlaying)
		{
			SingletonMonoBehaviour<AudioManager>.Instance.ambientAudioSource.Stop();
		}
	}

	public static void PlayMusic(RecRoomAudioClip music)
	{
		if (music != null && !(SingletonMonoBehaviour<AudioManager>.Instance == null))
		{
			AudioSource audioSource = SingletonMonoBehaviour<AudioManager>.Instance.musicAudioSource;
			if (audioSource.isPlaying)
			{
				audioSource.Stop();
			}
			audioSource.volume = music.volume;
			audioSource.clip = music.audioClip;
			audioSource.Play();
		}
	}

	public static void StopMusic()
	{
		if (SingletonMonoBehaviour<AudioManager>.Instance != null && SingletonMonoBehaviour<AudioManager>.Instance.musicAudioSource.isPlaying)
		{
			SingletonMonoBehaviour<AudioManager>.Instance.musicAudioSource.Stop();
		}
	}

	private static SFXAudioSource PlaySFX(SFXType type, AudioClip clip, Vector3 point, float volume, float pitchVariation, AudioRolloff customRolloff)
	{
		if (clip == null || SingletonMonoBehaviour<AudioManager>.Instance == null)
		{
			return null;
		}
		SFXAudioSource sFXAudioSource = null;
		switch (type)
		{
		case SFXType._2D:
			sFXAudioSource = ObjectPool.Instance.Acquire(SingletonMonoBehaviour<AudioManager>.Instance.sfx2DAudioSourcePrefab);
			break;
		case SFXType._3D:
			sFXAudioSource = ObjectPool.Instance.Acquire(SingletonMonoBehaviour<AudioManager>.Instance.sfx3DAudioSourcePrefab);
			break;
		}
		if (sFXAudioSource != null)
		{
			sFXAudioSource.Play(clip, volume, point, pitchVariation, customRolloff);
		}
		return sFXAudioSource;
	}

	private static int GetRandomClipNumber(int arrayLength, int lastClipNumber, bool allowRepeat)
	{
		if (!allowRepeat && arrayLength == 1)
		{
			allowRepeat = true;
		}
		int num = Random.Range(0, arrayLength);
		if (!allowRepeat && num == lastClipNumber)
		{
			num = ++num % arrayLength;
		}
		return num;
	}
}
