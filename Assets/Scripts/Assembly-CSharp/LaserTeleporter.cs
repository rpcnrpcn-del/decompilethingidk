using System;
using UnityEngine;

public class LaserTeleporter : MonoBehaviour
{
	public enum LaserMode
	{
		ValidTeleportPosition = 0,
		InvalidTeleportPosition = 1,
		InvalidTeleportDirection = 2,
		RestrictedTeleportPosition = 3,
		ValidPortalPosition = 4,
		Cooldown = 5,
		Disabled = 6,
		ValidTeleportPositionWithRotation = 7,
		InvalidPortalPosition = 8,
		ValidLevelLoadPortal = 9
	}

	private enum MovementActionState
	{
		NONE = 0,
		PREPARING = 1,
		RUNNING = 2
	}

	private struct TeleportSelectionData
	{
		public Vector3 Position;

		public Tool HitTool;

		public Vector3 HitToolPosition;

		public Vector3 HitToolNormal;

		public Vector3 HitToolDirection;
	}

	[SerializeField]
	private Transform laserStartPosition;

	[SerializeField]
	private Transform visualRoot;

	[SerializeField]
	private ParabolicPointer parabolicPointer;

	[Header("Visual feedback")]
	[SerializeField]
	private Renderer cooldownRenderer;

	[SerializeField]
	private Transform cooldownVisualCenter;

	[Header("Teleportation")]
	[SerializeField]
	private PlayerTeleportDecal playerTeleportDecalPrefab;

	[Tooltip("We will use this radius in CheckSphere to prevent you from teleporting into other sh.t")]
	[SerializeField]
	private float headCollisionRadius = 0.2f;

	[SerializeField]
	private float toolHitWindow = 0.25f;

	[SerializeField]
	private float teleportRotationMinChargeTime = 0.2f;

	[Header("Continous Rotation")]
	[SerializeField]
	private float continousRotationInputStepSize = 7.5f;

	[SerializeField]
	private float continousRotationOutputStepSize = 22.5f;

	[SerializeField]
	private float continuousRotationInputScalar = 2f;

	[Header("Menu")]
	[SerializeField]
	private LineRenderer menuLaser;

	[Header("Collision")]
	[SerializeField]
	private BoxCollider worldOverlapCollider;

	private PlayerTeleportDecal playerTeleportDecal;

	private GameObject pointer;

	private float lastToolHitSelectionTime;

	private TeleportSelectionData lastToolHitData;

	private float teleportRotationChargeStartTime;

	private bool teleportRotationEnabled;

	private PlayerHand otherHand;

	private MovementActionState _teleportState;

	private MovementActionState currentRotationState;

	private MovementActionState continousRotationState;

	private Vector3 lastContinuousRotationVector = Vector3.zero;

	[NonSerialized]
	public Vector3 TeleportTarget;

	private LaserMode laserMode = LaserMode.Disabled;

	public static float FlightSpeed = 10f;

	public ParabolicPointer ParabolicPointer
	{
		get
		{
			return parabolicPointer;
		}
	}

	public Vector3 TeleportRotationForward { get; private set; }

	[HideInInspector]
	public Player Player { get; internal set; }

	public PlayerHand Hand { get; internal set; }

	public bool Visible
	{
		get
		{
			return visualRoot.gameObject.activeSelf;
		}
		set
		{
			visualRoot.gameObject.SetActive(value);
		}
	}

	public bool TeleportLaserVisible
	{
		get
		{
			return Mode != LaserMode.Disabled;
		}
	}

	public bool MenuLaserVisible
	{
		get
		{
			return menuLaser.enabled;
		}
	}

	private bool ToolHitAvailable
	{
		get
		{
			return IsWithinToolHitWindow(toolHitWindow);
		}
	}

	private bool ToolHitThisFrame
	{
		get
		{
			return IsWithinToolHitWindow(2f * Time.deltaTime);
		}
	}

	private bool IsOutOfBounds
	{
		get
		{
			return Player.IsOutOfBounds || worldOverlapCollider.CheckOverlap(2048, QueryTriggerInteraction.Ignore);
		}
	}

	private MovementActionState teleportState
	{
		get
		{
			return _teleportState;
		}
		set
		{
			if ((_teleportState == MovementActionState.RUNNING && value != MovementActionState.RUNNING) || _teleportState == MovementActionState.RUNNING || value == MovementActionState.RUNNING)
			{
			}
			_teleportState = value;
		}
	}

	public LaserMode Mode
	{
		get
		{
			return laserMode;
		}
		set
		{
			laserMode = value;
			switch (laserMode)
			{
			case LaserMode.Disabled:
				parabolicPointer.Mode = ParabolicPointer.MeshMode.Disabled;
				playerTeleportDecal.VisibilityMode = PlayerTeleportDecal.DecalVisibilityMode.None;
				break;
			case LaserMode.InvalidTeleportPosition:
				parabolicPointer.Mode = ParabolicPointer.MeshMode.FadeOut;
				playerTeleportDecal.VisibilityMode = PlayerTeleportDecal.DecalVisibilityMode.None;
				break;
			case LaserMode.InvalidTeleportDirection:
				parabolicPointer.Mode = ParabolicPointer.MeshMode.Shortened;
				playerTeleportDecal.VisibilityMode = PlayerTeleportDecal.DecalVisibilityMode.None;
				break;
			case LaserMode.RestrictedTeleportPosition:
				parabolicPointer.Mode = ParabolicPointer.MeshMode.Invalid;
				playerTeleportDecal.VisibilityMode = PlayerTeleportDecal.DecalVisibilityMode.Ground;
				playerTeleportDecal.IsValidPosition = false;
				break;
			case LaserMode.ValidTeleportPosition:
				parabolicPointer.Mode = ParabolicPointer.MeshMode.Valid;
				playerTeleportDecal.VisibilityMode = PlayerTeleportDecal.DecalVisibilityMode.AvatarAndGround;
				playerTeleportDecal.IsValidPosition = true;
				break;
			case LaserMode.ValidTeleportPositionWithRotation:
				parabolicPointer.Mode = ParabolicPointer.MeshMode.Valid;
				playerTeleportDecal.VisibilityMode = PlayerTeleportDecal.DecalVisibilityMode.AvatarAndGroundAndArrow;
				playerTeleportDecal.IsValidPosition = true;
				break;
			case LaserMode.ValidPortalPosition:
				parabolicPointer.Mode = ParabolicPointer.MeshMode.Valid;
				playerTeleportDecal.VisibilityMode = PlayerTeleportDecal.DecalVisibilityMode.AvatarAndGround;
				playerTeleportDecal.IsValidPosition = true;
				break;
			case LaserMode.InvalidPortalPosition:
				parabolicPointer.Mode = ParabolicPointer.MeshMode.Invalid;
				playerTeleportDecal.VisibilityMode = PlayerTeleportDecal.DecalVisibilityMode.Ground;
				playerTeleportDecal.IsValidPosition = false;
				break;
			case LaserMode.Cooldown:
				parabolicPointer.Mode = ParabolicPointer.MeshMode.Invisible;
				playerTeleportDecal.VisibilityMode = PlayerTeleportDecal.DecalVisibilityMode.None;
				break;
			case LaserMode.ValidLevelLoadPortal:
				parabolicPointer.Mode = ParabolicPointer.MeshMode.Valid;
				playerTeleportDecal.VisibilityMode = PlayerTeleportDecal.DecalVisibilityMode.None;
				playerTeleportDecal.IsValidPosition = true;
				break;
			}
		}
	}

	private void Awake()
	{
		playerTeleportDecal = UnityEngine.Object.Instantiate(playerTeleportDecalPrefab);
		playerTeleportDecal.VisibilityMode = PlayerTeleportDecal.DecalVisibilityMode.None;
		parabolicPointer.transform.localPosition = laserStartPosition.localPosition;
		parabolicPointer.LaserTeleporter = this;
		visualRoot.gameObject.SetActive(false);
		menuLaser.enabled = false;
	}

	private void Start()
	{
		Player.SetParentPlayerRoot(playerTeleportDecal.transform);
		GameObject obj = base.gameObject;
		obj.name = obj.name + "_" + Hand.Type;
		otherHand = ((Hand.Type != PlayerHand.HandType.Left) ? Player.LeftHand : Player.RightHand);
		parabolicPointer.StraightLaser = false;
	}

	private void OnDestroy()
	{
		if (pointer != null)
		{
			UnityEngine.Object.Destroy(pointer.gameObject);
		}
		if (playerTeleportDecal != null)
		{
			UnityEngine.Object.Destroy(playerTeleportDecal.gameObject);
		}
	}

	private void OnDisable()
	{
		if (pointer != null)
		{
			pointer.gameObject.SetActive(false);
		}
		if (playerTeleportDecal != null)
		{
			playerTeleportDecal.VisibilityMode = PlayerTeleportDecal.DecalVisibilityMode.None;
		}
	}

	private void Update()
	{
		if (Hand != null)
		{
			base.transform.position = Hand.transform.position;
			base.transform.rotation = Hand.transform.rotation;
		}
		else
		{
			Debug.LogError("Laser teleporter update without Hand should never happen.");
		}
		if (Hand.isLocal)
		{
			UpdateTeleportation();
			UpdateInPlaceRotation();
			UpdateContinuousRotation();
			UpdateMenuLaser();
			UpdateButtonMaterial();
		}
		UpdateDecal();
	}

	private void UpdateTeleportation()
	{
		bool teleportButtonDown = Hand.ControllerIO != null && Hand.ControllerIO.TeleportButtonDown;
		bool teleportButtonPressed = Hand.ControllerIO != null && Hand.ControllerIO.TeleportButtonPressed;
		bool teleportButtonUp = Hand.ControllerIO != null && Hand.ControllerIO.TeleportButtonUp;
		ClearTeleportLaser();
		switch (teleportState)
		{
		case MovementActionState.NONE:
			UpdateTeleportNoneState(teleportButtonDown, teleportButtonPressed, teleportButtonUp);
			break;
		case MovementActionState.RUNNING:
			UpdateTeleportRunningState(teleportButtonDown, teleportButtonPressed, teleportButtonUp);
			break;
		}
	}

	private void UpdateInPlaceRotation()
	{
		if (Hand.PlayerHandRemote.teleportState == MovementActionState.NONE && otherHand.PlayerHandRemote.teleportState == MovementActionState.NONE && Hand.ControllerIO != null)
		{
			if (Hand.ControllerIO.RotateLeftButtonDown)
			{
				Player.PlayerLocomotion.RotateInPlaceLeft();
			}
			else if (Hand.ControllerIO.RotateRightButtonDown)
			{
				Player.PlayerLocomotion.RotateInPlaceRight();
			}
			else if (Hand.ControllerIO.RotateDownButtonDown)
			{
				Player.PlayerLocomotion.RotateInPlace180();
			}
		}
	}

	private void UpdateContinuousRotation()
	{
		bool buttonPressed = Hand.ControllerIO != null && Hand.ControllerIO.PushToTalkButtonPressed && otherHand.ControllerIO != null && otherHand.ControllerIO.PushToTalkButtonPressed;
		Debug.DrawLine(Player.CurrentFloorPosition, Player.CurrentFloorPosition + lastContinuousRotationVector, Color.green);
		switch (continousRotationState)
		{
		case MovementActionState.NONE:
			UpdateContinousRotationNoneState(buttonPressed);
			break;
		case MovementActionState.RUNNING:
			UpdateContinuousRotationRunningState(buttonPressed);
			break;
		}
	}

	private void UpdateContinousRotationNoneState(bool buttonPressed)
	{
		this.ThrowIfNull(Player, "Player");
		this.ThrowIfNull(Player.PlayerLocomotion, "Player.PlayerLocomotion");
		this.ThrowIfNull(Hand, "Hand");
		this.ThrowIfNull(Hand.PlayerHandRemote, "Hand.PlayerHandRemote");
		this.ThrowIfNull(otherHand, "otherHand");
		this.ThrowIfNull(otherHand.PlayerHandRemote, "otherHand.PlayerHandRemote");
		if (Player.PlayerLocomotion.ContinuousRotationMode != ContinuousRotationMode.DISABLED && otherHand.PlayerHandRemote.continousRotationState == MovementActionState.NONE && Hand.PlayerHandRemote.teleportState == MovementActionState.NONE && otherHand.PlayerHandRemote.teleportState == MovementActionState.NONE && buttonPressed && TryGetContinuousRotationVector(out lastContinuousRotationVector))
		{
			Player.PlayerLocomotion.StartContinuousRotation();
			continousRotationState = MovementActionState.RUNNING;
			UpdateContinuousRotationRunningState(buttonPressed);
		}
	}

	private void UpdateContinuousRotationRunningState(bool buttonPressed)
	{
		Vector3 continuousRotationVector;
		if (Player.PlayerLocomotion.ContinuousRotationMode == ContinuousRotationMode.DISABLED || otherHand.PlayerHandRemote.continousRotationState != MovementActionState.NONE || Hand.PlayerHandRemote.teleportState != MovementActionState.NONE || otherHand.PlayerHandRemote.teleportState != MovementActionState.NONE || !buttonPressed || !TryGetContinuousRotationVector(out continuousRotationVector))
		{
			Player.PlayerLocomotion.StopContinuousRotation();
			continousRotationState = MovementActionState.NONE;
			return;
		}
		float num = lastContinuousRotationVector.AngleSignedVector3(continuousRotationVector);
		if (Player.PlayerLocomotion.ContinuousRotationMode == ContinuousRotationMode.DISCRETE && Mathf.Abs(num) >= continousRotationInputStepSize)
		{
			lastContinuousRotationVector = continuousRotationVector;
			Player.PlayerLocomotion.AddContinuousRotation(continousRotationOutputStepSize * Mathf.Sign(num));
		}
		else if (Player.PlayerLocomotion.ContinuousRotationMode == ContinuousRotationMode.CONTINUOUS)
		{
			Player.PlayerLocomotion.SetContinuousRotation(num * continuousRotationInputScalar);
		}
	}

	private bool TryGetContinuousRotationVector(out Vector3 continuousRotationVector)
	{
		Vector3 vector = Vector3.ProjectOnPlane(Player.RightHand.transform.position - Player.LeftHand.transform.position, Vector3.up);
		if (vector.sqrMagnitude <= 0.01f)
		{
			continuousRotationVector = Vector3.zero;
			return false;
		}
		continuousRotationVector = Vector3.Cross(vector.normalized, Vector3.up);
		continuousRotationVector = Player.transform.InverseTransformDirection(continuousRotationVector).normalized;
		return true;
	}

	private void UpdateTeleportNoneState(bool teleportButtonDown, bool teleportButtonPressed, bool teleportButtonUp)
	{
		if (otherHand.PlayerHandRemote.teleportState == MovementActionState.NONE && teleportButtonDown)
		{
			teleportState = MovementActionState.RUNNING;
			UpdateTeleportRunningState(teleportButtonDown, teleportButtonPressed, teleportButtonUp);
		}
	}

	private void UpdateTeleportRunningState(bool teleportButtonDown, bool teleportButtonPressed, bool teleportButtonUp)
	{
		if (otherHand.PlayerHandRemote.teleportState != MovementActionState.NONE || (!teleportButtonPressed && !teleportButtonUp))
		{
			teleportState = MovementActionState.NONE;
			return;
		}
		Mode = UpdateLaser(parabolicPointer);
		UpdateTeleportRotation();
		if (currentRotationState == MovementActionState.RUNNING && Mode == LaserMode.ValidTeleportPosition)
		{
			Mode = LaserMode.ValidTeleportPositionWithRotation;
		}
		if (teleportButtonUp)
		{
			teleportState = MovementActionState.NONE;
			if (Mode == LaserMode.ValidPortalPosition || Mode == LaserMode.ValidLevelLoadPortal)
			{
				PerformPortalTeleport(parabolicPointer.TeleportationPortal);
			}
			else if (Mode == LaserMode.ValidTeleportPosition)
			{
				PerformTeleport(TeleportTarget, parabolicPointer.HitTool, parabolicPointer.HitToolPosition, parabolicPointer.HitToolNormal);
			}
			else if (Mode == LaserMode.ValidTeleportPositionWithRotation)
			{
				PerformTeleport(TeleportTarget, TeleportRotationForward, parabolicPointer.HitTool, parabolicPointer.HitToolPosition, parabolicPointer.HitToolNormal);
			}
		}
	}

	private void ClearTeleportLaser()
	{
		Mode = LaserMode.Disabled;
		parabolicPointer.ClearParabolicRaycasts();
	}

	private LaserMode UpdateLaser(ParabolicPointer laser)
	{
		if (!Player.PlayerLocomotion.TeleportEnabled || Player.PlayerLocomotion.TeleportSuppressed || Player.PlayerLocomotion.TeleportOnCooldown || IsOutOfBounds)
		{
			return LaserMode.Cooldown;
		}
		if (Vector3.Angle(base.transform.forward, Vector3.up) < Player.PlayerLocomotion.MaxTeleportPitchDegrees)
		{
			return LaserMode.InvalidTeleportDirection;
		}
		laser.PerformParabolicRaycasts(Player.PlayerLocomotion.MaxTeleportDistance, Player.PlayerLocomotion.ChargeAmount);
		if (laser.HitTool != null)
		{
			lastToolHitSelectionTime = Time.time;
			lastToolHitData = new TeleportSelectionData
			{
				Position = laser.FinalParabolaPoint,
				HitTool = laser.HitTool,
				HitToolPosition = laser.HitToolPosition,
				HitToolNormal = laser.HitToolNormal,
				HitToolDirection = (laser.FinalParabolaPoint - base.transform.position).normalized
			};
		}
		if (!laser.HitSomething)
		{
			return LaserMode.InvalidTeleportPosition;
		}
		if (laser.TeleportationPortal != null && laser.TeleportationPortal.IsValid)
		{
			TeleportTarget = laser.TeleportationPortal.DestinationPosition;
			if (laser.TeleportationPortal.SupportsActivityLoading || laser.TeleportationPortal.SupportsPlayerRespawn)
			{
				return LaserMode.ValidLevelLoadPortal;
			}
			if (CheckSafeTeleportPosition(TeleportTarget))
			{
				return LaserMode.ValidPortalPosition;
			}
			return LaserMode.InvalidPortalPosition;
		}
		TeleportTarget = laser.FinalParabolaPoint;
		if (laser.PointOnTeleportRegion && CheckSafeTeleportPosition(laser.FinalParabolaPoint) && (laser.TeleportRestrictions == null || laser.TeleportRestrictions.LocalPlayerCanTeleport))
		{
			return LaserMode.ValidTeleportPosition;
		}
		return LaserMode.RestrictedTeleportPosition;
	}

	private void UpdateTeleportRotation()
	{
		bool rotatingPressed = otherHand.ControllerIO != null && !otherHand.ControllerIO.TeleportRotateAxisInDeadZone;
		switch (currentRotationState)
		{
		case MovementActionState.NONE:
			UpdateTeleportRotationNoneState(rotatingPressed);
			break;
		case MovementActionState.PREPARING:
			UpdateTeleportRotationPrepareState(rotatingPressed);
			break;
		case MovementActionState.RUNNING:
			UpdateTeleportRotationChargingState(rotatingPressed);
			break;
		}
	}

	private void UpdateTeleportRotationNoneState(bool rotatingPressed)
	{
		if (teleportRotationEnabled && rotatingPressed)
		{
			teleportRotationChargeStartTime = Time.time;
			currentRotationState = MovementActionState.PREPARING;
			UpdateTeleportRotationPrepareState(rotatingPressed);
		}
	}

	private void UpdateTeleportRotationPrepareState(bool rotatingPressed)
	{
		if (!rotatingPressed)
		{
			currentRotationState = MovementActionState.NONE;
			UpdateTeleportRotationNoneState(rotatingPressed);
		}
		else if (Time.time - teleportRotationChargeStartTime >= teleportRotationMinChargeTime)
		{
			currentRotationState = MovementActionState.RUNNING;
			UpdateTeleportRotationChargingState(rotatingPressed);
		}
	}

	private void UpdateTeleportRotationChargingState(bool rotatingPressed)
	{
		if (!rotatingPressed)
		{
			currentRotationState = MovementActionState.NONE;
			UpdateTeleportRotationNoneState(rotatingPressed);
		}
		else
		{
			TeleportRotationForward = RotationVectorFromRotationAxis(otherHand.ControllerIO.TeleportRotateAxis);
		}
	}

	private Vector3 RotationVectorFromRotationAxis(Vector2 rotationAxis)
	{
		Vector3 normalized = Vector3.ProjectOnPlane(Player.Head.transform.forward, Vector3.up).normalized;
		Vector3 vector = -Vector3.Cross(normalized, Vector3.up).normalized;
		return normalized * rotationAxis.y + vector * rotationAxis.x;
	}

	private void UpdateMenuLaser()
	{
		if (Mode != LaserMode.Disabled || !Hand.ControllerIO.UIRaycastPosition.HasValue)
		{
			menuLaser.enabled = false;
			return;
		}
		menuLaser.enabled = true;
		Vector3 value = Hand.ControllerIO.UIRaycastPosition.Value;
		menuLaser.SetPosition(0, laserStartPosition.position);
		menuLaser.SetPosition(1, value);
		Color color = menuLaser.material.color;
		color.a = ((!Hand.ControllerIO.UISelectable) ? 0.3f : 0.85f);
		menuLaser.material.color = color;
	}

	private void UpdateDecal()
	{
		if (playerTeleportDecal.VisibilityMode != PlayerTeleportDecal.DecalVisibilityMode.None)
		{
			playerTeleportDecal.UpdateAvatarDecal(Player, TeleportTarget, currentRotationState == MovementActionState.RUNNING, TeleportRotationForward);
			bool flag = Hand.isLocal && ToolHitThisFrame;
			playerTeleportDecal.UpdateToolHitDecal(flag, Player.PlayerLocomotion.ChargeAmount, lastToolHitData.HitToolPosition, lastToolHitData.HitToolNormal);
		}
	}

	public void PerformPortalTeleport(TeleportationPortal portal)
	{
		Player.PlayerLocomotion.TeleportTo(TeleportTarget, false, Vector3.zero, portal, null, Vector3.zero, Vector3.zero);
	}

	public void PerformTeleport(Vector3 position, Tool hitTool, Vector3 hitToolPosition, Vector3 hitToolNormal)
	{
		if (ToolHitAvailable)
		{
			Player.PlayerLocomotion.TeleportTo(lastToolHitData.Position, false, Vector3.zero, null, lastToolHitData.HitTool, lastToolHitData.HitToolPosition, lastToolHitData.HitToolDirection);
		}
		else
		{
			Player.PlayerLocomotion.TeleportTo(position, false, Vector3.zero, null, null, Vector3.zero, Vector3.zero);
		}
		Hand.Vibrate(1, 1000);
	}

	public void PerformTeleport(Vector3 position, Vector3 rotationForward, Tool hitTool, Vector3 hitToolPosition, Vector3 hitToolNormal)
	{
		if (ToolHitAvailable)
		{
			Player.PlayerLocomotion.TeleportTo(lastToolHitData.Position, true, rotationForward, null, lastToolHitData.HitTool, lastToolHitData.HitToolPosition, lastToolHitData.HitToolDirection);
		}
		else
		{
			Player.PlayerLocomotion.TeleportTo(position, true, rotationForward, null, null, Vector3.zero, Vector3.zero);
		}
		Hand.Vibrate(1, 1000);
	}

	private bool IsWithinToolHitWindow(float window)
	{
		return Time.time - lastToolHitSelectionTime < window;
	}

	private bool IsValidTeleportMode(LaserMode mode)
	{
		return mode != LaserMode.Disabled;
	}

	private bool CheckSafeTeleportPosition(Vector3 newPlayerFloorPosition)
	{
		Vector3 position = newPlayerFloorPosition + Player.Head.HeightOffset;
		if (Physics.CheckSphere(position, headCollisionRadius, 3073, QueryTriggerInteraction.Ignore))
		{
			return false;
		}
		PhotonPlayer[] otherPlayers = PhotonNetwork.otherPlayers;
		foreach (PhotonPlayer photonPlayer in otherPlayers)
		{
			Player player = photonPlayer.ToPlayer();
			if (player != null)
			{
				float magnitude = (player.CurrentFloorPosition - newPlayerFloorPosition).magnitude;
				float num = ((!Player.PlayerLocomotion.HasOverrideTeleportBuffer) ? player.PlayerLocomotion.TeleportBufferDistance : Player.PlayerLocomotion.OverrideTeleportBuffer);
				if (magnitude <= num)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void UpdateButtonMaterial()
	{
		float teleportCooldownValue = Player.PlayerLocomotion.TeleportCooldownValue;
		Vector3 vector = visualRoot.InverseTransformPoint(cooldownVisualCenter.position);
		Vector3 vector2 = visualRoot.InverseTransformDirection(cooldownVisualCenter.up);
		Vector3 vector3 = visualRoot.InverseTransformDirection(cooldownVisualCenter.forward);
		Material[] materials = cooldownRenderer.materials;
		foreach (Material material in materials)
		{
			material.SetVector("_PlaneCenterLocal", vector);
			material.SetVector("_PlaneNormalLocal", vector2);
			material.SetVector("_PlaneForwardLocal", vector3);
			material.SetFloat("_Progress", teleportCooldownValue);
		}
	}
}
