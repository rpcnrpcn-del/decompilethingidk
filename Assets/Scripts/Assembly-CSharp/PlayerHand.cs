using System;
using System.Collections;
using Photon;
using UnityEngine;

public class PlayerHand : Photon.MonoBehaviour, IPunObservable
{
	public enum HandType
	{
		Unknown = -1,
		Left = 0,
		Right = 1
	}

	public enum PickupDirectionMode
	{
		HEAD_TO_HAND = 0,
		HAND_HALF_VECTOR = 1
	}

	private static int openClosedAnimatorId = Animator.StringToHash("OpenClosed");

	private static int handshakeAnimatorId = Animator.StringToHash("Handshake");

	private static int thumbsUpAnimatorId = Animator.StringToHash("ThumbsUp");

	[Header("Configuration")]
	[SerializeField]
	private HandType type = HandType.Unknown;

	[SerializeField]
	private float preReleaseWindow = 0.05f;

	[Tooltip("Create a window of time before and after the hair trigger is pressed to attempt picking up items.")]
	[SerializeField]
	private float hairTriggerReactionWindow = 0.1f;

	[Header("Magnet Pickup")]
	[SerializeField]
	private float defaultGravityPickupFingerOffset;

	[SerializeField]
	private float defaultGravityPickupCapsuleLength = 2f;

	[SerializeField]
	private float defaultGravityPickupCapsuleRadius = 0.15f;

	[SerializeField]
	private float defaultGravityPickupSphereRadius = 0.25f;

	[Header("Physical Pickup")]
	[SerializeField]
	private float defaultPhysicalPickupSphereRadius = 0.11f;

	[Header("Remote")]
	[SerializeField]
	private LaserTeleporter playerHandRemotePrefab;

	[Header("Gestures")]
	[SerializeField]
	private Transform handshakeConnectionTransform;

	[SerializeField]
	private Transform fistbumpConnectionTransform;

	[SerializeField]
	private Transform highFiveConnectionTransform;

	[Header("Component References")]
	[SerializeField]
	private Transform handCenter;

	[SerializeField]
	private Transform meshRoot;

	[SerializeField]
	private Transform watchMenuOrigin;

	[SerializeField]
	private Transform watchAnchor;

	private Rigidbody rigidbody;

	private PlayerHandRigidbodyPickup rigidbodyPickup;

	private PlayerHandCollider[] handColliders;

	private Animator handAnimator;

	private Tool previousHoveredTool;

	private float previousHoveredToolTime;

	private bool hoveredToolInPhysicalPickupRange;

	private Vector3 gravityPickupVector;

	private Tool _hoveredTool;

	private string controllerTrackingProperty = "controllerTrackingProperty";

	[HideInInspector]
	public Player ThisPlayer;

	private GameManager gameManager;

	private OutfitTrigger _triggeredOutfit;

	private TeleportationPortal _triggeredPortal;

	private ControllerIO _controllerIO;

	private bool isHandTrackingCached;

	private Renderer[] visualRenderers;

	private bool _handMeshesVisible = true;

	private float inPersonalBubbleTimeout;

	private float lastTimeInPersonalBubble;

	private bool _inPersonalBubble;

	private bool wasRemoteVisible;

	private bool wasToolVisible;

	private bool wasVisible = true;

	private bool wasHandTracking = true;

	private const float remoteHideGracePeriod = 0.01f;

	private float lastRemoteShowTime;

	private const int MAX_RAYCAST_HITS = 256;

	private static Collider[] colliderHits = new Collider[256];

	private static RaycastHit[] raycastHits = new RaycastHit[256];

	private float outfitForwardPickupRange = 0.15f;

	private float outfitBackwardPickupRange = 0.1f;

	public LaserTeleporter PlayerHandRemote { get; private set; }

	public float GravityPickupFingerOffset { get; set; }

	public float GravityPickupCapsuleLength { get; set; }

	public float GravityPickupCapsuleRadius { get; set; }

	public float GravityPickupSphereRadius { get; set; }

	public float PhysicalPickupSphereRadius { get; set; }

	public PickupDirectionMode GravityPickupDirectionMode { get; set; }

	public Vector3 GravityPickupOrigin
	{
		get
		{
			return handCenter.position + handCenter.forward * GravityPickupFingerOffset;
		}
	}

	public Vector3 GravityPickupDirection
	{
		get
		{
			PickupDirectionMode gravityPickupDirectionMode = GravityPickupDirectionMode;
			if (gravityPickupDirectionMode == PickupDirectionMode.HAND_HALF_VECTOR)
			{
				return (handCenter.forward + handCenter.right).normalized;
			}
			return (GravityPickupOrigin - ThisPlayer.Head.transform.position).normalized;
		}
	}

	private Tool hoveredTool
	{
		get
		{
			if (_hoveredTool == null && previousHoveredTool != null && Time.unscaledTime - previousHoveredToolTime <= hairTriggerReactionWindow)
			{
				return previousHoveredTool;
			}
			return _hoveredTool;
		}
		set
		{
			if (_hoveredTool != value)
			{
				if (_hoveredTool != null)
				{
					_hoveredTool.OnHoverEnd();
				}
				previousHoveredTool = _hoveredTool;
				previousHoveredToolTime = Time.unscaledTime;
				_hoveredTool = value;
				if (_hoveredTool != null)
				{
					_hoveredTool.OnHoverStart(hoveredToolInPhysicalPickupRange);
				}
			}
		}
	}

	public OutfitTrigger TriggeredOutfit
	{
		get
		{
			return _triggeredOutfit;
		}
		set
		{
			if (_triggeredOutfit != value)
			{
				if (_triggeredOutfit != null)
				{
					_triggeredOutfit.OutfitItem.Highlight = false;
				}
				_triggeredOutfit = value;
				if (_triggeredOutfit != null)
				{
					_triggeredOutfit.OutfitItem.Highlight = true;
				}
			}
		}
	}

	public TeleportationPortal TriggeredPortal
	{
		get
		{
			return _triggeredPortal;
		}
		set
		{
			if (_triggeredPortal != value)
			{
				if (_triggeredPortal != null)
				{
					_triggeredPortal.Highlight = false;
				}
				_triggeredPortal = value;
				if (_triggeredPortal != null)
				{
					_triggeredPortal.Highlight = true;
				}
			}
		}
	}

	public PUNNetworkTransform NetworkTransform { get; private set; }

	public PlayerHandGestures Gestures { get; private set; }

	public ControllerIO ControllerIO
	{
		get
		{
			return _controllerIO;
		}
		set
		{
			if (_controllerIO != value)
			{
				if ((bool)_controllerIO)
				{
					_controllerIO.TransformUpdated -= OnTrackerTransformUpdate;
				}
				_controllerIO = value;
				if ((bool)_controllerIO)
				{
					_controllerIO.TransformUpdated += OnTrackerTransformUpdate;
				}
			}
		}
	}

	public Tool Tool { get; private set; }

	public HandType Type
	{
		get
		{
			return type;
		}
	}

	public Animator HandAnimator
	{
		get
		{
			return handAnimator;
		}
	}

	public TrackedVelocity TrackedVelocity { get; private set; }

	public bool IsHoldingTool
	{
		get
		{
			return Tool != null;
		}
	}

	public Vector3 PalmDirection
	{
		get
		{
			return (type != HandType.Left) ? (-base.transform.right) : base.transform.right;
		}
	}

	public Vector3 FingerDirection
	{
		get
		{
			return base.transform.forward;
		}
	}

	public Vector3 ThumbDirection
	{
		get
		{
			return base.transform.up;
		}
	}

	public Transform WatchMenuOrigin
	{
		get
		{
			return watchMenuOrigin;
		}
	}

	public Transform WatchAnchor
	{
		get
		{
			return watchAnchor;
		}
	}

	public bool IsUsingMenu
	{
		get
		{
			return ThisPlayer.PlayerUI.Menu.Visible && ThisPlayer.PlayerUI.Menu.MenuAnchorHand == this;
		}
	}

	public bool IsHandTracking
	{
		get
		{
			return base.photonView.owner != null && (!base.photonView.owner.customProperties.ContainsKey(controllerTrackingProperty) || (bool)base.photonView.owner.customProperties[controllerTrackingProperty]);
		}
		private set
		{
			if (base.isLocal && base.photonView.owner != null && value != IsHandTracking)
			{
				base.photonView.owner.SetCustomProperties(controllerTrackingProperty, value);
			}
		}
	}

	public bool HandshakeAnimationEnabled
	{
		set
		{
			handAnimator.SetBool(handshakeAnimatorId, value);
		}
	}

	public bool ThumbsUpAnimationEnabled
	{
		set
		{
			handAnimator.SetBool(thumbsUpAnimatorId, value);
		}
	}

	public bool RemoteVisible { get; set; }

	public bool MeshesVisible
	{
		get
		{
			return _handMeshesVisible;
		}
		set
		{
			_handMeshesVisible = value;
		}
	}

	public bool InPersonalBubble
	{
		get
		{
			return _inPersonalBubble || (Tool != null && Tool.InPersonalBubble) || (!_inPersonalBubble && Time.time - lastTimeInPersonalBubble < inPersonalBubbleTimeout);
		}
		private set
		{
			_inPersonalBubble = value;
		}
	}

	public bool IsVisible
	{
		get
		{
			return (base.isLocal || ThisPlayer.IsVisible) && MeshesVisible && !InPersonalBubble && !RemoteVisible && isHandTrackingCached;
		}
	}

	public float OpenClosedAxis { get; private set; }

	public event Action<PlayerHand, Tool> PickupToolEvent;

	protected override void Awake()
	{
		base.Awake();
		if (ThisPlayer == null)
		{
			ThisPlayer = base.gameObject.GetComponentInParents<Player>();
		}
		GravityPickupFingerOffset = defaultGravityPickupFingerOffset;
		GravityPickupCapsuleLength = defaultGravityPickupCapsuleLength;
		GravityPickupCapsuleRadius = defaultGravityPickupCapsuleRadius;
		GravityPickupSphereRadius = defaultGravityPickupSphereRadius;
		PhysicalPickupSphereRadius = defaultPhysicalPickupSphereRadius;
		NetworkTransform = GetComponent<PUNNetworkTransform>();
		rigidbody = GetComponent<Rigidbody>();
		handColliders = GetComponentsInChildren<PlayerHandCollider>();
		TrackedVelocity = GetComponent<TrackedVelocity>();
		handAnimator = GetComponentInChildren<Animator>();
		Gestures = GetComponent<PlayerHandGestures>();
		PlayerHandRemote = UnityEngine.Object.Instantiate(playerHandRemotePrefab);
		PlayerHandRemote.Player = ThisPlayer;
		PlayerHandRemote.Hand = this;
		ThisPlayer.SetParentPlayerRoot(PlayerHandRemote.transform);
		controllerTrackingProperty = "controllerTrackingProperty_" + Type;
		rigidbodyPickup = GetComponent<PlayerHandRigidbodyPickup>();
		visualRenderers = meshRoot.GetComponentsInChildren<Renderer>();
	}

	private void Start()
	{
		gameManager = RecRoomSceneManager.Instance.GameManager;
		if (gameManager != null)
		{
			gameManager.TeamManager.TeamChangeEvent += OnGameTeamChange;
			gameManager.StateChangeEvent += OnGameStateChanged;
		}
		UpdateVisibility(true);
	}

	public void PickupTool(Tool tool, Vector3 handSpacePickupPosition, Quaternion handSpacePickupRotation)
	{
		if (this.PickupToolEvent != null)
		{
			this.PickupToolEvent(this, tool);
		}
		MeshesVisible = false;
		PlayerHandCollider[] array = handColliders;
		foreach (PlayerHandCollider playerHandCollider in array)
		{
			playerHandCollider.DisableHandToolCollisions(tool);
		}
		Tool = tool;
		Tool.ToolRenderer.SetVisible(ThisPlayer.IsVisible);
		Tool.Pickup(ThisPlayer, rigidbody, handSpacePickupPosition, handSpacePickupRotation);
		hoveredTool = null;
		if (ThisPlayer.isLocal)
		{
			Vibrate(50, 1000);
		}
	}

	public void ReleaseTool()
	{
		PlayerHandCollider[] array = handColliders;
		foreach (PlayerHandCollider playerHandCollider in array)
		{
			playerHandCollider.ReEnableHandToolCollisions(Tool);
		}
		Vector3 averageLinearVelocity = TrackedVelocity.GetAverageLinearVelocity(preReleaseWindow);
		Vector3 averageAngularVelocity = TrackedVelocity.GetAverageAngularVelocity(preReleaseWindow);
		Tool.Release(ThisPlayer, averageLinearVelocity, averageAngularVelocity);
		Tool = null;
		MeshesVisible = true;
	}

	public void GetToolPickupPositionAndRotation(Tool tool, bool useGravityPickup, Vector3 gravityPickupDirection, out Vector3 handSpacePickupPosition, out Quaternion handSpacePickupRotation)
	{
		if ((useGravityPickup && !tool.SupportsOffCenterGravityPickup) || !tool.SupportsOffCenterPickup)
		{
			tool.GetHandSpacePickupTransform(base.transform, Type, out handSpacePickupPosition, out handSpacePickupRotation);
			return;
		}
		if (useGravityPickup && tool.SupportsOffCenterGravityPickup)
		{
			Vector3 position = base.transform.position + Vector3.ProjectOnPlane(tool.transform.position - base.transform.position, gravityPickupDirection);
			handSpacePickupPosition = base.transform.InverseTransformPoint(position);
		}
		else
		{
			handSpacePickupPosition = base.transform.InverseTransformPoint(tool.transform.position);
		}
		handSpacePickupRotation = base.transform.InverseTransformRotation(tool.transform.rotation);
	}

	public void Vibrate(float duration, float intensity = 1f)
	{
		Vibrate((int)(duration * 1000f), (ushort)(intensity * 1000f));
	}

	public void Vibrate(int milliseconds = 1, ushort intensity = 1000)
	{
		if (ControllerIO != null)
		{
			ControllerIO.Vibrate(milliseconds, intensity);
		}
	}

	private IEnumerator VibrateCoroutine(int milliseconds, ushort intensity)
	{
		float endTime = Time.unscaledTime + (float)milliseconds / 1000f;
		while (Time.unscaledTime < endTime)
		{
			if (ControllerIO != null)
			{
				ControllerIO.Vibrate(intensity, 1000);
			}
			yield return null;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (ControllerIO != null)
		{
			ControllerIO.TransformUpdated -= OnTrackerTransformUpdate;
			ControllerIO.UIInteractionEnabled = true;
		}
		if (PlayerHandRemote != null)
		{
			UnityEngine.Object.Destroy(PlayerHandRemote);
		}
		if (gameManager != null)
		{
			gameManager.TeamManager.TeamChangeEvent -= OnGameTeamChange;
			gameManager.StateChangeEvent -= OnGameStateChanged;
		}
		if (Tool != null)
		{
			ThisPlayer.ToolController.ReleaseTool(Tool);
		}
	}

	private void OnTrackerTransformUpdate()
	{
		rigidbody.MovePosition(ControllerIO.transform.position);
		rigidbody.MoveRotation(ControllerIO.transform.rotation);
	}

	private void FixedUpdate()
	{
		InPersonalBubble = false;
		if (ControllerIO == null)
		{
			return;
		}
		if (Tool == null && !RemoteVisible && isHandTrackingCached && !IsUsingMenu)
		{
			FindClosestOutfitTrigger();
			Tool tool = FindClosestToolInPhysicalPickupRange();
			if (tool != null)
			{
				hoveredToolInPhysicalPickupRange = true;
			}
			else
			{
				tool = FindClosestToolInMagnetPickupRange(out gravityPickupVector);
				hoveredToolInPhysicalPickupRange = false;
			}
			hoveredTool = tool;
		}
		else
		{
			hoveredTool = null;
		}
		FindClosestTeleportationPortal();
	}

	private void Update()
	{
		isHandTrackingCached = IsHandTracking;
		UpdateVisibility();
		if (ControllerIO == null)
		{
			IsHandTracking = false;
			return;
		}
		if (base.isLocal)
		{
			IsHandTracking = ControllerIO.IsTracking;
		}
		UpdateAnimation();
		UpdateTool();
		UpdateRemoteVisibility();
	}

	public void SetInPersonalBubble(float turnVisibleTimeout)
	{
		InPersonalBubble = true;
		inPersonalBubbleTimeout = turnVisibleTimeout;
		lastTimeInPersonalBubble = Time.time;
	}

	private void UpdateVisibility(bool forceUpdate = false)
	{
		bool flag = base.isLocal || ThisPlayer.IsVisible;
		bool flag2 = flag && !InPersonalBubble;
		bool flag3 = flag && RemoteVisible && !InPersonalBubble;
		bool isVisible = IsVisible;
		if (!forceUpdate && wasVisible == isVisible && wasToolVisible == flag2 && flag3 == wasRemoteVisible && isHandTrackingCached == wasHandTracking)
		{
			return;
		}
		Renderer[] array = visualRenderers;
		foreach (Renderer renderer in array)
		{
			renderer.gameObject.SetActive(isVisible);
		}
		PlayerHandCollider[] array2 = handColliders;
		foreach (PlayerHandCollider playerHandCollider in array2)
		{
			if (playerHandCollider != null && !playerHandCollider.IsTrigger)
			{
				playerHandCollider.gameObject.SetActive(isVisible);
			}
		}
		ThisPlayer.PlayerOutfit.SetHandVisible(Type, isVisible);
		if (Tool != null)
		{
			Tool.ToolRenderer.SetVisible(flag2, flag3);
		}
		PlayerHandRemote.Visible = flag3;
		if (!isVisible)
		{
			hoveredTool = null;
		}
		wasVisible = isVisible;
		wasToolVisible = flag2;
		wasRemoteVisible = flag3;
		wasHandTracking = isHandTrackingCached;
	}

	private void UpdateRemoteVisibility()
	{
		if (base.isLocal)
		{
			ControllerIO.UIInteractionEnabled = CanMenuLaserInterract();
			bool flag = PlayerHandRemote.TeleportLaserVisible || PlayerHandRemote.MenuLaserVisible || (ControllerIO.UIInteractionEnabled && ControllerIO.UIRaycastPosition.HasValue);
			if (flag)
			{
				lastRemoteShowTime = Time.time;
			}
			if (flag || (!flag && Time.time - lastRemoteShowTime > 0.01f))
			{
				RemoteVisible = flag;
			}
		}
	}

	private bool CanMenuLaserInterract()
	{
		return !PlayerHandRemote.TeleportLaserVisible && isHandTrackingCached && (((Tool == null || Tool.CanInterractWithUI) && hoveredTool == null) || PlayerHandRemote.MenuLaserVisible);
	}

	private Tool FindClosestToolInPhysicalPickupRange()
	{
		Tool closestTool = null;
		float closestToolDistance = float.MaxValue;
		FindClosestToolInSphere(ref closestTool, ref closestToolDistance, PhysicalPickupSphereRadius, true);
		return closestTool;
	}

	private Tool FindClosestToolInSphere(ref Tool closestTool, ref float closestToolDistance, float radius, bool physicalPickup)
	{
		int num = Physics.OverlapSphereNonAlloc(handCenter.position, radius, colliderHits, 226894848);
		for (int i = 0; i < num; i++)
		{
			ToolCollider toolCollider = null;
			Tool colliderTool = colliderHits[i].GetColliderTool(out toolCollider);
			if (colliderTool != null)
			{
				colliderTool = colliderTool.GetHighlightedTool(ThisPlayer, toolCollider, physicalPickup, handCenter.position);
				float distance;
				if (colliderTool != null && TryGetDistanceToTool(colliderTool, out distance) && distance < closestToolDistance)
				{
					closestTool = colliderTool;
					closestToolDistance = distance;
				}
			}
		}
		return closestTool;
	}

	private bool TryGetDistanceToTool(Tool tool, out float distance)
	{
		Vector3 lhs = tool.ColliderBounds.center - handCenter.position;
		distance = lhs.magnitude;
		return Vector3.Dot(lhs, handCenter.right) >= 0f;
	}

	private Tool FindClosestToolInMagnetPickupRange(out Vector3 selectionVector)
	{
		selectionVector = GravityPickupDirection;
		Vector3 origin = GravityPickupOrigin + selectionVector * GravityPickupCapsuleRadius;
		float maxDistance = defaultGravityPickupCapsuleLength - 2f * GravityPickupCapsuleRadius;
		int num = Physics.SphereCastNonAlloc(origin, GravityPickupCapsuleRadius, selectionVector, raycastHits, maxDistance, 226894848);
		float num2 = float.MaxValue;
		float closestToolDistance = float.MaxValue;
		Tool closestTool = null;
		for (int i = 0; i < num; i++)
		{
			ToolCollider toolCollider = null;
			Tool colliderTool = raycastHits[i].collider.GetColliderTool(out toolCollider);
			if (!(colliderTool != null))
			{
				continue;
			}
			colliderTool = colliderTool.GetHighlightedTool(ThisPlayer, toolCollider, false, GravityPickupOrigin);
			if (colliderTool != null)
			{
				Bounds colliderBounds = colliderTool.ColliderBounds;
				Vector3 center = colliderBounds.center;
				Vector3 vector = GravityPickupOrigin - center;
				Vector3 vector2 = vector - Vector3.Project(vector, selectionVector);
				float magnitude = colliderBounds.extents.magnitude;
				Vector3 vector3 = center + vector2.normalized * magnitude;
				Vector3 lhs = vector3 - GravityPickupOrigin;
				float num3 = Vector3.Dot(lhs, selectionVector);
				if (num3 >= 0f && num3 < num2)
				{
					closestTool = colliderTool;
					closestToolDistance = vector.magnitude;
				}
			}
		}
		FindClosestToolInSphere(ref closestTool, ref closestToolDistance, GravityPickupSphereRadius, false);
		return closestTool;
	}

	private void FindClosestOutfitTrigger()
	{
		bool flag = false;
		if (SingletonMonoBehaviour<TutorialManager>.Instance.CanRemoveOutfit)
		{
			int num = Physics.RaycastNonAlloc(handCenter.position - handCenter.forward * outfitBackwardPickupRange, handCenter.forward, raycastHits, outfitBackwardPickupRange + outfitForwardPickupRange, 32768);
			if (num != 0)
			{
				raycastHits.SortByDistanceToCenter(num);
				for (int i = 0; i < num; i++)
				{
					OutfitTrigger component = raycastHits[i].collider.GetComponent<OutfitTrigger>();
					if (component != null && component.OutfitItem != null && component.OutfitItem.Owner == ThisPlayer && component.OutfitItem.CurrentBodyPart != (Player.BodyPart)((Type != HandType.Left) ? 3 : 2) && component.OutfitItem.Type != OutfitManager.OutfitType.Shirt && (component.EnabledOutsideChangingRoom || ThisPlayer.IsInChangingRoom))
					{
						TriggeredOutfit = component;
						flag = true;
						break;
					}
				}
			}
		}
		if (!flag)
		{
			TriggeredOutfit = null;
		}
	}

	private void FindClosestTeleportationPortal()
	{
		TeleportationPortal teleportationPortal = null;
		RaycastHit hitInfo;
		if (IsVisible && Physics.Raycast(handCenter.position - handCenter.forward * outfitBackwardPickupRange, handCenter.forward, out hitInfo, outfitBackwardPickupRange + outfitForwardPickupRange, 18433, QueryTriggerInteraction.Collide))
		{
			teleportationPortal = hitInfo.collider.GetComponent<TeleportationPortal>();
		}
		if (teleportationPortal != null && teleportationPortal.SupportsHandInteraction)
		{
			TriggeredPortal = teleportationPortal;
		}
		else
		{
			TriggeredPortal = null;
		}
	}

	private void UpdateAnimation()
	{
		if (IsVisible)
		{
			OpenClosedAxis = ControllerIO.PickupAxis;
			if (handAnimator.isActiveAndEnabled)
			{
				handAnimator.SetFloat(openClosedAnimatorId, OpenClosedAxis);
			}
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isReading)
		{
			RemoteVisible = (bool)stream.ReceiveNext() && ThisPlayer.IsVisible;
			OpenClosedAxis = (float)stream.ReceiveNext();
			if (handAnimator.isActiveAndEnabled)
			{
				handAnimator.SetFloat(openClosedAnimatorId, OpenClosedAxis);
			}
			LaserTeleporter.LaserMode laserMode = (LaserTeleporter.LaserMode)stream.ReceiveNext();
			if (!ThisPlayer.IsVisible)
			{
				PlayerHandRemote.Mode = LaserTeleporter.LaserMode.Disabled;
			}
			else
			{
				PlayerHandRemote.Mode = laserMode;
			}
			if (laserMode != LaserTeleporter.LaserMode.Disabled)
			{
				Vector3 initialTangent = (Vector3)stream.ReceiveNext();
				Vector3 vector = (Vector3)stream.ReceiveNext();
				Vector3 finalPoint = (Vector3)stream.ReceiveNext();
				Vector3 teleportTarget = (Vector3)stream.ReceiveNext();
				Vector3 zero = Vector3.zero;
				Vector3 zero2 = Vector3.zero;
				if ((bool)stream.ReceiveNext())
				{
					zero = (Vector3)stream.ReceiveNext();
					zero2 = (Vector3)stream.ReceiveNext();
					PlayerHandRemote.ParabolicPointer.SetParabolaPointsWithReflection(initialTangent, vector, zero, zero2, finalPoint);
				}
				else
				{
					PlayerHandRemote.ParabolicPointer.SetParabolaPoints(initialTangent, vector, finalPoint);
				}
				PlayerHandRemote.TeleportTarget = teleportTarget;
			}
			return;
		}
		stream.SendNext(RemoteVisible);
		stream.SendNext(OpenClosedAxis);
		stream.SendNext((int)PlayerHandRemote.Mode);
		if (PlayerHandRemote.Mode != LaserTeleporter.LaserMode.Disabled)
		{
			stream.SendNext(PlayerHandRemote.ParabolicPointer.InitialParabolaTangent);
			stream.SendNext(PlayerHandRemote.ParabolicPointer.FinalParabolaTangent);
			stream.SendNext(PlayerHandRemote.ParabolicPointer.FinalParabolaPoint);
			stream.SendNext(PlayerHandRemote.TeleportTarget);
			stream.SendNext(PlayerHandRemote.ParabolicPointer.Reflected);
			if (PlayerHandRemote.ParabolicPointer.Reflected)
			{
				stream.SendNext(PlayerHandRemote.ParabolicPointer.ReflectionPoint);
				stream.SendNext(PlayerHandRemote.ParabolicPointer.ReflectionNormal);
			}
		}
	}

	private void UpdateTool()
	{
		bool flag = ControllerIO.HairPickupButtonDown || (ControllerIO.HairPickupButtonPressed && !ControllerIO.HairPickupButtonDownConsumed && Time.unscaledTime - ControllerIO.HairPickupButtonDownTime <= hairTriggerReactionWindow);
		bool hairPickupButtonDown = ControllerIO.HairPickupButtonDown;
		bool hairPickupButtonUp = ControllerIO.HairPickupButtonUp;
		bool pickupButtonUp = ControllerIO.PickupButtonUp;
		bool menuButtonUp = ControllerIO.MenuButtonUp;
		bool flag2 = ControllerIO.HairTriggerButtonDown || (ControllerIO.HairTriggerButtonPressed && !ControllerIO.HairTriggerButtonDownConsumed && Time.unscaledTime - ControllerIO.HairTriggerButtonDownTime <= hairTriggerReactionWindow);
		bool triggerButtonPressed = ControllerIO.TriggerButtonPressed;
		bool hairTriggerButtonUp = ControllerIO.HairTriggerButtonUp;
		float triggerAxis = ControllerIO.TriggerAxis;
		if (Tool != null)
		{
			if (Tool.IsLocked)
			{
				Tool.InputAmount = triggerAxis;
				if (flag2)
				{
					Tool.OnInputDown();
					ControllerIO.HairTriggerButtonDownConsumed = true;
				}
				if (triggerButtonPressed)
				{
					Tool.OnInputPressed();
				}
				if (hairTriggerButtonUp)
				{
					Tool.OnInputUp();
				}
				if (menuButtonUp && Tool.IsLocked && Tool.CanBeUnlocked)
				{
					Tool.IsLocked = false;
					Vibrate(500, 1000);
					if (!triggerButtonPressed)
					{
						ThisPlayer.ToolController.ReleaseTool(Tool);
					}
				}
			}
			else if (!Tool.IsLocked && menuButtonUp)
			{
				Tool.IsLocked = true;
				Vibrate(500, 1000);
			}
			else if ((!Tool.IsSmartTool && hairPickupButtonUp) || (Tool.IsSmartTool && pickupButtonUp))
			{
				if (Tool.IsSmartTool)
				{
					ControllerIO.HairPickupButtonDownConsumed = true;
				}
				ThisPlayer.ToolController.ReleaseTool(Tool);
			}
		}
		else
		{
			if (!isHandTrackingCached)
			{
				return;
			}
			if (!TryRemoveTriggeredOutfit(hairPickupButtonDown) && hoveredTool != null)
			{
				bool flag3 = hairPickupButtonDown;
				if (hoveredTool.SupportsAssistedCatching)
				{
					flag3 = flag;
				}
				if (flag3)
				{
					TryPickupHoveredTool();
					ControllerIO.HairPickupButtonDownConsumed = true;
				}
			}
			else if (TriggeredPortal != null && hairPickupButtonDown)
			{
				PlayerHandRemote.TeleportTarget = TriggeredPortal.DestinationPosition;
				PlayerHandRemote.PerformPortalTeleport(TriggeredPortal);
			}
		}
	}

	public bool TryRemoveTriggeredOutfit(bool buttonDown)
	{
		Vector3 handSpacePickupPosition = Vector3.zero;
		Quaternion handSpacePickupRotation = Quaternion.identity;
		if (!RemoteVisible && buttonDown && TriggeredOutfit != null)
		{
			OutfitTool.ToolPurpose purpose = OutfitTool.ToolPurpose.Doffing;
			if (ThisPlayer.IsInChangingRoom)
			{
				purpose = OutfitTool.ToolPurpose.Drawer;
			}
			OutfitTool outfitTool = OutfitTool.Find(ThisPlayer, TriggeredOutfit.OutfitItem.Selection, purpose);
			if (outfitTool != null)
			{
				ThisPlayer.PlayerOutfit.RemoveOutfit(TriggeredOutfit.OutfitItem.Selection, true);
				outfitTool.DontCollideWithPlayer(1f);
				outfitTool.CurrentState = OutfitTool.OutfitToolState.InHand;
				outfitTool.transform.position = TriggeredOutfit.transform.position;
				outfitTool.transform.localRotation = TriggeredOutfit.transform.rotation;
				GetToolPickupPositionAndRotation(outfitTool, true, Vector3.zero, out handSpacePickupPosition, out handSpacePickupRotation);
				ThisPlayer.ToolController.TryPickupTool(outfitTool, handSpacePickupPosition, handSpacePickupRotation, (int)Type);
			}
			else
			{
				Debug.LogWarning("Null outfitTool in TryRemoveTriggeredOutfit");
			}
			ControllerIO.HairPickupButtonDownConsumed = true;
			return true;
		}
		return false;
	}

	private void TryPickupHoveredTool()
	{
		if (hoveredTool != null && hoveredTool.IsEnabled)
		{
			Vector3 handSpacePickupPosition = Vector3.zero;
			Quaternion handSpacePickupRotation = Quaternion.identity;
			GetToolPickupPositionAndRotation(hoveredTool, !hoveredToolInPhysicalPickupRange, gravityPickupVector, out handSpacePickupPosition, out handSpacePickupRotation);
			ThisPlayer.ToolController.TryPickupTool(hoveredTool, handSpacePickupPosition, handSpacePickupRotation, (int)Type);
		}
		else
		{
			hoveredTool = null;
		}
	}

	private void OnGameStateChanged(ushort currentStateId, ushort previousStateId, ushort currentSubStateId, ushort previousSubStateId)
	{
		if (ThisPlayer != null)
		{
			OnGameTeamChange();
		}
	}

	private void OnGameTeamChange()
	{
		if (ThisPlayer != null && Tool != null)
		{
			if (Tool.PlayerInteractionRestriction != null && !Tool.PlayerInteractionRestriction.PlayerInteractionAllowed(ThisPlayer.PhotonPlayer))
			{
				ReleaseTool();
			}
			else
			{
				Tool.OnOwnerRoleUpdated();
			}
		}
	}

	public static void ConnectHandshake(PlayerHand lhs, PlayerHand rhs)
	{
		if (!(lhs == null) && !(rhs == null))
		{
			PlayerHand masterHand;
			PlayerHand slaveHand;
			GetGestureHandRoles(lhs, rhs, out masterHand, out slaveHand);
			if (slaveHand.rigidbodyPickup.Target != masterHand.rigidbody)
			{
				masterHand.handAnimator.SetBool(handshakeAnimatorId, true);
				slaveHand.handAnimator.SetBool(handshakeAnimatorId, true);
				Vector3 handSpacePickupPosition = masterHand.transform.InverseTransformPoint(masterHand.handshakeConnectionTransform.position);
				Quaternion handSpacePickupRotation = masterHand.transform.InverseTransformRotation(masterHand.handshakeConnectionTransform.rotation);
				slaveHand.rigidbodyPickup.Pickup(masterHand.rigidbody, handSpacePickupPosition, handSpacePickupRotation);
			}
		}
	}

	public static void BreakHandshake(PlayerHand lhs, PlayerHand rhs)
	{
		if (!(lhs == null) && !(rhs == null))
		{
			PlayerHand masterHand;
			PlayerHand slaveHand;
			GetGestureHandRoles(lhs, rhs, out masterHand, out slaveHand);
			masterHand.handAnimator.SetBool(handshakeAnimatorId, false);
			slaveHand.handAnimator.SetBool(handshakeAnimatorId, false);
			slaveHand.rigidbodyPickup.Release();
		}
	}

	public static void ConnectCollisionGesture(PlayerHandGestures.Gesture gesture, PlayerHand lhs, PlayerHand rhs)
	{
		if (lhs == null || rhs == null)
		{
			return;
		}
		PlayerHand masterHand;
		PlayerHand slaveHand;
		GetGestureHandRoles(lhs, rhs, out masterHand, out slaveHand);
		if (slaveHand.rigidbodyPickup.Target != masterHand.rigidbody)
		{
			Transform transform = ((gesture != PlayerHandGestures.Gesture.Fistbump) ? masterHand.highFiveConnectionTransform : masterHand.fistbumpConnectionTransform);
			if (transform == null)
			{
				transform = slaveHand.transform;
			}
			Vector3 handSpacePickupPosition = masterHand.transform.InverseTransformPoint(transform.position);
			Quaternion handSpacePickupRotation = masterHand.transform.InverseTransformRotation(slaveHand.transform.rotation);
			slaveHand.rigidbodyPickup.Pickup(masterHand.rigidbody, handSpacePickupPosition, handSpacePickupRotation);
		}
	}

	public static void BreakCollisionGesture(PlayerHandGestures.Gesture gesture, PlayerHand lhs, PlayerHand rhs)
	{
		if (!(lhs == null) && !(rhs == null))
		{
			PlayerHand masterHand;
			PlayerHand slaveHand;
			GetGestureHandRoles(lhs, rhs, out masterHand, out slaveHand);
			slaveHand.rigidbodyPickup.Release();
		}
	}

	private static void GetGestureHandRoles(PlayerHand hand1, PlayerHand hand2, out PlayerHand masterHand, out PlayerHand slaveHand)
	{
		if (hand1.isLocal)
		{
			masterHand = hand1;
			slaveHand = hand2;
		}
		else if (hand2.isLocal)
		{
			masterHand = hand2;
			slaveHand = hand1;
		}
		else if (hand1.photonView.viewID < hand2.photonView.viewID)
		{
			masterHand = hand1;
			slaveHand = hand2;
		}
		else
		{
			masterHand = hand2;
			slaveHand = hand1;
		}
	}

	public static PlayerHand Find(int photonViewId)
	{
		PhotonView photonView = PhotonView.Find(photonViewId);
		if (photonView != null)
		{
			return photonView.GetComponent<PlayerHand>();
		}
		return null;
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			Gizmos.color = Color.green;
			float gravityPickupCapsuleRadius = GravityPickupCapsuleRadius;
			float num = GravityPickupCapsuleLength - 2f * gravityPickupCapsuleRadius;
			int num2 = (int)(num / (2f * gravityPickupCapsuleRadius));
			Vector3 gravityPickupDirection = GravityPickupDirection;
			Vector3 vector = num / (float)num2 * gravityPickupDirection;
			Vector3 vector2 = GravityPickupOrigin + gravityPickupDirection * gravityPickupCapsuleRadius;
			for (int i = 0; i < num2; i++)
			{
				Gizmos.DrawWireSphere(vector2 + i * vector, gravityPickupCapsuleRadius);
			}
			Gizmos.DrawWireSphere(vector2 + num * gravityPickupDirection, gravityPickupCapsuleRadius);
		}
	}
}
