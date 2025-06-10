using System;
using System.Text;
using RecNet;
using UnityEngine;

public class PanicMenuController : MenuController
{
	[SerializeField]
	private UIPanicPlayerPanel playerPanelPrefab;

	[SerializeField]
	private RectTransform localPlayerList;

	[SerializeField]
	private RectTransform noPeoplePanel;

	private PanicPlayerMenuController playerSpecificMenu;

	private UIPanicPlayerPanel[] localPlayerPanels;

	private bool initialized;

	private void LazyInit()
	{
		if (initialized)
		{
			return;
		}
		playerPanelPrefab.gameObject.SetActive(false);
		if (base.Owner != null)
		{
			localPlayerPanels = new UIPanicPlayerPanel[7];
			for (int i = 0; i < localPlayerPanels.Length; i++)
			{
				localPlayerPanels[i] = UnityEngine.Object.Instantiate(playerPanelPrefab);
				localPlayerPanels[i].transform.SetParent(localPlayerList, false);
			}
			initialized = true;
		}
		if (subMenu != null)
		{
			playerSpecificMenu = subMenu as PanicPlayerMenuController;
			playerSpecificMenu.PanicMenuController = this;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.Owner != null)
		{
			Images.OnProfileImageUpdated += OnProfileImageUpdated;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (base.Owner != null)
		{
			Images.OnProfileImageUpdated -= OnProfileImageUpdated;
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		if (base.Owner != null)
		{
			LazyInit();
			RefreshLocalSessionPlayers();
		}
	}

	private void RefreshLocalSessionPlayers()
	{
		int num = 0;
		PhotonPlayer[] otherPlayers = PhotonNetwork.otherPlayers;
		foreach (PhotonPlayer photonPlayer in otherPlayers)
		{
			Player player = photonPlayer.ToPlayer();
			if (player != null && num < localPlayerPanels.Length)
			{
				UIPanicPlayerPanel uIPanicPlayerPanel = localPlayerPanels[num];
				num++;
				uIPanicPlayerPanel.gameObject.SetActive(true);
				uIPanicPlayerPanel.SetPlayer(player);
				string playerName = player.PlayerName;
				string localPlayerStatus = GetLocalPlayerStatus(player);
				Texture texture = Images.GetProfileImageFromCache(player.PlayerId);
				if (texture == null || !player.PlayerProgression.IsMember)
				{
					texture = Player.LocalPlayer.DefaultProfileImage;
				}
				Color? partyColor = ((!player.PlayerParty.IsInParty) ? ((Color?)null) : player.PlayerParty.PartyColor);
				uIPanicPlayerPanel.PlayerName = playerName;
				uIPanicPlayerPanel.PlayerStatus = localPlayerStatus;
				uIPanicPlayerPanel.PlayerAvatar = texture;
				uIPanicPlayerPanel.PartyColor = partyColor;
				uIPanicPlayerPanel.PlayerLevel = player.PlayerProgression.Level;
				uIPanicPlayerPanel.PlayerIsMember = player.PlayerProgression.IsMember;
				uIPanicPlayerPanel.PlayerDistanceSqrFromLocalPlayer = Player.DistanceSqrBetweenPlayers(Player.LocalPlayer, player);
				if (playerSpecificMenu.Target == player)
				{
					playerSpecificMenu.PlayerName = playerName;
					playerSpecificMenu.PlayerStatus = localPlayerStatus;
					playerSpecificMenu.PlayerAvatar = texture;
					playerSpecificMenu.PartyColor = partyColor;
				}
				if (player.PlayerProgression.IsMember)
				{
					Images.RefreshCachedProfileImage(player.PlayerId);
				}
			}
		}
		for (int j = num; j < localPlayerPanels.Length; j++)
		{
			localPlayerPanels[j].gameObject.SetActive(false);
		}
		Array.Sort(localPlayerPanels, (UIPanicPlayerPanel a, UIPanicPlayerPanel b) => a.PlayerDistanceSqrFromLocalPlayer.CompareTo(b.PlayerDistanceSqrFromLocalPlayer));
		for (int num2 = 0; num2 < localPlayerPanels.Length; num2++)
		{
			localPlayerPanels[num2].transform.SetSiblingIndex(num2);
		}
		noPeoplePanel.gameObject.SetActive(num == 0);
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
		playerSpecificMenu.InitForPlayer(player);
		playerSpecificMenu.PlayerName = player.PlayerName;
		playerSpecificMenu.PlayerStatus = GetLocalPlayerStatus(player);
		playerSpecificMenu.PlayerAvatar = Images.GetProfileImageFromCache(player.PlayerId);
		playerSpecificMenu.PartyColor = ((!player.PlayerParty.IsInParty) ? ((Color?)null) : player.PlayerParty.PartyColor);
		ShowSubMenu(true);
	}

	private void OnProfileImageUpdated(ulong id, Texture2D image)
	{
		if (image == null || !IsPlayerMember(id))
		{
			image = Player.LocalPlayer.DefaultProfileImage;
		}
		if (localPlayerPanels != null)
		{
			for (int i = 0; i < localPlayerPanels.Length; i++)
			{
				if (localPlayerPanels[i].PlayerId == id)
				{
					localPlayerPanels[i].PlayerAvatar = image;
				}
			}
		}
		if (playerSpecificMenu != null && playerSpecificMenu.Target != null && playerSpecificMenu.Target.PlayerId == id)
		{
			playerSpecificMenu.PlayerAvatar = image;
		}
	}

	private bool IsPlayerMember(ulong playerId)
	{
		PhotonPlayer[] otherPlayers = PhotonNetwork.otherPlayers;
		foreach (PhotonPlayer photonPlayer in otherPlayers)
		{
			Player player = photonPlayer.ToPlayer();
			if (player != null && player.PlayerId == playerId)
			{
				return player.PlayerProgression.IsMember;
			}
		}
		return false;
	}
}
