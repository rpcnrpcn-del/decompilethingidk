using System.Collections;
using UnityEngine;

public class GameScoreboardProjector : MonoBehaviour
{
	[Header("Proximity")]
	[SerializeField]
	private float innerProximityTriggerRadius = 2f;

	[SerializeField]
	private float outerProximityTriggerRadius = 10f;

	[Header("Scoreboard View")]
	[SerializeField]
	private GameScoreboardView scoreboardViewPrefab;

	[SerializeField]
	private Transform scoreboardViewParent;

	[Header("Audio")]
	[SerializeField]
	private RecRoomAudioClip scoreboardOpen;

	[SerializeField]
	private RecRoomAudioClip scoreboardClose;

	[SerializeField]
	private RecRoomAudioClip scoreboardHum;

	private static readonly int rotationYawAnimatorFloat = Animator.StringToHash("Rotate");

	private static readonly int openAnimatorBool = Animator.StringToHash("Open");

	private Animator animator;

	private SFXAudioSource scoreboardHumSource;

	private Coroutine fadeHumAudioCoroutine;

	private bool _open;

	private bool Open
	{
		get
		{
			return _open;
		}
		set
		{
			if (_open != value)
			{
				if (value)
				{
					AudioManager.Play3DSFX(scoreboardOpen, base.transform.position);
					FadeInScoreboardHum();
				}
				else
				{
					AudioManager.Play3DSFX(scoreboardClose, base.transform.position);
					FadeOutScoreboardHum();
				}
			}
			_open = value;
			animator.SetBool(openAnimatorBool, _open);
		}
	}

	private float RotationYaw
	{
		set
		{
			animator.SetFloat(rotationYawAnimatorFloat, value);
		}
	}

	private void Awake()
	{
		animator = GetComponentInChildren<Animator>();
		GameScoreboardView gameScoreboardView = Object.Instantiate(scoreboardViewPrefab);
		gameScoreboardView.transform.SetParent(scoreboardViewParent, false);
		gameScoreboardView.transform.localPosition = Vector3.zero;
		gameScoreboardView.transform.localRotation = Quaternion.identity;
	}

	private void Start()
	{
		Open = false;
	}

	private void Update()
	{
		if (Player.LocalPlayer != null)
		{
			Vector3 vector = Vector3.ProjectOnPlane(Player.LocalPlayer.Head.transform.position - base.transform.position, Vector3.up);
			float sqrMagnitude = vector.sqrMagnitude;
			bool flag = sqrMagnitude <= outerProximityTriggerRadius * outerProximityTriggerRadius && sqrMagnitude >= innerProximityTriggerRadius * innerProximityTriggerRadius;
			if (flag && !Open)
			{
				Open = true;
			}
			else if (!flag && Open)
			{
				Open = false;
			}
			if (Open)
			{
				Vector3 normalized = Vector3.ProjectOnPlane(base.transform.forward, Vector3.up).normalized;
				vector.Normalize();
				float num = Mathf.Sign(Vector3.Dot(Vector3.Cross(normalized, vector), Vector3.up));
				RotationYaw = Vector3.Angle(normalized, vector) * num;
			}
		}
	}

	private void FadeInScoreboardHum()
	{
		StopFadeHumCoroutine();
		fadeHumAudioCoroutine = StartCoroutine(FadeInScoreboardHumCoroutine());
	}

	private void FadeOutScoreboardHum()
	{
		StopFadeHumCoroutine();
		fadeHumAudioCoroutine = StartCoroutine(FadeOutScoreboardHumCoroutine());
	}

	private IEnumerator FadeInScoreboardHumCoroutine()
	{
		if (scoreboardHumSource == null)
		{
			scoreboardHumSource = AudioManager.StartLooping3DSFX(scoreboardHum, base.transform);
		}
		yield return SingletonMonoBehaviour<AudioManager>.Instance.RunChangeAudioVolume(scoreboardHumSource.AudioSource, 0f, scoreboardHum.volume, 3f);
		fadeHumAudioCoroutine = null;
	}

	private IEnumerator FadeOutScoreboardHumCoroutine()
	{
		yield return SingletonMonoBehaviour<AudioManager>.Instance.RunChangeAudioVolume(scoreboardHumSource.AudioSource, scoreboardHum.volume, 0f, 1f);
		AudioManager.StopLoopingSFX(scoreboardHumSource);
		scoreboardHumSource = null;
		fadeHumAudioCoroutine = null;
	}

	private void StopFadeHumCoroutine()
	{
		if (fadeHumAudioCoroutine != null)
		{
			StopCoroutine(fadeHumAudioCoroutine);
			fadeHumAudioCoroutine = null;
		}
	}
}
