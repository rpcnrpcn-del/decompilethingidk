using System;
using UnityEngine;

public abstract class GroupableTool : Tool
{
	[Header("Groupable Tool")]
	[SerializeField]
	private GameObject frontGroupAddHighlightVisual;

	[SerializeField]
	private GameObject backGroupAddHighlightVisual;

	[SerializeField]
	protected GroupSearch defaultSearchCollider;

	[SerializeField]
	protected GroupSearch heldSearchCollider;

	private SynchronizedField<int> _thisGroupId;

	[NonSerialized]
	public ToolGroup Group;

	private GroupableTool[] newGroupInitialTools;

	private GroupableTool _hoveredGroupableTool;

	private ToolGroup _hoveredGroup;

	public ToolGroup ThisGroup
	{
		get
		{
			PhotonView photonView = PhotonView.Find(_thisGroupId.Get());
			return (!(photonView != null)) ? null : photonView.GetComponent<ToolGroup>();
		}
		set
		{
			int newValue = ((!(value != null)) ? (-1) : value.photonView.viewID);
			_thisGroupId.ForceSet(newValue);
		}
	}

	public Vector3 GroupLocalPosition { get; protected set; }

	public Quaternion GroupLocalRotation { get; protected set; }

	public Vector3 ColliderLocalPosition { get; protected set; }

	public Quaternion ColliderLocalRotation { get; protected set; }

	public Bounds LocalBoundingBox { get; private set; }

	public bool IsInGroup
	{
		get
		{
			return Group != null;
		}
	}

	protected GroupableTool hoveredGroupableTool
	{
		get
		{
			return _hoveredGroupableTool;
		}
		set
		{
			if (!(_hoveredGroupableTool == value))
			{
				if (_hoveredGroupableTool != null)
				{
					_hoveredGroupableTool.DisplayAddToolHighlight(null);
				}
				_hoveredGroupableTool = value;
				if (_hoveredGroupableTool != null)
				{
					_hoveredGroupableTool.DisplayAddToolHighlight(this);
				}
			}
		}
	}

	protected ToolGroup hoveredGroup
	{
		get
		{
			return _hoveredGroup;
		}
		set
		{
			if (!(_hoveredGroup == value))
			{
				if (_hoveredGroup != null)
				{
					_hoveredGroup.DisplayAddToolHighlight(null);
				}
				_hoveredGroup = value;
				if (_hoveredGroup != null)
				{
					_hoveredGroup.DisplayAddToolHighlight(this);
				}
			}
		}
	}

	public bool FrontAddHighlightEnabled
	{
		get
		{
			return frontGroupAddHighlightVisual != null && frontGroupAddHighlightVisual.activeSelf;
		}
		set
		{
			if (frontGroupAddHighlightVisual != null)
			{
				frontGroupAddHighlightVisual.SetActive(value);
			}
		}
	}

	public bool BackAddHighlightEnabled
	{
		get
		{
			return backGroupAddHighlightVisual != null && backGroupAddHighlightVisual.activeSelf;
		}
		set
		{
			if (backGroupAddHighlightVisual != null)
			{
				backGroupAddHighlightVisual.SetActive(value);
			}
		}
	}

	protected GroupSearch SearchCollider
	{
		get
		{
			return (!base.IsHeld || !(heldSearchCollider != null)) ? defaultSearchCollider : heldSearchCollider;
		}
	}

	protected abstract string GroupPrefabName { get; }

	protected override void Awake()
	{
		base.Awake();
		GroupLocalPosition = Vector3.zero;
		GroupLocalRotation = Quaternion.identity;
		DisplayAddToolHighlight(null);
	}

	protected override void Start()
	{
		base.Start();
		Quaternion rotation = base.transform.rotation;
		base.transform.rotation = Quaternion.identity;
		Bounds colliderBounds = base.ColliderBounds;
		LocalBoundingBox = new Bounds(base.transform.InverseTransformPoint(colliderBounds.center), colliderBounds.size);
		base.transform.rotation = rotation;
		base.RigidbodyPickup.ReachedHand += OnReachedHand;
	}

	public virtual bool CanAddTool(GroupableTool tool)
	{
		return true;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		UpdateHover();
		if (!base.IsHeld && (FrontAddHighlightEnabled || BackAddHighlightEnabled))
		{
			FrontAddHighlightEnabled = false;
			BackAddHighlightEnabled = false;
		}
	}

	public void UpdateGroupLayout(Vector3 localPosition, Quaternion localRotation, Vector3 colliderLocalPosition, Quaternion colliderLocalRotation)
	{
		if (IsInGroup)
		{
			GroupLocalPosition = localPosition;
			GroupLocalRotation = localRotation;
			ColliderLocalPosition = colliderLocalPosition;
			ColliderLocalRotation = colliderLocalRotation;
			base.RigidbodyPickup.Pickup(Group.Rigidbody, GroupLocalPosition, GroupLocalRotation);
		}
	}

	public override void Pickup(Player player, Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		if (IsInGroup)
		{
			if (!Group.hasAuthority && player.isLocal)
			{
				Group.photonView.TransferOwnership(PhotonNetwork.player);
			}
			ToolGroup toolGroup = Group;
			Group.AuthorityRemoveTool(this);
			if (toolGroup.Size == 0)
			{
				toolGroup.AuthorityDestroy();
			}
		}
		base.Pickup(player, target, targetSpacePickupPosition, targetSpacePickupRotation);
	}

	private void OnReachedHand()
	{
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		base.Release(player, linearVelocity, angularVelocity);
		if (IsInGroup && !player.isLocal)
		{
			UpdateGroupLayout(GroupLocalPosition, GroupLocalRotation, ColliderLocalPosition, ColliderLocalRotation);
		}
		TryAddToHoveredGroup();
	}

	private void UpdateHover()
	{
		if (IsInGroup || !base.hasAuthority)
		{
			hoveredGroupableTool = null;
			hoveredGroup = null;
			return;
		}
		hoveredGroupableTool = SearchCollider.FindGroupableTool(this);
		if (hoveredGroupableTool != null)
		{
			hoveredGroup = null;
		}
		else
		{
			hoveredGroup = SearchCollider.FindGroup(this);
		}
	}

	protected void TryAddToHoveredGroup()
	{
		if (!base.hasAuthority)
		{
			return;
		}
		if (hoveredGroup != null)
		{
			if (!hoveredGroup.hasAuthority)
			{
				hoveredGroup.photonView.TransferOwnership(PhotonNetwork.player);
			}
			hoveredGroup.AuthorityAddTool(this);
			hoveredGroup = null;
		}
		else if (hoveredGroupableTool != null)
		{
			ToolGroup toolGroup = hoveredGroupableTool.CreateNewGroup();
			if (hoveredGroupableTool.hoveredGroupableTool == this)
			{
				hoveredGroupableTool.hoveredGroupableTool = null;
			}
			toolGroup.AuthorityAddTool(hoveredGroupableTool);
			toolGroup.AuthorityAddTool(this);
			hoveredGroupableTool = null;
		}
	}

	private void InitializeNewGroup(ToolGroup newGroup)
	{
		if (newGroupInitialTools != null && newGroupInitialTools.Length > 0)
		{
			for (int i = 0; i < newGroupInitialTools.Length; i++)
			{
				newGroupInitialTools[i].hoveredGroupableTool = null;
			}
			newGroup.AuthorityAddTools(newGroupInitialTools);
		}
	}

	public ToolGroup CreateNewGroup()
	{
		if (PhotonNetwork.isMasterClient)
		{
			return MasterCreateNewGroup();
		}
		RequestMasterCreateNewGroup();
		return AuthorityCreateTemporaryGroup();
	}

	private void RequestMasterCreateNewGroup()
	{
		base.photonView.RPC("RpcMasterCreateNewGroup", PhotonTargets.MasterClient, PhotonNetwork.player);
	}

	private ToolGroup MasterCreateNewGroup()
	{
		ToolGroup result = null;
		if (PhotonNetwork.isMasterClient)
		{
			result = PhotonNetwork.InstantiateSceneObject(GroupPrefabName, base.transform.position, base.transform.rotation, 0, null).GetComponent<ToolGroup>();
		}
		return result;
	}

	private ToolGroup AuthorityCreateTemporaryGroup()
	{
		return PhotonNetwork.Instantiate(GroupPrefabName, base.transform.position, base.transform.rotation, 0, null).GetComponent<ToolGroup>();
	}

	private void OnMasterGroupCreation(ToolGroup newGroup)
	{
		if (!newGroup.hasAuthority)
		{
			newGroup.photonView.TransferOwnership(PhotonNetwork.player);
		}
		if (!IsInGroup || (Group.IsHeld && !Group.Owner.isLocal))
		{
			newGroup.AuthorityDestroy();
			return;
		}
		ToolGroup toolGroup = Group;
		newGroup.AuthorityClone(toolGroup);
		toolGroup.AuthorityDestroy();
	}

	public virtual void DisplayAddToolHighlight(GroupableTool toolToAdd)
	{
		FrontAddHighlightEnabled = toolToAdd != null;
		BackAddHighlightEnabled = false;
	}

	protected override void OnDestroy()
	{
		if (IsInGroup && Group.hasAuthority)
		{
			Group.AuthorityRemoveTool(this);
		}
		base.RigidbodyPickup.ReachedHand -= OnReachedHand;
		base.OnDestroy();
	}

	public override void OnHoverStart(bool physicalHover)
	{
		base.OnHoverStart(physicalHover);
		if (IsInGroup)
		{
			Group.OnToolInGroupHoverStart(this, physicalHover);
		}
	}

	public override void OnHoverEnd()
	{
		if (IsInGroup)
		{
			base.ToolRenderer.Mode = Group.ToolRenderer.Mode;
			Group.OnToolInGroupHoverEnd(this);
		}
		else
		{
			base.OnHoverEnd();
		}
	}

	public override Tool GetHighlightedTool(Player player, ToolCollider collider, bool physicalPickup, Vector3 pickupPosition)
	{
		if (IsInGroup)
		{
			GroupableTool highlightedToolInGroup = Group.GetHighlightedToolInGroup(this, player, physicalPickup, pickupPosition);
			if (highlightedToolInGroup != null)
			{
				return highlightedToolInGroup.GetBaseHighlightedTool(player, collider, physicalPickup, pickupPosition);
			}
			return Group.GetHighlightedTool(player, collider, physicalPickup, pickupPosition);
		}
		return base.GetHighlightedTool(player, collider, physicalPickup, pickupPosition);
	}

	private Tool GetBaseHighlightedTool(Player player, ToolCollider collider, bool physicalPickup, Vector3 pickupPosition)
	{
		return base.GetHighlightedTool(player, collider, physicalPickup, pickupPosition);
	}

	[PunRPC]
	protected void RpcMasterCreateNewGroup(PhotonPlayer requester)
	{
		if (PhotonNetwork.isMasterClient && requester != null)
		{
			ToolGroup toolGroup = MasterCreateNewGroup();
			if (toolGroup != null)
			{
				base.photonView.RPC("RpcOnMasterGroupCreation", requester, toolGroup.photonView.viewID);
			}
		}
	}

	[PunRPC]
	protected void RpcOnMasterGroupCreation(int toolGroupPhotonViewId)
	{
		PhotonView photonView = PhotonView.Find(toolGroupPhotonViewId);
		ToolGroup toolGroup = ((!(photonView != null)) ? null : photonView.GetComponent<ToolGroup>());
		if (toolGroup != null)
		{
			OnMasterGroupCreation(toolGroup);
		}
	}
}
