using System;
using System.Collections.Generic;
using RecNet;
using UnityEngine;

public class DiscGolfManager : GameManager
{
	private enum DiscGolfGameStates
	{
		HOLE_RUNNING = 0,
		HOLE_FINISHED = 1,
		INVALID = 65535
	}

	private class TeamData
	{
		private SynchronizedField<int> _stroke;

		private SynchronizedField<Vector3> _anchor;

		private SynchronizedField<bool> _holeFinished;

		private SynchronizedField<bool> _powerupUsed;

		public int Stroke
		{
			get
			{
				return _stroke.Get();
			}
			set
			{
				_stroke.ForceSet(value);
			}
		}

		public Vector3 Anchor
		{
			get
			{
				return _anchor.Get();
			}
			set
			{
				_anchor.ForceSet(value);
			}
		}

		public bool HoleFinished
		{
			get
			{
				return _holeFinished.Get();
			}
			set
			{
				_holeFinished.ForceSet(value);
			}
		}

		public bool PowerupUsed
		{
			get
			{
				return _powerupUsed.Get();
			}
			set
			{
				_powerupUsed.ForceSet(value);
			}
		}

		public TeamData(GameTeam team, DiscGolfManager manager)
		{
			_stroke = new SynchronizedField<int>(manager, GetTeamKey(team, "HOLE_STROKE"), 0, SetterPermissionMode.ANYONE);
			_anchor = new SynchronizedField<Vector3>(manager, GetTeamKey(team, "ANCHOR"), Vector3.zero, SetterPermissionMode.ANYONE);
			_holeFinished = new SynchronizedField<bool>(manager, GetTeamKey(team, "HOLE_FINISHED"), false, SetterPermissionMode.ANYONE);
			_powerupUsed = new SynchronizedField<bool>(manager, GetTeamKey(team, "HOLE_USED_POWERUP"), false, SetterPermissionMode.ANYONE);
		}

		private static string GetTeamKey(GameTeam team, string subkey)
		{
			return string.Format("{0}.{1}", (int)team, subkey);
		}
	}

	[Header("Disc Golf")]
	[SerializeField]
	private float strokeIncrementThreshold = 3f;

	[SerializeField]
	private float mercyRuleHoleScore = 7f;

	[Header("Holes")]
	[SerializeField]
	private DiscGolfHole firstHole;

	[SerializeField]
	private DiscGolfHole lastHole = DiscGolfHole.HOLE_9;

	[Header("Score Notifications")]
	[SerializeField]
	private string scoreHoleInOne = "Hole In One!";

	[SerializeField]
	private string scoreLessThanNeg2 = "Albatross";

	[SerializeField]
	private string scoreNeg2 = "Eagle";

	[SerializeField]
	private string scoreNeg1 = "Birdie";

	[SerializeField]
	private string scoreZero = "Par";

	[SerializeField]
	private string scorePos1 = "Bogey";

	[SerializeField]
	private string scorePos2 = "Double Bogey";

	[SerializeField]
	private string scorePos3 = "Triple Bogey";

	private const float HOLE_FINISHED_STATE_DURATION = 6f;

	private const int DEFAULT_HOLE_PAR = 3;

	private SynchronizedStateMachine discGolfStateMachine;

	private SynchronizedTimer masterDiscGolfStateTimer;

	private List<DiscGolfDisc> gameplayDiscs = new List<DiscGolfDisc>();

	private DiscGolfGoal[] goals;

	private DiscGolfTee[] tees;

	private DiscGolfPowerUp[] powerups;

	private SynchronizedField<int> _currentHoleId;

	private SynchronizedField<int> _firstHoleId;

	private SynchronizedField<int> _lastHoleId;

	private Dictionary<GameTeam, TeamData> teamData = new Dictionary<GameTeam, TeamData>();

	private DiscGolfHole CurrentHole
	{
		get
		{
			return (DiscGolfHole)_currentHoleId.Get();
		}
		set
		{
			_currentHoleId.ForceSet((int)value);
		}
	}

	private int CurrentHoleNumber
	{
		get
		{
			return (int)(CurrentHole + 1);
		}
	}

	private DiscGolfHole FirstHole
	{
		get
		{
			return (DiscGolfHole)_firstHoleId.Get();
		}
		set
		{
			_firstHoleId.ForceSet((int)value);
		}
	}

	private DiscGolfHole LastHole
	{
		get
		{
			return (DiscGolfHole)_lastHoleId.Get();
		}
		set
		{
			_lastHoleId.ForceSet((int)value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_currentHoleId = new SynchronizedField<int>(this, "CURRENT_HOLE_ID", -1, SetterPermissionMode.MASTER);
		_firstHoleId = new SynchronizedField<int>(this, "FIRST_HOLE_ID", -1, SetterPermissionMode.MASTER);
		_lastHoleId = new SynchronizedField<int>(this, "LAST_HOLE_ID", -1, SetterPermissionMode.MASTER);
		discGolfStateMachine = new SynchronizedStateMachine("DISC_GOLF", SetterPermissionMode.MASTER);
		discGolfStateMachine.AddState(ushort.MaxValue, null, null, null);
		discGolfStateMachine.AddState(0, OnEnterHoleRunningState, null, OnUpdateHoleRunningState);
		discGolfStateMachine.AddState(1, OnEnterHoleFinishedState, null, OnUpdateHoleFinishedState);
		masterDiscGolfStateTimer = new SynchronizedTimer(this, "DISC_GOLF_TIMER", SetterPermissionMode.MASTER);
		goals = UnityEngine.Object.FindObjectsOfType<DiscGolfGoal>();
		tees = UnityEngine.Object.FindObjectsOfType<DiscGolfTee>();
		DiscGolfDisc[] array = UnityEngine.Object.FindObjectsOfType<DiscGolfDisc>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SuppressesTeleport = true;
			array[i].SupportsKillzones = true;
			if (array[i].GetComponent<GamePracticeObject>() == null)
			{
				DiscGolfDisc discGolfDisc = array[i];
				discGolfDisc.DiscPickupEvent = (DiscGolfDisc.DiscPickup)Delegate.Combine(discGolfDisc.DiscPickupEvent, new DiscGolfDisc.DiscPickup(OnDiscPickup));
				discGolfDisc.DiscScoreEvent = (DiscGolfDisc.DiscScore)Delegate.Combine(discGolfDisc.DiscScoreEvent, new DiscGolfDisc.DiscScore(OnDiscScore));
				discGolfDisc.DiscHazardEvent = (DiscGolfDisc.DiscHazard)Delegate.Combine(discGolfDisc.DiscHazardEvent, new DiscGolfDisc.DiscHazard(OnDiscHazard));
				discGolfDisc.DiscStopMovingEvent += OnDiscStopMoving;
				gameplayDiscs.Add(discGolfDisc);
			}
		}
		powerups = UnityEngine.Object.FindObjectsOfType<DiscGolfPowerUp>();
		for (int j = 0; j < powerups.Length; j++)
		{
			DiscGolfPowerUp obj = powerups[j];
			obj.PickupEvent = (DiscGolfPowerUp.Pickup)Delegate.Combine(obj.PickupEvent, new DiscGolfPowerUp.Pickup(OnPowerupPickup));
		}
	}

	protected override void Initialize()
	{
		base.Initialize();
		discGolfStateMachine.Initialize(this, ushort.MaxValue);
	}

	protected override void Update()
	{
		base.Update();
		discGolfStateMachine.Update();
		masterDiscGolfStateTimer.UpdateCallbacks();
	}

	protected override void OnGameStart()
	{
		if (PhotonNetwork.isMasterClient)
		{
			FirstHole = firstHole;
			LastHole = lastHole;
			CurrentHole = firstHole;
			discGolfStateMachine.EnterState(0);
		}
	}

	protected override void OnGameEnd()
	{
		if (PhotonNetwork.isMasterClient)
		{
			discGolfStateMachine.EnterState(ushort.MaxValue);
		}
	}

	protected override void ResetScene()
	{
		base.ResetScene();
		if (PhotonNetwork.isMasterClient)
		{
			MasterDisableGameplayDiscs();
		}
		UpdateHelpBeams();
	}

	private void OnDiscScore(DiscGolfDisc disc, DiscGolfGoal goal, Player thrower)
	{
		if (thrower.PhotonPlayer.isLocal && IsValidScore(disc, goal, thrower.PhotonPlayer))
		{
			base.photonView.RPC("RpcMasterFinishHole", PhotonTargets.MasterClient, disc.photonView.viewID, goal.photonView.viewID, thrower.PhotonPlayer, false);
		}
	}

	private void OnDiscPickup(DiscGolfDisc disc, Player pickupPlayer)
	{
		if (pickupPlayer.PhotonPlayer.isLocal && IsValidThrow(disc, pickupPlayer.PhotonPlayer))
		{
			IncrementTeamStroke(disc, pickupPlayer);
			int teamStroke = GetTeamStroke(disc.Team);
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "Stroke " + teamStroke, 1.5f);
		}
	}

	private void OnDiscHazard(DiscGolfDisc disc, Killzone hazard, Player throwPlayer)
	{
		if (throwPlayer.PhotonPlayer.isLocal && IsValidThrow(disc, throwPlayer.PhotonPlayer))
		{
			IncrementTeamStroke(disc, throwPlayer, false);
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Hazard", 1.5f);
		}
	}

	private void OnDiscStopMoving(DiscGolfDisc disc, Player throwPlayer)
	{
		if (throwPlayer.PhotonPlayer.isLocal && IsValidThrow(disc, throwPlayer.PhotonPlayer))
		{
			IncrementTeamStroke(disc, throwPlayer);
		}
	}

	private void MasterFinishHole(DiscGolfDisc disc, DiscGolfGoal goal, PhotonPlayer thrower, bool forfeit)
	{
		if (PhotonNetwork.isMasterClient && IsValidScore(disc, goal, thrower))
		{
			disc.Disable();
			SetTeamHoleFinished(disc.Team, true);
			int teamStroke = GetTeamStroke(disc.Team);
			int deltaScore = teamStroke - goal.Par;
			base.StatsManager.AddScore(base.TeamManager.GetPlayerTeam(thrower), deltaScore);
			base.photonView.RPC("RpcOnFinishHole", PhotonTargets.All, goal.photonView.viewID, thrower, forfeit, teamStroke);
		}
	}

	private void OnFinishHole(DiscGolfGoal goal, PhotonPlayer thrower, bool forfeit, int throwerHoleStrokeCount)
	{
		int score = GetScore(goal.Par, throwerHoleStrokeCount);
		string strokeCountString = GetStrokeCountString(throwerHoleStrokeCount, score);
		GameTeam playerTeam = base.TeamManager.GetPlayerTeam(thrower);
		ScreenSpaceNotificationManager.NotificationType style = ((!thrower.isLocal) ? ScreenSpaceNotificationManager.NotificationType.Minor : ScreenSpaceNotificationManager.NotificationType.Medium);
		if (forfeit)
		{
			ScreenSpaceNotificationManager.Instance.Play(style, "Mercy Rule: " + base.TeamManager.GetTeamName(playerTeam), strokeCountString, 1.5f);
		}
		else
		{
			ScreenSpaceNotificationManager.Instance.Play(style, base.TeamManager.GetTeamName(playerTeam) + " Scores!", strokeCountString, 1.5f);
			base.FxManager.PlayFX(FxType.GOAL, goal.transform.position, goal.transform.rotation);
		}
		if (thrower.isLocal)
		{
			Player.LocalPlayer.PlayerEvents.DiscGolfCompletedHole(score);
			Profiles.LocalAddScore(RecRoomSceneManager.CurrentSceneName, CurrentHole.ToString(), score, "Par " + goal.Par, base.StatsManager.GetScore(playerTeam));
		}
	}

	private bool IsValidScore(DiscGolfDisc disc, DiscGolfGoal goal, PhotonPlayer thrower)
	{
		return discGolfStateMachine.CurrentStateId == 0 && CurrentHole == goal.Hole && !base.TeamManager.IsPlayerSpectator(thrower) && !GetTeamHoleFinished(disc.Team);
	}

	private bool IsValidThrow(DiscGolfDisc disc, PhotonPlayer player)
	{
		return discGolfStateMachine.CurrentStateId == 0 && !base.TeamManager.IsPlayerSpectator(player) && !GetTeamHoleFinished(disc.Team);
	}

	private void IncrementTeamStroke(DiscGolfDisc disc, Player thrower, bool checkDistance = true)
	{
		Vector3 vector = Vector3.ProjectOnPlane(disc.transform.position - GetTeamAnchorPoint(disc.Team), Vector3.up);
		if (checkDistance && !(vector.magnitude >= strokeIncrementThreshold))
		{
			return;
		}
		AddTeamStroke(disc.Team, 1);
		SetTeamAnchorPoint(disc.Team, disc.transform.position);
		int teamStroke = GetTeamStroke(disc.Team);
		DiscGolfGoal currentGoal = GetCurrentGoal();
		if (currentGoal != null)
		{
			int score = GetScore(GetCurrentGoal().Par, teamStroke);
			if ((float)score >= mercyRuleHoleScore)
			{
				base.photonView.RPC("RpcMasterFinishHole", PhotonTargets.MasterClient, disc.photonView.viewID, currentGoal.photonView.viewID, thrower.PhotonPlayer, true);
			}
		}
	}

	private DiscGolfDisc GetTeamDisc(GameTeam team)
	{
		for (int i = 0; i < gameplayDiscs.Count; i++)
		{
			if (gameplayDiscs[i].Team == team)
			{
				return gameplayDiscs[i];
			}
		}
		return null;
	}

	private void MasterDisableGameplayDiscs()
	{
		if (PhotonNetwork.isMasterClient)
		{
			for (int i = 0; i < gameplayDiscs.Count; i++)
			{
				gameplayDiscs[i].Disable();
			}
		}
	}

	private void OnPowerupPickup(DiscGolfPowerUp powerup, DiscGolfDisc disc, Player discThrower)
	{
		if (discThrower.PhotonPlayer.isLocal && IsValidPowerup(disc, powerup, discThrower.PhotonPlayer))
		{
			base.photonView.RPC("RpcMasterRequestPowerup", PhotonTargets.MasterClient, disc.photonView.viewID, powerup.photonView.viewID, discThrower.PhotonPlayer);
		}
	}

	private void MasterRequestPowerup(DiscGolfDisc disc, DiscGolfPowerUp powerup, PhotonPlayer discThrower)
	{
		if (PhotonNetwork.isMasterClient && IsValidPowerup(disc, powerup, discThrower))
		{
			SetTeamPowerupUsed(disc.Team, true);
			AddTeamStroke(disc.Team, powerup.ScoreAdjust);
			base.photonView.RPC("RpcOnPowerup", PhotonTargets.All, powerup.photonView.viewID, discThrower);
		}
	}

	private void OnPowerup(DiscGolfPowerUp powerup, PhotonPlayer discThrower)
	{
		base.FxManager.PlayFX(FxType.POWERUP_PICKUP, powerup.transform.position, powerup.transform.rotation);
		powerup.OnPickup();
		if (discThrower.isLocal)
		{
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Bonus!", "Stroke " + powerup.ScoreAdjust, 1.5f);
		}
	}

	private bool IsValidPowerup(DiscGolfDisc disc, DiscGolfPowerUp powerup, PhotonPlayer discThrower)
	{
		return discGolfStateMachine.CurrentStateId == 0 && CurrentHole == powerup.Hole && !base.TeamManager.IsPlayerSpectator(discThrower) && !GetTeamHoleFinished(disc.Team) && !GetTeamPowerupUsed(disc.Team);
	}

	private DiscGolfGoal GetCurrentGoal()
	{
		DiscGolfHole currentHole = CurrentHole;
		for (int i = 0; i < goals.Length; i++)
		{
			if (goals[i].Hole == currentHole)
			{
				return goals[i];
			}
		}
		return null;
	}

	private DiscGolfTee GetTee(GameTeam team)
	{
		DiscGolfHole currentHole = CurrentHole;
		for (int i = 0; i < tees.Length; i++)
		{
			if (tees[i].Hole == currentHole && tees[i].Team == team)
			{
				return tees[i];
			}
		}
		return null;
	}

	private void UpdateHelpBeams()
	{
		DiscGolfHole currentHole = CurrentHole;
		bool flag = discGolfStateMachine.CurrentStateId == 0;
		for (int i = 0; i < goals.Length; i++)
		{
			goals[i].BeamEnabled = flag && goals[i].Hole == currentHole;
		}
		if (!base.TeamManager.IsPlayerSpectator(PhotonNetwork.player))
		{
			GameTeam playerTeam = base.TeamManager.GetPlayerTeam(PhotonNetwork.player);
			for (int j = 0; j < gameplayDiscs.Count; j++)
			{
				gameplayDiscs[j].BeamEnabled = flag && gameplayDiscs[j].Team == playerTeam;
			}
		}
	}

	private void OnEnterHoleRunningState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			MasterInitializeHole();
		}
		UpdateHelpBeams();
		DiscGolfGoal currentGoal = GetCurrentGoal();
		base.PresenceSettings.CurrentPresenceString = "Hole " + CurrentHoleNumber + " Par " + currentGoal.Par;
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Hole " + CurrentHoleNumber, "Par " + currentGoal.Par, 3f);
	}

	private void OnUpdateHoleRunningState()
	{
		if (PhotonNetwork.isMasterClient && IsHoleFinished())
		{
			discGolfStateMachine.EnterState(1);
		}
	}

	private bool IsHoleFinished()
	{
		GameTeam[] activeTeams = base.TeamManager.GetActiveTeams();
		for (int i = 0; i < activeTeams.Length; i++)
		{
			if (!GetTeamHoleFinished(activeTeams[i]))
			{
				return false;
			}
		}
		return true;
	}

	private void OnEnterHoleFinishedState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			masterDiscGolfStateTimer.StartTimer(6f);
		}
		masterDiscGolfStateTimer.AddTimeRemainingCallback(4.5f, PlayHoleCompletedFeedback);
	}

	private void OnUpdateHoleFinishedState()
	{
		if (PhotonNetwork.isMasterClient && masterDiscGolfStateTimer.TimerOver)
		{
			if (CurrentHole >= LastHole)
			{
				MasterStopGame();
				return;
			}
			CurrentHole++;
			discGolfStateMachine.EnterState(0);
		}
	}

	private void PlayHoleCompletedFeedback()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "Hole " + CurrentHoleNumber + " Completed!", 3f);
	}

	private TeamData GetTeamData(GameTeam team)
	{
		TeamData value;
		if (!teamData.TryGetValue(team, out value))
		{
			value = new TeamData(team, this);
			teamData[team] = value;
		}
		return value;
	}

	private int GetScore(int par, int strokeCount)
	{
		return strokeCount - par;
	}

	private int GetTeamStroke(GameTeam team)
	{
		return GetTeamData(team).Stroke;
	}

	private void SetTeamStroke(GameTeam team, int strokeCount)
	{
		GetTeamData(team).Stroke = strokeCount;
	}

	private void AddTeamStroke(GameTeam team, int deltaScore)
	{
		GetTeamData(team).Stroke += deltaScore;
	}

	private Vector3 GetTeamAnchorPoint(GameTeam team)
	{
		return GetTeamData(team).Anchor;
	}

	private void SetTeamAnchorPoint(GameTeam team, Vector3 anchorPoint)
	{
		GetTeamData(team).Anchor = anchorPoint;
	}

	private bool GetTeamHoleFinished(GameTeam team)
	{
		return GetTeamData(team).HoleFinished;
	}

	private void SetTeamHoleFinished(GameTeam team, bool isHoleFinished)
	{
		GetTeamData(team).HoleFinished = isHoleFinished;
	}

	private bool GetTeamPowerupUsed(GameTeam team)
	{
		return GetTeamData(team).PowerupUsed;
	}

	private void SetTeamPowerupUsed(GameTeam team, bool isPowerupUsed)
	{
		GetTeamData(team).PowerupUsed = isPowerupUsed;
	}

	private void MasterInitializeHole()
	{
		GameTeam[] activeTeams = base.TeamManager.GetActiveTeams();
		for (int i = 0; i < activeTeams.Length; i++)
		{
			DiscGolfTee tee = GetTee(activeTeams[i]);
			DiscGolfDisc teamDisc = GetTeamDisc(activeTeams[i]);
			if (tee == null || teamDisc == null)
			{
				Debug.LogError("Could not spawn " + GameTeamSettings.GetTeamName(activeTeams[i]) + " Team disc on Hole " + CurrentHoleNumber);
			}
			else
			{
				teamDisc.MasterResetToDefault(tee.transform.position, tee.transform.rotation);
				SetTeamAnchorPoint(activeTeams[i], tee.transform.position);
			}
			SetTeamHoleFinished(activeTeams[i], false);
			SetTeamStroke(activeTeams[i], 1);
			SetTeamPowerupUsed(activeTeams[i], false);
		}
	}

	private string GetStrokeCountString(int strokeCount, int score)
	{
		string empty = string.Empty;
		if (strokeCount == 1)
		{
			return scoreHoleInOne;
		}
		if (score < -2)
		{
			return scoreLessThanNeg2;
		}
		switch (score)
		{
		case -2:
			return scoreNeg2;
		case -1:
			return scoreNeg1;
		case 0:
			return scoreZero;
		case 1:
			return scorePos1;
		case 2:
			return scorePos2;
		case 3:
			return scorePos3;
		default:
			return "+" + score;
		}
	}

	[PunRPC]
	private void RpcMasterFinishHole(int discPhotonViewId, int goalPhotonViewId, PhotonPlayer thrower, bool forfeit)
	{
		PhotonView photonView = PhotonView.Find(discPhotonViewId);
		DiscGolfDisc discGolfDisc = ((!(photonView != null)) ? null : photonView.GetComponent<DiscGolfDisc>());
		PhotonView photonView2 = PhotonView.Find(goalPhotonViewId);
		DiscGolfGoal discGolfGoal = ((!(photonView2 != null)) ? null : photonView2.GetComponent<DiscGolfGoal>());
		if (discGolfDisc != null && discGolfGoal != null)
		{
			MasterFinishHole(discGolfDisc, discGolfGoal, thrower, forfeit);
		}
	}

	[PunRPC]
	private void RpcOnFinishHole(int goalPhotonViewId, PhotonPlayer thrower, bool forfeit, int throwerHoleStrokeCount)
	{
		PhotonView photonView = PhotonView.Find(goalPhotonViewId);
		DiscGolfGoal discGolfGoal = ((!(photonView != null)) ? null : photonView.GetComponent<DiscGolfGoal>());
		if (discGolfGoal != null)
		{
			OnFinishHole(discGolfGoal, thrower, forfeit, throwerHoleStrokeCount);
		}
	}

	[PunRPC]
	private void RpcMasterRequestPowerup(int discPhotonViewId, int powerupPhotonViewId, PhotonPlayer discThrower)
	{
		PhotonView photonView = PhotonView.Find(discPhotonViewId);
		DiscGolfDisc discGolfDisc = ((!(photonView != null)) ? null : photonView.GetComponent<DiscGolfDisc>());
		PhotonView photonView2 = PhotonView.Find(powerupPhotonViewId);
		DiscGolfPowerUp discGolfPowerUp = ((!(photonView2 != null)) ? null : photonView2.GetComponent<DiscGolfPowerUp>());
		if (discGolfDisc != null && discGolfPowerUp != null)
		{
			MasterRequestPowerup(discGolfDisc, discGolfPowerUp, discThrower);
		}
	}

	[PunRPC]
	private void RpcOnPowerup(int powerupPhotonViewId, PhotonPlayer discThrower)
	{
		PhotonView photonView = PhotonView.Find(powerupPhotonViewId);
		DiscGolfPowerUp discGolfPowerUp = ((!(photonView != null)) ? null : photonView.GetComponent<DiscGolfPowerUp>());
		if (discGolfPowerUp != null)
		{
			OnPowerup(discGolfPowerUp, discThrower);
		}
	}

	protected override void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		if (PhotonNetwork.isMasterClient && discGolfStateMachine.CurrentStateId == 0 && !base.TeamManager.IsPlayerSpectator(player))
		{
			GameTeam playerTeam = base.TeamManager.GetPlayerTeam(player);
			DiscGolfDisc teamDisc = GetTeamDisc(playerTeam);
			teamDisc.Disable();
		}
		base.OnPhotonPlayerDisconnected(player);
	}
}
