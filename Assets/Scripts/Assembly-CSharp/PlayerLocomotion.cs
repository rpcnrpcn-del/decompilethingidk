using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon;
using UnityEngine;

public class PlayerLocomotion : Photon.MonoBehaviour
{
	public enum RotationIncrement
	{
		FOURTY_FIVE_DEGREES = 0,
		NINETY_DEGREES = 1
	}

	public enum CoordinateSpace
	{
		PLAYER_SPACE = 0,
		TRACKING_SPACE = 1
	}

	private const string TELEPORT_BUFFER_PROP = "PlayerTeleportBuffer";

	private const string MOTION_TELEPORT_ENABLED_KEY = "VIGNETTED_TELEPORT_ENABLED";

	private const string ROTATE_IN_PLACE_ENABLED_KEY = "ROTATE_IN_PLACE_ENABLED";

	private const string ROTATION_INCREMENT_KEY = "ROTATION_INCREMENT";

	private const string CONTINUOUS_ROTATION_MODE_KEY = "CONTINUOUS_ROTATION_MODE";

	[Header("Teleport")]
	[Tooltip("Duration with which we blind the user before teleportation.")]
	[SerializeField]
	private float teleportBlindDuration = 0.05f;

	[SerializeField]
	private float inPlaceRotationSpeed = 540f;

	[Header("Vignetting")]
	[SerializeField]
	private float vignetteTeleportFadeInTime;

	[SerializeField]
	private float vignetteTeleportFadeOutTime = 0.3f;

	[SerializeField]
	private float vignetteRotateFadeInTime = 0.05f;

	[SerializeField]
	private float vignetteRotateFadeOutTime = 0.05f;

	[SerializeField]
	private float vignetteContinuousRotationFadeInTime = 0.25f;

	[SerializeField]
	private float vignetteContinuousRotationFadeOutTime = 0.25f;

	[SerializeField]
	private float continuousRotationVignetteIntensity = 0.75f;

	[Header("Teleport Tool Hit")]
	[SerializeField]
	private float defaultMaxToolHitHoldTime = 2f;

	[SerializeField]
	private FloatValueCurve toolHitForceCurve;

	[Header("Visuals")]
	[SerializeField]
	private PlayerTrail playerTrailPrefab;

	public bool HasOverrideTeleportBuffer;

	public float OverrideTeleportBuffer;

	private Dictionary<TeleportCooldownType, float> teleportCooldowns = new Dictionary<TeleportCooldownType, float>();

	private Stack<float> teleportMaxDistanceScalar = new Stack<float>();

	[NonSerialized]
	public float BaseMaxTeleportDistance = 6f;

	[NonSerialized]
	public float MaxTeleportPitchDegrees = 45f;

	private Stack<float> maxToolHitHoldTimeStack = new Stack<float>();

	private Player thisPlayer;

	private PUNNetworkTransform[] childNetworkTransforms;

	private Coroutine teleportCoroutine;

	private float lastTeleportTime;

	private float lastTeleportEnabledTime;

	private float lastInPlaceRotationDirection = 1f;

	private bool continuousRotationIsRunning = true;

	private Vector3 initialContinuousRotationForward = Vector3.zero;

	private PlayerTrail trail;

	[HideInInspector]
	public bool TeleportEnabled = true;

	[HideInInspector]
	public bool JustHitToolWithTeleport;

	private Coroutine rotateTeleportCoroutine;

	public bool PreparingToTeleport { get; private set; }

	public float TeleportCooldownValue { get; private set; }

	public bool TeleportOnCooldown { get; private set; }

	public TeleportBuffer TeleportBuffer
	{
		get
		{
			return (TeleportBuffer)thisPlayer.PlayerData.GetData("PlayerTeleportBuffer", 1);
		}
		set
		{
			thisPlayer.PlayerData.SetData("PlayerTeleportBuffer", (int)value);
		}
	}

	public bool MotionTeleportEnabled
	{
		get
		{
			return thisPlayer.PlayerData.GetData("VIGNETTED_TELEPORT_ENABLED", false);
		}
		set
		{
			thisPlayer.PlayerData.SetData("VIGNETTED_TELEPORT_ENABLED", value);
		}
	}

	public bool RotateInPlaceEnabled
	{
		get
		{
			return thisPlayer.PlayerData.GetData("ROTATE_IN_PLACE_ENABLED", true);
		}
		set
		{
			thisPlayer.PlayerData.SetData("ROTATE_IN_PLACE_ENABLED", value);
		}
	}

	public RotationIncrement RotateInPlaceIncrement
	{
		get
		{
			return (RotationIncrement)thisPlayer.PlayerData.GetData("ROTATION_INCREMENT", 1);
		}
		set
		{
			thisPlayer.PlayerData.SetData("ROTATION_INCREMENT", (int)value);
		}
	}

	public ContinuousRotationMode ContinuousRotationMode
	{
		get
		{
			return (ContinuousRotationMode)thisPlayer.PlayerData.GetData("CONTINUOUS_ROTATION_MODE", 0);
		}
		set
		{
			thisPlayer.PlayerData.SetData("CONTINUOUS_ROTATION_MODE", (int)value);
		}
	}

	public float TeleportBufferDistance
	{
		get
		{
			return SingletonMonoBehaviour<SettingsManager>.Instance.GetTeleportBufferSize(TeleportBuffer);
		}
	}

	public float ChargeAmount
	{
		get
		{
			return GetChargeAmount(MaxToolHitHoldTime);
		}
	}

	public float TeleportCooldown
	{
		get
		{
			return teleportCooldowns.Max((KeyValuePair<TeleportCooldownType, float> x) => x.Value);
		}
	}

	public float MaxTeleportDistance
	{
		get
		{
			return BaseMaxTeleportDistance * TeleportMaxDistanceScalar;
		}
	}

	public float TeleportMaxDistanceScalar
	{
		get
		{
			return teleportMaxDistanceScalar.Peek();
		}
	}

	public float MaxToolHitHoldTime
	{
		get
		{
			return maxToolHitHoldTimeStack.Peek();
		}
	}

	private bool IsMaxToolHitHoldTimeImproved
	{
		get
		{
			return MaxToolHitHoldTime < defaultMaxToolHitHoldTime;
		}
	}

	public bool TeleportSuppressed
	{
		get
		{
			return (thisPlayer.LeftHand != null && thisPlayer.LeftHand.Tool != null && thisPlayer.LeftHand.Tool.SuppressesTeleport) || (thisPlayer.RightHand != null && thisPlayer.RightHand.Tool != null && thisPlayer.RightHand.Tool.SuppressesTeleport);
		}
	}

	public bool JustTeleported { get; private set; }

	private float RotateInPlaceAngle
	{
		get
		{
			float num = 0f;
			switch (RotateInPlaceIncrement)
			{
			case RotationIncrement.FOURTY_FIVE_DEGREES:
				return 45f;
			default:
				return 90f;
			}
		}
	}

	public event Action<Player, Vector3, Vector3, TeleportationPortal> TeleportEvent;

	public void AddTeleportCooldown(TeleportCooldownType cooldownType, float cooldown)
	{
		teleportCooldowns[cooldownType] = cooldown;
	}

	public void RemoveTeleportCooldown(TeleportCooldownType cooldownType)
	{
		teleportCooldowns.Remove(cooldownType);
	}

	public void PushTeleportMaxDistanceScalar(float scalar)
	{
		teleportMaxDistanceScalar.Push(scalar);
	}

	public void PopTeleportMaxDistanceScalar()
	{
		teleportMaxDistanceScalar.Pop();
	}

	public void PushMaxToolHitHoldTime(float time)
	{
		maxToolHitHoldTimeStack.Push(time);
	}

	public void PopMaxToolHitHoldTime()
	{
		maxToolHitHoldTimeStack.Pop();
	}

	protected override void Awake()
	{
		base.Awake();
		PreparingToTeleport = false;
		TeleportOnCooldown = false;
		TeleportCooldownValue = 0f;
		TeleportEnabled = true;
		JustTeleported = false;
		JustHitToolWithTeleport = false;
		lastTeleportTime = 0f;
		lastTeleportEnabledTime = 0f;
		thisPlayer = GetComponent<Player>();
		childNetworkTransforms = GetComponentsInChildren<PUNNetworkTransform>(true);
		trail = UnityEngine.Object.Instantiate(playerTrailPrefab);
		thisPlayer.SetParentPlayerRoot(trail.transform);
		AddTeleportCooldown(TeleportCooldownType.DEFAULT, 0f);
		PushTeleportMaxDistanceScalar(1f);
		PushMaxToolHitHoldTime(defaultMaxToolHitHoldTime);
		continuousRotationIsRunning = false;
	}

	private void Start()
	{
		if (thisPlayer.isLocal)
		{
			TeleportBuffer = SingletonMonoBehaviour<SettingsManager>.Instance.TeleportBuffer;
			MotionTeleportEnabled = SingletonMonoBehaviour<SettingsManager>.Instance.MotionTeleportEnabled;
			RotateInPlaceEnabled = SingletonMonoBehaviour<SettingsManager>.Instance.RotateInPlaceEnabled;
			RotateInPlaceIncrement = SingletonMonoBehaviour<SettingsManager>.Instance.RotationIncrement;
			ContinuousRotationMode = SingletonMonoBehaviour<SettingsManager>.Instance.ContinuousRotationMode;
		}
	}

	private void FixedUpdate()
	{
		UpdateTeleportCooldown();
	}

	public void Translate(Vector3 velocity)
	{
		TranslateCameraRig(SingletonMonoBehaviour<CameraRig>.Instance.transform.position + velocity * Time.deltaTime, SingletonMonoBehaviour<CameraRig>.Instance.transform.rotation);
	}

	public void Translate(Vector3 rigPosition, Quaternion rigRotation)
	{
		TranslateCameraRig(rigPosition, rigRotation);
	}

	public void TeleportTo(Vector3 destinationPlayerPosition, bool hasRotation, Vector3 rotationForward, TeleportationPortal portal, Tool hitTool, Vector3 hitToolPoint, Vector3 hitToolDirection)
	{
		PreparingToTeleport = false;
		Vector3 currentFloorPosition = thisPlayer.CurrentFloorPosition;
		if (teleportCoroutine != null)
		{
			return;
		}
		if (portal != null)
		{
			if (portal.SupportsActivityLoading && RecRoomSceneManager.Instance != null)
			{
				if (thisPlayer.PlayerParty.PartySize > 1)
				{
					thisPlayer.PlayerUI.Menu.RunSwitchActivity(portal.DestinationActivity, false);
				}
				else
				{
					RecRoomSceneManager.Instance.SwitchActivity(portal.DestinationActivity, false);
				}
				return;
			}
			if (portal.SupportsPlayerRespawn && RecRoomSceneManager.Instance != null && RecRoomSceneManager.Instance.GameManager != null)
			{
				RecRoomSceneManager.Instance.GameManager.SpawnManager.LocalPlayerRequestRespawn(!portal.CanCarryTools);
				return;
			}
			if (!portal.CanCarryTools)
			{
				thisPlayer.ReleaseToolsFromBothHands();
			}
		}
		if (thisPlayer.LeftHand.Tool != null)
		{
			thisPlayer.LeftHand.Tool.OnPlayerTeleport(teleportBlindDuration);
		}
		if (thisPlayer.RightHand.Tool != null)
		{
			thisPlayer.RightHand.Tool.OnPlayerTeleport(teleportBlindDuration);
		}
		teleportCoroutine = StartCoroutine(RunTeleportTo(currentFloorPosition, destinationPlayerPosition, hasRotation, rotationForward, teleportBlindDuration, portal, hitTool, hitToolPoint, hitToolDirection));
		thisPlayer.PlayerAudio.OnTeleport();
		if (thisPlayer.IsVisible)
		{
			base.photonView.RPC("RpcTeleport", PhotonTargets.Others, currentFloorPosition, (!(portal != null)) ? destinationPlayerPosition : portal.transform.position);
		}
		if (thisPlayer.PlayerUI.Menu != null && thisPlayer.PlayerUI.Menu.Visible)
		{
			thisPlayer.PlayerUI.Menu.Visible = false;
		}
		lastTeleportTime = Time.time;
		JustTeleported = true;
		JustHitToolWithTeleport = hitTool != null;
	}

	private void RunRotateTeleport(float rotationAngle, float duration)
	{
		if (rotateTeleportCoroutine != null)
		{
			StopCoroutine(rotateTeleportCoroutine);
			rotateTeleportCoroutine = null;
		}
		rotateTeleportCoroutine = StartCoroutine(RotateTeleportCoroutine(rotationAngle, duration));
	}

	private IEnumerator RotateTeleportCoroutine(float angle, float duration)
	{
		SingletonMonoBehaviour<CameraRig>.Instance.VignetteImageEffect.FadeIn(vignetteRotateFadeInTime);
		Vector3 startingForward = Vector3.ProjectOnPlane(thisPlayer.Head.transform.forward, Vector3.up);
		Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
		for (float timer = 0f; timer <= duration; timer += Time.deltaTime)
		{
			MoveCameraRig(forward: Quaternion.Slerp(Quaternion.identity, rotation, timer / duration) * startingForward, playerFloorPosition: thisPlayer.CurrentFloorPosition, insertDiscontinuity: false);
			yield return null;
		}
		MoveCameraRig(thisPlayer.CurrentFloorPosition, rotation * startingForward, false);
		SingletonMonoBehaviour<CameraRig>.Instance.VignetteImageEffect.FadeOut(vignetteRotateFadeOutTime);
	}

	public void Rotate(Vector3 playerForward)
	{
		playerForward = Vector3.ProjectOnPlane(playerForward, Vector3.up);
		Quaternion rotation = Quaternion.Euler(0f, SingletonMonoBehaviour<CameraRig>.Instance.transform.InverseTransformRotation(SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.rotation).eulerAngles.y, 0f);
		Quaternion rigRotation = Quaternion.LookRotation(playerForward, Vector3.up) * Quaternion.Inverse(rotation);
		Rotate(rigRotation);
	}

	public void Rotate(Quaternion rigRotation)
	{
		TranslateCameraRig(SingletonMonoBehaviour<CameraRig>.Instance.transform.position, rigRotation);
	}

	public void OnTeleportError()
	{
		thisPlayer.PlayerAudio.OnTeleportError();
	}

	public void RotateInPlaceLeft()
	{
		if (TryRotateInPlace(0f - RotateInPlaceAngle, inPlaceRotationSpeed))
		{
			thisPlayer.PlayerAudio.OnInPlaceRotateLeft();
			lastInPlaceRotationDirection = 1f;
		}
	}

	public void RotateInPlaceRight()
	{
		if (TryRotateInPlace(RotateInPlaceAngle, inPlaceRotationSpeed))
		{
			thisPlayer.PlayerAudio.OnInPlaceRotateRight();
			lastInPlaceRotationDirection = -1f;
		}
	}

	public void RotateInPlace180()
	{
		if (TryRotateInPlace(180f * lastInPlaceRotationDirection, inPlaceRotationSpeed))
		{
			thisPlayer.PlayerAudio.OnInPlaceRotate180();
		}
	}

	public void StartContinuousRotation()
	{
		if (!continuousRotationIsRunning)
		{
			initialContinuousRotationForward = Vector3.ProjectOnPlane(thisPlayer.transform.forward, Vector3.up).normalized;
			SingletonMonoBehaviour<CameraRig>.Instance.VignetteImageEffect.FadeIn(vignetteContinuousRotationFadeInTime, continuousRotationVignetteIntensity);
		}
		continuousRotationIsRunning = true;
	}

	public void StopContinuousRotation()
	{
		if (continuousRotationIsRunning)
		{
			SingletonMonoBehaviour<CameraRig>.Instance.VignetteImageEffect.FadeOut(vignetteContinuousRotationFadeOutTime);
		}
		continuousRotationIsRunning = false;
	}

	public void AddContinuousRotation(float deltaAngle)
	{
		if (continuousRotationIsRunning)
		{
			Vector3 normalized = Vector3.ProjectOnPlane(thisPlayer.transform.forward, Vector3.up).normalized;
			MoveCameraRig(thisPlayer.CurrentFloorPosition, Quaternion.Euler(0f, deltaAngle, 0f) * normalized, false, CoordinateSpace.TRACKING_SPACE);
		}
	}

	public void SetContinuousRotation(float angle)
	{
		if (continuousRotationIsRunning)
		{
			MoveCameraRig(thisPlayer.CurrentFloorPosition, Quaternion.Euler(0f, angle, 0f) * initialContinuousRotationForward, false, CoordinateSpace.TRACKING_SPACE);
		}
	}

	private bool TryRotateInPlace(float angle, float speed)
	{
		if (continuousRotationIsRunning || (PlatformManager.Instance.CurrentTrackingMode == PlatformManager.TrackingMode.THREE_SIXTY_DEGREE && !RotateInPlaceEnabled))
		{
			return false;
		}
		float duration = Mathf.Abs(angle) / speed;
		bool flag = true;
		flag &= thisPlayer.LeftHand.Tool == null || thisPlayer.LeftHand.Tool.OnPlayerTryingToRotateInPlace(angle, duration);
		flag &= thisPlayer.RightHand.Tool == null || thisPlayer.RightHand.Tool.OnPlayerTryingToRotateInPlace(angle, duration);
		if (flag)
		{
			RunRotateTeleport(angle, duration);
		}
		return flag;
	}

	public void MoveCameraRig(Vector3 playerFloorPosition, bool insertDiscontinuity = true)
	{
		MoveCameraRig(playerFloorPosition, SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.forward, insertDiscontinuity);
	}

	public void MoveCameraRig(Vector3 playerFloorPosition, Vector3 forward, bool insertDiscontinuity = true, CoordinateSpace forwardVectorCoordinateSpace = CoordinateSpace.PLAYER_SPACE)
	{
		forward = Vector3.ProjectOnPlane(forward, Vector3.up);
		Quaternion quaternion = Quaternion.identity;
		switch (forwardVectorCoordinateSpace)
		{
		case CoordinateSpace.PLAYER_SPACE:
		{
			Quaternion rotation = Quaternion.Euler(0f, SingletonMonoBehaviour<CameraRig>.Instance.transform.InverseTransformRotation(SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.rotation).eulerAngles.y, 0f);
			quaternion = Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Inverse(rotation);
			break;
		}
		case CoordinateSpace.TRACKING_SPACE:
			quaternion = Quaternion.LookRotation(forward, Vector3.up);
			break;
		}
		Vector3 position = playerFloorPosition - quaternion * SingletonMonoBehaviour<CameraRig>.Instance.CameraFloorOffsetLocalSpace;
		TranslateCameraRig(position, quaternion);
		if (insertDiscontinuity)
		{
			PUNNetworkTransform[] array = childNetworkTransforms;
			foreach (PUNNetworkTransform pUNNetworkTransform in array)
			{
				pUNNetworkTransform.InsertPositionDiscontinuity();
			}
			if (thisPlayer.LeftHand.Tool != null)
			{
				thisPlayer.LeftHand.Tool.SnapToHand();
			}
			if (thisPlayer.RightHand.Tool != null)
			{
				thisPlayer.RightHand.Tool.SnapToHand();
			}
		}
	}

	private void TranslateCameraRig(Vector3 position, Quaternion rotation)
	{
		position = position.ValueOrZeroIfBogus();
		rotation = rotation.ValueOrIdentityIfBogus();
		Vector3 position2 = SingletonMonoBehaviour<CameraRig>.Instance.transform.InverseTransformPoint(SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.position);
		Vector3 position3 = SingletonMonoBehaviour<CameraRig>.Instance.transform.InverseTransformPoint(SingletonMonoBehaviour<CameraRig>.Instance.LeftHandControllerIO.transform.position);
		Vector3 position4 = SingletonMonoBehaviour<CameraRig>.Instance.transform.InverseTransformPoint(SingletonMonoBehaviour<CameraRig>.Instance.RightHandControllerIO.transform.position);
		Quaternion rotation2 = SingletonMonoBehaviour<CameraRig>.Instance.transform.InverseTransformRotation(SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.rotation);
		Quaternion rotation3 = SingletonMonoBehaviour<CameraRig>.Instance.transform.InverseTransformRotation(SingletonMonoBehaviour<CameraRig>.Instance.LeftHandControllerIO.transform.rotation);
		Quaternion rotation4 = SingletonMonoBehaviour<CameraRig>.Instance.transform.InverseTransformRotation(SingletonMonoBehaviour<CameraRig>.Instance.RightHandControllerIO.transform.rotation);
		SingletonMonoBehaviour<CameraRig>.Instance.transform.rotation = rotation;
		SingletonMonoBehaviour<CameraRig>.Instance.transform.position = position;
		base.transform.position = position;
		base.transform.rotation = rotation;
		SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.position = SingletonMonoBehaviour<CameraRig>.Instance.transform.TransformPoint(position2);
		SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.rotation = SingletonMonoBehaviour<CameraRig>.Instance.transform.TransformRotation(rotation2);
		thisPlayer.Head.transform.position = SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.position;
		thisPlayer.Head.transform.rotation = SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.rotation;
		SingletonMonoBehaviour<CameraRig>.Instance.LeftHandControllerIO.transform.position = SingletonMonoBehaviour<CameraRig>.Instance.transform.TransformPoint(position3);
		SingletonMonoBehaviour<CameraRig>.Instance.LeftHandControllerIO.transform.rotation = SingletonMonoBehaviour<CameraRig>.Instance.transform.TransformRotation(rotation3);
		thisPlayer.LeftHand.transform.position = SingletonMonoBehaviour<CameraRig>.Instance.LeftHandControllerIO.transform.position;
		thisPlayer.LeftHand.transform.rotation = SingletonMonoBehaviour<CameraRig>.Instance.LeftHandControllerIO.transform.rotation;
		if (thisPlayer.LeftHand.Tool != null)
		{
			thisPlayer.LeftHand.Tool.RigidbodyPickup.InstantaneousUpdate();
		}
		SingletonMonoBehaviour<CameraRig>.Instance.RightHandControllerIO.transform.position = SingletonMonoBehaviour<CameraRig>.Instance.transform.TransformPoint(position4);
		SingletonMonoBehaviour<CameraRig>.Instance.RightHandControllerIO.transform.rotation = SingletonMonoBehaviour<CameraRig>.Instance.transform.TransformRotation(rotation4);
		thisPlayer.RightHand.transform.position = SingletonMonoBehaviour<CameraRig>.Instance.RightHandControllerIO.transform.position;
		thisPlayer.RightHand.transform.rotation = SingletonMonoBehaviour<CameraRig>.Instance.RightHandControllerIO.transform.rotation;
		if (thisPlayer.RightHand.Tool != null)
		{
			thisPlayer.RightHand.Tool.RigidbodyPickup.InstantaneousUpdate();
		}
	}

	private float GetChargeAmount(float maxTime)
	{
		return Mathf.Clamp01((Time.time - lastTeleportEnabledTime) / maxTime);
	}

	private void UpdateTeleportCooldown()
	{
		if (TeleportCooldown == 0f)
		{
			TeleportCooldownValue = 1f;
		}
		else
		{
			TeleportCooldownValue = Mathf.Clamp01((Time.time - lastTeleportTime) / TeleportCooldown);
		}
		TeleportOnCooldown = TeleportCooldownValue < 1f;
	}

	public IEnumerator RunTeleportTo(Vector3 sourcePlayerPosition, Vector3 destinationPlayerPosition, bool hasRotation, Vector3 rotationForward, float duration, TeleportationPortal portal, Tool hitTool, Vector3 hitToolPoint, Vector3 hitToolDirection)
	{
		if (portal != null)
		{
			SingletonMonoBehaviour<CameraRig>.Instance.Blind = true;
			CoordinateSpace portalTeleportRotationCoordinateSpace = CoordinateSpace.PLAYER_SPACE;
			if (PlatformManager.Instance.CurrentTrackingMode == PlatformManager.TrackingMode.ONE_EIGHTY_DEGREE)
			{
				portalTeleportRotationCoordinateSpace = CoordinateSpace.TRACKING_SPACE;
			}
			Vector3 newForward = Vector3.ProjectOnPlane((!portal.TargetRotationOverride.HasValue) ? portal.Target.forward : (portal.TargetRotationOverride.Value * Vector3.forward), Vector3.up);
			MoveCameraRig(destinationPlayerPosition, newForward, true, portalTeleportRotationCoordinateSpace);
			yield return new WaitForSeconds(duration);
			SingletonMonoBehaviour<CameraRig>.Instance.Blind = false;
		}
		else if (!MotionTeleportEnabled || hasRotation)
		{
			SingletonMonoBehaviour<CameraRig>.Instance.Blind = true;
			if (hasRotation)
			{
				MoveCameraRig(destinationPlayerPosition, Vector3.ProjectOnPlane(rotationForward, Vector3.up).normalized);
			}
			else
			{
				MoveCameraRig(destinationPlayerPosition);
			}
			yield return new WaitForSeconds(duration);
			SingletonMonoBehaviour<CameraRig>.Instance.Blind = false;
		}
		else
		{
			SingletonMonoBehaviour<CameraRig>.Instance.VignetteImageEffect.FadeIn(vignetteTeleportFadeInTime);
			for (float timer = 0f; timer <= duration; timer += Time.deltaTime)
			{
				Vector3 currentPlayerPosition = Vector3.Lerp(sourcePlayerPosition, destinationPlayerPosition, Mathf.Clamp01(timer / duration));
				MoveCameraRig(currentPlayerPosition, false);
				yield return null;
			}
			MoveCameraRig(destinationPlayerPosition);
			SingletonMonoBehaviour<CameraRig>.Instance.VignetteImageEffect.FadeOut(vignetteTeleportFadeOutTime);
		}
		PlayTeleportTrail(sourcePlayerPosition, destinationPlayerPosition);
		if (hitTool != null)
		{
			float num = toolHitForceCurve.Evaluate(ChargeAmount);
			hitTool.ApplyForce(hitToolPoint, hitToolDirection * num, IsMaxToolHitHoldTimeImproved);
		}
		FireTeleportEvent(sourcePlayerPosition, destinationPlayerPosition, portal);
		teleportCoroutine = null;
	}

	[PunRPC]
	public void RpcTeleport(Vector3 sourceFloorPosition, Vector3 destinationFloorPosition)
	{
		if (thisPlayer.IsVisible)
		{
			thisPlayer.PlayerAudio.OnTeleport();
			PlayTeleportTrail(sourceFloorPosition, destinationFloorPosition);
			FireTeleportEvent(sourceFloorPosition, destinationFloorPosition, null);
		}
	}

	private void FireTeleportEvent(Vector3 sourcePosition, Vector3 destinationPosition, TeleportationPortal portal)
	{
		if (this.TeleportEvent != null)
		{
			this.TeleportEvent(thisPlayer, sourcePosition, destinationPosition, portal);
		}
		if (portal != null)
		{
			portal.FirePlayerUseEvent(thisPlayer);
		}
		thisPlayer.PlayerEvents.Teleport();
	}

	private void PlayTeleportTrail(Vector3 sourcePosition, Vector3 destinationPosition)
	{
		GameTeam team = thisPlayer.Team;
		if (team != GameTeam.INVALID)
		{
			trail.Color = GameTeamSettings.GetTeamColor(team);
		}
		trail.PlayTeleportTrail(thisPlayer.Head.HeightOffset, sourcePosition, destinationPosition);
	}

	public void OnPlayerDespawn()
	{
		trail.Clear();
	}
}
