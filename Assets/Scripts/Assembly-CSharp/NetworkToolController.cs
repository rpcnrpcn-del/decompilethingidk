using Photon;
using UnityEngine;

[DisallowMultipleComponent]
public class NetworkToolController : Photon.MonoBehaviour
{
	private class ToolPickupProperty
	{
		private const string TOOL_PICKUP_PROPERTY = "Tool_Pickup.";

		public Tool tool { get; private set; }

		public Vector3 pickupPosition { get; private set; }

		public Quaternion pickupRotation { get; private set; }

		public int handType { get; private set; }

		public int serverTime { get; private set; }

		public ToolPickupProperty(object[] values)
		{
			tool = Tool.Find((int)values[0]);
			pickupPosition = (Vector3)values[1];
			pickupRotation = (Quaternion)values[2];
			handType = (int)values[3];
			serverTime = (int)values[4];
		}

		public ToolPickupProperty(Tool tool, Vector3 pickupPosition, Quaternion pickupRotation, int handType)
		{
			this.tool = tool;
			this.pickupPosition = pickupPosition;
			this.pickupRotation = pickupRotation;
			this.handType = handType;
			serverTime = PhotonNetwork.ServerTimestamp;
		}

		public object[] GetPhotonProperty()
		{
			return new object[5]
			{
				(!(tool.photonView != null)) ? (-1) : tool.photonView.viewID,
				pickupPosition,
				pickupRotation,
				handType,
				serverTime
			};
		}

		public static ToolPickupProperty GetToolPickup(Player player, PlayerHand.HandType handType)
		{
			string key = "Tool_Pickup." + (int)handType;
			if (player.owner.customProperties.ContainsKey(key))
			{
				return new ToolPickupProperty((object[])player.owner.customProperties[key]);
			}
			return null;
		}

		public static void RemoveProperty(Player player, PlayerHand.HandType handType)
		{
			string text = "Tool_Pickup." + (int)handType;
			if (player.owner.customProperties.ContainsKey(text))
			{
				player.owner.RemoveCustomProperties(text);
			}
		}

		public void SetPropertyForPlayer(Player player)
		{
			string propertyName = "Tool_Pickup." + handType;
			player.photonView.owner.SetCustomProperties(propertyName, GetPhotonProperty());
		}
	}

	private Player thisPlayer;

	protected override void Awake()
	{
		base.Awake();
		thisPlayer = GetComponent<Player>();
		Tool.HolderChanged += OnToolHolderChanged;
		Tool[] array = Object.FindObjectsOfType<Tool>();
		foreach (Tool tool in array)
		{
			OnToolHolderChanged(tool);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Tool.HolderChanged -= OnToolHolderChanged;
	}

	private void Start()
	{
		base.enabled = base.isLocal;
	}

	public void TryPickupToolWithDominantHand(Tool tool)
	{
		int type = (int)thisPlayer.DominantHand.Type;
		TryPickupTool(tool, type);
	}

	public void TryPickupTool(Tool tool, int handTypeId)
	{
		PlayerHand hand = thisPlayer.GetHand(handTypeId);
		Vector3 handSpacePickupPosition;
		Quaternion handSpacePickupRotation;
		hand.GetToolPickupPositionAndRotation(tool, true, Vector3.zero, out handSpacePickupPosition, out handSpacePickupRotation);
		TryPickupTool(tool, handSpacePickupPosition, handSpacePickupRotation, handTypeId);
	}

	public void TryPickupTool(Tool tool, Vector3 handSpacePickupPosition, Quaternion handSpacePickupRotation, int handTypeId)
	{
		int holderId = tool.HolderId;
		if (holderId != PhotonPlayer.Invalid && (holderId != base.photonView.ownerId || !thisPlayer.CanInteractWithTools))
		{
			return;
		}
		OutfitTool outfitTool = tool as OutfitTool;
		if (!(outfitTool != null) || outfitTool.TryPickupOutfitTool(thisPlayer))
		{
			PlayerHand hand = thisPlayer.GetHand(handTypeId);
			if (hand != null && hand.IsHoldingTool)
			{
				ReleaseTool(hand.Tool);
			}
			ToolPickupProperty toolPickupProperty = new ToolPickupProperty(tool, handSpacePickupPosition, handSpacePickupRotation, handTypeId);
			toolPickupProperty.SetPropertyForPlayer(thisPlayer);
			tool.HolderId = base.photonView.ownerId;
			PickupToolSuccess(tool);
		}
	}

	public void UpdateHeldToolPickupTransform(Tool tool, Vector3 pickupPosition, Quaternion pickupRotation)
	{
		if (thisPlayer.isLocal)
		{
			PlayerHand playerHand = null;
			if (thisPlayer.LeftHand.Tool == tool)
			{
				playerHand = thisPlayer.LeftHand;
			}
			else if (thisPlayer.RightHand.Tool == tool)
			{
				playerHand = thisPlayer.RightHand;
			}
			if (playerHand != null)
			{
				int type = (int)playerHand.Type;
				ToolPickupProperty toolPickupProperty = new ToolPickupProperty(tool, pickupPosition, pickupRotation, type);
				toolPickupProperty.SetPropertyForPlayer(thisPlayer);
				base.photonView.RPC("RpcUpdateHeldToolPickupTransform", PhotonTargets.All, tool.photonView.viewID, pickupPosition, pickupRotation);
			}
		}
	}

	private void OnToolHolderChanged(Tool tool)
	{
		if (!thisPlayer.isLocal)
		{
			return;
		}
		Player player = Player.Find(tool.HolderId);
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		foreach (PhotonPlayer photonPlayer in playerList)
		{
			Player player2 = photonPlayer.ToPlayer();
			if (player2 != null && player2 != player && (player2.LeftHand.Tool == tool || player2.RightHand.Tool == tool))
			{
				player2.ToolController.ReleaseTool(tool);
			}
		}
		if (player != null && player.CanInteractWithTools)
		{
			player.ToolController.PickupToolSuccess(tool);
		}
	}

	private void PickupToolSuccess(Tool tool)
	{
		ToolPickupProperty toolPickup = ToolPickupProperty.GetToolPickup(thisPlayer, PlayerHand.HandType.Left);
		ToolPickupProperty toolPickup2 = ToolPickupProperty.GetToolPickup(thisPlayer, PlayerHand.HandType.Right);
		ToolPickupProperty toolPickupProperty = null;
		bool flag = toolPickup != null && toolPickup.tool == tool;
		bool flag2 = toolPickup2 != null && toolPickup2.tool == tool;
		if (flag && flag2)
		{
			toolPickupProperty = ((toolPickup.serverTime >= toolPickup2.serverTime) ? toolPickup : toolPickup2);
		}
		else if (flag)
		{
			toolPickupProperty = toolPickup;
		}
		else if (flag2)
		{
			toolPickupProperty = toolPickup2;
		}
		if (toolPickupProperty != null)
		{
			tool.photonView.TransferOwnership(thisPlayer.owner);
			PickupToolSuccessHandPickup(tool, toolPickupProperty.pickupPosition, toolPickupProperty.pickupRotation, thisPlayer.GetHand(toolPickupProperty.handType));
		}
	}

	private void PickupToolSuccessHandPickup(Tool tool, Vector3 pickupPosition, Quaternion pickupRotation, PlayerHand hand)
	{
		PlayerHand otherHand = thisPlayer.GetOtherHand(hand.Type);
		if (otherHand != null && otherHand != hand && otherHand.Tool == tool)
		{
			ReleaseTool(tool, otherHand, true);
		}
		if (hand.Tool != tool)
		{
			if (hand.Tool != null)
			{
				ReleaseTool(hand.Tool, hand);
			}
			hand.PickupTool(tool, pickupPosition, pickupRotation);
		}
	}

	public void ReleaseTool(Tool tool)
	{
		if (!(tool == null))
		{
			PlayerHand playerHand = null;
			if (thisPlayer.LeftHand.Tool == tool)
			{
				playerHand = thisPlayer.LeftHand;
			}
			else if (thisPlayer.RightHand.Tool == tool)
			{
				playerHand = thisPlayer.RightHand;
			}
			if (playerHand != null)
			{
				ReleaseTool(tool, playerHand);
			}
		}
	}

	private void ReleaseTool(Tool tool, PlayerHand hand, bool ignoreHolderProperty = false)
	{
		if (!(hand.Tool == tool))
		{
			return;
		}
		hand.ReleaseTool();
		if (base.hasAuthority)
		{
			if (!ignoreHolderProperty && tool.hasAuthority)
			{
				tool.HolderId = PhotonPlayer.Invalid;
			}
			ToolPickupProperty.RemoveProperty(thisPlayer, hand.Type);
		}
	}

	[PunRPC]
	private void RpcUpdateHeldToolPickupTransform(int toolPhotonViewId, Vector3 pickupPosition, Quaternion pickupRotation)
	{
		PhotonView photonView = PhotonView.Find(toolPhotonViewId);
		Tool tool = ((!(photonView != null)) ? null : photonView.GetComponent<Tool>());
		if (tool != null && tool.IsHeld && tool.Owner == thisPlayer)
		{
			tool.UpdatePickupTransform(pickupPosition, pickupRotation);
		}
	}
}
