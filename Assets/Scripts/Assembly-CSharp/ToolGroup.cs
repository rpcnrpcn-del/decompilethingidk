using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ToolGroup : Tool
{
	protected List<GroupableTool> tools = new List<GroupableTool>();

	protected BoxCollider groupBoxCollider;

	protected Bounds groupBounds = default(Bounds);

	[SerializeField]
	protected GroupSearchCustomGeometry defaultSearchCollider;

	[SerializeField]
	protected GroupSearchCustomGeometry heldSearchCollider;

	private Coroutine delayedRemoveUpdateCoroutine;

	protected static readonly Vector3[] CornerMasks = new Vector3[8]
	{
		new Vector3(1f, 1f, -1f),
		new Vector3(1f, -1f, -1f),
		new Vector3(-1f, 1f, -1f),
		new Vector3(-1f, -1f, -1f),
		new Vector3(1f, 1f, 1f),
		new Vector3(1f, -1f, 1f),
		new Vector3(-1f, 1f, 1f),
		new Vector3(-1f, -1f, 1f)
	};

	private ToolGroup _hoveredGroup;

	[SerializeField]
	private float removeMinDistance = 0.05f;

	protected GroupSearchCustomGeometry SearchCollider
	{
		get
		{
			return (!base.IsHeld || !(heldSearchCollider != null)) ? defaultSearchCollider : heldSearchCollider;
		}
	}

	public int Size
	{
		get
		{
			return tools.Count;
		}
	}

	public bool GroupToolRenderersAreVisible
	{
		get
		{
			for (int i = 0; i < tools.Count; i++)
			{
				if (tools[i].ToolRenderer.Visible)
				{
					return true;
				}
			}
			return false;
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
					_hoveredGroup.DisplayAddGroupHighlight(null);
				}
				_hoveredGroup = value;
				if (_hoveredGroup != null)
				{
					_hoveredGroup.DisplayAddGroupHighlight(this);
				}
			}
		}
	}

	protected bool DelayedUpdateCoroutineIsRunning
	{
		get
		{
			return delayedRemoveUpdateCoroutine != null;
		}
	}

	private bool DefaultAddHighlightEnabled
	{
		set
		{
			for (int i = 0; i < tools.Count; i++)
			{
				bool frontAddHighlightEnabled = i == tools.Count - 1 && value;
				tools[i].FrontAddHighlightEnabled = frontAddHighlightEnabled;
				tools[i].BackAddHighlightEnabled = false;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		groupBoxCollider = GetComponentInChildren<BoxCollider>();
		DisplayAddToolHighlight(null);
		DisplayAddGroupHighlight(null);
	}

	public void AuthorityDestroy()
	{
		if (base.hasAuthority)
		{
			PhotonNetwork.Destroy(base.gameObject);
		}
	}

	public void AuthorityAddTools(GroupableTool[] tools)
	{
		if (base.hasAuthority && tools != null && tools.Length > 0 && CanAddTool(tools[0]))
		{
			Vector3[] array = new Vector3[tools.Length];
			Quaternion[] array2 = new Quaternion[tools.Length];
			for (int i = 0; i < tools.Length; i++)
			{
				array[i] = base.transform.InverseTransformPoint(tools[i].transform.position);
				array2[i] = base.transform.InverseTransformRotation(tools[i].transform.rotation);
			}
			AuthorityAddTools(tools, array, array2);
		}
	}

	public void AuthorityAddTools(GroupableTool[] tools, Vector3[] localPositions, Quaternion[] localRotations)
	{
		if (base.hasAuthority && tools != null && localPositions != null && localRotations != null && tools.Length > 0 && tools.Length == localPositions.Length && tools.Length == localRotations.Length && CanAddTool(tools[0]))
		{
			int[] array = new int[tools.Length];
			for (int i = 0; i < tools.Length; i++)
			{
				array[i] = tools[i].photonView.viewID;
			}
			int desiredNewToolIndex = GetDesiredNewToolIndex(tools[0], localPositions[0], localRotations[0]);
			base.photonView.RPC("RpcAddTools", PhotonTargets.All, array, localPositions, localRotations, desiredNewToolIndex);
		}
	}

	public void AuthorityAddTool(GroupableTool tool)
	{
		Vector3 localPosition = base.transform.InverseTransformPoint(tool.transform.position);
		Quaternion localRotation = base.transform.InverseTransformRotation(tool.transform.rotation);
		AuthorityAddTool(tool, localPosition, localRotation);
	}

	public void AuthorityAddTool(GroupableTool tool, Vector3 localPosition, Quaternion localRotation)
	{
		if (base.hasAuthority && ((tool != null) & CanAddTool(tool)))
		{
			int desiredNewToolIndex = GetDesiredNewToolIndex(tool, localPosition, localRotation);
			base.photonView.RPC("RpcAddTool", PhotonTargets.All, tool.photonView.viewID, localPosition, localRotation, desiredNewToolIndex);
		}
	}

	public void AuthorityAddGroup(ToolGroup group)
	{
		if (base.hasAuthority && group != null && group.hasAuthority && CanAddGroup(group))
		{
			GroupableTool[] array = group.tools.ToArray();
			group.AuthorityClearTools();
			AuthorityAddTools(array);
		}
	}

	public void AuthorityRemoveTool(GroupableTool tool)
	{
		if (base.hasAuthority && tool != null)
		{
			base.photonView.RPC("RpcRemoveTool", PhotonTargets.All, tool.photonView.viewID);
		}
	}

	public void AuthorityClearTools()
	{
		if (base.hasAuthority)
		{
			base.photonView.RPC("RpcClearTools", PhotonTargets.All);
		}
	}

	public void AuthorityReorderTools(int[] toolPhotonViewIds)
	{
		if (base.hasAuthority)
		{
			base.photonView.RPC("RpcReorderTools", PhotonTargets.All, toolPhotonViewIds);
		}
	}

	private void AddTool(GroupableTool tool, Vector3 localPosition, Quaternion localRotation, int desiredIndex, bool applyChangesToEntireGroup = true)
	{
		if (tool != this && !tools.Contains(tool))
		{
			OnAddTool(tool);
			if (base.hasAuthority && Size == 0 && tool.IsHeld && tool.Owner.isLocal)
			{
				Player player = tool.Owner;
				PlayerHand playerHand = ((!(player.LeftHand.Tool == tool)) ? player.RightHand : player.LeftHand);
				bool isLocked = tool.IsLocked;
				player.ToolController.ReleaseTool(tool);
				player.ToolController.TryPickupTool(this, tool.LastTargetSpacePickupPosition, tool.LastTargetSpacePickupRotation, (int)playerHand.Type);
				base.IsLocked = isLocked;
			}
			if (desiredIndex >= 0 && desiredIndex < tools.Count)
			{
				tools.Insert(desiredIndex, tool);
			}
			else
			{
				tools.Add(tool);
			}
			tool.Group = this;
			tool.UpdateGroupLayout(localPosition, localRotation, localPosition, localRotation);
			for (int i = 0; i < tool.Colliders.Count; i++)
			{
				tool.Colliders[i].ThisCollider.isTrigger = true;
			}
			if (applyChangesToEntireGroup)
			{
				UpdateGroupLayout();
				UpdateGroupCollider();
			}
			tool.ToolRenderer.Mode = base.ToolRenderer.Mode;
		}
	}

	protected void StartDelayedUpdateOnRemove(GroupableTool removedTool)
	{
		StopDelayedUpdateOnRemove();
		delayedRemoveUpdateCoroutine = StartCoroutine(DelayedUpdateGroupOnRemove(removedTool));
	}

	protected void StopDelayedUpdateOnRemove()
	{
		if (DelayedUpdateCoroutineIsRunning)
		{
			StopCoroutine(delayedRemoveUpdateCoroutine);
			delayedRemoveUpdateCoroutine = null;
		}
	}

	private IEnumerator DelayedUpdateGroupOnRemove(GroupableTool removedTool)
	{
		while (!((removedTool.transform.position - base.transform.position).sqrMagnitude > removeMinDistance * removeMinDistance))
		{
			yield return null;
		}
		UpdateGroupLayout();
		UpdateGroupCollider();
		delayedRemoveUpdateCoroutine = null;
	}

	private void RemoveTool(GroupableTool tool, bool removingSingleTool = true)
	{
		if (!(tool.Group == this) || !tools.Contains(tool))
		{
			return;
		}
		tools.Remove(tool);
		tool.Group = null;
		tool.RigidbodyPickup.Release();
		for (int i = 0; i < tool.Colliders.Count; i++)
		{
			tool.Colliders[i].ThisCollider.isTrigger = tool.Colliders[i].IsTriggerByDefault;
		}
		OnRemoveTool(tool);
		if (removingSingleTool)
		{
			StartCoroutine(DelayedUpdateGroupOnRemove(tool));
			if (base.hasAuthority && Size == 0 && base.IsHeld && base.Owner.isLocal)
			{
				base.Owner.ToolController.ReleaseTool(this);
			}
		}
	}

	private void ClearTools()
	{
		while (tools.Count > 0)
		{
			RemoveTool(tools[0], false);
		}
	}

	private void ReorderTools(int[] photonViewIds)
	{
		for (int i = 0; i < photonViewIds.Length; i++)
		{
			PhotonView photonView = PhotonView.Find(photonViewIds[i]);
			if (photonView != null)
			{
				GroupableTool component = photonView.GetComponent<GroupableTool>();
				if (component != null)
				{
					tools[i] = component;
				}
			}
		}
		UpdateGroupLayout();
		UpdateGroupCollider();
	}

	protected virtual void OnAddTool(GroupableTool tool)
	{
	}

	protected virtual void OnRemoveTool(GroupableTool tool)
	{
	}

	protected virtual int GetDesiredNewToolIndex(GroupableTool newTool, Vector3 localPosition, Quaternion localRotation)
	{
		return -1;
	}

	public virtual bool CanAddTool(GroupableTool tool)
	{
		return true;
	}

	public virtual bool CanAddGroup(ToolGroup group)
	{
		return true;
	}

	public void AuthorityClone(ToolGroup oldGroup)
	{
		if (base.hasAuthority)
		{
			if (!base.IsHeld && oldGroup.IsHeld && oldGroup.Owner.isLocal)
			{
				Player player = oldGroup.Owner;
				PlayerHand playerHand = ((!(player.LeftHand.Tool == oldGroup)) ? player.RightHand : player.LeftHand);
				player.ToolController.ReleaseTool(oldGroup);
				player.ToolController.TryPickupTool(this, oldGroup.LastTargetSpacePickupPosition, oldGroup.LastTargetSpacePickupRotation, (int)playerHand.Type);
			}
			base.transform.position = oldGroup.transform.position;
			base.transform.rotation = oldGroup.transform.rotation;
			AuthorityAddGroup(oldGroup);
			OnCloned(oldGroup);
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		UpdateHover();
	}

	protected void UpdateHover()
	{
		if (!base.hasAuthority)
		{
			hoveredGroup = null;
		}
		else
		{
			hoveredGroup = SearchCollider.FindGroup(null, this);
		}
	}

	protected void TryAddToHoveredGroup()
	{
		if (base.hasAuthority && hoveredGroup != null)
		{
			if (!hoveredGroup.hasAuthority)
			{
				hoveredGroup.photonView.TransferOwnership(PhotonNetwork.player);
			}
			if (hoveredGroup.hoveredGroup == this)
			{
				hoveredGroup.hoveredGroup = null;
			}
			hoveredGroup.AuthorityAddGroup(this);
			hoveredGroup = null;
			AuthorityDestroy();
		}
	}

	protected virtual void OnCloned(ToolGroup oldGroup)
	{
	}

	public virtual void DisplayAddToolHighlight(GroupableTool toolToAdd)
	{
		DefaultAddHighlightEnabled = toolToAdd != null;
	}

	public virtual void DisplayAddGroupHighlight(ToolGroup groupToAdd)
	{
		DefaultAddHighlightEnabled = groupToAdd != null;
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		base.Release(player, linearVelocity, angularVelocity);
		TryAddToHoveredGroup();
	}

	public virtual GroupableTool GetHighlightedToolInGroup(GroupableTool tool, Player player, bool physicalPickup, Vector3 pickupPosition)
	{
		return null;
	}

	public override Tool GetHighlightedTool(Player player, ToolCollider toolCollider, bool physicalPickup, Vector3 pickupPosition)
	{
		if (!player.CanInteractWithTools)
		{
			return null;
		}
		if (toolCollider.Tool == this)
		{
			return null;
		}
		if (base.IsHeld && (base.Owner != player || !physicalPickup))
		{
			return null;
		}
		if (base.OnlyOwnerCanPickup && base.Owner != player)
		{
			return null;
		}
		if (!physicalPickup && !SupportsAssistedCatching)
		{
			return null;
		}
		if (!GroupToolRenderersAreVisible)
		{
			return null;
		}
		if (base.IsLocked)
		{
			return null;
		}
		if (base.PlayerInteractionRestriction != null && !base.PlayerInteractionRestriction.PlayerInteractionAllowed(player.PhotonPlayer))
		{
			return null;
		}
		return ((!physicalPickup || !base.SupportsPhysicalPickup) && (physicalPickup || (!base.SupportsGravityPickup && !base.SupportsOffCenterGravityPickup))) ? null : this;
	}

	public virtual void OnToolInGroupHoverStart(GroupableTool tool, bool physicalHover)
	{
	}

	public virtual void OnToolInGroupHoverEnd(GroupableTool tool)
	{
	}

	protected override void OnIsLockedChanged()
	{
		base.OnIsLockedChanged();
		ApplyToolRendererModeToGroup();
	}

	public override void OnHoverStart(bool physicalHover)
	{
		base.OnHoverStart(physicalHover);
		ApplyToolRendererModeToGroup();
	}

	public override void OnHoverEnd()
	{
		base.OnHoverEnd();
		ApplyToolRendererModeToGroup();
	}

	private void ApplyToolRendererModeToGroup()
	{
		if (Size > 0)
		{
			for (int i = 0; i < tools.Count; i++)
			{
				tools[i].ToolRenderer.Mode = base.ToolRenderer.Mode;
			}
		}
	}

	protected virtual void UpdateGroupLayout()
	{
	}

	protected void UpdateGroupCollider()
	{
		if (!(groupBoxCollider != null))
		{
			return;
		}
		groupBounds.center = Vector3.zero;
		groupBounds.size = Vector3.zero;
		for (int i = 0; i < tools.Count; i++)
		{
			Vector3 vector = tools[i].LocalBoundingBox.center.TransformToWorldSpace(tools[i].ColliderLocalPosition, tools[i].ColliderLocalRotation);
			for (int j = 0; j < CornerMasks.Length; j++)
			{
				Vector3 vector2 = Vector3.Scale(tools[i].LocalBoundingBox.extents, CornerMasks[j]).TransformToWorldSpace(Vector3.zero, tools[i].ColliderLocalRotation);
				groupBounds.Encapsulate(vector + vector2);
			}
		}
		groupBoxCollider.center = groupBounds.center;
		groupBoxCollider.size = groupBounds.size;
		SearchCollider.Center = groupBoxCollider.center;
		SearchCollider.Size = groupBoxCollider.size;
	}

	[PunRPC]
	protected void RpcAddTool(int toolPhotonViewId, Vector3 localPosition, Quaternion localRotation, int desiredIndex)
	{
		PhotonView photonView = PhotonView.Find(toolPhotonViewId);
		if (photonView != null)
		{
			GroupableTool component = photonView.GetComponent<GroupableTool>();
			if (component != null && component != this)
			{
				AddTool(component, localPosition, localRotation, desiredIndex);
			}
		}
	}

	[PunRPC]
	protected void RpcAddTools(int[] toolPhotonViewIds, Vector3[] localPositions, Quaternion[] localRotations, int desiredStartingIndex)
	{
		for (int i = 0; i < toolPhotonViewIds.Length; i++)
		{
			PhotonView photonView = PhotonView.Find(toolPhotonViewIds[i]);
			if (photonView != null)
			{
				GroupableTool component = photonView.GetComponent<GroupableTool>();
				if (component != null && component != this)
				{
					AddTool(component, localPositions[i], localRotations[i], desiredStartingIndex + i, false);
				}
			}
		}
		UpdateGroupLayout();
		UpdateGroupCollider();
	}

	[PunRPC]
	protected void RpcRemoveTool(int toolPhotonViewId)
	{
		PhotonView photonView = PhotonView.Find(toolPhotonViewId);
		if (photonView != null)
		{
			GroupableTool component = photonView.GetComponent<GroupableTool>();
			if (component != null && component != this)
			{
				RemoveTool(component);
			}
		}
	}

	[PunRPC]
	protected void RpcClearTools()
	{
		ClearTools();
	}

	[PunRPC]
	protected void RpcInitializeGroup(int[] toolPhotonViewIds, Vector3[] toolLocalPositions, Quaternion[] toolLocalRotations)
	{
		RpcAddTools(toolPhotonViewIds, toolLocalPositions, toolLocalRotations, -1);
	}

	[PunRPC]
	protected void RpcReorderTools(int[] toolPhotonViewIds)
	{
		ReorderTools(toolPhotonViewIds);
	}

	protected virtual void OnPhotonPlayerConnected(PhotonPlayer otherPlayer)
	{
		if (base.hasAuthority)
		{
			int[] array = new int[tools.Count];
			Vector3[] array2 = new Vector3[tools.Count];
			Quaternion[] array3 = new Quaternion[tools.Count];
			for (int i = 0; i < tools.Count; i++)
			{
				array[i] = tools[i].photonView.viewID;
				array2[i] = tools[i].GroupLocalPosition;
				array3[i] = tools[i].GroupLocalRotation;
			}
			base.photonView.RPC("RpcInitializeGroup", otherPlayer, array, array2, array3);
		}
	}

	protected void TransferOwnership(PhotonPlayer newOwner)
	{
		foreach (GroupableTool tool in tools)
		{
			tool.photonView.TransferOwnership(newOwner);
		}
		base.photonView.TransferOwnership(newOwner);
	}
}
