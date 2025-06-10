using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GhostingPulse : MonoBehaviour
{
	private CapsuleCollider ghostingCollider;

	private Player thisPlayer;

	private float startRadiusScale;

	private float startHeightScale;

	[Header("Audio")]
	[SerializeField]
	private RecRoomAudioClip onEnableAudioClip;

	[SerializeField]
	private RecRoomAudioClip activeLoopingAudioClip;

	[Header("Visuals")]
	[SerializeField]
	private FloatValueCurve radiusAnimationCurve;

	[SerializeField]
	private FloatValueCurve heightAnimationCurve;

	[SerializeField]
	private ParticleSystem EMPParticle;

	[SerializeField]
	private FloatValueCurve alphaOverDistanceCurve;

	[SerializeField]
	private Material ghostMaterial;

	[SerializeField]
	private GameObject situationRingPrefab;

	private SFXAudioSource loopingAudioSource;

	private bool _pulseEnabled = true;

	private float activeTime;

	private bool active;

	public Material GhostMaterial
	{
		get
		{
			return ghostMaterial;
		}
	}

	public bool PulseEnabled
	{
		get
		{
			return _pulseEnabled;
		}
		set
		{
			_pulseEnabled = value;
			if (Active && !_pulseEnabled)
			{
				Active = false;
			}
		}
	}

	public bool Active
	{
		get
		{
			return active;
		}
		set
		{
			if (!PulseEnabled)
			{
				value = false;
			}
			if (!value)
			{
				StopPulseActiveAudio();
			}
			else if (!active)
			{
				PlayPulseActiveAudio();
			}
			activeTime = 0f;
			ghostingCollider.enabled = value;
			SingletonMonoBehaviour<AudioManager>.Instance.VoiceVolumeLowered = value;
			foreach (Transform item in base.transform)
			{
				item.gameObject.SetActive(value);
			}
			bool flag = active;
			active = value;
			if (active != flag && this.PanicToggled != null)
			{
				this.PanicToggled(this);
			}
		}
	}

	public float Radius
	{
		get
		{
			return ghostingCollider.bounds.extents.x;
		}
	}

	public float MaxRadius
	{
		get
		{
			return ghostingCollider.radius;
		}
	}

	public event PanicToggledEventHandler PanicToggled;

	private void Awake()
	{
		ghostingCollider = GetComponent<CapsuleCollider>();
		if (ghostingCollider == null)
		{
			Debug.LogError("Ghosting pulse collider is not a Capsule Collider.");
		}
		thisPlayer = base.gameObject.GetComponentInParents<Player>();
	}

	private void Start()
	{
		GameObject gameObject = Object.Instantiate(situationRingPrefab, base.transform, false);
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		startRadiusScale = base.transform.localScale.x;
		startHeightScale = base.transform.localScale.z;
		Active = false;
	}

	private void Update()
	{
		if (!Active)
		{
			return;
		}
		base.transform.position = thisPlayer.CurrentFloorPosition;
		base.transform.localPosition = Vector3.zero;
		if (activeTime <= radiusAnimationCurve.Duration)
		{
			float num = startRadiusScale + radiusAnimationCurve.Evaluate(activeTime / radiusAnimationCurve.Duration);
			float z = startHeightScale + heightAnimationCurve.Evaluate(activeTime / heightAnimationCurve.Duration);
			base.transform.localScale = new Vector3(num, num, z);
			if (activeTime + Time.deltaTime > radiusAnimationCurve.Duration && EMPParticle != null)
			{
				EMPParticle.Play();
			}
		}
		activeTime += Time.deltaTime;
	}

	public void TogglePanic()
	{
		Active = !Active;
	}

	public float AlphaForDistance(float distance)
	{
		float t = Mathf.Min(distance / MaxRadius, 1f);
		return alphaOverDistanceCurve.Evaluate(t);
	}

	private void PlayPulseActiveAudio()
	{
		StopPulseActiveAudio();
		AudioManager.Play3DSFX(onEnableAudioClip, thisPlayer.CurrentFloorPosition);
		loopingAudioSource = AudioManager.StartLooping2DSFX(activeLoopingAudioClip);
	}

	private void StopPulseActiveAudio()
	{
		if (loopingAudioSource != null)
		{
			AudioManager.StopLoopingSFX(loopingAudioSource);
			loopingAudioSource = null;
		}
	}
}
