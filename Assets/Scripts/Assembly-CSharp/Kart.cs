using System;
using System.Collections;
using Photon;
using UnityEngine;

public class Kart : Photon.MonoBehaviour
{
	[Header("Steering")]
	[SerializeField]
	private AnimationCurve steeringInputCurve;

	[SerializeField]
	private AnimationCurve maxSteerAngleCurve;

	[SerializeField]
	private float minSpeedMaxSteerAngle = 45f;

	[SerializeField]
	private float maxSpeedMaxSteerAngle = 10f;

	[Header("Acceleration")]
	[SerializeField]
	private float maxSpeed = 10f;

	[SerializeField]
	private AnimationCurve accelerationCurve;

	[SerializeField]
	private AnimationCurve decelerationCurve;

	[SerializeField]
	private float maxAccelerationRate = 5f;

	[SerializeField]
	private float maxDecelerationRate = 5f;

	[Header("Camera Smoothing")]
	[SerializeField]
	private float rotationSmoothTime = 1f;

	[SerializeField]
	private float positionSmoothTime;

	[Header("Wheels")]
	[SerializeField]
	private Transform[] frontWheels;

	[SerializeField]
	private Transform[] backWheels;

	[SerializeField]
	private Transform[] wheelMeshes;

	[SerializeField]
	private float wheelRadius = 0.15f;

	[Header("Vignetting")]
	[SerializeField]
	private float vignetteSmoothTime = 0.5f;

	[SerializeField]
	private float minVignetteSpeed = 5f;

	[SerializeField]
	private float minVignetteRotationRate = 30f;

	[SerializeField]
	private float movingVignetteAmount = 0.8f;

	[SerializeField]
	private float rotatingVignetteAmount = 0.95f;

	private float linearAcceleration;

	private Vector3 angularAcceleration = Vector3.zero;

	private float steering;

	private float throttle;

	private float speed;

	private Vector3 angularVelocity = Vector3.zero;

	private float heightSpeed;

	private float pitchSpeed;

	private float rollSpeed;

	private float vignetteSpeed;

	private float currentVignetting;

	private PhotonView rigidbodyPhotonView;

	private KartSteeringWheel steeringWheel;

	private TeleportationPortal[] seatPortals;

	private Collider[] bodyColliders;

	private Rigidbody bodyRigidbody;

	private SynchronizedField<int> driverPlayerId;

	private Vector3 kartToDriverRigPosition;

	private Quaternion kartToDriverRigRotation;

	private PhotonPlayer previousDriver;

	private SFXAudioSource engineIdleLoop;

	private SFXAudioSource engineRevLoop;

	private Coroutine fadeEngineRevAudioCoroutine;

	private Coroutine fadeEngineIdleAudioCoroutine;

	[Header("Custom Audio")]
	[SerializeField]
	private Transform engineAudioTransform;

	[SerializeField]
	private RecRoomAudioClip engineIdleLoopAudio;

	[SerializeField]
	private RecRoomAudioClip engineRevLoopAudio;

	[SerializeField]
	private RecRoomAudioClip engineStartAudio;

	[SerializeField]
	private RecRoomAudioClip engineRevAudio;

	public float Speed
	{
		get
		{
			return speed;
		}
	}

	public PhotonPlayer Driver
	{
		get
		{
			int iD = driverPlayerId.Get();
			return PhotonPlayer.Find(iD);
		}
		private set
		{
			int newValue = ((value == null) ? PhotonPlayer.Invalid : value.ID);
			driverPlayerId.ForceSet(newValue);
		}
	}

	public bool IsAccelerating { get; private set; }

	protected void OnDriverIdChange()
	{
		PhotonPlayer driver = Driver;
		if (driver != null && driver != previousDriver)
		{
			if (driver.isLocal)
			{
				Player player = driver.ToPlayer();
				if (player != null)
				{
					bodyRigidbody.isKinematic = false;
					player.PlayerLocomotion.TeleportEvent += OnPlayerTeleport;
					kartToDriverRigPosition = base.transform.InverseTransformPoint(player.transform.position);
					kartToDriverRigRotation = base.transform.InverseTransformRotation(player.transform.rotation);
					base.photonView.TransferOwnership(driver);
					rigidbodyPhotonView.TransferOwnership(driver);
				}
			}
			else
			{
				bodyRigidbody.isKinematic = true;
			}
			StartEngineIdleAudio();
		}
		else if (previousDriver != null && driver != previousDriver)
		{
			if (previousDriver.isLocal)
			{
				Player player2 = previousDriver.ToPlayer();
				if (player2 != null)
				{
					player2.PlayerLocomotion.TeleportEvent -= OnPlayerTeleport;
				}
			}
			bodyRigidbody.isKinematic = false;
			StopEngineIdleAudio();
		}
		previousDriver = driver;
	}

	protected override void Awake()
	{
		base.Awake();
		seatPortals = GetComponentsInChildren<TeleportationPortal>();
		for (int i = 0; i < seatPortals.Length; i++)
		{
			seatPortals[i].PlayerUseEvent += OnSeatUsed;
		}
		bodyRigidbody = GetComponentInChildren<Rigidbody>();
		rigidbodyPhotonView = bodyRigidbody.GetComponentInChildren<PhotonView>();
		bodyColliders = bodyRigidbody.GetComponentsInChildren<Collider>();
		steeringWheel = GetComponentInChildren<KartSteeringWheel>();
		GameObject gameObject = new GameObject(base.name + "_root");
		base.transform.SetParent(gameObject.transform, true);
		bodyRigidbody.transform.SetParent(gameObject.transform, true);
		steeringWheel.transform.SetParent(gameObject.transform, true);
		driverPlayerId = new SynchronizedField<int>(this, "DRIVER_ID", PhotonPlayer.Invalid, SetterPermissionMode.ANYONE, OnDriverIdChange);
		IsAccelerating = false;
	}

	private void Start()
	{
		OnDriverIdChange();
		for (int i = 0; i < steeringWheel.Colliders.Count; i++)
		{
			for (int j = 0; j < bodyColliders.Length; j++)
			{
				Physics.IgnoreCollision(steeringWheel.Colliders[i].ThisCollider, bodyColliders[j]);
			}
		}
	}

	public void Steer(float steer)
	{
		float time = Mathf.Clamp01(Mathf.Abs(steer));
		steering = Mathf.Sign(steer) * steeringInputCurve.Evaluate(time);
	}

	private void OnStartAccelerating()
	{
		IsAccelerating = true;
		StartEngineRevAudio();
	}

	private void OnStopAccelerating()
	{
		IsAccelerating = false;
		StopEngineRevAudio();
	}

	public void Accelerate(float normalizedAccelerationAmount)
	{
		float num = 0.001f;
		if (normalizedAccelerationAmount < num)
		{
			normalizedAccelerationAmount = 0f;
		}
		if (!IsAccelerating && throttle <= num && normalizedAccelerationAmount > num)
		{
			OnStartAccelerating();
		}
		else if (IsAccelerating && throttle > num && normalizedAccelerationAmount < num)
		{
			OnStopAccelerating();
		}
		throttle = normalizedAccelerationAmount;
	}

	public bool TryRotateInPlace(float angle)
	{
		if (Speed <= 1f)
		{
			bodyRigidbody.transform.rotation *= Quaternion.Euler(0f, angle, 0f);
			return true;
		}
		return false;
	}

	private void FixedUpdate()
	{
		float steerAngle = GetSteerAngle();
		UpdatePhysics(steerAngle);
		ApplyPhysics();
		UpdateWheelRotation(steerAngle, Time.fixedDeltaTime);
	}

	private float GetSteerAngle()
	{
		float num = Mathf.InverseLerp(0f, maxSpeed, speed);
		if (num >= 0.9f)
		{
			num *= 1f;
		}
		float t = maxSteerAngleCurve.Evaluate(num);
		float num2 = Mathf.Lerp(minSpeedMaxSteerAngle, maxSpeedMaxSteerAngle, t);
		return num2 * steering;
	}

	private float GetAcceleration()
	{
		float time = Mathf.InverseLerp(0f, maxSpeed, speed);
		float result = 0f;
		if (throttle >= 0.5f)
		{
			float t = accelerationCurve.Evaluate(time);
			result = Mathf.Lerp(0f, maxAccelerationRate, t);
		}
		else if (speed > Mathf.Epsilon)
		{
			float t2 = decelerationCurve.Evaluate(time);
			result = 0f - Mathf.Lerp(0f, maxDecelerationRate, t2);
		}
		return result;
	}

	private void UpdatePhysics(float steerAngle)
	{
		Vector3 target = Vector3.zero;
		if (Mathf.Abs(steerAngle) >= 0.01f)
		{
			float magnitude = (frontWheels[0].position - backWheels[0].position).magnitude;
			float num = magnitude / Mathf.Sin(steerAngle * ((float)Math.PI / 180f));
			target = base.transform.up * speed / num * 57.29578f;
		}
		angularVelocity = Vector3.SmoothDamp(angularVelocity, target, ref angularAcceleration, 0f, float.PositiveInfinity, Time.fixedDeltaTime);
		Vector3 forward = base.transform.forward;
		bodyRigidbody.rotation *= Quaternion.AngleAxis(angularVelocity.magnitude * Time.fixedDeltaTime, angularVelocity.normalized);
		linearAcceleration = GetAcceleration();
		speed = Mathf.Clamp(speed * Vector3.Dot(base.transform.forward, forward) + linearAcceleration * Time.fixedDeltaTime, 0f, maxSpeed);
		bodyRigidbody.position += base.transform.forward * speed * Time.fixedDeltaTime;
	}

	private void ApplyPhysics()
	{
		Vector3 up = Vector3.up;
		Vector3 vector = Vector3.ProjectOnPlane(bodyRigidbody.position, up);
		float num = Mathf.SmoothDamp(base.transform.position.y, Vector3.Dot(bodyRigidbody.position, up), ref heightSpeed, positionSmoothTime);
		base.transform.position = vector + up * num;
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		Vector3 eulerAngles2 = bodyRigidbody.rotation.eulerAngles;
		eulerAngles2.x = Mathf.SmoothDampAngle(eulerAngles.x, eulerAngles2.x, ref pitchSpeed, rotationSmoothTime);
		eulerAngles2.z = Mathf.SmoothDampAngle(eulerAngles.z, eulerAngles2.z, ref rollSpeed, rotationSmoothTime);
		base.transform.rotation = Quaternion.Euler(eulerAngles2);
		if (Driver == null || !Driver.isLocal)
		{
			return;
		}
		Player player = Driver.ToPlayer();
		if (player != null)
		{
			float target = 0f;
			float f = (eulerAngles2.y - eulerAngles.y) / Time.fixedDeltaTime;
			if (Mathf.Abs(f) >= minVignetteRotationRate)
			{
				target = rotatingVignetteAmount;
			}
			else if (speed >= minVignetteSpeed)
			{
				target = movingVignetteAmount;
			}
			currentVignetting = Mathf.SmoothDamp(currentVignetting, target, ref vignetteSpeed, vignetteSmoothTime);
			SingletonMonoBehaviour<CameraRig>.Instance.VignetteImageEffect.SetVignette(currentVignetting);
			player.PlayerLocomotion.Translate(base.transform.TransformPoint(kartToDriverRigPosition), base.transform.TransformRotation(kartToDriverRigRotation));
		}
	}

	private void UpdateWheelRotation(float steerAngle, float deltaTime)
	{
		float num = speed / wheelRadius * 57.29578f;
		Quaternion quaternion = Quaternion.Euler(0f, 0f, num * deltaTime);
		for (int i = 0; i < frontWheels.Length; i++)
		{
			frontWheels[i].localRotation = Quaternion.Euler(0f, steerAngle, 0f);
		}
		for (int j = 0; j < wheelMeshes.Length; j++)
		{
			wheelMeshes[j].localRotation *= quaternion;
		}
	}

	private void OnPlayerTeleport(Player player, Vector3 sourcePosition, Vector3 targetPosition, TeleportationPortal portal)
	{
		bool flag = false;
		if (portal != null)
		{
			for (int i = 0; i < seatPortals.Length; i++)
			{
				if (seatPortals[i] == portal)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag && player.PhotonPlayer == Driver)
		{
			Driver = null;
		}
	}

	private void OnSeatUsed(Player player)
	{
		if (Driver == null && player.isLocal)
		{
			Driver = player.PhotonPlayer;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		for (int i = 0; i < wheelMeshes.Length; i++)
		{
			Gizmos.DrawLine(wheelMeshes[i].transform.position, wheelMeshes[i].transform.position - wheelMeshes[i].transform.up * wheelRadius);
		}
	}

	private void StartEngineRevAudio()
	{
		StopEngineIdleAudio(true);
		StopFadeEngineRevAudioCoroutine();
		fadeEngineRevAudioCoroutine = StartCoroutine(FadeInEngineRevAudioCoroutine());
		AudioManager.Play3DSFX(engineRevAudio, engineAudioTransform);
	}

	private void StopEngineRevAudio()
	{
		StopFadeEngineRevAudioCoroutine();
		fadeEngineRevAudioCoroutine = StartCoroutine(FadeOutEngineRevAudioCoroutine());
		StartEngineIdleAudio(true);
	}

	private void StartEngineIdleAudio(bool startImmediately = false)
	{
		StopFadeEngineIdleAudioCoroutine();
		fadeEngineIdleAudioCoroutine = StartCoroutine(FadeInEngineIdleAudioCoroutine(startImmediately));
		if (!startImmediately)
		{
			AudioManager.Play3DSFX(engineStartAudio, engineAudioTransform);
		}
	}

	private void StopEngineIdleAudio(bool stopImmediately = false)
	{
		StopFadeEngineIdleAudioCoroutine();
		fadeEngineIdleAudioCoroutine = StartCoroutine(FadeOutEngineIdleAudioCoroutine(stopImmediately));
	}

	private IEnumerator FadeInEngineRevAudioCoroutine()
	{
		engineRevLoop = AudioManager.StartLooping3DSFX(engineRevLoopAudio, engineAudioTransform);
		yield return SingletonMonoBehaviour<AudioManager>.Instance.RunChangeAudioVolume(engineRevLoop.AudioSource, 0f, engineRevLoopAudio.volume, 1f);
		fadeEngineRevAudioCoroutine = null;
	}

	private IEnumerator FadeOutEngineRevAudioCoroutine()
	{
		if (engineRevLoop != null)
		{
			yield return SingletonMonoBehaviour<AudioManager>.Instance.RunChangeAudioVolume(engineRevLoop.AudioSource, engineRevLoopAudio.volume, 0f, 1f);
			AudioManager.StopLoopingSFX(engineRevLoop);
		}
		engineRevLoop = null;
		fadeEngineRevAudioCoroutine = null;
	}

	private IEnumerator FadeInEngineIdleAudioCoroutine(bool startImmediately)
	{
		if (engineIdleLoop == null)
		{
			engineIdleLoop = AudioManager.StartLooping3DSFX(engineIdleLoopAudio, engineAudioTransform);
		}
		yield return SingletonMonoBehaviour<AudioManager>.Instance.RunChangeAudioVolume(engineIdleLoop.AudioSource, 0f, engineIdleLoopAudio.volume, (!startImmediately) ? 2f : 0f);
		fadeEngineIdleAudioCoroutine = null;
	}

	private IEnumerator FadeOutEngineIdleAudioCoroutine(bool stopImmediately)
	{
		if (engineIdleLoop != null)
		{
			yield return SingletonMonoBehaviour<AudioManager>.Instance.RunChangeAudioVolume(engineIdleLoop.AudioSource, engineIdleLoopAudio.volume, 0f, (!stopImmediately) ? 1.5f : 0f);
			AudioManager.StopLoopingSFX(engineIdleLoop);
		}
		engineIdleLoop = null;
		fadeEngineIdleAudioCoroutine = null;
	}

	private void StopFadeEngineRevAudioCoroutine()
	{
		if (fadeEngineRevAudioCoroutine != null)
		{
			StopCoroutine(fadeEngineRevAudioCoroutine);
			fadeEngineRevAudioCoroutine = null;
		}
	}

	private void StopFadeEngineIdleAudioCoroutine()
	{
		if (fadeEngineIdleAudioCoroutine != null)
		{
			StopCoroutine(fadeEngineIdleAudioCoroutine);
			fadeEngineIdleAudioCoroutine = null;
		}
	}
}
