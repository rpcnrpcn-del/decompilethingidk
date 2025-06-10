using System;
using System.Collections.Generic;
using Photon;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RigidbodyPickup))]
[RequireComponent(typeof(ToolRenderer))]
public abstract class Tool : Photon.MonoBehaviour
{
	[Serializable]
	private class PickupTransforms
	{
		public Transform Default;

		public Transform SteamVrVive;

		public Transform SteamVrOculus;

		public Transform Oculus;
	}

	public delegate void KillzoneReset(Tool tool, Killzone killzone);

	private const float MAX_HUMAN_THROW_SPEED = 22f;

	private const float MIN_MOVING_SPEED = 0.5f;

	private const float DYNAMIC_COLLISION_COOLDOWN = 0.25f;

	private const float MOVEMENT_EPSILON = 0.01f;

	[Header("Smart Tools")]
	[SerializeField]
	protected bool isSmartTool;

	[SerializeField]
	protected bool canBeUnlocked = true;

	[Header("Pickup")]
	[SerializeField]
	private PickupTransforms leftHandPickupTransforms = new PickupTransforms();

	[SerializeField]
	private PickupTransforms rightHandPickupTransforms = new PickupTransforms();

	[SerializeField]
	protected bool supportsGravityPickup = true;

	[SerializeField]
	protected bool supportsOffCenterGravityPickup;

	[SerializeField]
	protected bool supportsPhysicalPickup = true;

	[SerializeField]
	protected bool supportsOffCenterPickup = true;

	[SerializeField]
	protected bool supportsTeleportHits;

	[SerializeField]
	protected Layers pickedCollisionLayer = Layers.DynamicPhysicsIgnorePlayerPhysics;

	[Header("Release")]
	[SerializeField]
	private bool overrideVelocityOnRelease = true;

	[SerializeField]
	private bool multiplyVelocityOnRelease;

	[SerializeField]
	private FloatValueCurve releaseLinearSpeedMultiplierCurve;

	[Header("Physics")]
	[SerializeField]
	private bool dynamicKinematicCollisionsEnabled;

	[SerializeField]
	private bool hasMaxSpeed;

	[SerializeField]
	private float maxSpeed = 10f;

	[SerializeField]
	private float spin;

	[HideInInspector]
	public bool SupportsAssistedCatching = true;

	[NonSerialized]
	public bool AutoLock;

	[NonSerialized]
	public bool SpawnedFromPool;

	[NonSerialized]
	public bool SuppressesTeleport;

	[NonSerialized]
	public bool SupportsKillzones;

	[NonSerialized]
	public bool IsReleasedOnPlayerRespawn = true;

	[NonSerialized]
	public bool CanInterractWithUI;

	protected bool usesGravity = true;

	protected Vector3 lastPickupPosition;

	protected Quaternion lastPickupRotation;

	protected PlayerHand.HandType lastReleaseHandType;

	protected Vector3 lastSpawnPosition;

	protected Quaternion lastSpawnRotation;

	private Vector3 previousVelocity = Vector3.zero;

	private Vector3 previousPreviousVelocity = Vector3.zero;

	private float lastDynamicKinematicCollisionTime;

	private Rigidbody lastDynamicKinematicRigidbody;

	private Vector3 dynamicKinematicCollisionVelocity = Vector3.zero;

	private Vector3 dynamicKinematicCollisionAngularVelocity = Vector3.zero;

	private bool dynamicKinematicCollisionThisFrame;

	protected List<Killzone> intersectingKillzones = new List<Killzone>();

	protected bool resetOnKillzoneThisFrame;

	private SynchronizedField<bool> _isLocked;

	private SynchronizedField<bool> _isEnabled;

	private SynchronizedField<int> _holderId;

	[NonSerialized]
	public Layers PickupCollisionLayer = Layers.DynamicPhysicsIgnorePlayerPhysics;

	[NonSerialized]
	public Layers DefaultCollisionLayer = Layers.DynamicPhysics;

	[HideInInspector]
	public float InputAmount;

	public bool IsSmartTool
	{
		get
		{
			return isSmartTool;
		}
	}

	public bool CanBeUnlocked
	{
		get
		{
			return canBeUnlocked;
		}
	}

	public bool SupportsGravityPickup
	{
		get
		{
			return supportsGravityPickup;
		}
	}

	public bool SupportsOffCenterGravityPickup
	{
		get
		{
			return supportsOffCenterGravityPickup;
		}
	}

	public bool SupportsPhysicalPickup
	{
		get
		{
			return supportsPhysicalPickup;
		}
	}

	public bool SupportsOffCenterPickup
	{
		get
		{
			return supportsOffCenterPickup;
		}
	}

	public bool SupportsTeleportHits
	{
		get
		{
			return supportsTeleportHits;
		}
	}

	public bool OnlyOwnerCanPickup { get; set; }

	public TrackedVelocity TrackedVelocity { get; protected set; }

	public PUNNetworkTransform NetworkTransform { get; protected set; }

	public List<ToolCollider> Colliders { get; protected set; }

	public Rigidbody Rigidbody { get; protected set; }

	public RigidbodyPickup RigidbodyPickup { get; protected set; }

	public ToolRenderer ToolRenderer { get; protected set; }

	public ToolAudio ToolAudio { get; protected set; }

	public PlayerInteractionRestriction PlayerInteractionRestriction { get; protected set; }

	public AnimateInOut AnimateInOut { get; protected set; }

	public Vector3 LastTargetSpacePickupPosition { get; protected set; }

	public Quaternion LastTargetSpacePickupRotation { get; protected set; }

	public bool IsLocked
	{
		get
		{
			return _isLocked.Get();
		}
		set
		{
			_isLocked.ForceSet(value);
		}
	}

	public bool IsEnabled
	{
		get
		{
			return _isEnabled.Get();
		}
		set
		{
			_isEnabled.ForceSet(value);
		}
	}

	public int HolderId
	{
		get
		{
			return _holderId.Get();
		}
		set
		{
			_holderId.ForceSet(value);
		}
	}

	public Player Owner
	{
		get
		{
			if (base.authority != null)
			{
				return base.authority.TagObject as Player;
			}
			return Player.LocalPlayer;
		}
	}

	public bool IsHeld
	{
		get
		{
			return PhotonNetwork.room == null || HolderId != PhotonPlayer.Invalid;
		}
	}

	public bool IsMoving
	{
		get
		{
			return Rigidbody.velocity.sqrMagnitude >= 0.25f;
		}
	}

	public virtual bool OwnershipTransferAllowed
	{
		get
		{
			return true;
		}
	}

	public PlayerHand HolderHand
	{
		get
		{
			PlayerHand result = null;
			if (IsHeld && Owner != null)
			{
				if (Owner.LeftHand.Tool == this)
				{
					result = Owner.LeftHand;
				}
				else if (Owner.RightHand.Tool == this)
				{
					result = Owner.RightHand;
				}
			}
			return result;
		}
	}

	public bool InPersonalBubble { get; set; }

	public Bounds ColliderBounds
	{
		get
		{
			Bounds result = default(Bounds);
			bool flag = false;
			for (int i = 0; i < Colliders.Count; i++)
			{
				if (Colliders[i] != null && Colliders[i].ThisCollider != null)
				{
					if (!flag)
					{
						result = Colliders[i].ThisCollider.bounds;
						flag = true;
					}
					else
					{
						result.Encapsulate(Colliders[i].ThisCollider.bounds);
					}
				}
			}
			return result;
		}
	}

	public event Action<Tool, Vector3, Quaternion> ResetEvent;

	public event Action<Tool> PickupEvent;

	public event Action<Tool> ReleaseEvent;

	public event Action<Tool> OwnerRoleUpdateEvent;

	public event Action<Player, Tool, Vector3, Vector3> ForceApplied;

	public static event Action<Tool> HolderChanged;

	private void OnHolderChanged()
	{
		if (Tool.HolderChanged != null)
		{
			Tool.HolderChanged(this);
		}
	}

	private void OnEnable()
	{
		if (Rigidbody != null)
		{
			Rigidbody.maxAngularVelocity = 1f / Time.fixedDeltaTime;
		}
	}

	protected virtual void OnDisable()
	{
		if (Owner != null && Owner.isLocal)
		{
			if (Owner.ToolController != null)
			{
				Owner.ToolController.ReleaseTool(this);
			}
			else
			{
				Debug.LogError("Tool.Owner.ToolController is unexpectedly null");
			}
		}
		if (base.hasAuthority)
		{
			if (Rigidbody != null)
			{
				Rigidbody.ClearVelocity();
			}
			else
			{
				Debug.LogError("Tool.Rigidbody is unexpectedly null");
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		PickupCollisionLayer = pickedCollisionLayer;
		AutoLock = isSmartTool;
		NetworkTransform = GetComponent<PUNNetworkTransform>();
		TrackedVelocity = GetComponent<TrackedVelocity>();
		RigidbodyPickup = GetComponent<RigidbodyPickup>();
		Rigidbody = GetComponent<Rigidbody>();
		ToolRenderer = GetComponent<ToolRenderer>();
		ToolAudio = GetComponent<ToolAudio>();
		PlayerInteractionRestriction = GetComponent<PlayerInteractionRestriction>();
		AnimateInOut = GetComponent<AnimateInOut>();
		Colliders = new List<ToolCollider>(GetComponentsInChildren<ToolCollider>(true));
		foreach (ToolCollider collider in Colliders)
		{
			collider.Tool = this;
			collider.ThisRigidbody = Rigidbody;
			collider.ThisTrackedVelocity = TrackedVelocity;
		}
		lastSpawnPosition = (lastPickupPosition = base.transform.position);
		lastSpawnRotation = (lastPickupRotation = base.transform.rotation);
		OnlyOwnerCanPickup = false;
		_isLocked = new SynchronizedField<bool>(this, "Tool.IsLocked", false, SetterPermissionMode.ANYONE, OnIsLockedChanged);
		_isEnabled = new SynchronizedField<bool>(this, "Tool.Enabled", true, SetterPermissionMode.MASTER_OR_AUTHORITY, OnIsEnabledChanged);
		_holderId = new SynchronizedField<int>(this, "Holder", PhotonPlayer.Invalid, SetterPermissionMode.ANYONE, OnHolderChanged);
	}

	protected virtual void Start()
	{
		if (ToolRenderer != null)
		{
			ToolRenderer.Mode = (IsLocked ? ToolRenderer.HighlightMode.Locked : ToolRenderer.HighlightMode.None);
		}
		OnIsEnabledChanged();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected virtual void FixedUpdate()
	{
		if (base.hasAuthority && hasMaxSpeed)
		{
			float magnitude = Rigidbody.velocity.magnitude;
			if (magnitude > maxSpeed)
			{
				Rigidbody.velocity = Rigidbody.velocity / magnitude * maxSpeed;
			}
		}
		if (base.hasAuthority && previousVelocity.sqrMagnitude >= 0.01f && Rigidbody.velocity.sqrMagnitude < 0.01f)
		{
			if (intersectingKillzones.Count > 0 && !resetOnKillzoneThisFrame)
			{
				ResetOnKillzone(intersectingKillzones[0]);
			}
			else
			{
				OnStopMoving();
			}
		}
		if (resetOnKillzoneThisFrame)
		{
			resetOnKillzoneThisFrame = false;
		}
		UpdateDynamicKinematicCollision();
		InPersonalBubble = false;
	}

	protected virtual void OnStopMoving()
	{
	}

	protected virtual void OnIsLockedChanged()
	{
		bool isLocked = IsLocked;
		if (ToolRenderer != null)
		{
			ToolRenderer.Mode = (isLocked ? ToolRenderer.HighlightMode.Locked : ToolRenderer.HighlightMode.None);
		}
		if (isLocked)
		{
			OnLock();
		}
		else if (!isLocked)
		{
			OnUnlock();
		}
		if (SingletonMonoBehaviour<AudioManager>.Instance != null)
		{
			AudioManager.Play3DSFX((!isLocked) ? SingletonMonoBehaviour<AudioManager>.Instance.ToolUnLockedAudio : SingletonMonoBehaviour<AudioManager>.Instance.ToolLockedAudio, base.transform.position);
		}
	}

	private void OnIsEnabledChanged()
	{
		base.gameObject.SetActive(IsEnabled);
	}

	public virtual void OnShake()
	{
	}

	public virtual void OnHoverStart(bool physicalHover)
	{
		if (IsSmartTool)
		{
			ToolRenderer.Mode = ToolRenderer.HighlightMode.SmartTool;
		}
		else if (physicalHover)
		{
			ToolRenderer.Mode = ToolRenderer.HighlightMode.Physical;
		}
		else
		{
			ToolRenderer.Mode = ToolRenderer.HighlightMode.Magnet;
		}
	}

	public virtual void OnHoverEnd()
	{
		if (ToolRenderer.Mode != ToolRenderer.HighlightMode.Locked)
		{
			ToolRenderer.Mode = ToolRenderer.HighlightMode.None;
		}
	}

	protected virtual void OnPhotonPlayerDisconnected(PhotonPlayer oldPlayer)
	{
		ReevaluateIsHeld();
	}

	protected virtual void OnMasterClientSwitched(PhotonPlayer newMaster)
	{
		ReevaluateIsHeld();
	}

	private void ReevaluateIsHeld()
	{
		if (IsHeld && PhotonNetwork.isMasterClient && PhotonPlayer.Find(HolderId) == null)
		{
			HolderId = PhotonPlayer.Invalid;
			IsLocked = false;
		}
	}

	public virtual void OnOwnerRoleUpdated()
	{
		if (this.OwnerRoleUpdateEvent != null)
		{
			this.OwnerRoleUpdateEvent(this);
		}
	}

	public virtual void Pickup(Player player, Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		player.PlayerEvents.ToolPickup(this);
		if (this.PickupEvent != null)
		{
			this.PickupEvent(this);
		}
		lastPickupPosition = base.transform.position;
		lastPickupRotation = base.transform.rotation;
		LastTargetSpacePickupPosition = targetSpacePickupPosition;
		LastTargetSpacePickupRotation = targetSpacePickupRotation;
		RigidbodyPickup.Pickup(target, targetSpacePickupPosition, targetSpacePickupRotation);
		if (player.isLocal)
		{
			SetCollisionLayer(PickupCollisionLayer);
			if (AutoLock)
			{
				IsLocked = true;
			}
		}
		if (ToolAudio != null)
		{
			ToolAudio.OnPickup();
		}
	}

	public virtual void GetHandSpacePickupTransform(Transform handTransform, PlayerHand.HandType handType, out Vector3 position, out Quaternion rotation)
	{
		Transform pickupTransform = GetPickupTransform(handType);
		position = ((!(pickupTransform != null)) ? Vector3.zero : pickupTransform.InverseTransformPoint(base.transform.position));
		rotation = ((!(pickupTransform != null)) ? Quaternion.identity : pickupTransform.InverseTransformRotation(base.transform.rotation));
	}

	protected Transform GetPickupTransform(PlayerHand.HandType handType)
	{
		Transform transform = null;
		PickupTransforms pickupTransforms = ((handType != PlayerHand.HandType.Left) ? rightHandPickupTransforms : leftHandPickupTransforms);
		switch (ControllerIO.CurrentInputMode)
		{
		case ControllerIO.InputMode.SteamVR_Vive:
			transform = pickupTransforms.SteamVrVive;
			break;
		case ControllerIO.InputMode.SteamVR_Oculus:
			transform = pickupTransforms.SteamVrOculus;
			break;
		case ControllerIO.InputMode.Oculus:
			transform = pickupTransforms.Oculus;
			break;
		}
		if (transform == null)
		{
			transform = pickupTransforms.Default;
		}
		return transform;
	}

	private void OnHoldJointBroke()
	{
		Owner.ToolController.ReleaseTool(this);
	}

	public virtual void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		lastReleaseHandType = ((!(player.LeftHand.Tool == this)) ? player.RightHand.Type : player.LeftHand.Type);
		if (Owner != null)
		{
			Owner.PlayerEvents.ToolRelease(this);
		}
		if (this.ReleaseEvent != null)
		{
			this.ReleaseEvent(this);
		}
		ToolRenderer.SetVisible(true);
		RigidbodyPickup.Release();
		if (player.isLocal)
		{
			IsLocked = false;
			SetCollisionLayer(Layers.DynamicPhysics);
			if (overrideVelocityOnRelease)
			{
				if (multiplyVelocityOnRelease)
				{
					MultiplyReleaseVelocity(ref linearVelocity);
				}
				Rigidbody.velocity = linearVelocity;
				Rigidbody.angularVelocity = angularVelocity;
			}
		}
		if (ToolAudio != null)
		{
			ToolAudio.OnRelease(linearVelocity);
		}
	}

	public void UpdatePickupTransform(Vector3 handRelativePosition, Quaternion handRelativeRotation)
	{
		if (RigidbodyPickup.Target != null)
		{
			LastTargetSpacePickupPosition = handRelativePosition;
			LastTargetSpacePickupRotation = handRelativeRotation;
			RigidbodyPickup.Pickup(RigidbodyPickup.Target, handRelativePosition, handRelativeRotation);
		}
	}

	protected void MultiplyReleaseVelocity(ref Vector3 linearVelocity)
	{
		float magnitude = linearVelocity.magnitude;
		float t = Mathf.Clamp01(magnitude / 22f);
		linearVelocity *= releaseLinearSpeedMultiplierCurve.Evaluate(t);
	}

	public virtual Tool GetHighlightedTool(Player player, ToolCollider collider, bool physicalPickup, Vector3 pickupPosition)
	{
		if (!player.CanInteractWithTools)
		{
			return null;
		}
		if (IsHeld && (Owner != player || !physicalPickup))
		{
			return null;
		}
		if (OnlyOwnerCanPickup && Owner != player)
		{
			return null;
		}
		if (!physicalPickup && !SupportsAssistedCatching)
		{
			return null;
		}
		if (!ToolRenderer.Visible)
		{
			return null;
		}
		if (IsLocked)
		{
			return null;
		}
		if (PlayerInteractionRestriction != null && !PlayerInteractionRestriction.PlayerInteractionAllowed(player.PhotonPlayer))
		{
			return null;
		}
		return ((!physicalPickup || !SupportsPhysicalPickup) && (physicalPickup || (!SupportsGravityPickup && !SupportsOffCenterGravityPickup))) ? null : this;
	}

	protected virtual void OnLock()
	{
	}

	protected virtual void OnUnlock()
	{
	}

	public virtual void OnInputDown()
	{
	}

	public virtual void OnInputPressed()
	{
	}

	public virtual void OnInputUp()
	{
	}

	public virtual bool OnPlayerTryingToRotateInPlace(float angle, float duration)
	{
		return true;
	}

	public virtual void OnPlayerTeleport(float duration)
	{
	}

	public virtual void OnPowerup(Powerup powerup)
	{
	}

	public void AddToolCollider(ToolCollider collider)
	{
		collider.Tool = this;
		collider.ThisRigidbody = Rigidbody;
		collider.ThisTrackedVelocity = TrackedVelocity;
		Colliders.Add(collider);
	}

	[PunRPC]
	public void RpcSetCollisionLayer(int layer)
	{
		SetCollisionLayerLocalOnly((Layers)layer);
	}

	public void SetCollisionLayer(Layers layer)
	{
		if (base.hasAuthority)
		{
			base.photonView.RPC("RpcSetCollisionLayer", PhotonTargets.All, (int)layer);
		}
	}

	public void SetCollisionLayerLocalOnly(Layers layer)
	{
		foreach (ToolCollider collider in Colliders)
		{
			collider.gameObject.layer = (int)layer;
		}
	}

	public void Disable()
	{
		IsEnabled = false;
	}

	public void SnapToHand()
	{
		base.transform.position = RigidbodyPickup.TargetRigidbodyPosition;
		base.transform.rotation = RigidbodyPickup.TargetRigidbodyRotation;
		Snap();
	}

	public void Snap()
	{
		if (NetworkTransform != null)
		{
			NetworkTransform.InsertPositionDiscontinuity();
		}
		else
		{
			Rigidbody.ClearVelocity();
		}
	}

	public void MasterDisableAndMove(Vector3 position, Quaternion rotation)
	{
		if (PhotonNetwork.isMasterClient)
		{
			if (!base.hasAuthority)
			{
				base.photonView.TransferOwnership(PhotonNetwork.masterClient);
			}
			AuthorityDisableAndMove(position, rotation);
		}
	}

	public void MasterDisableAndMoveToLastSpawnPosition()
	{
		MasterDisableAndMove(lastSpawnPosition, lastSpawnRotation);
	}

	public void MasterResetToDefault(Vector3 position, Quaternion rotation, bool wasCleanedUp = false)
	{
		if (PhotonNetwork.isMasterClient)
		{
			if (!base.hasAuthority)
			{
				base.photonView.TransferOwnership(PhotonNetwork.masterClient);
			}
			AuthorityResetToDefault(position, rotation, wasCleanedUp);
		}
	}

	public void MasterResetToLastSpawnPosition()
	{
		MasterResetToDefault(lastSpawnPosition, lastSpawnRotation);
	}

	public void MasterResetToLastPickup()
	{
		MasterResetToDefault(lastPickupPosition, lastPickupRotation);
	}

	public void AuthorityResetToDefault(Vector3 position, Quaternion rotation, bool wasCleanedUp = false)
	{
		if (base.hasAuthority)
		{
			HolderId = PhotonPlayer.Invalid;
			IsLocked = false;
			bool isEnabled = IsEnabled;
			Vector3 position2 = base.transform.position;
			base.transform.position = position;
			base.transform.rotation = rotation;
			Snap();
			IsEnabled = true;
			base.photonView.RPC("RpcOnReset", PhotonTargets.All, position, rotation, isEnabled, position2, wasCleanedUp);
		}
	}

	public void AuthorityDisableAndMove(Vector3 position, Quaternion rotation)
	{
		if (base.hasAuthority)
		{
			IsEnabled = false;
			HolderId = PhotonPlayer.Invalid;
			IsLocked = false;
			base.transform.position = position;
			base.transform.rotation = rotation;
			Snap();
		}
	}

	public void AuthorityResetToLastSpawnPosition()
	{
		AuthorityResetToDefault(lastSpawnPosition, lastSpawnRotation);
	}

	public void AuthorityResetToLastPickup()
	{
		AuthorityResetToDefault(lastPickupPosition, lastPickupRotation);
	}

	[PunRPC]
	protected void RpcOnReset(Vector3 position, Quaternion rotation, bool wasEnabled, Vector3 oldPosition, bool wasCleanedUp)
	{
		OnReset(position, rotation, wasEnabled, oldPosition, wasCleanedUp);
	}

	protected virtual void OnReset(Vector3 position, Quaternion rotation, bool wasEnabled, Vector3 oldPosition, bool wasCleanedUp)
	{
		if (ToolAudio != null)
		{
			ToolAudio.OnReset();
		}
		if (wasEnabled && AnimateInOut != null)
		{
			AnimateInOut.PlayEffect(false, oldPosition);
			AnimateInOut.PlayEffect(true);
		}
		intersectingKillzones.Clear();
		lastSpawnPosition = (lastPickupPosition = position);
		lastSpawnRotation = (lastPickupRotation = rotation);
		SetCollisionLayerLocalOnly(Layers.DynamicPhysics);
		if (this.ResetEvent != null)
		{
			this.ResetEvent(this, position, rotation);
		}
	}

	protected virtual void ResetOnKillzone(Killzone killzone)
	{
		resetOnKillzoneThisFrame = true;
		intersectingKillzones.Clear();
		if (!IsHeld && base.hasAuthority)
		{
			if (killzone.ToolRespawnLocation == Killzone.ToolSpawnLocation.LAST_PICKUP_POINT)
			{
				AuthorityResetToLastPickup();
			}
			else if (killzone.ToolRespawnLocation == Killzone.ToolSpawnLocation.LAST_SPAWN_POINT)
			{
				AuthorityResetToLastSpawnPosition();
			}
		}
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
		Vector3 point = ((collision.contacts.Length <= 0) ? base.transform.position : collision.contacts[0].point);
		if (IsHeld && Owner != null)
		{
			if (Owner.LeftHand.Tool == this)
			{
				Owner.LeftHand.Vibrate(50, 1000);
			}
			else if (Owner.RightHand.Tool == this)
			{
				Owner.RightHand.Vibrate(50, 1000);
			}
		}
		Tool colliderTool = collision.GetColliderTool();
		if (colliderTool != null)
		{
			OnToolCollisionEnter(colliderTool, point, collision);
			return;
		}
		Player.BodyPart bodyPart;
		Player colliderPlayer = collision.GetColliderPlayer(out bodyPart);
		if (colliderPlayer != null)
		{
			OnPlayerCollisionEnter(colliderPlayer, bodyPart, point, collision);
			return;
		}
		PhotonView colliderPhotonView = collision.GetColliderPhotonView();
		if (colliderPhotonView != null)
		{
			OnPhotonViewCollisionEnter(colliderPhotonView, point, collision);
		}
		else
		{
			OnOtherCollisionEnter((Layers)collision.gameObject.layer, point, collision);
		}
	}

	protected virtual void OnCollisionExit(Collision collision)
	{
		Tool colliderTool = collision.GetColliderTool();
		if (colliderTool != null && colliderTool.IsHeld)
		{
			OnDynamicKinematicCollision(colliderTool.Rigidbody, collision);
		}
	}

	protected virtual void OnToolCollisionEnter(Tool hitTool, Vector3 point, Collision collision)
	{
		if (hitTool.IsHeld)
		{
			OnDynamicKinematicCollision(hitTool.Rigidbody, collision);
		}
		if (hitTool.OwnershipTransferAllowed && base.hasAuthority && !hitTool.IsHeld && base.photonView.isOwnerActive && hitTool.photonView.ownerId != base.photonView.ownerId)
		{
			hitTool.photonView.TransferOwnership(base.photonView.ownerId);
		}
	}

	protected virtual void OnPlayerCollisionEnter(Player hitPlayer, Player.BodyPart bodyPart, Vector3 point, Collision collision)
	{
		hitPlayer.PlayerEvents.ToolHit(bodyPart, this);
		if (OwnershipTransferAllowed && !base.hasAuthority && !IsHeld && hitPlayer.isLocal)
		{
			base.photonView.TransferOwnership(hitPlayer.photonView.ownerId);
		}
		switch (bodyPart)
		{
		case Player.BodyPart.LeftHand:
			hitPlayer.LeftHand.Vibrate(50, 1000);
			break;
		case Player.BodyPart.RightHand:
			hitPlayer.RightHand.Vibrate(50, 1000);
			break;
		}
	}

	protected virtual void OnPhotonViewCollisionEnter(PhotonView hitPhotonView, Vector3 point, Collision collision)
	{
	}

	protected virtual void OnOtherCollisionEnter(Layers hitLayer, Vector3 point, Collision collision)
	{
	}

	protected virtual void OnTriggerEnter(Collider collider)
	{
		Vector3 position = base.transform.position;
		Killzone colliderKillzone = collider.GetColliderKillzone();
		if (colliderKillzone != null)
		{
			OnKillzoneTriggerEnter(colliderKillzone);
			return;
		}
		Tool colliderTool = collider.GetColliderTool();
		if (colliderTool != null)
		{
			OnToolTriggerEnter(colliderTool, position);
			return;
		}
		Player.BodyPart bodyPart;
		Player colliderPlayer = collider.GetColliderPlayer(out bodyPart);
		if (colliderPlayer != null)
		{
			OnPlayerTriggerEnter(colliderPlayer, bodyPart, position);
			return;
		}
		PhotonView colliderPhotonView = collider.GetColliderPhotonView();
		if (colliderPhotonView != null)
		{
			OnPhotonViewTriggerEnter(colliderPhotonView, position);
		}
	}

	protected virtual void OnTriggerExit(Collider collider)
	{
		Killzone colliderKillzone = collider.GetColliderKillzone();
		if (colliderKillzone != null)
		{
			OnKillzoneTriggerExit(colliderKillzone);
		}
	}

	protected virtual void OnKillzoneTriggerEnter(Killzone hitKillzone)
	{
		if (base.hasAuthority && SupportsKillzones && !resetOnKillzoneThisFrame)
		{
			if (hitKillzone.KillOnEnter)
			{
				ResetOnKillzone(hitKillzone);
			}
			else if (!intersectingKillzones.Contains(hitKillzone))
			{
				intersectingKillzones.Add(hitKillzone);
			}
		}
	}

	protected virtual void OnKillzoneTriggerExit(Killzone hitKillzone)
	{
		if (base.hasAuthority && intersectingKillzones.Contains(hitKillzone))
		{
			intersectingKillzones.Remove(hitKillzone);
		}
	}

	protected virtual void OnToolTriggerEnter(Tool hitTool, Vector3 point)
	{
	}

	protected virtual void OnPlayerTriggerEnter(Player hitPlayer, Player.BodyPart bodyPart, Vector3 point)
	{
		hitPlayer.PlayerEvents.ToolHit(bodyPart, this);
	}

	protected virtual void OnPhotonViewTriggerEnter(PhotonView hitPhotonView, Vector3 point)
	{
	}

	public virtual void ApplyForce(Vector3 position, Vector3 force, bool playVFX, bool applyForceAtPosition = false)
	{
		if (OwnershipTransferAllowed && !base.hasAuthority && !IsHeld)
		{
			base.photonView.TransferOwnership(PhotonNetwork.player);
		}
		if (applyForceAtPosition)
		{
			Rigidbody.AddForceAtPosition(force, position);
		}
		else
		{
			Vector3 normalized = force.normalized;
			float magnitude = Rigidbody.velocity.magnitude;
			Rigidbody.velocity = normalized * magnitude;
			Rigidbody.AddForce(force, ForceMode.Impulse);
		}
		base.photonView.RPC("RpcOnToolForceApplied", PhotonTargets.All, PhotonNetwork.player, position, force, playVFX);
	}

	public virtual void OnToolForceApplied(Player hittingPlayer, Vector3 position, Vector3 force)
	{
	}

	[PunRPC]
	public void RpcOnToolForceApplied(PhotonPlayer photonPlayer, Vector3 position, Vector3 force, bool playVFX)
	{
		FireForceApplied(photonPlayer.ToPlayer(), position, force);
		if (playVFX)
		{
			OnApplyForceVFX(photonPlayer, position, force);
		}
		ToolAudio.OnApplyForce(photonPlayer, position);
		OnToolForceApplied(photonPlayer.ToPlayer(), position, force);
	}

	protected void FireForceApplied(Player player, Vector3 position, Vector3 force)
	{
		if (this.ForceApplied != null)
		{
			this.ForceApplied(player, this, position, force);
		}
	}

	protected virtual void OnApplyForceVFX(PhotonPlayer photonPlayer, Vector3 position, Vector3 force)
	{
	}

	private void UpdateDynamicKinematicCollision()
	{
		previousPreviousVelocity = previousVelocity;
		previousVelocity = ((!Rigidbody.isKinematic) ? Rigidbody.velocity : Vector3.zero);
		if (lastDynamicKinematicRigidbody == null)
		{
			return;
		}
		if (dynamicKinematicCollisionThisFrame)
		{
			dynamicKinematicCollisionThisFrame = false;
			lastDynamicKinematicCollisionTime = Time.unscaledTime;
			Rigidbody.velocity = dynamicKinematicCollisionVelocity;
			Rigidbody.angularVelocity = dynamicKinematicCollisionAngularVelocity;
		}
		foreach (ToolCollider collider in Colliders)
		{
			if (collider.IsOverlapping(lastDynamicKinematicRigidbody))
			{
				lastDynamicKinematicCollisionTime = Time.unscaledTime;
				break;
			}
		}
		if (Time.unscaledTime - lastDynamicKinematicCollisionTime > 0.25f)
		{
			DisableDynamicKinematicCollisions(false);
			lastDynamicKinematicRigidbody = null;
		}
	}

	private void OnDynamicKinematicCollision(Rigidbody dynamicKinematicRigidbody, Collision collision)
	{
		if (dynamicKinematicCollisionsEnabled && !(lastDynamicKinematicRigidbody != null) && !(collision.impulse.magnitude <= Mathf.Epsilon))
		{
			CalculateDynamicKinematicCollisionPhysics(dynamicKinematicRigidbody, collision, out dynamicKinematicCollisionVelocity, out dynamicKinematicCollisionAngularVelocity);
			dynamicKinematicCollisionThisFrame = true;
			lastDynamicKinematicRigidbody = dynamicKinematicRigidbody;
			DisableDynamicKinematicCollisions(true);
		}
	}

	private void CalculateDynamicKinematicCollisionPhysics(Rigidbody dynamicKinematicRigidbody, Collision collision, out Vector3 linearVelocity, out Vector3 angularVelocity)
	{
		float mass = Rigidbody.mass;
		Vector3 vector = previousPreviousVelocity;
		Vector3 vector2 = mass * vector;
		float mass2 = dynamicKinematicRigidbody.mass;
		TrackedVelocity component = dynamicKinematicRigidbody.GetComponent<TrackedVelocity>();
		Vector3 recentLinearVelocity = component.RecentLinearVelocity;
		Vector3 vector3 = mass2 * recentLinearVelocity;
		Vector3 vector4 = recentLinearVelocity - vector;
		RecRoomCollider component2 = collision.collider.GetComponent<RecRoomCollider>();
		Vector3 collisionNormal = component2.GetCollisionNormal(collision);
		float kinematicBounciness = component2.KinematicBounciness;
		linearVelocity = collisionNormal * ((vector2 + vector3 + mass2 * kinematicBounciness * vector4) / (mass + mass2)).magnitude;
		Vector3 rhs = Vector3.ProjectOnPlane(recentLinearVelocity, collisionNormal);
		Vector3 normalized = Vector3.Cross(-collisionNormal, rhs).normalized;
		angularVelocity = normalized * rhs.magnitude * spin * ((float)Math.PI / 180f);
	}

	private void DisableDynamicKinematicCollisions(bool disable)
	{
		if (disable)
		{
			SetCollisionLayerLocalOnly(Layers.DynamicPhysicsIgnoreDynamicPhysics);
		}
		else
		{
			SetCollisionLayerLocalOnly(Layers.DynamicPhysics);
		}
	}

	public static Tool Find(int viewId)
	{
		PhotonView photonView = PhotonView.Find(viewId);
		return (!(photonView == null)) ? photonView.GetComponent<Tool>() : null;
	}
}
