using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OutfitToolRenderer))]
public class OutfitTool : Tool
{
	public enum OutfitToolState
	{
		None = -1,
		InOpenDrawer = 0,
		InOpenBox = 1,
		AnimatingIn = 2,
		AnimatingOut = 3,
		InHand = 4,
		InWorld = 5,
		Disabled = 6
	}

	public enum PlayerEquipState
	{
		None = -1,
		Wearing = 0,
		NotWearing = 1,
		Attaching = 2
	}

	public enum ToolPurpose
	{
		Drawer = 0,
		GiftBox = 1,
		Doffing = 2
	}

	[SerializeField]
	private Vector3 minScale = Vector3.one * 0.05f;

	[SerializeField]
	private Vector3 maxScale = Vector3.one;

	private Vector3 animationTargetScale = Vector3.one;

	private static Dictionary<int, List<OutfitTool>> ToolWearerMap = new Dictionary<int, List<OutfitTool>>();

	private const int TRUE = 1;

	private const int FALSE = 0;

	private int wearerId;

	private PhotonPlayer _wearer;

	private PlayerEquipState _equipState = PlayerEquipState.None;

	private OutfitToolState _currentState = OutfitToolState.None;

	private Vector3 startPosition = Vector3.zero;

	private Quaternion startRotation = Quaternion.identity;

	private float returnHomeSpeed = 0.5f;

	private float returnStartTime;

	[Header("Return to drawer position")]
	private float drawerSpawSpeed = 180f;

	[SerializeField]
	private float animateInScaleSpeed = 15f;

	private bool canCollideWithPlayer = true;

	private float dontCollideWithPlayerStartTime;

	private float dontCollideWithPlayerDuration;

	private float DrawerScaleFactor
	{
		get
		{
			if (Drawer == null)
			{
				return 1f;
			}
			return Drawer.ItemScaleFactor;
		}
	}

	private Vector3 DrawerScale
	{
		get
		{
			return Vector3.Lerp(minScale, maxScale, DrawerScaleFactor);
		}
	}

	public string IS_NEW_PROPERTY
	{
		get
		{
			return Player.LocalPlayer.PlayerId + ".Outfit.Used." + OutfitTarget.ToString();
		}
	}

	public bool IsNew
	{
		get
		{
			return IsWearerLocal && PlayerPrefs.GetInt(IS_NEW_PROPERTY, 0) == 1;
		}
		set
		{
			if (IsWearerLocal)
			{
				PlayerPrefs.SetInt(IS_NEW_PROPERTY, value ? 1 : 0);
				OutfitSelection outfitSelection = OutfitManager.Instance.UnlockedOutfitSelections.FindLast((OutfitSelection s) => s.Equals(OutfitTarget));
				if (outfitSelection != null)
				{
					outfitSelection.IsNew = value;
				}
				UpdateItemUI();
				if (Drawer != null)
				{
					Drawer.UpdateNewIcon();
				}
			}
		}
	}

	public PhotonPlayer Wearer
	{
		get
		{
			return _wearer;
		}
		set
		{
			_wearer = value;
			if (_wearer != null)
			{
				wearerId = _wearer.ID;
				GetToolsForWearer(_wearer).DoIfNotNull(delegate(List<OutfitTool> list)
				{
					list.Add(this);
				});
			}
		}
	}

	public bool IsWearerLocal
	{
		get
		{
			return Wearer != null && Wearer.isLocal;
		}
	}

	private bool IsVisible
	{
		get
		{
			return base.gameObject.activeSelf && (ToolVisual.gameObject.activeSelf || GhostVisual.gameObject.activeSelf);
		}
	}

	private Renderer ToolVisual
	{
		get
		{
			return base.ToolRenderer.Renderers[0];
		}
	}

	public PlayerEquipState EquipState
	{
		get
		{
			return _equipState;
		}
		set
		{
			if (_equipState == value)
			{
				return;
			}
			if (value == PlayerEquipState.Attaching || value == PlayerEquipState.Wearing)
			{
				bool visible = true;
				bool visible2 = false;
				if (OutfitManager.GetMatchingBodyPart(OutfitTarget.outfitItem.Type).IsHand())
				{
					visible = false;
					visible2 = true;
				}
				SetRendererVisible(GhostVisual, visible);
				SetRendererVisible(ToolVisual, visible2);
				if (IsNew)
				{
					IsNew = false;
				}
				if (Purpose == ToolPurpose.GiftBox)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
			else
			{
				SetRendererVisible(GhostVisual, false);
				SetRendererVisible(ToolVisual, true);
			}
			_equipState = value;
		}
	}

	public bool IsLevelLocked
	{
		get
		{
			Player player = Wearer.ToPlayer();
			return player != null && player.PlayerProgression.Level < OutfitTarget.Level;
		}
	}

	public OutfitToolState CurrentState
	{
		get
		{
			return _currentState;
		}
		set
		{
			if (value == _currentState)
			{
				return;
			}
			OutfitToolState currentState = CurrentState;
			_currentState = value;
			switch (value)
			{
			case OutfitToolState.InWorld:
				SetUsingGravityRigidBody(true);
				ToolCollider.enabled = true;
				SetRendererVisible(GhostVisual, false);
				SetRendererVisible(ToolVisual, true);
				SafeParentToWearer();
				base.transform.localScale = maxScale;
				SetEnabledIfLocal(true);
				if (Purpose == ToolPurpose.GiftBox)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
				break;
			case OutfitToolState.InHand:
				SetUsingGravityRigidBody(false);
				ToolCollider.enabled = true;
				SetRendererVisible(GhostVisual, false);
				SetRendererVisible(ToolVisual, true);
				SafeParentToWearer();
				base.transform.localScale = maxScale;
				SetEnabledIfLocal(true);
				if (base.AnimateInOut != null)
				{
					base.AnimateInOut.SuppressAnimation = false;
				}
				break;
			case OutfitToolState.InOpenDrawer:
				if (Drawer == null)
				{
					Debug.LogWarning("Tried to move to InDrawer state with no Drawer set.");
					CurrentState = OutfitToolState.Disabled;
					return;
				}
				if (!Drawer.IsSelected)
				{
					CurrentState = OutfitToolState.Disabled;
					return;
				}
				SetUsingGravityRigidBody(false);
				ToolCollider.enabled = true;
				base.transform.localPosition = targetSpawnPosition;
				base.transform.localRotation = targetSpawnRotation;
				base.transform.localScale = DrawerScale;
				base.transform.SetParent(Drawer.StartTransform, false);
				SetEnabledIfLocal(true);
				break;
			case OutfitToolState.InOpenBox:
				if (Box == null)
				{
					Debug.LogWarning("Tried to move to InBox state with no Box set.");
					CurrentState = OutfitToolState.Disabled;
					return;
				}
				if (Box.BoxState != GiftBox.GiftBoxState.Open)
				{
					CurrentState = OutfitToolState.Disabled;
					SetEnabledIfLocal(false);
					return;
				}
				SetUsingGravityRigidBody(false);
				ToolCollider.enabled = true;
				base.transform.localPosition = Vector3.zero;
				base.transform.localRotation = Quaternion.identity;
				base.transform.localScale = DrawerScale;
				base.transform.SetParent(Box.ToolAnchor, false);
				SetEnabledIfLocal(true);
				if (base.IsEnabled != base.gameObject.activeSelf)
				{
					base.gameObject.SetActive(base.IsEnabled);
				}
				break;
			case OutfitToolState.AnimatingIn:
				SetUsingGravityRigidBody(false);
				ToolCollider.enabled = false;
				base.transform.position = Drawer.OutfitSpawnPosition;
				base.transform.SetParent(Drawer.StartTransform, true);
				animationTargetScale = DrawerScale;
				startPosition = base.transform.localPosition;
				startRotation = base.transform.localRotation;
				returnHomeSpeed = drawerSpawSpeed;
				returnStartTime = Time.time;
				base.transform.localScale = Vector3.one * 0.05f;
				SetEnabledIfLocal(true);
				break;
			case OutfitToolState.AnimatingOut:
				if (currentState == OutfitToolState.Disabled)
				{
					CurrentState = OutfitToolState.Disabled;
					return;
				}
				SetUsingGravityRigidBody(false);
				ToolCollider.enabled = false;
				animationTargetScale = minScale;
				returnHomeSpeed = drawerSpawSpeed;
				returnStartTime = Time.time;
				startPosition = base.transform.localPosition;
				startRotation = base.transform.localRotation;
				SetEnabledIfLocal(true);
				break;
			default:
				SetUsingGravityRigidBody(false);
				ToolCollider.enabled = false;
				SetEnabledIfLocal(false);
				break;
			}
			UpdateItemUI();
		}
	}

	public ToolPurpose Purpose { get; set; }

	public OutfitToolUIControl OutfitToolUIControl { get; set; }

	public OutfitSelection OutfitTarget { get; set; }

	public OutfitDrawer Drawer { get; set; }

	public int DrawerIndex { get; set; }

	public GiftBox Box { get; set; }

	public Vector3 DrawerPosition { get; set; }

	public Quaternion DrawerRotation { get; set; }

	public Collider ToolCollider { get; set; }

	public Renderer GhostVisual { get; set; }

	public Collider GhostCollider { get; set; }

	private Vector3 targetSpawnPosition
	{
		get
		{
			return DrawerPosition;
		}
	}

	private Quaternion targetSpawnRotation
	{
		get
		{
			return DrawerRotation;
		}
	}

	public event Action<OutfitTool> PickupAttemptEvent;

	private bool TryGetPlayerLevel(out int level)
	{
		level = 0;
		Player player = Wearer.ToPlayer();
		if (player != null)
		{
			level = player.PlayerProgression.Level;
			return true;
		}
		return false;
	}

	protected override void Start()
	{
		base.Start();
		pickedCollisionLayer = Layers.DynamicPhysicsIgnoreStaticPhysics;
		if (OutfitTarget.outfitItem.UsesHairColor && Wearer.ToPlayer() != null)
		{
			if (Wearer.ToPlayer().PlayerOutfit.HairColor != null)
			{
				PlayerOutfit_HairColorChanged(Wearer.ToPlayer().PlayerOutfit.HairColor.color);
			}
			if (IsWearerLocal)
			{
				Player.LocalPlayer.PlayerOutfit.HairColorChanged += PlayerOutfit_HairColorChanged;
			}
		}
		if (Drawer != null)
		{
			Drawer.UpdateToolVisibility();
		}
		Wearer.ToPlayer().DoIfNotNull(delegate(Player player)
		{
			player.PlayerEvents.LevelUpEvent += UpdateItemUI;
		});
		UpdateItemUI();
	}

	protected override void OnDestroy()
	{
		if (Player.LocalPlayer != null && OutfitTarget != null && OutfitTarget.outfitItem.UsesHairColor)
		{
			Player.LocalPlayer.PlayerOutfit.HairColorChanged -= PlayerOutfit_HairColorChanged;
		}
		Wearer.ToPlayer().DoIfNotNull(delegate(Player player)
		{
			player.PlayerEvents.LevelUpEvent -= UpdateItemUI;
		});
		GetToolsForWearer(wearerId).Remove(this);
		base.OnDestroy();
	}

	protected void Update()
	{
		if (!(Player.LocalPlayer != null) || Wearer == null || !Wearer.isLocal)
		{
			return;
		}
		if ((IsVisible && Vector3.Distance(base.transform.localScale, animationTargetScale) != 0f && CurrentState == OutfitToolState.AnimatingIn) || CurrentState == OutfitToolState.AnimatingOut)
		{
			Vector3 localScale = Vector3.Slerp(base.transform.localScale, animationTargetScale, Time.deltaTime * animateInScaleSpeed);
			base.transform.localScale = localScale;
			if (Vector3.Distance(base.transform.localScale, animationTargetScale) < 0.01f && CurrentState == OutfitToolState.AnimatingOut)
			{
				CurrentState = OutfitToolState.Disabled;
			}
		}
		if (CurrentState == OutfitToolState.AnimatingIn)
		{
			UpdatePathing(returnHomeSpeed, returnStartTime, Time.time, startPosition, targetSpawnPosition, startRotation, targetSpawnRotation);
		}
		if (!canCollideWithPlayer && Time.time - dontCollideWithPlayerStartTime > dontCollideWithPlayerDuration)
		{
			canCollideWithPlayer = true;
		}
	}

	protected override void OnPlayerTriggerEnter(Player hitPlayer, Player.BodyPart bodyPart, Vector3 point)
	{
		if (!canCollideWithPlayer || !hitPlayer.isLocal || !IsWearerLocal)
		{
			return;
		}
		bool flag = bodyPart.IsHand();
		PlayerHand playerHand = null;
		if (flag)
		{
			playerHand = ((bodyPart != Player.BodyPart.LeftHand) ? base.Owner.RightHand : base.Owner.LeftHand);
		}
		if ((!flag || playerHand.Tool != this) && OutfitManager.IsMatchingBodyPart(bodyPart, OutfitTarget.outfitItem.Type))
		{
			if (base.IsHeld)
			{
				SetOutfitToPlayer(hitPlayer, bodyPart);
			}
			else if (!hitPlayer.IsInChangingRoom && Purpose == ToolPurpose.Doffing)
			{
				SetOutfitToPlayer(hitPlayer, bodyPart);
				EquipState = PlayerEquipState.Attaching;
				AttachToPlayer(Wearer.ToPlayer());
			}
		}
	}

	private void SafeParentToWearer()
	{
		Player player;
		if (Wearer != null && (player = Wearer.ToPlayer()) != null)
		{
			base.transform.SetParent(player.transform.parent, true);
		}
	}

	private void SetEnabledIfLocal(bool enabled)
	{
		if (Wearer != null && Wearer.isLocal)
		{
			base.IsEnabled = enabled;
		}
	}

	private void PlayerOutfit_HairColorChanged(Color color)
	{
		base.ToolRenderer.Color = color;
	}

	private void SetUsingGravityRigidBody(bool usingGravity)
	{
		if (base.Rigidbody != null)
		{
			base.Rigidbody.useGravity = usingGravity;
			base.Rigidbody.isKinematic = !usingGravity;
			if (!usingGravity)
			{
				base.Rigidbody.ClearVelocity();
			}
		}
	}

	private void SetRendererVisible(Renderer renderer, bool visible)
	{
		renderer.gameObject.SetActive(visible);
		renderer.enabled = visible;
	}

	public void DontCollideWithPlayer(float duration)
	{
		dontCollideWithPlayerDuration = duration;
		dontCollideWithPlayerStartTime = Time.time;
		canCollideWithPlayer = false;
	}

	private void SetOutfitToPlayer(Player player, Player.BodyPart bodyPart)
	{
		if (EquipState == PlayerEquipState.Attaching)
		{
			return;
		}
		if (player != null)
		{
			player.PlayerOutfit.SetOutfit(OutfitTarget, bodyPart, true);
			if (base.IsHeld)
			{
				EquipState = PlayerEquipState.Attaching;
				player.ToolController.ReleaseTool(this);
			}
			else
			{
				EquipState = PlayerEquipState.Wearing;
			}
		}
		else
		{
			Debug.LogError(string.Concat("SetOutfitToPlayer has null player. BodyPart : ", bodyPart, " - ", base.gameObject.GetGameObjectHierarchy()));
		}
	}

	public override void Pickup(Player player, Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		base.Pickup(player, target, targetSpacePickupPosition, targetSpacePickupRotation);
		CurrentState = OutfitToolState.InHand;
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		base.Release(player, linearVelocity, angularVelocity);
		if (CurrentState != OutfitToolState.Disabled)
		{
			SetStateForToolRelease(player);
		}
	}

	private bool AttachToPlayer(Player player)
	{
		if (EquipState == PlayerEquipState.Attaching)
		{
			EquipState = PlayerEquipState.Wearing;
			if (Drawer != null)
			{
				CurrentState = OutfitToolState.InOpenDrawer;
			}
			else
			{
				CurrentState = OutfitToolState.Disabled;
			}
			return true;
		}
		return false;
	}

	private void SetStateForToolRelease(Player player)
	{
		if (!AttachToPlayer(player))
		{
			if (Drawer != null && player.IsInChangingRoom)
			{
				CurrentState = OutfitToolState.InOpenDrawer;
			}
			else
			{
				CurrentState = OutfitToolState.InWorld;
			}
			EquipState = PlayerEquipState.NotWearing;
		}
	}

	public override void GetHandSpacePickupTransform(Transform handTransform, PlayerHand.HandType handType, out Vector3 position, out Quaternion rotation)
	{
		position = Vector3.zero;
		rotation = handTransform.InverseTransformRotation(base.transform.rotation);
	}

	public bool TryPickupOutfitTool(Player player)
	{
		if (this.PickupAttemptEvent != null)
		{
			this.PickupAttemptEvent(this);
		}
		if (OutfitTarget.Level > player.PlayerProgression.Level)
		{
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "You need to be level " + OutfitTarget.Level + " to use this", 1.5f);
			SingletonMonoBehaviour<TutorialManager>.Instance.PlayVO(OutfitManager.Instance.LockedOutfitToolClip);
			if (Purpose == ToolPurpose.GiftBox)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			return false;
		}
		if (Purpose == ToolPurpose.GiftBox && IsNew)
		{
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "Item saved!", 3f);
		}
		if (Purpose == ToolPurpose.Drawer)
		{
			IsNew = false;
		}
		return true;
	}

	protected override void OnPhotonPlayerDisconnected(PhotonPlayer oldPlayer)
	{
		ClearToolsForPlayer(oldPlayer);
		base.OnPhotonPlayerDisconnected(oldPlayer);
	}

	public static OutfitTool Find(Player player, OutfitSelection selection, ToolPurpose purpose)
	{
		if (player == null)
		{
			return null;
		}
		return Find(player.PhotonPlayer, selection, purpose);
	}

	public static OutfitTool Find(PhotonPlayer player, OutfitSelection selection, ToolPurpose purpose)
	{
		if (player == null)
		{
			return null;
		}
		List<OutfitTool> toolsForWearer = GetToolsForWearer(player);
		if (toolsForWearer != null)
		{
			foreach (OutfitTool item in toolsForWearer)
			{
				if (item.OutfitTarget != null && item.OutfitTarget == selection && item.Purpose == purpose)
				{
					return item;
				}
			}
		}
		return null;
	}

	private bool ClearToolsForPlayer(PhotonPlayer photonPlayer)
	{
		return ToolWearerMap.Remove(photonPlayer.ID);
	}

	private static List<OutfitTool> GetToolsForWearer(PhotonPlayer photonPlayer)
	{
		if (photonPlayer == null)
		{
			Debug.LogWarning("Looking up tools for invalid wearer");
			return null;
		}
		return GetToolsForWearer(photonPlayer.ID);
	}

	private static List<OutfitTool> GetToolsForWearer(int photonPlayerID)
	{
		List<OutfitTool> value;
		if (!ToolWearerMap.TryGetValue(photonPlayerID, out value))
		{
			value = new List<OutfitTool>();
			ToolWearerMap[photonPlayerID] = value;
		}
		return value;
	}

	private void UpdateItemUI(int level)
	{
		UpdateItemUI();
	}

	private void UpdateItemUI()
	{
		if (OutfitToolUIControl != null)
		{
			OutfitToolUIControl.State = OutfitToolUIControl.ItemProgressionState.None;
			if (IsNew)
			{
				OutfitToolUIControl.State |= OutfitToolUIControl.ItemProgressionState.New;
			}
			if (IsLevelLocked)
			{
				OutfitToolUIControl.State |= OutfitToolUIControl.ItemProgressionState.Locked;
			}
		}
	}

	private void UpdatePathing(float speed, float startTime, float currentTime, Vector3 startPosition, Vector3 targetPosition, Quaternion startRotation, Quaternion targetRotation)
	{
		Vector3 a = Vector3.Project(startPosition, Vector3.up);
		Vector3 b = Vector3.Project(targetPosition, Vector3.up);
		Vector3 vector = Vector3.ProjectOnPlane(startPosition, Vector3.up);
		Vector3 vector2 = Vector3.ProjectOnPlane(targetPosition, Vector3.up);
		float magnitude = vector.magnitude;
		vector /= magnitude;
		float magnitude2 = vector2.magnitude;
		vector2 /= magnitude2;
		float num = Vector3.Angle(vector, vector2);
		if (num >= Mathf.Epsilon && Vector3.Dot(Vector3.Cross(vector, vector2), Vector3.up) < 0f)
		{
			num = 360f - num;
		}
		float num2 = num / speed;
		float num3 = Mathf.Clamp01((currentTime - startTime) / num2);
		if (num3 >= 1f)
		{
			CurrentState = OutfitToolState.InOpenDrawer;
			return;
		}
		num *= num3;
		Vector3 normalized = (Quaternion.AngleAxis(num, Vector3.up) * vector).normalized;
		Vector3 vector3 = normalized * Mathf.Lerp(magnitude, magnitude2, num3);
		Vector3 vector4 = Vector3.Lerp(a, b, num3);
		base.transform.localPosition = vector3 + vector4;
		base.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, num3);
	}
}
