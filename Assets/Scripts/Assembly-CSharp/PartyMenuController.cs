using System.Collections.Generic;
using System.Linq;
using System.Text;
using RecNet;
using UnityEngine;
using UnityEngine.UI;

public class PartyMenuController : MenuController
{
	[SerializeField]
	private UIPlayerPanel playerPanelPrefab;

	[SerializeField]
	private RectTransform localPlayerList;

	[SerializeField]
	private RectTransform friendList;

	[SerializeField]
	private RectTransform noLocalPlayersPanel;

	[SerializeField]
	private RectTransform noFriendsPanel;

	private RemotePlayerMenuController playerSpecificMenu;

	private Dictionary<ulong, UIPlayerPanel> localPlayerPanels;

	private Dictionary<ulong, UIPlayerPanel> remoteFriendPanels;

	private List<ulong> friendIds;

	private List<ulong> localPlayerIds;

	private bool friendListResortRequested;

	public override void Initialize(PlayerMenu playerMenu)
	{
		base.Initialize(playerMenu);
		playerPanelPrefab.gameObject.SetActive(false);
		if (base.Owner != null)
		{
			remoteFriendPanels = new Dictionary<ulong, UIPlayerPanel>();
			localPlayerPanels = new Dictionary<ulong, UIPlayerPanel>();
			friendIds = new List<ulong>();
			localPlayerIds = new List<ulong>();
		}
		if (subMenu != null)
		{
			playerSpecificMenu = subMenu as RemotePlayerMenuController;
			playerSpecificMenu.PartyMenuController = this;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.Owner != null)
		{
			base.Owner.PlayerParty.PartyUpdateReceived += Refresh;
			Relationships.RelationshipListUpdated += OnFriendListUpdated;
			Profiles.OnProfileUpdated += OnProfileUpdated;
			Images.OnProfileImageUpdated += OnProfileImageUpdated;
			PlayerPresenceManager.OnPlayerPresenceUpdated += OnPlayerPresenceUpdated;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (base.Owner != null)
		{
			base.Owner.PlayerParty.PartyUpdateReceived -= Refresh;
			Relationships.RelationshipListUpdated -= OnFriendListUpdated;
			Profiles.OnProfileUpdated -= OnProfileUpdated;
			Images.OnProfileImageUpdated -= OnProfileImageUpdated;
			PlayerPresenceManager.OnPlayerPresenceUpdated -= OnPlayerPresenceUpdated;
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		if (base.Owner != null)
		{
			RefreshLocalSessionPlayers();
			RefreshFriendsList();
		}
	}

	private void RefreshLocalSessionPlayers()
	{
		localPlayerIds = (from x in PhotonNetwork.otherPlayers
			where x.ToPlayer() != null
			select x.ToPlayer().PlayerId).ToList();
		foreach (ulong item in localPlayerPanels.Keys.ToList())
		{
			if (!localPlayerIds.Contains(item))
			{
				Object.Destroy(localPlayerPanels[item].gameObject);
				localPlayerPanels.Remove(item);
			}
		}
		Profiles.RefreshCachedProfiles(localPlayerIds);
		PlayerPresenceManager.RefreshCachedPlayerPresences(friendIds);
		foreach (ulong localPlayerId in localPlayerIds)
		{
			Images.RefreshCachedProfileImage(localPlayerId);
			Profile profileFromCache = Profiles.GetProfileFromCache(localPlayerId);
			if (profileFromCache != null)
			{
				RefreshLocalPlayerPanel(profileFromCache);
			}
		}
		noLocalPlayersPanel.gameObject.SetActive(localPlayerPanels.Count == 0);
	}

	private string GetLocalPlayerStatus(Player player)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(player.LocalPlayerStatus);
		if (player.PermaGhost)
		{
			stringBuilder.Append(" [GHOSTED]");
		}
		else if (player.Mute)
		{
			stringBuilder.Append(" [MUTED]");
		}
		return stringBuilder.ToString();
	}

	public void ShowPlayerMenu(Player player)
	{
		playerSpecificMenu.InitForLocalPlayer(player);
		UIPlayerPanel value;
		if (localPlayerPanels.TryGetValue(player.PlayerId, out value))
		{
			RefreshPlayerSpecificMenu(value);
			ShowSubMenu(true);
		}
	}

	public void ShowFriendMenu(ulong friendId)
	{
		playerSpecificMenu.InitForFriend(friendId);
		UIPlayerPanel panel = remoteFriendPanels[friendId];
		RefreshPlayerSpecificMenu(panel);
		ShowSubMenu(true);
	}

	private void RefreshPlayerSpecificMenu(UIPlayerPanel panel)
	{
		playerSpecificMenu.GuestTag.gameObject.SetActive(IsPlayerGuest(panel.PlayerId));
		playerSpecificMenu.PlayerName = panel.PlayerName;
		playerSpecificMenu.PlayerStatus = panel.PlayerStatus.Replace("[Guest]", string.Empty).Trim();
		playerSpecificMenu.PlayerAvatar = panel.PlayerAvatar;
		playerSpecificMenu.PlayerLevel = panel.PlayerLevel;
		playerSpecificMenu.PartyColor = panel.PartyColor;
		playerSpecificMenu.Refresh();
	}

	private void OnFriendListUpdated()
	{
		RefreshFriendsList();
	}

	private void RefreshFriendsList()
	{
		friendIds = (from r in Relationships.RelationshipList
			where r.Type == Relationship.RelationshipType.Friend
			select r.PlayerID).ToList();
		foreach (ulong item in remoteFriendPanels.Keys.ToList())
		{
			if (!friendIds.Contains(item))
			{
				Object.Destroy(remoteFriendPanels[item].gameObject);
				remoteFriendPanels.Remove(item);
			}
		}
		Profiles.RefreshCachedProfiles(friendIds);
		PlayerPresenceManager.RefreshCachedPlayerPresences(friendIds);
		foreach (ulong friendId in friendIds)
		{
			Images.RefreshCachedProfileImage(friendId);
			Profile profileFromCache = Profiles.GetProfileFromCache(friendId);
			if (profileFromCache != null)
			{
				RefreshFriendPanel(profileFromCache);
			}
		}
		noFriendsPanel.gameObject.SetActive(remoteFriendPanels.Count == 0);
	}

	private void ResortFriendsList()
	{
		List<UIPlayerPanel> list = remoteFriendPanels.Values.OrderBy((UIPlayerPanel p) => p.PlayerName).ToList();
		bool flag = false;
		for (int num = 0; num < list.Count; num++)
		{
			if (num != list[num].transform.GetSiblingIndex())
			{
				list[num].transform.SetSiblingIndex(num);
				flag = true;
			}
		}
		if (flag)
		{
			LayoutRebuilder.MarkLayoutForRebuild(friendList);
		}
	}

	private void LateUpdate()
	{
		if (friendListResortRequested)
		{
			friendListResortRequested = false;
			ResortFriendsList();
		}
	}

	private void OnProfileUpdated(ulong id, Profile profile)
	{
		if (profile != null)
		{
			if (friendIds.Contains(id))
			{
				RefreshFriendPanel(profile);
			}
			if (localPlayerIds.Contains(id))
			{
				RefreshLocalPlayerPanel(profile);
			}
		}
	}

	private void RefreshFriendPanel(Profile profile)
	{
		UIPlayerPanel value;
		if (!remoteFriendPanels.TryGetValue(profile.Id, out value))
		{
			value = Object.Instantiate(playerPanelPrefab);
			value.transform.SetParent(friendList, false);
			value.gameObject.SetActive(true);
			value.setPlayerId(profile.Id);
			remoteFriendPanels.Add(profile.Id, value);
			noFriendsPanel.gameObject.SetActive(false);
		}
		PlayerPresence playerPresenceFromCache = PlayerPresenceManager.GetPlayerPresenceFromCache(profile.Id);
		value.PlayerName = profile.DisplayName;
		value.IsOnline = PUNNetworkManager.Instance.IsOnline(playerPresenceFromCache);
		value.PlayerStatus = PUNNetworkManager.Instance.GetPlayerStatusString(playerPresenceFromCache);
		value.PlayerLevel = profile.Level.ToString();
		value.PartyColor = null;
		Texture texture = Images.GetProfileImageFromCache(profile.Id);
		if (texture == null || !profile.Verified)
		{
			texture = Player.LocalPlayer.DefaultProfileImage;
		}
		value.PlayerAvatar = texture;
		if (playerSpecificMenu != null && playerSpecificMenu.PlayerId == profile.Id)
		{
			RefreshPlayerSpecificMenu(value);
		}
		friendListResortRequested = true;
	}

	private void RefreshLocalPlayerPanel(Profile profile)
	{
		UIPlayerPanel value;
		if (!localPlayerPanels.TryGetValue(profile.Id, out value))
		{
			value = Object.Instantiate(playerPanelPrefab);
			value.transform.SetParent(localPlayerList, false);
			value.gameObject.SetActive(true);
			localPlayerPanels.Add(profile.Id, value);
			noLocalPlayersPanel.gameObject.SetActive(false);
		}
		value.PlayerName = profile.DisplayName;
		Player player = null;
		foreach (Player item in PhotonNetwork.otherPlayers.Select(UnityExtensions.ToPlayer))
		{
			if (item != null && item.PlayerId == profile.Id)
			{
				player = item;
			}
		}
		if (player == null)
		{
			Object.Destroy(value.gameObject);
			localPlayerPanels.Remove(profile.Id);
			localPlayerIds.Remove(profile.Id);
			return;
		}
		value.SetPlayer(player);
		value.IsOnline = true;
		value.PlayerStatus = ((!profile.Verified) ? "[Guest] " : string.Empty) + GetLocalPlayerStatus(player);
		value.PlayerLevel = profile.Level.ToString();
		value.PlayerIsMember = profile.Verified;
		value.PartyColor = player.PlayerParty.PartyColor;
		Texture texture = Images.GetProfileImageFromCache(profile.Id);
		if (texture == null || !profile.Verified)
		{
			texture = Player.LocalPlayer.DefaultProfileImage;
		}
		value.PlayerAvatar = texture;
		if (playerSpecificMenu != null && playerSpecificMenu.PlayerId == profile.Id)
		{
			RefreshPlayerSpecificMenu(value);
		}
	}

	private void OnProfileImageUpdated(ulong id, Texture2D image)
	{
		if (image == null || IsPlayerGuest(id))
		{
			image = Player.LocalPlayer.DefaultProfileImage;
		}
		UIPlayerPanel value;
		if (localPlayerPanels.TryGetValue(id, out value))
		{
			value.PlayerAvatar = image;
		}
		if (remoteFriendPanels.TryGetValue(id, out value))
		{
			value.PlayerAvatar = image;
		}
		if (playerSpecificMenu != null && playerSpecificMenu.PlayerId == id)
		{
			playerSpecificMenu.PlayerAvatar = image;
		}
	}

	private void OnPlayerPresenceUpdated(ulong id, PlayerPresence playerPresence)
	{
		UIPlayerPanel value;
		if (remoteFriendPanels.TryGetValue(id, out value))
		{
			value.IsOnline = PUNNetworkManager.Instance.IsOnline(playerPresence);
			value.PlayerStatus = ((!IsPlayerGuest(id)) ? string.Empty : "[Guest] ") + PUNNetworkManager.Instance.GetPlayerStatusString(playerPresence);
			if (playerSpecificMenu != null && playerSpecificMenu.PlayerId == id)
			{
				RefreshPlayerSpecificMenu(value);
			}
			friendListResortRequested = true;
		}
		if (localPlayerPanels.TryGetValue(id, out value) && value.Player != null)
		{
			value.IsOnline = true;
			value.PlayerStatus = ((!IsPlayerGuest(id)) ? string.Empty : "[Guest] ") + GetLocalPlayerStatus(value.Player);
			if (playerSpecificMenu != null && playerSpecificMenu.PlayerId == id)
			{
				RefreshPlayerSpecificMenu(value);
			}
		}
	}

	private string GetPlayerStatus(ulong playerId)
	{
		PlayerPresence playerPresenceFromCache = PlayerPresenceManager.GetPlayerPresenceFromCache(playerId);
		return PUNNetworkManager.Instance.GetPlayerStatusString(playerPresenceFromCache);
	}

	private bool IsPlayerGuest(ulong playerId)
	{
		PhotonPlayer[] otherPlayers = PhotonNetwork.otherPlayers;
		foreach (PhotonPlayer photonPlayer in otherPlayers)
		{
			Player player = photonPlayer.ToPlayer();
			if (player != null && player.PlayerId == playerId)
			{
				return !player.PlayerProgression.IsMember;
			}
		}
		Profile profileFromCache = Profiles.GetProfileFromCache(playerId);
		return profileFromCache != null && !profileFromCache.Verified;
	}
}
