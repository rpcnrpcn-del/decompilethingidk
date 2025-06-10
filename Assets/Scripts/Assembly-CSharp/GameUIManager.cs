using System;
using UnityEngine;

public class GameUIManager : IGameComponent
{
	private GameManager gameManager;

	private GameUIView[] uiViews;

	private GameUIState uiState;

	private GameUIDataModel currentDataModel;

	private GameUIDataModel previousDataModel;

	private GameUITimerDataModel currentTimerDataModel;

	private GameUITimerDataModel previousTimerDataModel;

	private bool gameDataIsDirty = true;

	private bool supportsPreviousGameResults;

	public void OnAwake(GameManager gameManager)
	{
		this.gameManager = gameManager;
		this.gameManager.TeamManager.TeamChangeEvent += OnTeamChange;
		this.gameManager.StatsManager.StatsChangeEvent += OnStatChange;
		this.gameManager.ModeManager.ModeChangeEvent += OnModeChange;
		this.gameManager.PresenceSettings.PresenceChangeEvent += OnPresenceChange;
		this.gameManager.StateChangeEvent += OnGameStateChange;
	}

	public void OnStart()
	{
		uiViews = UnityEngine.Object.FindObjectsOfType<GameUIView>();
		for (int i = 0; i < uiViews.Length; i++)
		{
			uiViews[i].JoinLeaveGameButtonPressEvent += OnJoinLeaveGameButtonPressed;
			uiViews[i].StartGameButtonPressEvent += OnStartGameButtonPressed;
			uiViews[i].SwitchTeamButtonPressEvent += OnSwitchTeamButtonPressed;
			uiViews[i].SwitchModeButtonPressEvent += OnSwitchModeButtonPressed;
			uiViews[i].ViewResultsButtonPressEvent += OnViewResultsButtonPressed;
		}
		currentDataModel = CreateUIDataModel();
		previousDataModel = new GameUIDataModel();
		UpdateUIState();
		BroadcastGameUIDataModel();
		currentTimerDataModel = new GameUITimerDataModel();
		previousTimerDataModel = new GameUITimerDataModel();
		UpdateUITimerDataModel();
		BroadcastUITimerDataModel();
	}

	public void OnUpdate()
	{
		UpdateUITimerDataModel();
		BroadcastUITimerDataModel();
		if (gameDataIsDirty)
		{
			UpdateCurrentGameModel();
			BroadcastGameUIDataModel();
			gameDataIsDirty = false;
		}
	}

	public void OnDestroy()
	{
		if (gameManager != null)
		{
			gameManager.TeamManager.TeamChangeEvent -= OnTeamChange;
			gameManager.StatsManager.StatsChangeEvent -= OnStatChange;
			gameManager.ModeManager.ModeChangeEvent -= OnModeChange;
			gameManager.PresenceSettings.PresenceChangeEvent -= OnPresenceChange;
			gameManager.StateChangeEvent -= OnGameStateChange;
		}
		if (uiViews != null)
		{
			for (int i = 0; i < uiViews.Length; i++)
			{
				uiViews[i].JoinLeaveGameButtonPressEvent -= OnJoinLeaveGameButtonPressed;
				uiViews[i].StartGameButtonPressEvent -= OnStartGameButtonPressed;
				uiViews[i].SwitchTeamButtonPressEvent -= OnSwitchTeamButtonPressed;
				uiViews[i].SwitchModeButtonPressEvent -= OnSwitchModeButtonPressed;
				uiViews[i].ViewResultsButtonPressEvent -= OnViewResultsButtonPressed;
			}
		}
	}

	public void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
	}

	public void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
	}

	public void OnPlayerDisconnected(PhotonPlayer player)
	{
	}

	private void OnTeamChange()
	{
		gameDataIsDirty = true;
	}

	private void OnStatChange()
	{
		gameDataIsDirty = true;
	}

	private void OnModeChange()
	{
		gameDataIsDirty = true;
	}

	private void OnGameStateChange(ushort currentStateId, ushort previousStateId, ushort currentSubStateId, ushort previousSubStateId)
	{
		UpdateUIState();
		gameDataIsDirty = true;
	}

	private void OnPresenceChange()
	{
		gameDataIsDirty = true;
	}

	private void OnJoinLeaveGameButtonPressed()
	{
		gameManager.LocalPlayerJoinLeaveGame();
	}

	private void OnStartGameButtonPressed()
	{
		gameManager.LocalPlayerStartGame();
	}

	private void OnSwitchTeamButtonPressed()
	{
		gameManager.LocalPlayerSwitchTeam();
	}

	private void OnSwitchModeButtonPressed()
	{
		gameManager.LocalPlayerSwitchMode();
	}

	private void OnViewResultsButtonPressed()
	{
		if (gameManager.CurrentState == GameStates.PRE_GAME)
		{
			if (uiState == GameUIState.RESULTS && gameManager.CurrentPreGameState == PreGameSubStates.WAITING_FOR_PLAYERS)
			{
				uiState = GameUIState.WAITING_FOR_PLAYERS;
			}
			else if (uiState == GameUIState.RESULTS && gameManager.CurrentPreGameState == PreGameSubStates.WAITING_FOR_START_GAME)
			{
				uiState = GameUIState.PRE_GAME;
			}
			else if (uiState == GameUIState.PRE_GAME || uiState == GameUIState.WAITING_FOR_PLAYERS)
			{
				uiState = GameUIState.RESULTS;
			}
			gameDataIsDirty = true;
		}
	}

	private GameUIDataModel CreateUIDataModel()
	{
		GameUIDataModel gameUIDataModel = new GameUIDataModel();
		gameUIDataModel.Name = RecRoomSceneManager.CurrentSceneFriendlyName;
		gameUIDataModel.SupportsScore = gameManager.StatsManager.SupportsScore;
		gameUIDataModel.InviteOnly = PUNNetworkManager.Instance.IsActivityInviteOnly;
		gameUIDataModel.SupportsPreviousGameResults = false;
		gameUIDataModel.SupportsTeamSwitching = gameManager.TeamManager.GetAllTeams().Length > 1;
		gameUIDataModel.SupportsModeSwitching = gameManager.ModeManager.SupportsModeSwitching;
		gameUIDataModel.ModeName = string.Empty;
		gameUIDataModel.WinCondition = string.Empty;
		gameUIDataModel.PresenceString = string.Empty;
		int statsCount = gameManager.StatsManager.GetStatsCount();
		gameUIDataModel.StatNames = new string[statsCount];
		for (int i = 0; i < statsCount; i++)
		{
			gameUIDataModel.StatNames[i] = gameManager.StatsManager.GetPlayerStatName((PlayerStatType)i);
		}
		GameTeam[] allTeams = gameManager.TeamManager.GetAllTeams();
		Array.Sort(allTeams, delegate(GameTeam a, GameTeam b)
		{
			int num4 = (int)a;
			return num4.CompareTo((int)b);
		});
		int num = allTeams.Length;
		gameUIDataModel.TeamModels = new GameUITeamDataModel[num];
		for (int num2 = 0; num2 < num; num2++)
		{
			gameUIDataModel.TeamModels[num2] = new GameUITeamDataModel();
			gameUIDataModel.TeamModels[num2].Active = false;
			gameUIDataModel.TeamModels[num2].IsLocal = false;
			gameUIDataModel.TeamModels[num2].Color = GameTeamSettings.GetTeamColor(allTeams[num2]);
			gameUIDataModel.TeamModels[num2].Score = gameManager.StatsManager.GetScore(allTeams[num2]);
			int teamMaxPlayerCount = gameManager.TeamManager.GetTeamMaxPlayerCount(allTeams[num2]);
			gameUIDataModel.TeamModels[num2].PlayerModels = new GameUIPlayerDataModel[teamMaxPlayerCount];
			for (int num3 = 0; num3 < teamMaxPlayerCount; num3++)
			{
				gameUIDataModel.TeamModels[num2].PlayerModels[num3] = new GameUIPlayerDataModel();
				gameUIDataModel.TeamModels[num2].PlayerModels[num3].Active = false;
				gameUIDataModel.TeamModels[num2].PlayerModels[num3].IsLocal = false;
				gameUIDataModel.TeamModels[num2].PlayerModels[num3].Stats = new int[statsCount];
			}
		}
		gameUIDataModel.LocalPlayer = new GameUILocalPlayerDataModel();
		gameUIDataModel.LocalPlayer.IsSpectator = true;
		gameUIDataModel.LocalPlayer.CanSwitchTeams = false;
		gameUIDataModel.LocalPlayer.CanJoinGame = false;
		gameUIDataModel.LocalPlayer.CanLeaveGame = false;
		return gameUIDataModel;
	}

	private void UpdateCurrentGameModel()
	{
		currentDataModel.SupportsPreviousGameResults = supportsPreviousGameResults;
		currentDataModel.InviteOnly = PUNNetworkManager.Instance.IsActivityInviteOnly;
		GameMode mode = gameManager.ModeManager.GetMode();
		string text = gameManager.ModeManager.GetModeScoreName(mode).ToUpper();
		if (gameManager.ModeManager.GetModeHasScoreWinCondition(mode))
		{
			int modeScoreWinCondition = gameManager.ModeManager.GetModeScoreWinCondition(mode);
			currentDataModel.WinCondition = modeScoreWinCondition + " " + text;
			if (modeScoreWinCondition > 1)
			{
				currentDataModel.WinCondition += "S";
			}
		}
		else
		{
			currentDataModel.WinCondition = "TIMED";
		}
		string text2 = gameManager.ModeManager.GetCurrentModeName().ToUpper();
		if (currentDataModel.InviteOnly)
		{
			if (!string.IsNullOrEmpty(text2))
			{
				text2 += " ";
			}
			text2 += "(INVITE ONLY)";
		}
		currentDataModel.ModeName = text2;
		currentDataModel.PresenceString = gameManager.PresenceSettings.CurrentPresenceString;
		int statsCount = gameManager.StatsManager.GetStatsCount();
		GameTeam[] allTeams = gameManager.TeamManager.GetAllTeams();
		Array.Sort(allTeams, delegate(GameTeam a, GameTeam b)
		{
			int num4 = (int)a;
			return num4.CompareTo((int)b);
		});
		for (int num = 0; num < allTeams.Length; num++)
		{
			currentDataModel.TeamModels[num].IsLocal = false;
			currentDataModel.TeamModels[num].Score = gameManager.StatsManager.GetScore(allTeams[num]);
			PhotonPlayer[] teamPlayersSortedByIndex = gameManager.TeamManager.GetTeamPlayersSortedByIndex(allTeams[num]);
			currentDataModel.TeamModels[num].Active = teamPlayersSortedByIndex.Length > 0;
			int num2;
			for (num2 = 0; num2 < teamPlayersSortedByIndex.Length; num2++)
			{
				if (teamPlayersSortedByIndex[num2] == PhotonNetwork.player)
				{
					currentDataModel.TeamModels[num].PlayerModels[num2].IsLocal = true;
					currentDataModel.TeamModels[num].IsLocal = true;
				}
				else
				{
					currentDataModel.TeamModels[num].PlayerModels[num2].IsLocal = false;
				}
				currentDataModel.TeamModels[num].PlayerModels[num2].Active = true;
				currentDataModel.TeamModels[num].PlayerModels[num2].Name = teamPlayersSortedByIndex[num2].name;
				for (int num3 = 0; num3 < statsCount; num3++)
				{
					currentDataModel.TeamModels[num].PlayerModels[num2].Stats[num3] = gameManager.StatsManager.GetPlayerStat(teamPlayersSortedByIndex[num2], (PlayerStatType)num3);
				}
			}
			for (; num2 < currentDataModel.TeamModels[num].PlayerModels.Length; num2++)
			{
				currentDataModel.TeamModels[num].PlayerModels[num2].Active = false;
			}
		}
		currentDataModel.LocalPlayer.IsSpectator = gameManager.TeamManager.IsPlayerSpectator(PhotonNetwork.player);
		currentDataModel.LocalPlayer.CanSwitchTeams = gameManager.TeamManager.PlayerCanSwitchTeam(PhotonNetwork.player);
		currentDataModel.LocalPlayer.CanLeaveGame = gameManager.TeamManager.PlayerCanLeaveGame(PhotonNetwork.player);
		switch (gameManager.CurrentState)
		{
		case GameStates.PRE_GAME:
			currentDataModel.LocalPlayer.CanJoinGame = gameManager.TeamManager.GetGameVacancy() > 0;
			break;
		case GameStates.GAME_RUNNING:
			currentDataModel.LocalPlayer.CanJoinGame = gameManager.ReadyToJoinInProgress;
			break;
		default:
			currentDataModel.LocalPlayer.CanJoinGame = false;
			break;
		}
	}

	private void UpdateUITimerDataModel()
	{
		if (gameManager.GameTimerManager.SupportsGameTimer)
		{
			currentTimerDataModel.GameDuration = gameManager.GameTimerManager.GameDuration;
			currentTimerDataModel.GameTimeRemaining = gameManager.GameTimerManager.TimeRemaining;
		}
		else
		{
			currentTimerDataModel.GameDuration = 0f;
			currentTimerDataModel.GameTimeRemaining = gameManager.GameTimerManager.TimeElapsed;
		}
		if (gameManager.CurrentPreGameState == PreGameSubStates.GAME_LOADING)
		{
			currentTimerDataModel.GameStateTransitionTimeRemaining = gameManager.LoadingGameTimeRemaining;
		}
		else if (gameManager.CurrentState == GameStates.POST_GAME)
		{
			currentTimerDataModel.GameStateTransitionTimeRemaining = gameManager.PostGameTimeRemaining;
		}
		else
		{
			currentTimerDataModel.GameStateTransitionTimeRemaining = 0f;
		}
	}

	private void BroadcastGameUIDataModel()
	{
		GameUIDataModel dataModel = ((uiState != GameUIState.RESULTS && uiState != GameUIState.RESULTS_LOCKED) ? currentDataModel : previousDataModel);
		for (int i = 0; i < uiViews.Length; i++)
		{
			uiViews[i].SetGameDataModel(uiState, dataModel);
		}
	}

	private void BroadcastUITimerDataModel()
	{
		GameUITimerDataModel timerDataModel = ((uiState != GameUIState.RESULTS) ? currentTimerDataModel : previousTimerDataModel);
		for (int i = 0; i < uiViews.Length; i++)
		{
			uiViews[i].SetTimerDataModel(uiState, timerDataModel);
		}
	}

	private void UpdateUIState()
	{
		switch (gameManager.CurrentState)
		{
		case GameStates.PRE_GAME:
			switch (gameManager.CurrentPreGameState)
			{
			case PreGameSubStates.WAITING_FOR_PLAYERS:
				uiState = GameUIState.WAITING_FOR_PLAYERS;
				break;
			case PreGameSubStates.WAITING_FOR_START_GAME:
				uiState = GameUIState.PRE_GAME;
				break;
			case PreGameSubStates.GAME_LOADING:
				uiState = GameUIState.GAME_STARTING;
				break;
			case PreGameSubStates.GAME_ON:
				uiState = GameUIState.GAME_ON;
				break;
			}
			break;
		case GameStates.GAME_RUNNING:
			uiState = GameUIState.GAME_RUNNING;
			break;
		case GameStates.POST_GAME:
			switch (gameManager.CurrentPostGameState)
			{
			case PostGameSubStates.GAME_OVER:
				uiState = GameUIState.GAME_OVER;
				break;
			case PostGameSubStates.RESULTS:
				uiState = GameUIState.RESULTS_LOCKED;
				GameUIDataModel.DeepCopy(currentDataModel, previousDataModel);
				GameUITimerDataModel.DeepCopy(currentTimerDataModel, previousTimerDataModel);
				supportsPreviousGameResults = true;
				break;
			}
			break;
		}
	}
}
