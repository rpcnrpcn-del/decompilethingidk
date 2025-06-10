using UnityEngine;
using UnityEngine.UI;

public class PanicPlayerMenuController : MenuController
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
	private Button muteButton;

	[SerializeField]
	private Button unmuteButton;

	[SerializeField]
	private Button ghostButton;

	[SerializeField]
	private Button unghostButton;

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

	public Player Target { get; private set; }

	public PanicMenuController PanicMenuController { get; set; }

	public override bool Visible
	{
		set
		{
			base.Visible = value;
			followCameraVisible = value;
			if (base.Owner.PlayerUI.UIPlayerCamera != null)
			{
				bool flag = true;
				UIPlayerCamera uIPlayerCamera = base.Owner.PlayerUI.UIPlayerCamera;
				if (Target != null && value && flag)
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
		moderatorKickPlayer.gameObject.SetActive(false);
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

	public void InitForPlayer(Player targetPlayer)
	{
		Target = targetPlayer;
		if (Target != null)
		{
			Target.PlayerData.RegisterCallback("VoteToKickCount", OnTargetVoteToKickCountChange);
		}
		Refresh();
	}

	private void OnTargetVoteToKickCountChange(PlayerData sender, string key)
	{
		Refresh();
	}

	public void ButtonPress_ToggleMute()
	{
		if (Target != null)
		{
			Target.Mute = !Target.Mute;
			PanicMenuController.Refresh();
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
		PanicMenuController.Refresh();
	}

	private void KickPlayerConfirmation(ConfirmationMenuController confirmMenu)
	{
		confirmMenu.ConfirmAction -= KickPlayerConfirmation;
		if (confirmMenu.Confirm == true)
		{
			PanicMenuController.Owner.PlayerParty.KickPlayer(Target);
		}
		followCameraVisible = true;
		PanicMenuController.Refresh();
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

	public override void Refresh()
	{
		base.Refresh();
		if (Target == null || Target.PhotonPlayer == null)
		{
			Visible = false;
			return;
		}
		muteButton.gameObject.SetActive(!Target.Mute);
		unmuteButton.gameObject.SetActive(Target.Mute);
		ghostButton.gameObject.SetActive(!Target.PermaGhost);
		unghostButton.gameObject.SetActive(Target.PermaGhost);
		voteToKickButton.interactable = !Target.PlayerModeration.IsVotedForKick;
		voteToKickButtonText.text = ((!Target.PlayerModeration.IsVotedForKick) ? voteToKickButtonDefaultText : string.Format("Vote: {0}", Target.PlayerModeration.VoteToKickCount));
		levelText.text = Target.PlayerProgression.Level.ToString();
		guestTag.gameObject.SetActive(!Target.PlayerProgression.IsMember);
	}
}
