using System;
using UnityEngine;

public class PaddleballManager : GameManager
{
	public enum PaddleballGameStates
	{
		ROUND_RUNNING = 0,
		BALL_SCORED = 1,
		INVALID = 65535
	}

	[Header("Scoring")]
	[SerializeField]
	private int scoreToWin = 5;

	[Header("Ball Scored State")]
	[SerializeField]
	private float ballScoredStateDuration = 2f;

	private SynchronizedStateMachine paddleballStateMachine;

	private SynchronizedTimer masterPaddleballStateTimer;

	private PaddleballBall ball;

	private PaddleballBallSpawnPoint[] ballSpawnPoints;

	private PaddleballGoal[] goals;

	private SynchronizedField<int> servingTeamId;

	private GameTeam ServingTeam
	{
		get
		{
			return (GameTeam)servingTeamId.Get();
		}
		set
		{
			servingTeamId.ForceSet((int)value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		paddleballStateMachine = new SynchronizedStateMachine("PADDLEBALL", SetterPermissionMode.MASTER);
		paddleballStateMachine.AddState(ushort.MaxValue, null, null, null);
		paddleballStateMachine.AddState(0, OnEnterRoundRunningState, null, null);
		paddleballStateMachine.AddState(1, OnEnterBallScoredState, null, OnUpdateBallScoredState);
		masterPaddleballStateTimer = new SynchronizedTimer(this, "PADDLEBALL_TIMER", SetterPermissionMode.MASTER);
		PaddleballBall[] array = UnityEngine.Object.FindObjectsOfType<PaddleballBall>();
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].GetComponent<GamePracticeObject>())
			{
				ball = array[i];
				break;
			}
		}
		ballSpawnPoints = UnityEngine.Object.FindObjectsOfType<PaddleballBallSpawnPoint>();
		goals = UnityEngine.Object.FindObjectsOfType<PaddleballGoal>();
		for (int j = 0; j < goals.Length; j++)
		{
			PaddleballGoal obj = goals[j];
			obj.ScoreEvent = (PaddleballGoal.Score)Delegate.Combine(obj.ScoreEvent, new PaddleballGoal.Score(OnGoalScore));
		}
		servingTeamId = new SynchronizedField<int>(this, "SERVING_TEAM", -1, SetterPermissionMode.MASTER);
		if (PhotonNetwork.isMasterClient)
		{
			ServingTeam = GameTeam.TEAM_1;
		}
	}

	protected override void Initialize()
	{
		base.Initialize();
		paddleballStateMachine.Initialize(this, ushort.MaxValue);
		ball.NetworkTransform.CurrentExtrapolationMode = PUNNetworkTransform.ExtrapolationMode.DynamicDistance;
	}

	protected override void Update()
	{
		base.Update();
		paddleballStateMachine.Update();
		masterPaddleballStateTimer.UpdateCallbacks();
	}

	protected override void ResetScene()
	{
		base.ResetScene();
		if (PhotonNetwork.isMasterClient)
		{
			ball.Disable();
		}
	}

	protected override void OnGameStart()
	{
		if (PhotonNetwork.isMasterClient)
		{
			paddleballStateMachine.EnterState(0);
		}
		base.PresenceSettings.CurrentPresenceString = "First to " + scoreToWin + " Goals";
	}

	protected override void OnGameEnd()
	{
		if (PhotonNetwork.isMasterClient)
		{
			paddleballStateMachine.EnterState(ushort.MaxValue);
		}
	}

	private void OnGoalScore(PaddleballGoal goal, Vector3 scorePoint)
	{
		if (goal.hasAuthority && IsValidScore())
		{
			base.photonView.RPC("RpcMasterRequestScore", PhotonTargets.MasterClient, goal.photonView.viewID, scorePoint);
		}
	}

	private void MasterRequestScore(PaddleballGoal goal, Vector3 scorePoint)
	{
		if (PhotonNetwork.isMasterClient && IsValidScore())
		{
			GameTeam anotherTeam = base.TeamManager.GetAnotherTeam(goal.Team);
			if (anotherTeam != GameTeam.INVALID)
			{
				ServingTeam = goal.Team;
				base.StatsManager.AddScore(anotherTeam, 1);
				paddleballStateMachine.EnterState(1);
				base.photonView.RPC("RpcOnScore", PhotonTargets.All, goal.photonView.viewID, scorePoint);
			}
		}
	}

	private bool IsValidScore()
	{
		return paddleballStateMachine.CurrentStateId == 0;
	}

	private void OnScore(PaddleballGoal goal, Vector3 scorePoint)
	{
		GameTeam anotherTeam = base.TeamManager.GetAnotherTeam(goal.Team);
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, base.TeamManager.GetTeamName(anotherTeam) + " Scores!", 3f);
		if (!base.TeamManager.IsPlayerSpectator(PhotonNetwork.player) && base.TeamManager.GetPlayerTeam(PhotonNetwork.player) == anotherTeam)
		{
			Player.LocalPlayer.PlayerEvents.PaddleballScore();
		}
		base.FxManager.PlayFX(FxType.GOAL, scorePoint);
	}

	private void MasterTransferGoalAuthority()
	{
		for (int i = 0; i < goals.Length; i++)
		{
			PhotonPlayer[] teamPlayers = base.TeamManager.GetTeamPlayers(goals[i].Team);
			if (teamPlayers.Length > 0 && goals[i].owner != teamPlayers[0])
			{
				goals[i].photonView.TransferOwnership(teamPlayers[0]);
			}
		}
	}

	private void AuthorityResetBall(GameTeam servingTeam)
	{
		Transform teamBallSpawnPoint = GetTeamBallSpawnPoint(servingTeam);
		if (teamBallSpawnPoint != null)
		{
			ball.AuthorityResetToDefault(teamBallSpawnPoint.position, teamBallSpawnPoint.rotation);
		}
		else
		{
			ball.AuthorityResetToLastSpawnPosition();
		}
	}

	private void AuthorityHideBall(GameTeam servingTeam)
	{
		Transform teamBallSpawnPoint = GetTeamBallSpawnPoint(servingTeam);
		if (teamBallSpawnPoint != null)
		{
			ball.AuthorityResetToDefault(teamBallSpawnPoint.position + Vector3.down * 20f, teamBallSpawnPoint.rotation);
		}
		else
		{
			AuthorityResetBall(servingTeam);
		}
	}

	private Transform GetTeamBallSpawnPoint(GameTeam team)
	{
		Transform transform = null;
		for (int i = 0; i < ballSpawnPoints.Length; i++)
		{
			if (ballSpawnPoints[i].Team == team)
			{
				transform = ballSpawnPoints[i].transform;
				break;
			}
		}
		if (transform == null)
		{
			Debug.LogError("Missing ball spawn point for team " + team);
		}
		return transform;
	}

	private void OnEnterRoundRunningState(ushort previousStateId, ushort previousSubStateId)
	{
		GameTeam servingTeam = ServingTeam;
		if (PhotonNetwork.isMasterClient)
		{
			MasterTransferGoalAuthority();
		}
		ball.PlayerInteractionRestriction.ForceRestricted = false;
		AuthorityResetBall(servingTeam);
		ball.ClearTrails();
		GameTeam winningTeam;
		string subtitleText = ((base.StatsManager.GetWinningScore(out winningTeam) != scoreToWin - 1) ? null : "Match Point!");
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, base.TeamManager.GetTeamName(ServingTeam) + "'s Serve!", subtitleText, 3f);
	}

	private void OnEnterBallScoredState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			masterPaddleballStateTimer.StartTimer(ballScoredStateDuration);
		}
		AuthorityHideBall(ServingTeam);
		ball.PlayerInteractionRestriction.ForceRestricted = true;
		ball.ClearTrails();
	}

	private void OnUpdateBallScoredState()
	{
		if (PhotonNetwork.isMasterClient && masterPaddleballStateTimer.TimerOver)
		{
			GameTeam winningTeam = GameTeam.INVALID;
			if (base.StatsManager.GetWinningScore(out winningTeam) >= scoreToWin)
			{
				MasterStopGame();
			}
			else
			{
				paddleballStateMachine.EnterState(0);
			}
		}
	}

	[PunRPC]
	private void RpcMasterRequestScore(int goalPhotonViewId, Vector3 scorePoint)
	{
		PhotonView photonView = PhotonView.Find(goalPhotonViewId);
		PaddleballGoal paddleballGoal = ((!(photonView != null)) ? null : photonView.GetComponent<PaddleballGoal>());
		if (paddleballGoal != null)
		{
			MasterRequestScore(paddleballGoal, scorePoint);
		}
	}

	[PunRPC]
	private void RpcOnScore(int goalPhotonViewId, Vector3 scorePoint)
	{
		PhotonView photonView = PhotonView.Find(goalPhotonViewId);
		PaddleballGoal paddleballGoal = ((!(photonView != null)) ? null : photonView.GetComponent<PaddleballGoal>());
		if (paddleballGoal != null)
		{
			OnScore(paddleballGoal, scorePoint);
		}
	}
}
