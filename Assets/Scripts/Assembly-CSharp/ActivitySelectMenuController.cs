using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ActivitySelectMenuController : MenuController
{
	[Header("Play Game")]
	[SerializeField]
	private Button playButton;

	[SerializeField]
	private Text playButtonText;

	[SerializeField]
	private Button replayButton;

	[SerializeField]
	private Button restartButton;

	[SerializeField]
	private string playButtonDefaultText = string.Empty;

	[Header("Join Game")]
	[SerializeField]
	private Button joinGameButton;

	[SerializeField]
	private Text joinGameButtonText;

	[SerializeField]
	private Button leaveGameButton;

	[SerializeField]
	private string joinGameButtonDefaultText = string.Empty;

	[Header("Activities")]
	[SerializeField]
	private List<ActivityMapSelectionMenuController> activityMapSelectionMenuControllers;

	[Header("Teams")]
	[SerializeField]
	private Button switchTeamsButton;

	[Header("Tabs")]
	[SerializeField]
	private TabbedPanel pickupGamePanel;

	[SerializeField]
	private TabbedPanel createPrivateGamePanel;

	[Header("Create Game Tab")]
	[SerializeField]
	private GameObject createPanelMemberUI;

	[SerializeField]
	private GameObject createPanelGuestUI;

	private bool createPrivateRoom;

	public override bool Visible
	{
		set
		{
			base.Visible = value;
			if (gameManager != null && base.Owner.isLocal)
			{
				if (value)
				{
					gameManager.StateChangeEvent += OnGameStateChanged;
					gameManager.TeamManager.TeamChangeEvent += OnTeamsChanged;
				}
				else
				{
					gameManager.StateChangeEvent -= OnGameStateChanged;
					gameManager.TeamManager.TeamChangeEvent -= OnTeamsChanged;
				}
			}
		}
	}

	public override void Initialize(PlayerMenu playerMenu)
	{
		base.Initialize(playerMenu);
		foreach (ActivityMapSelectionMenuController activityMapSelectionMenuController in activityMapSelectionMenuControllers)
		{
			activityMapSelectionMenuController.Visible = false;
		}
		createPrivateGamePanel.OnSelected += CreatePrivateGamePanel_OnSelected;
	}

	private void CreatePrivateGamePanel_OnSelected(TabbedPanel panel, bool isSelected)
	{
		createPrivateRoom = isSelected;
	}

	public override void Refresh()
	{
		base.Refresh();
		UpdateUI();
		UpdateCreatePanel();
	}

	private void OnGameStateChanged(ushort currentStateId, ushort previousStateId, ushort currentSubStateId, ushort previousSubStateId)
	{
		UpdateUI();
	}

	private void OnTeamsChanged()
	{
		UpdateUI();
	}

	private void UpdateUI()
	{
		if (!(base.Owner == null) && base.Owner.isLocal)
		{
			if (gameManager != null)
			{
				PhotonPlayer[] activePlayers = gameManager.TeamManager.GetActivePlayers();
				int activePlayersMaxCount = gameManager.TeamManager.GetActivePlayersMaxCount();
				bool flag = gameManager.CurrentState == GameStates.PRE_GAME;
				GameTeam[] allTeams = gameManager.TeamManager.GetAllTeams();
				bool flag2 = allTeams != null && allTeams.Length > 0;
				bool flag3 = gameManager.TeamManager.IsPlayerSpectator(PhotonNetwork.player);
				bool flag4 = activePlayers == null || activePlayers.Length < activePlayersMaxCount;
				joinGameButton.gameObject.SetActive(flag3 && flag);
				joinGameButton.interactable = flag4;
				joinGameButtonText.text = ((!flag4) ? "Game Full" : joinGameButtonDefaultText);
				leaveGameButton.gameObject.SetActive(!flag3 && flag);
				switchTeamsButton.gameObject.SetActive(flag2 && flag);
				switchTeamsButton.interactable = !flag3 && gameManager.TeamManager.PlayerCanSwitchTeam(PhotonNetwork.player);
			}
			else
			{
				switchTeamsButton.gameObject.SetActive(false);
				joinGameButton.gameObject.SetActive(false);
				leaveGameButton.gameObject.SetActive(false);
			}
			UpdatePlayButtons();
		}
	}

	private void UpdatePlayButtons()
	{
		if (gameManager != null)
		{
			bool active = gameManager.CurrentState == GameStates.PRE_GAME;
			bool flag = gameManager.TeamManager.IsPlayerSpectator(PhotonNetwork.player);
			restartButton.gameObject.SetActive(false);
			playButton.interactable = gameManager.ReadyToPlay && !flag;
			playButton.gameObject.SetActive(active);
			playButtonText.text = ((!playButton.interactable && !flag) ? "Not Enough Players..." : playButtonDefaultText);
			replayButton.gameObject.SetActive(false);
		}
		else
		{
			playButton.gameObject.SetActive(false);
			replayButton.gameObject.SetActive(false);
			restartButton.gameObject.SetActive(false);
		}
	}

	private void UpdateCreatePanel()
	{
		createPanelGuestUI.SetActive(!base.Owner.PlayerProgression.IsMember);
		createPanelMemberUI.SetActive(base.Owner.PlayerProgression.IsMember);
	}

	public void ButtonPress_Play()
	{
		if (gameManager != null)
		{
			gameManager.LocalPlayerStartGame();
			UpdateUI();
		}
	}

	public void ButtonPress_Replay()
	{
		if (gameManager != null)
		{
			gameManager.LocalPlayerStopGame();
			UpdateUI();
		}
	}

	public void ButtonPress_Restart()
	{
		if (gameManager != null)
		{
			gameManager.LocalPlayerStopGame();
			UpdateUI();
		}
	}

	public void ButtonPress_JoinGame()
	{
		if (gameManager != null)
		{
			gameManager.LocalPlayerJoinLeaveGame();
			UpdateUI();
		}
	}

	public void ButtonPress_LeaveGame()
	{
		if (gameManager != null)
		{
			gameManager.LocalPlayerJoinLeaveGame();
			UpdateUI();
		}
	}

	public void ButtonPress_SwitchTeam()
	{
		if (gameManager != null)
		{
			gameManager.LocalPlayerSwitchTeam();
			UpdateUI();
		}
	}

	public void Button_SwitchActivity(string activity)
	{
		base.PlayerMenu.RunSwitchActivity(activity, createPrivateRoom);
	}

	public void Button_OpenMapSelectSubMenu(string activityName)
	{
		ActivityMapSelectionMenuController activityMapSelectionMenuController = activityMapSelectionMenuControllers.FirstOrDefault((ActivityMapSelectionMenuController m) => m.ActivityName == activityName);
		if (activityMapSelectionMenuController != null)
		{
			subMenu = activityMapSelectionMenuController;
			activityMapSelectionMenuController.Visible = true;
		}
	}
}
