using System;
using System.Collections.Generic;
using Photon;
using RecNet;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Player))]
public class PlayerParty : Photon.MonoBehaviour
{
	public static readonly string PartyIdProperty = "PlayerPartyID";

	public static readonly string PartyColorIdProperty = "PlayerPartyColorID";

	private float lastTimePartyListUpdated;

	[Tooltip("Update the party membership list periodically.")]
	[SerializeField]
	private float partyListUpdateInterval = 1f;

	[Tooltip("Party indicator band.")]
	public OutfitSelection PartyBandPrefab;

	private OutfitItem partyBandLeft;

	private OutfitItem partyBandRight;

	private List<Player> groupPlayers;

	private Player thisPlayer;

	private bool partyColorDirty = true;

	private Color? _partyColor;

	public List<Player> PartyPlayers
	{
		get
		{
			return groupPlayers;
		}
	}

	public bool IsInParty
	{
		get
		{
			return !string.IsNullOrEmpty(CurrentPartyId);
		}
	}

	public int PartySize
	{
		get
		{
			return IsInParty ? (groupPlayers.Count + 1) : 0;
		}
	}

	public string CurrentPartyId
	{
		get
		{
			return thisPlayer.PlayerData.GetData<string>(PartyIdProperty, null);
		}
		private set
		{
			if (SingletonMonoBehaviour<SessionManager>.Instance != null)
			{
				SingletonMonoBehaviour<SessionManager>.Instance.PlayerPartyId = value;
			}
			if (value != null)
			{
				AnalyticsHelper.PartyJoined(value, PartySize);
			}
			thisPlayer.PlayerData.SetData(PartyIdProperty, value);
			if (value == null)
			{
				CurrentPartyColorId = null;
			}
			else
			{
				UpdatePartyLists(true);
				int partySize = PartySize;
				if (partySize < 2)
				{
					AnalyticsHelper.PartyCreatedSelf(value);
				}
				else
				{
					AnalyticsHelper.PartyJoinedSelf(PartySize, value);
				}
				string text = null;
				foreach (Player groupPlayer in groupPlayers)
				{
					text = groupPlayer.PlayerParty.CurrentPartyColorId;
					if (!string.IsNullOrEmpty(text))
					{
						break;
					}
				}
				if (string.IsNullOrEmpty(text))
				{
					text = ((string.IsNullOrEmpty(SingletonMonoBehaviour<SessionManager>.Instance.PlayerPartyColorId) || !IsGroupColorIdAvailable(SingletonMonoBehaviour<SessionManager>.Instance.PlayerPartyColorId)) ? GetNextAvailableGroupColorId() : SingletonMonoBehaviour<SessionManager>.Instance.PlayerPartyColorId);
				}
				CurrentPartyColorId = text;
			}
			partyColorDirty = true;
		}
	}

	public string CurrentPartyColorId
	{
		get
		{
			return thisPlayer.PlayerData.GetData<string>(PartyColorIdProperty, null);
		}
		private set
		{
			thisPlayer.PlayerData.SetData(PartyColorIdProperty, value);
			partyColorDirty = true;
			if (SingletonMonoBehaviour<SessionManager>.Instance != null)
			{
				SingletonMonoBehaviour<SessionManager>.Instance.PlayerPartyColorId = value;
			}
		}
	}

	public Color? PartyColor
	{
		get
		{
			if (partyColorDirty)
			{
				_partyColor = null;
				string currentPartyColorId = CurrentPartyColorId;
				if (!string.IsNullOrEmpty(currentPartyColorId) && PUNNetworkManager.Instance != null)
				{
					ColorVault.ColorToGuid colorToGuid = SingletonMonoBehaviour<SessionManager>.Instance.GroupColorVault.Find(currentPartyColorId);
					if (colorToGuid != null)
					{
						_partyColor = colorToGuid.color;
						partyColorDirty = false;
					}
				}
			}
			return _partyColor;
		}
	}

	public event Action PartyUpdateReceived;

	protected override void Awake()
	{
		base.Awake();
		thisPlayer = GetComponent<Player>();
		groupPlayers = new List<Player>();
	}

	private void Start()
	{
		if (base.isLocal && SingletonMonoBehaviour<SessionManager>.Instance != null)
		{
			CurrentPartyId = SingletonMonoBehaviour<SessionManager>.Instance.PlayerPartyId;
			if (IsInParty)
			{
				Invoke("DeferredUpdatePartyColor", 1f);
				Invoke("DeferredUpdatePartyColor", 5f);
			}
			thisPlayer.LeftHand.Gestures.GestureDetected += PlayerHandGestures_GestureDetected;
			thisPlayer.RightHand.Gestures.GestureDetected += PlayerHandGestures_GestureDetected;
		}
		partyBandRight = thisPlayer.PlayerOutfit.InstantiateNewOutfitPrefab(PartyBandPrefab, Player.BodyPart.RightHand, false);
		partyBandLeft = thisPlayer.PlayerOutfit.InstantiateNewOutfitPrefab(PartyBandPrefab, Player.BodyPart.LeftHand, false);
	}

	private void Update()
	{
		UpdateBraceletColor(PlayerHand.HandType.Left);
		UpdateBraceletColor(PlayerHand.HandType.Right);
		if (base.isLocal && Time.time - lastTimePartyListUpdated > partyListUpdateInterval)
		{
			lastTimePartyListUpdated = Time.time;
			UpdatePartyLists();
		}
	}

	private void UpdateBraceletColor(PlayerHand.HandType handType)
	{
		if (handType != PlayerHand.HandType.Unknown && !(thisPlayer == null))
		{
			Color? partyColor = PartyColor;
			PlayerHand playerHand = ((handType != PlayerHand.HandType.Left) ? thisPlayer.RightHand : thisPlayer.LeftHand);
			OutfitItem outfitItem = ((handType != PlayerHand.HandType.Left) ? partyBandRight : partyBandLeft);
			bool flag = partyColor.HasValue && playerHand != null && playerHand.IsVisible;
			outfitItem.gameObject.SetActive(flag);
			if (flag && outfitItem.SkinnedMeshRenderer.material.color != partyColor.Value)
			{
				outfitItem.SkinnedMeshRenderer.material.color = partyColor.Value;
				outfitItem.GetComponent<AnimateInOut>().PlayEffect(true);
			}
			else if (!IsInParty)
			{
				outfitItem.SkinnedMeshRenderer.material.color = Color.clear;
			}
		}
	}

	public virtual void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
	{
		partyColorDirty = true;
	}

	public virtual void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
	{
		UpdatePartyLists(true);
	}

	private void PlayerHandGestures_GestureDetected(PlayerHand myHand, PlayerHand otherHand, PlayerHandGestures.Gesture gesture)
	{
		if (!base.isLocal)
		{
			return;
		}
		switch (gesture)
		{
		case PlayerHandGestures.Gesture.HandshakeSuccess:
		{
			bool flag2 = false;
			bool flag3 = false;
			Relationship relationship = Relationships.RelationshipList.Find((Relationship f) => f.PlayerID == otherHand.ThisPlayer.PlayerId);
			if (relationship != null)
			{
				switch (relationship.Type)
				{
				case Relationship.RelationshipType.Friend:
					flag2 = true;
					break;
				case Relationship.RelationshipType.BlockedLocal:
				case Relationship.RelationshipType.BlockedRemote:
				case Relationship.RelationshipType.BlockedMutual:
					flag3 = true;
					break;
				}
			}
			Vector3 value = Vector3.Lerp(myHand.transform.position, otherHand.transform.position, 0.5f);
			if (flag3)
			{
				MenuNotification.PlayNext("Played Blocked!", 2f, null, false, value, 0.3f);
				break;
			}
			if (flag2)
			{
				MenuNotification.PlayNext("Friends", 2f, null, false, value, 0.3f);
				break;
			}
			Relationships.AddFriend(otherHand.ThisPlayer.PlayerId);
			MenuNotification.PlayNext("Friend Added", 2f, null, false, value, 0.3f);
			break;
		}
		case PlayerHandGestures.Gesture.Fistbump:
		{
			bool flag = false;
			if (otherHand.ThisPlayer.PlayerParty.IsInParty && !IsInParty)
			{
				CurrentPartyId = otherHand.ThisPlayer.PlayerParty.CurrentPartyId;
				UpdatePartyLists();
				flag = true;
			}
			else if (!otherHand.ThisPlayer.PlayerParty.IsInParty && IsInParty)
			{
				flag = true;
			}
			else if (otherHand.ThisPlayer.PlayerParty.IsInParty && IsInParty && otherHand.ThisPlayer.PlayerParty.CurrentPartyId != CurrentPartyId)
			{
				flag = true;
				if (base.photonView.owner.ID > otherHand.ThisPlayer.photonView.owner.ID)
				{
					UpdatePartyLists();
					string currentPartyId = otherHand.ThisPlayer.PlayerParty.CurrentPartyId;
					foreach (Player groupPlayer in groupPlayers)
					{
						groupPlayer.photonView.RPC("RpcAutoJoinParty", groupPlayer.photonView.owner, currentPartyId);
					}
					CurrentPartyId = currentPartyId;
				}
			}
			else if (!otherHand.ThisPlayer.PlayerParty.IsInParty && !IsInParty)
			{
				flag = true;
				if (base.photonView.owner.ID > otherHand.ThisPlayer.photonView.owner.ID)
				{
					CurrentPartyId = Guid.NewGuid().ToString();
					otherHand.ThisPlayer.photonView.RPC("RpcAutoJoinParty", otherHand.ThisPlayer.photonView.owner, CurrentPartyId);
				}
			}
			if (flag)
			{
				MenuNotification.PlayNext("Party Join", 2f, null, false, (myHand.transform.position + otherHand.transform.position) / 2f, 0.3f);
			}
			break;
		}
		}
	}

	[PunRPC]
	public void RpcAutoJoinParty(string partyId)
	{
		CurrentPartyId = partyId;
		UpdatePartyLists();
	}

	private void DeferredUpdatePartyColor()
	{
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		foreach (PhotonPlayer photonPlayer in playerList)
		{
			Player player = photonPlayer.TagObject as Player;
			if (player != null && player.PlayerParty.IsInParty && player.PlayerParty.CurrentPartyId == CurrentPartyId)
			{
				string currentPartyColorId = player.PlayerParty.CurrentPartyColorId;
				if (!string.IsNullOrEmpty(currentPartyColorId))
				{
					CurrentPartyColorId = currentPartyColorId;
					break;
				}
			}
		}
	}

	public bool CanInvitePlayer(Player player)
	{
		return player != null && !groupPlayers.Contains(player);
	}

	private string GetNextAvailableGroupColorId()
	{
		List<string> list = new List<string>();
		PhotonPlayer[] otherPlayers = PhotonNetwork.otherPlayers;
		foreach (PhotonPlayer player in otherPlayers)
		{
			string customPropertyString = player.GetCustomPropertyString(PartyColorIdProperty);
			if (!string.IsNullOrEmpty(customPropertyString) && !list.Contains(customPropertyString))
			{
				list.Add(customPropertyString);
			}
		}
		SingletonMonoBehaviour<SessionManager>.Instance.GroupColorVault.Colors.Shuffle();
		foreach (ColorVault.ColorToGuid color in SingletonMonoBehaviour<SessionManager>.Instance.GroupColorVault.Colors)
		{
			if (!list.Contains(color.guidString))
			{
				return color.guidString;
			}
		}
		Debug.LogError("Could not find an unused party color, this is madness. Should never happen in a max 8 player game, add colors to PUNNetworkManager ColorVault.");
		return null;
	}

	private bool IsGroupColorIdAvailable(string playerPartyColorId)
	{
		PhotonPlayer[] otherPlayers = PhotonNetwork.otherPlayers;
		foreach (PhotonPlayer player in otherPlayers)
		{
			string customPropertyString = player.GetCustomPropertyString(PartyColorIdProperty);
			string customPropertyString2 = player.GetCustomPropertyString(PartyIdProperty);
			if (!string.IsNullOrEmpty(customPropertyString) && playerPartyColorId == customPropertyString && !string.IsNullOrEmpty(customPropertyString2) && customPropertyString2 != CurrentPartyId)
			{
				return false;
			}
		}
		return true;
	}

	public void KickPlayer(Player target)
	{
		if (IsInPartyWith(target))
		{
			target.photonView.RPC("RpcKickedFromParty", target.photonView.owner, CurrentPartyId);
		}
	}

	public string[] GetExpectedPhotonUsers()
	{
		UpdatePartyLists();
		if (groupPlayers.Count > 0)
		{
			string[] array = new string[groupPlayers.Count];
			for (int i = 0; i < groupPlayers.Count; i++)
			{
				array[i] = groupPlayers[i].photonView.owner.userId;
			}
			return array;
		}
		return null;
	}

	[PunRPC]
	public void RpcKickedFromParty(string partyId)
	{
		if (!(partyId != CurrentPartyId))
		{
			LeaveCurrentParty();
		}
	}

	private bool IsValidPartyPlayer(Player player)
	{
		return player != null && player.photonView.owner != base.photonView.owner && IsInParty && player.PlayerParty.CurrentPartyId == CurrentPartyId;
	}

	private void UpdatePartyLists(bool forcePartyChange = false)
	{
		bool flag = false || forcePartyChange;
		if (IsInParty)
		{
			Player[] array = groupPlayers.ToArray();
			foreach (Player player in array)
			{
				if (!IsValidPartyPlayer(player))
				{
					GroupPlayersRemove(player);
					flag = true;
				}
			}
			PhotonPlayer[] otherPlayers = PhotonNetwork.otherPlayers;
			foreach (PhotonPlayer photonPlayer in otherPlayers)
			{
				Player player2 = photonPlayer.TagObject as Player;
				if (player2 != null && IsValidPartyPlayer(player2) && !groupPlayers.Contains(player2))
				{
					GroupPlayersAdd(player2);
					flag = true;
				}
			}
		}
		else if (groupPlayers.Count > 0)
		{
			groupPlayers.Clear();
			flag = true;
		}
		if (flag && this.PartyUpdateReceived != null)
		{
			this.PartyUpdateReceived();
		}
	}

	private void GroupPlayersAdd(Player other)
	{
		groupPlayers.Add(other);
		if (base.isLocal)
		{
			AnalyticsHelper.PartyJoinedOther(other, PartySize);
		}
	}

	private void GroupPlayersRemove(Player other)
	{
		groupPlayers.Remove(other);
		if (base.isLocal)
		{
			AnalyticsHelper.PartyLeftOther(other, PartySize);
		}
	}

	public void LeaveCurrentParty()
	{
		if (groupPlayers.Count > 0)
		{
			AnalyticsHelper.PartyLeftSelf(CurrentPartyId);
		}
		AnalyticsHelper.PartyLeft(CurrentPartyId, groupPlayers.Count);
		groupPlayers.Clear();
		CurrentPartyId = null;
	}

	public bool IsInPartyWith(Player player)
	{
		return player != null && IsInParty && groupPlayers.Contains(player);
	}

	[PunRPC]
	public void RpcDebugLeaveRoom()
	{
		if (RecRoomSceneManager.Instance != null)
		{
			RecRoomSceneManager.Instance.SwitchActivity("lockerroom", false);
		}
	}
}
