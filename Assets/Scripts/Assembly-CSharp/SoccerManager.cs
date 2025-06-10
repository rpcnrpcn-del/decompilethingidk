using System;
using UnityEngine;

public class SoccerManager : GameManager
{
	public enum SoccerGameStates
	{
		PRE_ROUND = 0,
		ROUND_RUNNING = 1,
		BALL_SCORED = 2,
		INVALID = 65535
	}

	private const float ROUND_START_STATE_DURATION = 3f;

	private const float BALL_SCORED_STATE_DURATION = 3f;

	private SynchronizedStateMachine soccerStateMachine;

	private SynchronizedTimer masterSoccerStateTimer;

	private SoccerBall ball;

	private SoccerGarageDoor garageDoor;

	private SoccerPowerup[] powerups;

	protected override void Awake()
	{
		base.Awake();
		soccerStateMachine = new SynchronizedStateMachine("SOCCER", SetterPermissionMode.MASTER);
		soccerStateMachine.AddState(ushort.MaxValue, null, null, null);
		soccerStateMachine.AddState(0, OnEnterPreRoundState, OnExitPreRoundState, OnUpdatePreRoundState);
		soccerStateMachine.AddState(1, OnEnterRoundRunningState, OnExitRoundRunningState, null);
		soccerStateMachine.AddState(2, OnEnterBallScoredState, OnExitBallScoredState, OnUpdateBallScoredState);
		masterSoccerStateTimer = new SynchronizedTimer(this, "SOCCER_TIMER", SetterPermissionMode.MASTER);
		SoccerBall[] array = UnityEngine.Object.FindObjectsOfType<SoccerBall>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].GetComponent<GamePracticeObject>() == null)
			{
				ball = array[i];
			}
		}
		if (ball == null)
		{
			Debug.LogError("There is no non-practice soccer ball in the soccer scene.");
		}
		else
		{
			ball.ScoreEvent += OnBallScore;
		}
		SoccerShield[] array2 = UnityEngine.Object.FindObjectsOfType<SoccerShield>();
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].IsReleasedOnPlayerRespawn = false;
		}
		powerups = UnityEngine.Object.FindObjectsOfType<SoccerPowerup>();
		for (int k = 0; k < powerups.Length; k++)
		{
			SoccerPowerup obj = powerups[k];
			obj.PickupEvent = (SoccerPowerup.Pickup)Delegate.Combine(obj.PickupEvent, new SoccerPowerup.Pickup(OnPowerupPickup));
		}
		garageDoor = UnityEngine.Object.FindObjectOfType<SoccerGarageDoor>();
		garageDoor.Open = true;
	}

	protected override void Initialize()
	{
		base.Initialize();
		soccerStateMachine.Initialize(this, ushort.MaxValue);
	}

	protected override void Update()
	{
		base.Update();
		soccerStateMachine.Update();
		masterSoccerStateTimer.UpdateCallbacks();
	}

	protected override void ResetScene()
	{
		base.ResetScene();
		if (PhotonNetwork.isMasterClient)
		{
			MasterDisableAllPowerups();
			ball.Disable();
		}
		garageDoor.Open = true;
	}

	protected override void OnGameStart()
	{
		garageDoor.Open = false;
		if (PhotonNetwork.isMasterClient)
		{
			soccerStateMachine.EnterState(0);
		}
	}

	protected override void OnGameEnd()
	{
		garageDoor.Open = true;
		if (PhotonNetwork.isMasterClient)
		{
			soccerStateMachine.EnterState(ushort.MaxValue);
		}
	}

	private void OnBallScore(SoccerBall ball, SoccerGoal scoredOnGoal, Vector3 scorePoint)
	{
		if (ball.Owner.isLocal && IsValidScore())
		{
			base.photonView.RPC("RpcMasterRequestScore", PhotonTargets.MasterClient, scoredOnGoal.Team, scorePoint);
		}
	}

	private void MasterRequestScore(GameTeam scoredOnTeam, Vector3 scorePoint)
	{
		if (PhotonNetwork.isMasterClient && IsValidScore())
		{
			GameTeam anotherTeam = base.TeamManager.GetAnotherTeam(scoredOnTeam);
			PhotonPlayer lastHitPlayer = ball.GetLastHitPlayer(anotherTeam);
			if (anotherTeam != GameTeam.INVALID)
			{
				base.StatsManager.AddScore(anotherTeam, 1);
				soccerStateMachine.EnterState(2);
				ball.Disable();
				base.photonView.RPC("RpcOnScore", PhotonTargets.All, lastHitPlayer, anotherTeam, scorePoint);
			}
		}
	}

	private bool IsValidScore()
	{
		return soccerStateMachine.CurrentStateId == 1;
	}

	private void OnScore(PhotonPlayer scorer, GameTeam scoringTeam, Vector3 scorePoint)
	{
		string subtitleText = ((scorer == null) ? string.Empty : string.Format("Goal scored by {0}", scorer.name));
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, base.TeamManager.GetTeamName(scoringTeam) + " Scores!", subtitleText, 3f);
		switch (scoringTeam)
		{
		case GameTeam.TEAM_1:
			base.FxManager.PlayFX(FxType.TEAM_1_SCORE);
			break;
		case GameTeam.TEAM_2:
			base.FxManager.PlayFX(FxType.TEAM_2_SCORE);
			break;
		}
		base.FxManager.PlayFX(FxType.GOAL, scorePoint, Quaternion.identity, true);
		if (!base.TeamManager.IsPlayerSpectator(PhotonNetwork.player) && base.TeamManager.GetPlayerTeam(PhotonNetwork.player) == scoringTeam)
		{
			Player.LocalPlayer.PlayerEvents.SoccerGoal();
		}
	}

	private void OnPowerupPickup(SoccerPowerup powerup, Player pickupPlayer)
	{
		if (pickupPlayer.PhotonPlayer.isLocal && IsValidPowerup(powerup, pickupPlayer.PhotonPlayer))
		{
			base.photonView.RPC("RpcMasterRequestPowerup", PhotonTargets.MasterClient, powerup.photonView.viewID, pickupPlayer.PhotonPlayer);
		}
	}

	private void MasterRequestPowerup(SoccerPowerup powerup, PhotonPlayer pickupPlayer)
	{
		if (PhotonNetwork.isMasterClient && IsValidPowerup(powerup, pickupPlayer))
		{
			powerup.MasterOnPickup();
			base.photonView.RPC("RpcOnPowerupPickup", PhotonTargets.All, powerup.photonView.viewID, pickupPlayer);
		}
	}

	private bool IsValidPowerup(SoccerPowerup powerup, PhotonPlayer pickupPlayer)
	{
		return soccerStateMachine.CurrentStateId == 1 && powerup.IsAlive && !base.TeamManager.IsPlayerSpectator(pickupPlayer);
	}

	private void OnPowerupPickup(SoccerPowerup powerup, PhotonPlayer pickupPlayer)
	{
		base.FxManager.PlayFX(FxType.POWERUP_PICKUP, powerup.transform.position, powerup.transform.rotation);
		if (pickupPlayer.isLocal)
		{
			if (Player.LocalPlayer.LeftHand.Tool != null)
			{
				Player.LocalPlayer.LeftHand.Tool.OnPowerup(powerup);
			}
			if (Player.LocalPlayer.RightHand.Tool != null)
			{
				Player.LocalPlayer.RightHand.Tool.OnPowerup(powerup);
			}
		}
	}

	private void MasterDisableAllPowerups()
	{
		for (int i = 0; i < powerups.Length; i++)
		{
			powerups[i].MasterDisable();
		}
	}

	private void MasterEnableAllPowerups()
	{
		for (int i = 0; i < powerups.Length; i++)
		{
			powerups[i].MasterEnable();
		}
	}

	private void OnEnterPreRoundState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			masterSoccerStateTimer.StartTimer(3f);
			base.GameTimerManager.Pause();
			ball.Disable();
		}
		if (!base.TeamManager.IsPlayerSpectator(PhotonNetwork.player) && Player.LocalPlayer != null)
		{
			Player.LocalPlayer.PlayerLocomotion.TeleportEnabled = false;
		}
		masterSoccerStateTimer.AddTimeElapsedCallback(Player.RespawnDuration, PlayRoundStartNotification);
	}

	private void OnExitPreRoundState(ushort nextStateId, ushort nextSubStateId)
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Go!", 1.5f);
	}

	private void OnUpdatePreRoundState()
	{
		if (PhotonNetwork.isMasterClient && masterSoccerStateTimer.TimerOver)
		{
			soccerStateMachine.EnterState(1);
		}
	}

	private void PlayRoundStartNotification()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Round Starting", 1.5f);
	}

	private void OnEnterRoundRunningState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			base.GameTimerManager.Resume();
			MasterEnableAllPowerups();
			ball.MasterResetToLastSpawnPosition();
		}
		if (!base.TeamManager.IsPlayerSpectator(PhotonNetwork.player) && Player.LocalPlayer != null)
		{
			Player.LocalPlayer.PlayerLocomotion.TeleportEnabled = true;
		}
	}

	private void OnExitRoundRunningState(ushort nextStateId, ushort nextSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			MasterDisableAllPowerups();
		}
	}

	private void OnEnterBallScoredState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			masterSoccerStateTimer.StartTimer(3f);
			base.GameTimerManager.Pause();
		}
	}

	private void OnExitBallScoredState(ushort nextStateId, ushort nextSubStateId)
	{
		if (!base.TeamManager.IsPlayerSpectator(PhotonNetwork.player))
		{
			base.SpawnManager.LocalPlayerRequestRespawn();
		}
	}

	private void OnUpdateBallScoredState()
	{
		if (PhotonNetwork.isMasterClient && masterSoccerStateTimer.TimerOver)
		{
			soccerStateMachine.EnterState(0);
		}
	}

	[PunRPC]
	private void RpcMasterRequestScore(GameTeam scoredOnTeam, Vector3 scorePoint)
	{
		MasterRequestScore(scoredOnTeam, scorePoint);
	}

	[PunRPC]
	private void RpcOnScore(PhotonPlayer scorer, GameTeam scoringTeam, Vector3 scorePoint)
	{
		OnScore(scorer, scoringTeam, scorePoint);
	}

	[PunRPC]
	private void RpcMasterRequestPowerup(int powerupPhotonViewId, PhotonPlayer pickupPlayer)
	{
		PhotonView photonView = PhotonView.Find(powerupPhotonViewId);
		SoccerPowerup soccerPowerup = ((!(photonView != null)) ? null : photonView.GetComponent<SoccerPowerup>());
		if (soccerPowerup != null)
		{
			MasterRequestPowerup(soccerPowerup, pickupPlayer);
		}
	}

	[PunRPC]
	private void RpcOnPowerupPickup(int powerupPhotonViewId, PhotonPlayer pickupPlayer)
	{
		PhotonView photonView = PhotonView.Find(powerupPhotonViewId);
		SoccerPowerup soccerPowerup = ((!(photonView != null)) ? null : photonView.GetComponent<SoccerPowerup>());
		if (soccerPowerup != null)
		{
			OnPowerupPickup(soccerPowerup, pickupPlayer);
		}
	}
}
