using System.Collections.Generic;
using UnityEngine;

public class CharadesManager : GameManager
{
	public enum CharadesGameStates
	{
		GAME_START = 0,
		SELECT_PERFORMER = 1,
		ROUND_START = 2,
		ROUND_RUNNING = 3,
		POST_ROUND = 4,
		INVALID = 65535
	}

	[Header("Charades")]
	[SerializeField]
	private float maxRoundTime = 30f;

	private const float MAX_ROUND_START_DURATION = 5f;

	private const float PRE_ROUND_DURATION = 6f;

	private const float POST_ROUND_DURATION = 3f;

	private List<MeshExtruderTool> gameplayTools = new List<MeshExtruderTool>();

	private PhysicalButton guessButton;

	private CardBox cardBox;

	private CharadesPodium[] podiums;

	private CharadesRoundTimer roundTimerVisual;

	private SynchronizedStateMachine charadesStateMachine;

	private SynchronizedTimer masterCharadesStateTimer;

	private SynchronizedField<int> _performerTeamPlayerIndexId;

	public bool IsRoundRunning
	{
		get
		{
			return charadesStateMachine.CurrentStateId == 3;
		}
	}

	public GameTeamPlayerIndex PerformerPlayerIndex
	{
		get
		{
			return (GameTeamPlayerIndex)_performerTeamPlayerIndexId.Get();
		}
		set
		{
			_performerTeamPlayerIndexId.ForceSet((int)value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		charadesStateMachine = new SynchronizedStateMachine("CHARADES", SetterPermissionMode.MASTER);
		charadesStateMachine.AddState(ushort.MaxValue, null, null, null);
		charadesStateMachine.AddState(1, OnEnterSelectPerformerState, null, OnUpdateSelectPerformerState);
		charadesStateMachine.AddState(2, OnEnterRoundStartState, null, OnUpdateRoundStartState);
		charadesStateMachine.AddState(3, OnEnterRoundRunningState, OnExitRoundRunningState, OnUpdateRoundRunningState);
		charadesStateMachine.AddState(4, OnEnterPostRoundState, OnExitPostRoundState, OnUpdatePostRoundState);
		masterCharadesStateTimer = new SynchronizedTimer(this, "CHARADES_TIMER", SetterPermissionMode.MASTER);
		_performerTeamPlayerIndexId = new SynchronizedField<int>(this, "PERFORMER_TEAM_PLAYER_INDEX", -1, SetterPermissionMode.MASTER, OnPerformerIdChange);
		MeshExtruderTool[] array = Object.FindObjectsOfType<MeshExtruderTool>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].GetComponent<GamePracticeObject>() == null)
			{
				gameplayTools.Add(array[i]);
			}
		}
		if (gameplayTools.Count == 0)
		{
			Debug.LogError("No non-practice extruder tools in Charades scene.");
		}
		base.SpawnManager.SpawnToolRestrictedByTeamPlayerIndex = true;
		guessButton = Object.FindObjectOfType<PhysicalButton>();
		guessButton.PushEvent += OnGuessButtonPush;
		roundTimerVisual = Object.FindObjectOfType<CharadesRoundTimer>();
		cardBox = Object.FindObjectOfType<CardBox>();
		podiums = Object.FindObjectsOfType<CharadesPodium>();
		ClearPodiums();
	}

	protected override void Initialize()
	{
		base.Initialize();
		charadesStateMachine.Initialize(this, ushort.MaxValue);
	}

	protected override void Update()
	{
		base.Update();
		charadesStateMachine.Update();
		masterCharadesStateTimer.UpdateCallbacks();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		guessButton.PushEvent -= OnGuessButtonPush;
	}

	protected override void OnGameStart()
	{
		if (PhotonNetwork.isMasterClient)
		{
			PerformerPlayerIndex = GameTeamPlayerIndex.ANY_INDEX;
			charadesStateMachine.EnterState(1);
		}
		UpdatePodiums();
	}

	protected override void OnGameEnd()
	{
		if (PhotonNetwork.isMasterClient)
		{
			charadesStateMachine.EnterState(ushort.MaxValue);
			MasterDisableGameplayTools();
		}
		ClearPodiums();
	}

	protected override void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
		base.InitializeLocalPlayer(localPlayerIsSpectator);
		base.SpawnManager.SpawnToolSupportedTeamPlayerIndex = GameTeamPlayerIndex.ANY_INDEX;
		if (!localPlayerIsSpectator)
		{
			Player.LocalPlayer.PlayerLocomotion.TeleportEnabled = false;
			base.SpawnManager.SetLocalPlayerSpawnPointTag(SpawnPointTagType.TAG_1);
			base.TeleportManager.SetLocalPlayerTeleportRegionTag(TeleportRegionTagType.TAG_1);
		}
		UpdateLocalPlayerWordVisibility();
	}

	protected override void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
		base.ResetLocalPlayer(localPlayerIsSpectator);
		if (!localPlayerIsSpectator)
		{
			Player.LocalPlayer.PlayerLocomotion.TeleportEnabled = true;
			base.SpawnManager.SetLocalPlayerSpawnPointTag(SpawnPointTagType.DEFAULT);
			base.TeleportManager.SetLocalPlayerTeleportRegionTag(TeleportRegionTagType.DEFAULT);
		}
	}

	protected override void ResetScene()
	{
		base.ResetScene();
		if (PhotonNetwork.isMasterClient)
		{
			MasterDisableGameplayTools();
		}
	}

	private void MasterDisableGameplayTools()
	{
		foreach (MeshExtruderTool gameplayTool in gameplayTools)
		{
			gameplayTool.Disable();
		}
	}

	protected override void OnTeamChange()
	{
		base.OnTeamChange();
		UpdatePodiums();
	}

	private void OnEnterSelectPerformerState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			masterCharadesStateTimer.StartTimer(6f);
			MasterIncrementPerformerTeam();
		}
		masterCharadesStateTimer.AddTimeElapsedCallback(3f, PlayNewPerformerNotification);
	}

	private void OnUpdateSelectPerformerState()
	{
		if (PhotonNetwork.isMasterClient && masterCharadesStateTimer.TimerOver)
		{
			charadesStateMachine.EnterState(2);
		}
	}

	private void PlayNewPerformerNotification()
	{
		PhotonPlayer playerAtTeamPlayerIndex = base.TeamManager.GetPlayerAtTeamPlayerIndex(GameTeam.TEAM_1, PerformerPlayerIndex);
		if (playerAtTeamPlayerIndex != null)
		{
			if (playerAtTeamPlayerIndex.isLocal)
			{
				ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Your Turn!", 2f);
			}
			else
			{
				ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, playerAtTeamPlayerIndex.name + "'s Turn!", 2f);
			}
		}
	}

	private void OnEnterRoundStartState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			masterCharadesStateTimer.StartTimer(5f);
			Extrusion[] array = Object.FindObjectsOfType<Extrusion>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].MasterResetToLastPickup();
			}
			cardBox.MasterClearCards();
		}
		if (!base.TeamManager.IsPlayerSpectator(PhotonNetwork.player) && IsPlayerPerforming(PhotonNetwork.player))
		{
			base.SpawnManager.SetLocalPlayerSpawnPointTag(SpawnPointTagType.TAG_2);
			base.TeleportManager.SetLocalPlayerTeleportRegionTag(TeleportRegionTagType.TAG_2);
			Player.LocalPlayer.PlayerLocomotion.TeleportEnabled = true;
			base.SpawnManager.LocalPlayerRequestRespawn();
		}
		UpdateLocalPlayerWordVisibility();
		roundTimerVisual.Timer = maxRoundTime;
	}

	private void OnUpdateRoundStartState()
	{
		if (PhotonNetwork.isMasterClient && masterCharadesStateTimer.TimerOver)
		{
			charadesStateMachine.EnterState(3);
		}
	}

	private void OnEnterRoundRunningState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			masterCharadesStateTimer.StartTimer(maxRoundTime);
		}
		base.FxManager.StartLoopingSFX(FxType.TIMER_RUNNING, roundTimerVisual.transform);
		roundTimerVisual.Timer = masterCharadesStateTimer.TimeRemaining;
	}

	private void OnUpdateRoundRunningState()
	{
		roundTimerVisual.Timer = masterCharadesStateTimer.TimeRemaining;
		if (PhotonNetwork.isMasterClient && masterCharadesStateTimer.TimerOver)
		{
			base.photonView.RPC("RpcOnTimerExpired", PhotonTargets.All);
			charadesStateMachine.EnterState(4);
		}
	}

	private void OnExitRoundRunningState(ushort nextStateId, ushort nextSubStateId)
	{
		base.FxManager.StopLoopingSFX(FxType.TIMER_RUNNING);
	}

	private void OnEnterPostRoundState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			masterCharadesStateTimer.StartTimer(3f);
		}
	}

	private void OnExitPostRoundState(ushort nextStateId, ushort nextSubStateId)
	{
		if (!base.TeamManager.IsPlayerSpectator(PhotonNetwork.player) && IsPlayerPerforming(PhotonNetwork.player))
		{
			base.SpawnManager.SetLocalPlayerSpawnPointTag(SpawnPointTagType.TAG_1);
			base.TeleportManager.SetLocalPlayerTeleportRegionTag(TeleportRegionTagType.TAG_1);
			Player.LocalPlayer.PlayerLocomotion.TeleportEnabled = false;
			base.SpawnManager.LocalPlayerRequestRespawn();
		}
	}

	private void OnUpdatePostRoundState()
	{
		if (PhotonNetwork.isMasterClient && masterCharadesStateTimer.TimerOver)
		{
			charadesStateMachine.EnterState(1);
		}
	}

	public bool IsPlayerPerforming(PhotonPlayer player)
	{
		return base.TeamManager.GetTeamPlayerIndex(player) == PerformerPlayerIndex;
	}

	private void OnPerformerIdChange()
	{
		base.SpawnManager.SpawnToolSupportedTeamPlayerIndex = PerformerPlayerIndex;
	}

	private void MasterIncrementPerformerTeam()
	{
		GameTeamPlayerIndex performerPlayerIndex = PerformerPlayerIndex;
		PhotonPlayer[] teamPlayersSortedByIndex = base.TeamManager.GetTeamPlayersSortedByIndex(GameTeam.TEAM_1);
		for (int i = 0; i < teamPlayersSortedByIndex.Length; i++)
		{
			GameTeamPlayerIndex teamPlayerIndex = base.TeamManager.GetTeamPlayerIndex(teamPlayersSortedByIndex[i]);
			if (teamPlayerIndex > performerPlayerIndex)
			{
				PerformerPlayerIndex = teamPlayerIndex;
				return;
			}
		}
		PerformerPlayerIndex = base.TeamManager.GetTeamPlayerIndex(teamPlayersSortedByIndex[0]);
	}

	private void UpdateLocalPlayerWordVisibility()
	{
		cardBox.UpdateLocalPlayerCardVisibility(IsPlayerPerforming(PhotonNetwork.player));
	}

	private void OnGuessButtonPush(PhysicalButton button, Player buttonPusher)
	{
		if (buttonPusher.PhotonPlayer.isLocal && IsValidGuess(buttonPusher.PhotonPlayer))
		{
			base.photonView.RPC("RpcMasterRequestGuess", PhotonTargets.MasterClient, buttonPusher.PhotonPlayer);
		}
	}

	private void MasterRequestGuess(PhotonPlayer guesser)
	{
		if (PhotonNetwork.isMasterClient && IsValidGuess(guesser))
		{
			GameTeam[] activeTeams = base.TeamManager.GetActiveTeams();
			for (int i = 0; i < activeTeams.Length; i++)
			{
				base.StatsManager.MasterAddScore(activeTeams[i], 1);
			}
			base.photonView.RPC("RpcOnGuess", PhotonTargets.All, guesser);
			charadesStateMachine.EnterState(4);
		}
	}

	private void OnGuess()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Team Scores!", "The word was: " + cardBox.PreviousWord, 3f);
		base.FxManager.PlayFX(FxType.GOAL, guessButton.transform.position);
	}

	private void OnTimerExpired()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Time's up!", "The word was: " + cardBox.PreviousWord, 3f);
		base.FxManager.PlayFX(FxType.TIMER_EXPIRED, roundTimerVisual.transform.position);
	}

	private bool IsValidGuess(PhotonPlayer guesser)
	{
		return IsPlayerPerforming(guesser) && charadesStateMachine.CurrentStateId == 3;
	}

	public void UpdatePodiums()
	{
		for (int i = 0; i < podiums.Length; i++)
		{
			PhotonPlayer playerAtTeamPlayerIndex = base.TeamManager.GetPlayerAtTeamPlayerIndex(GameTeam.TEAM_1, podiums[i].PlayerIndex);
			if (playerAtTeamPlayerIndex == null)
			{
				podiums[i].ClearPlayer();
			}
			else
			{
				podiums[i].SetPlayer(playerAtTeamPlayerIndex);
			}
		}
	}

	public void ClearPodiums()
	{
		for (int i = 0; i < podiums.Length; i++)
		{
			podiums[i].ClearPlayer();
		}
	}

	[PunRPC]
	private void RpcMasterRequestGuess(PhotonPlayer guesser)
	{
		MasterRequestGuess(guesser);
	}

	[PunRPC]
	private void RpcOnGuess(PhotonPlayer guesser)
	{
		OnGuess();
	}

	[PunRPC]
	private void RpcOnTimerExpired()
	{
		OnTimerExpired();
	}

	protected override void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		base.OnPhotonPlayerDisconnected(player);
		UpdatePodiums();
	}
}
