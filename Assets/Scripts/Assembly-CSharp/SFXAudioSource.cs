using UnityEngine;

public class SFXAudioSource : MonoBehaviour
{
	public bool Is3DSFX;

	private AudioRolloff defaultRolloff;

	private AudioSource _audioSource;

	private bool rolloffDirty = true;

	public Transform FollowTarget { get; set; }

	public AudioSource AudioSource
	{
		get
		{
			return _audioSource;
		}
	}

	private void Awake()
	{
		_audioSource = GetComponent<AudioSource>();
		defaultRolloff = new AudioRolloff(_audioSource);
	}

	private void OnDisable()
	{
		CancelInvoke("Release");
		FollowTarget = null;
		if (_audioSource != null)
		{
			_audioSource.loop = false;
		}
	}

	private void Update()
	{
		if (Is3DSFX && FollowTarget != null)
		{
			base.transform.position = FollowTarget.position;
		}
	}

	public void Play(AudioClip clip, float volume, Vector3 point, float pitchVariation, AudioRolloff customRolloff = null)
	{
		base.transform.position = point;
		_audioSource.clip = clip;
		_audioSource.volume = volume;
		if (pitchVariation > 0f)
		{
			_audioSource.pitch = 1f + Random.Range(0f - pitchVariation, pitchVariation);
		}
		else
		{
			_audioSource.pitch = 1f;
		}
		if (Is3DSFX)
		{
			if (customRolloff != null)
			{
				customRolloff.TrySet(_audioSource);
				rolloffDirty = true;
			}
			else if (rolloffDirty)
			{
				if (PlayerAudio.Custom3DSFXAudioRolloff != null)
				{
					PlayerAudio.Custom3DSFXAudioRolloff.TrySet(_audioSource);
				}
				else
				{
					defaultRolloff.TrySet(_audioSource);
				}
			}
		}
		_audioSource.Play();
		Invoke("Release", clip.length);
	}

	public void Loop()
	{
		if (_audioSource.isPlaying)
		{
			_audioSource.loop = true;
			CancelInvoke("Release");
		}
	}

	public void Stop()
	{
		_audioSource.Stop();
		_audioSource.clip = null;
	}

	public void Release()
	{
		FollowTarget = null;
		CancelInvoke("Release");
		if (_audioSource.isPlaying)
		{
			_audioSource.Stop();
		}
		ObjectPool.Instance.Release(this);
	}
}
