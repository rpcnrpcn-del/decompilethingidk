using RecNet;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProfileView : MonoBehaviour
{
	[SerializeField]
	private Text playerName;

	[SerializeField]
	private Text playerStatus;

	[SerializeField]
	private RawImage playerIcon;

	[SerializeField]
	private Slider playerXp;

	[SerializeField]
	private Text playerLevelText;

	[SerializeField]
	private Text playerGuestTag;

	[SerializeField]
	private Image playerLevelBackground;

	public string PlayerName
	{
		set
		{
			playerName.text = value;
		}
	}

	public string PlayerStatus
	{
		set
		{
			playerStatus.text = value;
		}
	}

	public Texture PlayerIcon
	{
		set
		{
			playerIcon.texture = value;
		}
	}

	public void Refresh(Player player)
	{
		if (player == null)
		{
			base.gameObject.SetActive(false);
			return;
		}
		base.gameObject.SetActive(true);
		PlayerName = player.PlayerName;
		//PlayerStatus = GetSteamUserStatus(player.SteamID);
		RefreshPlayerIcon(player);
		if (playerXp != null && player.isLocal)
		{
			playerXp.value = SingletonMonoBehaviour<ProgressionManager>.Instance.LocalXPProgression;
		}
		if (playerLevelText != null)
		{
			playerLevelText.text = player.PlayerProgression.Level.ToString();
		}
		if (playerGuestTag != null)
		{
			playerGuestTag.gameObject.SetActive(!player.PlayerProgression.IsMember);
		}
		playerLevelBackground.color = ((!player.PlayerParty.PartyColor.HasValue) ? Color.white : player.PlayerParty.PartyColor.Value);
	}

	public void RefreshPlayerIcon(Player player)
	{
		if (player != null)
		{
			Texture2D texture2D = Images.GetProfileImageFromCache(player.PlayerId);
			if (texture2D == null || !player.PlayerProgression.IsMember)
			{
				texture2D = Player.LocalPlayer.DefaultProfileImage;
			}
			PlayerIcon = texture2D;
		}
	}

	private string GetSteamUserStatus(CSteamID steamId)
	{
		return (PlatformManager.Instance.CurrentPlatform != PlatformManager.PlatformType.STEAM) ? null : SteamFriends.GetFriendRichPresence(steamId, "status");
	}
}
