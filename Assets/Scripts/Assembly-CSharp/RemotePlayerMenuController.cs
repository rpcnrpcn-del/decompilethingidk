using RecNet;
using UnityEngine;
using UnityEngine.UI;

public class RemotePlayerMenuController : MenuController
{
	[Header("Player specific")]
	[SerializeField]
	private RawImage avatar;

	[SerializeField]
	private Text playerNameText;

	[SerializeField]
	private Text playerStatusText;

	[SerializeField]
	private Image partyColorImage;

	[Header("Local Player Buttons")]
	[SerializeField]
	private RectTransform localPlayerUI;

	[SerializeField]
	private Button muteButton;

	[SerializeField]
	private Button unmuteButton;

	[SerializeField]
	private Button ghostButton;

	[SerializeField]
	private Button unghostButton;

	[SerializeField]
	private Button localPlayerSteamProfileButton;

	[SerializeField]
	private Button voteToKickButton;

	[SerializeField]
	private Text voteToKickButtonText;

	private string voteToKickButtonDefaultText = string.Empty;

	[SerializeField]
	private Text levelText;

	[SerializeField]
	private Text guestTag;

	[SerializeField]
	private Image levelTextBackground;

	[SerializeField]
	private Button moderatorKickPlayer;

	[Header("Follow Cam")]
	[SerializeField]
	private Transform uiButtonRoot;

	[SerializeField]
	private Renderer followCamRenderer;

	[SerializeField]
	private TeleportationPortal followCameraTeleportPortal;

	[Header("Remote Friend Buttons")]
	[SerializeField]
	private RectTransform remoteFriendUI;

	[SerializeField]
	private Button joinGameButton;

	[SerializeField]
	private Button inviteToGameButton;

	[SerializeField]
	private Button unfriendButton;

	[SerializeField]
	private Button friendSteamProfileButton;

	private bool isLocalPlayer;

	public Player Target { get; private set; }

	public ulong PlayerId { get; private set; }

	public PartyMenuController PartyMenuController { get; set; }

	public override bool Visible
	{
		set
		{
			base.Visible = value;
			followCameraVisible = value;
			if (base.Owner.PlayerUI.UIPlayerCamera != null)
			{
				bool flag = Target != null && Target.PhotonPlayer != null && (gameManager == null || gameManager.CurrentState != GameStates.GAME_RUNNING || gameManager.TeamManager.PlayersAreTeammates(PhotonNetwork.player, Target.PhotonPlayer));
				UIPlayerCamera uIPlayerCamera = base.Owner.PlayerUI.UIPlayerCamera;
				if (value && flag)
				{
					uIPlayerCamera.Setup(Target, followCamRenderer, followCameraTeleportPortal, uiButtonRoot);
					base.Owner.PlayerUI.UIPlayerCamera.gameObject.SetActive(true);
				}
				else
				{
					uIPlayerCamera.Setup(null);
					base.Owner.PlayerUI.UIPlayerCamera.gameObject.SetActive(false);
				}
			}
		}
	}

	public string PlayerName
	{
		get
		{
			return playerNameText.text;
		}
		set
		{
			playerNameText.text = value;
		}
	}

	public string PlayerStatus
	{
		get
		{
			return playerStatusText.text;
		}
		set
		{
			playerStatusText.text = value;
		}
	}

	public string PlayerLevel
	{
		get
		{
			return levelText.text;
		}
		set
		{
			levelText.text = value;
		}
	}

	public Texture PlayerAvatar
	{
		get
		{
			return avatar.texture;
		}
		set
		{
			avatar.texture = value;
		}
	}

	public Color? PartyColor
	{
		get
		{
			return (!partyColorImage.gameObject.activeSelf) ? ((Color?)null) : new Color?(partyColorImage.color);
		}
		set
		{
			partyColorImage.gameObject.SetActive(value.HasValue);
			if (value.HasValue)
			{
				partyColorImage.color = value.Value;
				levelTextBackground.color = value.Value;
			}
			else
			{
				levelTextBackground.color = Color.white;
			}
		}
	}

	public Text GuestTag
	{
		get
		{
			return guestTag;
		}
	}

	private bool followCameraVisible
	{
		set
		{
			followCamRenderer.gameObject.SetActive(value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		voteToKickButtonDefaultText = voteToKickButtonText.text;
		moderatorKickPlayer.gameObject.SetActive(SessionManager.IsDeveloper);
	}

	protected override void OnDisable()
	{
		if (Target != null)
		{
			Target.PlayerData.UnregisterCallback("VoteToKickCount", OnTargetVoteToKickCountChange);
		}
		base.OnDisable();
	}

	public void InitForLocalPlayer(Player targetPlayer)
	{
		isLocalPlayer = true;
		Target = targetPlayer;
		if (Target != null)
		{
			Target.PlayerData.RegisterCallback("VoteToKickCount", OnTargetVoteToKickCountChange);
		}
		PlayerId = targetPlayer.PlayerId;
		Refresh();
	}

	private void OnTargetVoteToKickCountChange(PlayerData sender, string key)
	{
		Refresh();
	}

	public void InitForFriend(ulong friendId)
	{
		isLocalPlayer = false;
		Target = null;
		PlayerId = friendId;
		Refresh();
	}

	public void ButtonPress_ToggleMute()
	{
		if (Target != null)
		{
			Target.Mute = !Target.Mute;
			PartyMenuController.Refresh();
		}
	}

	public void ButtonPress_ToggleGhost()
	{
		if (Target != null)
		{
			ConfirmationMenuController confirmationMenuController = base.Owner.PlayerUI.Menu.ShowConfirmation(((!Target.PermaGhost) ? "Ghost" : "Unghost") + " '" + Target.PlayerName + "'?", (!Target.PermaGhost) ? "This user will appear as a ghost to you." : "This user will appear normally to you.");
			confirmationMenuController.ConfirmAction += GhostPlayerConfirmation;
			followCameraVisible = false;
		}
	}

	public void ButtonPress_ToggleFollowCameraMode()
	{
		if (base.Owner != null && (bool)base.Owner.PlayerUI.UIPlayerCamera)
		{
			base.Owner.PlayerUI.UIPlayerCamera.CycleFollowMode();
		}
	}

	private void GhostPlayerConfirmation(ConfirmationMenuController confirmMenu)
	{
		confirmMenu.ConfirmAction -= GhostPlayerConfirmation;
		if (confirmMenu.Confirm == true)
		{
			Target.PermaGhost = !Target.PermaGhost;
		}
		followCameraVisible = true;
		PartyMenuController.Refresh();
	}

	private void KickPlayerConfirmation(ConfirmationMenuController confirmMenu)
	{
		confirmMenu.ConfirmAction -= KickPlayerConfirmation;
		if (confirmMenu.Confirm == true)
		{
			PartyMenuController.Owner.PlayerParty.KickPlayer(Target);
		}
		followCameraVisible = true;
		PartyMenuController.Refresh();
	}

	private void UnfriendPlayerConfirmation(ConfirmationMenuController confirmMenu)
	{
		confirmMenu.ConfirmAction -= UnfriendPlayerConfirmation;
		if (confirmMenu.Confirm == true)
		{
			Relationships.RemoveFriend(PlayerId);
		}
		followCameraVisible = true;
		Visible = false;
	}

	public void ButtonPress_ShowSteamProfile()
	{
		if (PlatformManager.Instance.CurrentPlatform != PlatformManager.PlatformType.STEAM)
		{
		}
	}

	public void ButtonPress_Unfriend()
	{
		ConfirmationMenuController confirmationMenuController = base.Owner.PlayerUI.Menu.ShowConfirmation("Unfriend '" + PlayerName + "'?", "Are you sure you want to unfriend '" + PlayerName + "'?");
		confirmationMenuController.ConfirmAction += UnfriendPlayerConfirmation;
		followCameraVisible = false;
		PartyMenuController.Refresh();
	}

	public void ButtonPress_JoinGame()
	{
		base.PlayerMenu.RunJoinPlayer(PlayerId);
	}

	public void ButtonPress_InviteToGame()
	{
		string playerName = PlayerName;
		AnalyticsHelper.SentGameInvite(Player.LocalPlayer, PlayerId, PlayerName, PlayerStatus);
		Messages.SendGameInvite(PlayerId, delegate(string error)
		{
			if (string.IsNullOrEmpty(error))
			{
				ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Invite Sent", playerName, 3f);
			}
			else
			{
				Debug.LogError("Unable to send game invite: " + error);
				ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Failed to Send Invite", 3f);
			}
		});
	}

	public void ButtonPress_ModeratorKickPlayer(int durationSeconds)
	{
		if (Target != null)
		{
			Target.PlayerModeration.KickPlayer(durationSeconds);
		}
	}

	public void ButtonPress_VoteToKick()
	{
		if (Target != null)
		{
			Target.PlayerModeration.VoteToKickPlayer();
		}
	}

	public void ButtonPress_ShowReportMenu()
	{
		ShowSubMenu(true);
	}

	public void ButtonPress_ReportPlayerForMicAbuse()
	{
		ReportPlayerHelper(PlayerReporting.ReportCategory.MicrophoneAbuse);
	}

	public void ButtonPress_ReportPlayerForHarassment()
	{
		ReportPlayerHelper(PlayerReporting.ReportCategory.Harassment);
	}

	public void ButtonPress_ReportPlayerForCheating()
	{
		ReportPlayerHelper(PlayerReporting.ReportCategory.Cheating);
	}

	public void ButtonPress_ReportPlayerForImmatureBehavior()
	{
		ReportPlayerHelper(PlayerReporting.ReportCategory.ImmatureBehavior);
	}

	private void ReportPlayerHelper(PlayerReporting.ReportCategory reportCategory)
	{
		ShowSubMenu(false);
		if (PlayerReporting.CreateReport(PlayerId, reportCategory))
		{
			ScreenSpaceNotificationManager.Instance.PlayDelayed(ScreenSpaceNotificationManager.NotificationType.Medium, "Thanks for your report. We will investigate.", 3f, 2f);
		}
		else
		{
			ScreenSpaceNotificationManager.Instance.PlayDelayed(ScreenSpaceNotificationManager.NotificationType.Medium, "You have already reported this player.", 3f, 2f);
		}
		if (Target != null)
		{
			Target.PlayerModeration.VoteToKickPlayer();
			Target.PermaGhost = true;
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		if (isLocalPlayer && (Target == null || Target.PhotonPlayer == null))
		{
			Visible = false;
		}
		else if (isLocalPlayer)
		{
			localPlayerUI.gameObject.SetActive(true);
			remoteFriendUI.gameObject.SetActive(false);
			muteButton.gameObject.SetActive(!Target.Mute);
			unmuteButton.gameObject.SetActive(Target.Mute);
			ghostButton.gameObject.SetActive(!Target.PermaGhost);
			unghostButton.gameObject.SetActive(Target.PermaGhost);
			voteToKickButton.interactable = !Target.PlayerModeration.IsVotedForKick;
			voteToKickButtonText.text = ((!Target.PlayerModeration.IsVotedForKick) ? voteToKickButtonDefaultText : string.Format("Vote: {0}", Target.PlayerModeration.VoteToKickCount));
		}
		else
		{
			localPlayerUI.gameObject.SetActive(false);
			remoteFriendUI.gameObject.SetActive(true);
			PlayerPresence playerPresenceFromCache = PlayerPresenceManager.GetPlayerPresenceFromCache(PlayerId);
			inviteToGameButton.interactable = PUNNetworkManager.Instance.CanInviteToCurrentGame() && playerPresenceFromCache != null && playerPresenceFromCache.AppVersion == PhotonNetwork.gameVersion && playerPresenceFromCache.GameSessionId != PhotonNetwork.room.name;
			joinGameButton.interactable = playerPresenceFromCache != null && playerPresenceFromCache.AppVersion == PhotonNetwork.gameVersion && playerPresenceFromCache.GameSessionId != PhotonNetwork.room.name && !playerPresenceFromCache.Private && playerPresenceFromCache.AvailableSpace > 0;
		}
	}
}
