using System.Collections.Generic;
using Photon;
using UnityEngine;

public abstract class GameManager : Photon.MonoBehaviour
{
	[Header("Join In Progress")]
	[SerializeField]
	private bool supportsJoinGameInProgress;

	[SerializeField]
	private bool automaticallyJoinInProgress;

	private const float GAME_ON_STATE_DURATION = 3f;

	private const float GAME_OVER_STATE_DURATION = 6f;

	private const float POST_GAME_STATE_DURATION = 15f;

	private const float GAME_START_DELAY = 10f;

	private const float JOIN_IN_PROGRESS_TIMEOUT = 5f;

	[SerializeField]
	private GameAFKSettings afkSettings;

	[SerializeField]
	private GameFxManager fxManager;

	[SerializeField]
	private GameConfigManager configManager;

	[SerializeField]
	private GameTimerManager gameTimerManager;

	[SerializeField]
	private GameCombatManager combatManager;

	[SerializeField]
	private GameModeManager modeManager;

	[SerializeField]
	private GamePresenceSettings presenceSettings;

	[SerializeField]
	private GameSpawnManager spawnManager;

	[SerializeField]
	private GameStatsManager statsManager;

	[SerializeField]
	private GameTeamManager teamManager;

	[SerializeField]
	private GameTeleportManager teleportManager;

	private SynchronizedStateMachine gameStateMachine;

	private SynchronizedTimer masterGameStateTimer;

	private Timer localGameStateTimer;

	private bool isInitialized;

	private List<IGameComponent> gameComponentManagers = new List<IGameComponent>();

	private GamePracticeObject[] practiceObjects;

	public GameAFKSettings AFKSettings
	{
		get
		{
			return afkSettings;
		}
	}

	public GameFxManager FxManager
	{
		get
		{
			return fxManager;
		}
	}

	public GameConfigManager ConfigManager
	{
		get
		{
			return configManager;
		}
	}

	public GameTimerManager GameTimerManager
	{
		get
		{
			return gameTimerManager;
		}
	}

	public GameCombatManager CombatManager
	{
		get
		{
			return combatManager;
		}
	}

	public GameModeManager ModeManager
	{
		get
		{
			return modeManager;
		}
	}

	public GamePresenceSettings PresenceSettings
	{
		get
		{
			return presenceSettings;
		}
	}

	public GameSpawnManager SpawnManager
	{
		get
		{
			return spawnManager;
		}
	}

	public GameStatsManager StatsManager
	{
		get
		{
			return statsManager;
		}
	}

	public GameTeamManager TeamManager
	{
		get
		{
			return teamManager;
		}
	}

	public GameTeleportManager TeleportManager
	{
		get
		{
			return teleportManager;
		}
	}

	public bool GhostingEnabled
	{
		get
		{
			return AFKSettings != null && (CurrentState != GameStates.GAME_RUNNING || AFKSettings.SupportsGhosting);
		}
	}

	public bool ReadyToPlay
	{
		get
		{
			return TeamManager.GameStartTeamRequirementsMet;
		}
	}

	public bool ReadyToJoinInProgress
	{
		get
		{
			return supportsJoinGameInProgress && CurrentState == GameStates.GAME_RUNNING && TeamManager.GetActivePlayerVacancy() > 0;
		}
	}

	public float LoadingGameTimeRemaining
	{
		get
		{
			return (CurrentPreGameState != PreGameSubStates.GAME_LOADING) ? 0f : masterGameStateTimer.TimeRemaining;
		}
	}

	public float PostGameTimeRemaining
	{
		get
		{
			return (CurrentPostGameState != PostGameSubStates.RESULTS) ? 0f : masterGameStateTimer.TimeRemaining;
		}
	}

	public GameStates CurrentState
	{
		get
		{
			return (GameStates)gameStateMachine.CurrentStateId;
		}
	}

	public bool IsGameRunning
	{
		get
		{
			return CurrentState == GameStates.GAME_RUNNING;
		}
	}

	public PreGameSubStates CurrentPreGameState
	{
		get
		{
			if (CurrentState != GameStates.PRE_GAME)
			{
				return PreGameSubStates.INVALID;
			}
			return (PreGameSubStates)gameStateMachine.CurrentSubStateId;
		}
	}

	public PostGameSubStates CurrentPostGameState
	{
		get
		{
			if (CurrentState != GameStates.POST_GAME)
			{
				return PostGameSubStates.INVALID;
			}
			return (PostGameSubStates)gameStateMachine.CurrentSubStateId;
		}
	}

	public event AbstractStateMachine.OnStateChange StateChangeEvent;

	protected override void Awake()
	{
		base.Awake();
		gameComponentManagers.Add(AFKSettings);
		gameComponentManagers.Add(FxManager);
		gameComponentManagers.Add(ConfigManager);
		gameComponentManagers.Add(GameTimerManager);
		gameComponentManagers.Add(CombatManager);
		gameComponentManagers.Add(ModeManager);
		gameComponentManagers.Add(PresenceSettings);
		gameComponentManagers.Add(SpawnManager);
		gameComponentManagers.Add(StatsManager);
		gameComponentManagers.Add(TeamManager);
		gameComponentManagers.Add(TeleportManager);
		gameComponentManagers.Add(new GameUIManager());
		for (int i = 0; i < gameComponentManagers.Count; i++)
		{
			gameComponentManagers[i].OnAwake(this);
		}
		practiceObjects = Object.FindObjectsOfType<GamePracticeObject>();
		TeamManager.LocalPlayerTeamChangeEvent += OnLocalPlayerTeamChange;
		gameStateMachine = new SynchronizedStateMachine("GAME", SetterPermissionMode.MASTER);
		gameStateMachine.StateChangeEvent += OnStateChange;
		gameStateMachine.AddState(0, OnEnterPreGameState, null, null);
		gameStateMachine.AddState(0, 0, OnEnterWaitingForPlayersSubState, OnExitWaitingForPlayersSubState, OnUpdateWaitingForPlayersSubState);
		gameStateMachine.AddState(0, 1, OnEnterWaitingForStartSubState, OnExitWaitingForStartSubState, OnUpdateWaitingForStartSubState);
		gameStateMachine.AddState(0, 2, OnEnterGameLoadingSubState, null, OnUpdateGameLoadingSubState);
		gameStateMachine.AddState(0, 3, OnEnterGameOnSubState, OnExitGameOnSubState, OnUpdateGameOnSubState);
		gameStateMachine.AddState(1, OnEnterGameRunningState, OnExitGameRunningState, OnUpdateGameRunningState);
		gameStateMachine.AddState(2, OnEnterPostGameState, null, null);
		gameStateMachine.AddState(2, 0, OnEnterGameOverSubState, OnExitGameOverSubState, OnUpdateGameOverSubState);
		gameStateMachine.AddState(2, 1, OnEnterResultsSubState, OnExitResultsSubState, OnUpdateResultsSubState);
		masterGameStateTimer = new SynchronizedTimer(this, "GAME_STATE_TIMER", SetterPermissionMode.MASTER);
		localGameStateTimer = new Timer();
	}

	private void Start()
	{
		PUNNetworkManager.Instance.OnRecRoomPlayerConnected += OnRecRoomPlayerCreated;
		if (Player.LocalPlayer != null)
		{
			OnRecRoomPlayerCreated(Player.LocalPlayer);
		}
	}

	private void OnRecRoomPlayerCreated(Player player)
	{
		if (player.isLocal && !isInitialized)
		{
			isInitialized = true;
			Initialize();
		}
	}

	protected virtual void Initialize()
	{
		for (int i = 0; i < gameComponentManagers.Count; i++)
		{
			gameComponentManagers[i].OnStart();
		}
		TeamManager.TeamChangeEvent += OnTeamChange;
		gameStateMachine.Initialize(this, 0, 0);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		gameStateMachine.StateChangeEvent -= OnStateChange;
		for (int i = 0; i < gameComponentManagers.Count; i++)
		{
			gameComponentManagers[i].OnDestroy();
		}
		TeamManager.TeamChangeEvent -= OnTeamChange;
		if (PUNNetworkManager.Instance != null)
		{
			PUNNetworkManager.Instance.OnRecRoomPlayerConnected -= OnRecRoomPlayerCreated;
		}
	}

	protected virtual void Update()
	{
		if (isInitialized)
		{
			gameStateMachine.Update();
			masterGameStateTimer.UpdateCallbacks();
			localGameStateTimer.UpdateCallbacks();
			for (int i = 0; i < gameComponentManagers.Count; i++)
			{
				gameComponentManagers[i].OnUpdate();
			}
		}
	}

	protected abstract void OnGameStart();

	protected abstract void OnGameEnd();

	protected virtual void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
		for (int i = 0; i < gameComponentManagers.Count; i++)
		{
			gameComponentManagers[i].InitializeLocalPlayer(localPlayerIsSpectator);
		}
	}

	protected virtual void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
		for (int i = 0; i < gameComponentManagers.Count; i++)
		{
			gameComponentManagers[i].ResetLocalPlayer(localPlayerIsSpectator);
		}
	}

	protected virtual void OnTeamChange()
	{
		MasterUpdateGameStateOnPlayerChange(null);
	}

	public void LocalPlayerStartGame()
	{
		base.photonView.RPC("RpcMasterStartGame", PhotonTargets.MasterClient);
	}

	public void LocalPlayerStopGame()
	{
		base.photonView.RPC("RpcMasterStopGame", PhotonTargets.MasterClient);
	}

	public void LocalPlayerJoinLeaveGame()
	{
		if (CurrentPreGameState == PreGameSubStates.WAITING_FOR_PLAYERS || CurrentPreGameState == PreGameSubStates.WAITING_FOR_START_GAME)
		{
			if (TeamManager.IsPlayerSpectator(PhotonNetwork.player))
			{
				TeamManager.RequestNextTeam(PhotonNetwork.player);
			}
			else
			{
				TeamManager.LeaveTeam(PhotonNetwork.player);
			}
		}
		else
		{
			TryJoinInProgress();
		}
	}

	public void LocalPlayerSwitchTeam()
	{
		if ((CurrentPreGameState == PreGameSubStates.WAITING_FOR_PLAYERS || CurrentPreGameState == PreGameSubStates.WAITING_FOR_START_GAME) && !TeamManager.IsPlayerSpectator(PhotonNetwork.player))
		{
			TeamManager.RequestTeamChange(PhotonNetwork.player);
		}
	}

	public void LocalPlayerSwitchMode()
	{
		if ((CurrentPreGameState == PreGameSubStates.WAITING_FOR_PLAYERS || CurrentPreGameState == PreGameSubStates.WAITING_FOR_START_GAME) && !TeamManager.IsPlayerSpectator(PhotonNetwork.player))
		{
			ModeManager.SwitchMode();
		}
	}

	private void OnLocalPlayerTeamChange()
	{
		if (CurrentState == GameStates.GAME_RUNNING && !TeamManager.IsPlayerSpectator(PhotonNetwork.player))
		{
			InitializeLocalPlayer(false);
			SpawnManager.LocalPlayerRequestRespawn();
		}
	}

	protected void MasterStopGame()
	{
		if (PhotonNetwork.isMasterClient && CurrentState == GameStates.GAME_RUNNING)
		{
			gameStateMachine.EnterState(2, 0);
		}
	}

	protected virtual void ResetScene()
	{
	}

	private void OnEnterPreGameState(ushort previousStateId, ushort previousSubStateId)
	{
		TeamManager.RequestNextTeam(PhotonNetwork.player);
		PresenceSettings.CurrentPresenceString = null;
		ResetScene();
		EnablePracticeObjects();
	}

	private void OnEnterWaitingForPlayersSubState(ushort previousStateId, ushort previousSubStateId)
	{
		localGameStateTimer.StartTimerWithoutMaxRunningTime();
		if (previousStateId == ushort.MaxValue)
		{
			localGameStateTimer.AddTimeElapsedCallback(4f, PlayWaitingForPlayersOneFeedback);
			localGameStateTimer.AddTimeElapsedCallback(8f, PlayWaitingForPlayersTwoFeedback);
			localGameStateTimer.AddTimeElapsedCallback(12f, PlayPracticeObjectAttentionEffects);
		}
		else
		{
			localGameStateTimer.AddTimeElapsedCallback(4f, PlayWaitingForPlayersTwoFeedback);
			localGameStateTimer.AddTimeElapsedCallback(8f, PlayPracticeObjectAttentionEffects);
		}
		localGameStateTimer.AddTimeElapsedCallback(45f, 60f, PlayWaitingForPlayersThreeFeedback);
		localGameStateTimer.AddTimeElapsedCallback(48f, 60f, PlayWaitingForPlayersFourFeedback);
	}

	private void OnUpdateWaitingForPlayersSubState()
	{
		if (PhotonNetwork.isMasterClient && ReadyToPlay)
		{
			gameStateMachine.EnterState(0, 1);
		}
	}

	private void OnExitWaitingForPlayersSubState(ushort nextStateId, ushort nextSubStateId)
	{
		localGameStateTimer.ClearCallbacks();
	}

	private void PlayWaitingForPlayersOneFeedback()
	{
		FxManager.PlaySFX((!PUNNetworkManager.Instance.IsActivityInviteOnly) ? FxType.WAITING_FOR_PLAYERS_ONE : FxType.WAITING_FOR_PLAYERS_ONE_INVITE_ONLY);
	}

	private void PlayWaitingForPlayersTwoFeedback()
	{
		FxManager.PlaySFX((!PUNNetworkManager.Instance.IsActivityInviteOnly) ? FxType.WAITING_FOR_PLAYERS_TWO : FxType.WAITING_FOR_PLAYERS_TWO_INVITE_ONLY);
	}

	private void PlayPracticeObjectAttentionEffects()
	{
		for (int i = 0; i < practiceObjects.Length; i++)
		{
			if (practiceObjects[i].PlayAttractEffect)
			{
				FxManager.PlayFX(FxType.ATTENTION_PING, practiceObjects[i].transform.position);
			}
		}
	}

	private void PlayWaitingForPlayersThreeFeedback()
	{
		FxManager.PlaySFX((!PUNNetworkManager.Instance.IsActivityInviteOnly) ? FxType.WAITING_FOR_PLAYERS_THREE : FxType.WAITING_FOR_PLAYERS_THREE_INVITE_ONLY);
	}

	private void PlayWaitingForPlayersFourFeedback()
	{
		FxManager.PlaySFX((!PUNNetworkManager.Instance.IsActivityInviteOnly) ? FxType.WAITING_FOR_PLAYERS_FOUR : FxType.WAITING_FOR_PLAYERS_FOUR_INVITE_ONLY);
	}

	private void OnEnterWaitingForStartSubState(ushort previousStateId, ushort previousSubStateId)
	{
		localGameStateTimer.StartTimerWithoutMaxRunningTime();
		localGameStateTimer.AddTimeElapsedCallback(5f, PlayReadyToStartFeedback);
		localGameStateTimer.AddTimeElapsedCallback(60f, 60f, PlayReadyToStartReminder);
	}

	private void OnUpdateWaitingForStartSubState()
	{
		if (PhotonNetwork.isMasterClient && !ReadyToPlay)
		{
			gameStateMachine.EnterState(0, 0);
		}
	}

	private void OnExitWaitingForStartSubState(ushort nextStateId, ushort nextSubStateId)
	{
		localGameStateTimer.ClearCallbacks();
	}

	private void PlayReadyToStartFeedback()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Major, "READY TO START", 3f);
		FxManager.PlayFX(FxType.READY_TO_START);
	}

	private void PlayReadyToStartReminder()
	{
		FxManager.PlayFX(FxType.READY_TO_START_REMINDER);
	}

	private void OnEnterGameLoadingSubState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			masterGameStateTimer.StartTimer(10f);
		}
		masterGameStateTimer.AddTimeRemainingCallback(10f, OnGameStartInTenSeconds);
		FxManager.StartLoadingGameMusic();
	}

	private void OnUpdateGameLoadingSubState()
	{
		if (PhotonNetwork.isMasterClient && masterGameStateTimer.TimerOver)
		{
			gameStateMachine.EnterState(0, 3);
		}
	}

	private void OnGameStartInTenSeconds()
	{
		GameMode mode = ModeManager.GetMode();
		string text = ((mode == GameMode.INVALID) ? "Game" : ModeManager.GetModeName(mode));
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, text + " starts in 10 seconds", 3f);
	}

	private void OnEnterGameOnSubState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			masterGameStateTimer.StartTimer(3f);
		}
		if (!TeamManager.IsPlayerSpectator(PhotonNetwork.player))
		{
			Player.LocalPlayer.CanInteractWithTools = false;
		}
		FxManager.StopMusic();
		PlayGameOnNotification();
	}

	private void OnUpdateGameOnSubState()
	{
		if (PhotonNetwork.isMasterClient && masterGameStateTimer.TimerOver)
		{
			gameStateMachine.EnterState(1);
		}
	}

	private void OnExitGameOnSubState(ushort nextStateId, ushort nextSubStateId)
	{
		if (!TeamManager.IsPlayerSpectator(PhotonNetwork.player))
		{
			Player.LocalPlayer.CanInteractWithTools = true;
		}
	}

	private void PlayGameOnNotification()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Major, "GAME ON!", 3f);
		FxManager.PlaySFX(FxType.GAME_ON);
	}

	private void OnEnterGameRunningState(ushort previousStateId, ushort previousSubStateId)
	{
		localGameStateTimer.StartTimerWithoutMaxRunningTime();
		DisablePracticeObjects();
		if (PhotonNetwork.isMasterClient)
		{
			GameTimerManager.StartTimer();
			StatsManager.MasterResetAllScores();
			PUNNetworkManager.Instance.SetGameInProgress(true);
		}
		bool flag = TeamManager.IsPlayerSpectator(PhotonNetwork.player);
		InitializeLocalPlayer(flag);
		if (!flag)
		{
			SpawnManager.LocalPlayerRequestRespawn(true, false, true);
		}
		OnGameStart();
		if (previousStateId == 0)
		{
			GameTimerManager.AddTimeElapsedCallback(Player.RespawnDuration + 1f, PlayGameModeNotification);
		}
		else if (automaticallyJoinInProgress && ReadyToJoinInProgress && TeamManager.IsPlayerSpectator(PhotonNetwork.player))
		{
			localGameStateTimer.AddTimeElapsedCallback(5f, PlayJoinGameInProgressNotification);
			localGameStateTimer.AddTimeElapsedCallback(8f, TryJoinInProgress);
		}
		else
		{
			localGameStateTimer.AddTimeElapsedCallback(5f, PlayGameInProgressNotification);
		}
		FxManager.StartPlayingGameMusic();
	}

	private void OnExitGameRunningState(ushort nextStateId, ushort nextSubStateId)
	{
		localGameStateTimer.ClearCallbacks();
		OnGameEnd();
		FxManager.StopMusic();
		if (PhotonNetwork.isMasterClient)
		{
			GameTimerManager.Pause();
			PUNNetworkManager.Instance.SetGameInProgress(false);
		}
	}

	private void OnUpdateGameRunningState()
	{
		if (PhotonNetwork.isMasterClient && GameTimerManager.SupportsGameTimer && GameTimerManager.TimerOver)
		{
			gameStateMachine.EnterState(2, 0);
		}
	}

	private void PlayGameModeNotification()
	{
		GameMode mode = ModeManager.GetMode();
		if (mode != GameMode.INVALID)
		{
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, ModeManager.GetModeName(mode), 3f);
			FxType audioType = FxType.MODE_1;
			switch (mode)
			{
			case GameMode.MODE_2:
				audioType = FxType.MODE_2;
				break;
			case GameMode.MODE_3:
				audioType = FxType.MODE_3;
				break;
			case GameMode.MODE_4:
				audioType = FxType.MODE_4;
				break;
			}
			FxManager.PlaySFX(audioType);
		}
	}

	private void PlayGameInProgressNotification()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Game In Progress", 3f);
	}

	private void PlayJoinGameInProgressNotification()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Joining Game In Progress", 3f);
	}

	private void TryJoinInProgress()
	{
		if (ReadyToJoinInProgress && TeamManager.IsPlayerSpectator(PhotonNetwork.player))
		{
			TeamManager.RequestNextTeam(PhotonNetwork.player);
		}
	}

	private void OnEnterPostGameState(ushort previousStateId, ushort previousSubStateId)
	{
		DisablePracticeObjects();
	}

	private void OnEnterGameOverSubState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			masterGameStateTimer.StartTimer(6f);
		}
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Major, "GAME OVER!", 3f);
		FxManager.PlaySFX(FxType.GAME_OVER);
		if (StatsManager.SupportsScore)
		{
			masterGameStateTimer.AddTimeRemainingCallback(3f, PlayWinnerNotification);
		}
		if (!TeamManager.IsPlayerSpectator(PhotonNetwork.player))
		{
			bool won = true;
			if (StatsManager.SupportsScore)
			{
				GameTeam winningTeam;
				StatsManager.GetWinningScore(out winningTeam);
				won = winningTeam == GameTeam.INVALID || winningTeam == TeamManager.GetPlayerTeam(PhotonNetwork.player);
			}
			Player.LocalPlayer.PlayerEvents.GameOver(won);
		}
	}

	private void OnExitGameOverSubState(ushort nextStateId, ushort nextSubStateId)
	{
		bool flag = TeamManager.IsPlayerSpectator(PhotonNetwork.player);
		ResetLocalPlayer(flag);
		if (!flag)
		{
			SpawnManager.LocalPlayerRequestRespawn(true, true);
		}
	}

	private void OnUpdateGameOverSubState()
	{
		if (PhotonNetwork.isMasterClient && masterGameStateTimer.TimerOver)
		{
			gameStateMachine.EnterState(2, 1);
		}
	}

	private void PlayWinnerNotification()
	{
		GameTeam winningTeam;
		StatsManager.GetWinningScore(out winningTeam);
		string empty = string.Empty;
		FxType fxType = FxType.INVALID;
		if (winningTeam == GameTeam.INVALID)
		{
			empty = "Tie Game";
			fxType = FxType.TIE_GAME;
		}
		else
		{
			empty = TeamManager.GetTeamName(winningTeam) + " Wins!";
			fxType = ((winningTeam != GameTeam.TEAM_1) ? FxType.TEAM_2_WIN : FxType.TEAM_1_WIN);
		}
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, empty, 3f);
		FxManager.PlaySFX(fxType);
	}

	private void OnEnterResultsSubState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			masterGameStateTimer.StartTimer(15f);
		}
		PresenceSettings.CurrentPresenceString = null;
	}

	private void OnExitResultsSubState(ushort nextStateId, ushort nextSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			TeamManager.MasterClearTeams();
		}
	}

	private void OnUpdateResultsSubState()
	{
		if (PhotonNetwork.isMasterClient && masterGameStateTimer.TimerOver)
		{
			gameStateMachine.EnterState(0, 0);
		}
	}

	private void OnStateChange(ushort currentStateId, ushort previousStateId, ushort currentSubStateId, ushort previousSubStateId)
	{
		if (this.StateChangeEvent != null)
		{
			this.StateChangeEvent(currentStateId, previousStateId, currentSubStateId, previousSubStateId);
		}
	}

	protected void EnablePracticeObjects()
	{
		for (int i = 0; i < practiceObjects.Length; i++)
		{
			practiceObjects[i].Enable();
		}
	}

	protected void DisablePracticeObjects()
	{
		for (int i = 0; i < practiceObjects.Length; i++)
		{
			practiceObjects[i].Disable();
		}
	}

	[PunRPC]
	protected void RpcMasterStartGame()
	{
		if (PhotonNetwork.isMasterClient && CurrentPreGameState == PreGameSubStates.WAITING_FOR_START_GAME)
		{
			gameStateMachine.EnterState(0, 2);
		}
	}

	[PunRPC]
	protected void RpcMasterStopGame()
	{
		MasterStopGame();
	}

	[PunRPC]
	protected void RpcMasterRequestTeam(PhotonPlayer player, int desiredTeamId, int forbiddenTeamId)
	{
		TeamManager.MasterAssignTeam(player, (GameTeam)desiredTeamId, (GameTeam)forbiddenTeamId);
	}

	[PunRPC]
	protected void RpcOnTeamChange()
	{
		TeamManager.FireTeamChangeEvent();
	}

	[PunRPC]
	protected void RpcMasterRequestPlayerRespawn(PhotonPlayer player, int spawnPointTagType, bool dropTools, bool requestSpawnTool, bool requestGiftSpawn)
	{
		SpawnManager.MasterRespawnPlayer(player, (SpawnPointTagType)spawnPointTagType, dropTools, requestSpawnTool, requestGiftSpawn);
	}

	[PunRPC]
	protected void RpcRespawnPlayer(Vector3 spawnPosition, Quaternion spawnRotation, bool dropTools, int spawnToolPhotonViewId, bool requestGiftSpawn)
	{
		if (Player.LocalPlayer != null)
		{
			Player.LocalPlayer.Respawn(spawnPosition, spawnRotation, dropTools, spawnToolPhotonViewId, requestGiftSpawn);
		}
	}

	[PunRPC]
	protected void RpcMasterAddScore(int gameTeam, int deltaScore)
	{
		StatsManager.MasterAddScore((GameTeam)gameTeam, deltaScore);
	}

	[PunRPC]
	protected void RpcMasterAddPlayerStat(PhotonPlayer player, int stat, int deltaValue)
	{
		StatsManager.MasterAddPlayerStat(player, (PlayerStatType)stat, deltaValue);
	}

	[PunRPC]
	protected void RpcBroadcastStatChange()
	{
		StatsManager.FireStatsChangeEvent();
	}

	[PunRPC]
	protected void RpcMasterSetConfig(int config, int value)
	{
		ConfigManager.MasterSetConfig((GameConfigType)config, value);
	}

	[PunRPC]
	protected void RpcBroadcastConfigChange()
	{
		ConfigManager.FireConfigChangeEvent();
	}

	[PunRPC]
	protected void RpcMasterSetMode(int mode)
	{
		ModeManager.MasterSetMode((GameMode)mode);
	}

	[PunRPC]
	protected void RpcBroadcastModeChange()
	{
		ModeManager.FireModeChangeEvent();
	}

	protected virtual void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, player.name + " joined the game", 3f);
	}

	protected virtual void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, player.name + " left the game", 3f);
		for (int i = 0; i < gameComponentManagers.Count; i++)
		{
			gameComponentManagers[i].OnPlayerDisconnected(player);
		}
		MasterUpdateGameStateOnPlayerChange(player);
	}

	protected virtual void OnMasterClientSwitched(PhotonPlayer newMasterClient)
	{
		MasterUpdateGameStateOnPlayerChange(null);
	}

	protected virtual void MasterUpdateGameStateOnPlayerChange(PhotonPlayer leavingPlayer)
	{
		if (PhotonNetwork.isMasterClient)
		{
			if ((CurrentPreGameState == PreGameSubStates.GAME_LOADING || CurrentPreGameState == PreGameSubStates.GAME_ON) && leavingPlayer != null && !TeamManager.IsPlayerSpectator(leavingPlayer))
			{
				gameStateMachine.EnterState(0, 0);
			}
			else if (CurrentState == GameStates.GAME_RUNNING && !ReadyToPlay)
			{
				MasterStopGame();
			}
		}
	}
}
