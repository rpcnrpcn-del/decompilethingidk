using System;
using System.Collections.Generic;
using UnityEngine;

public class DodgeballManager : GameManager
{
	private enum DodgeballGameStates
	{
		ROUND_START = 0,
		ROUND_RUNNING = 1,
		ROUND_OVER = 2,
		INVALID = 65535
	}

	private enum DodgeballSpawnPointTags
	{
		Court = 0,
		Sidelines = 1
	}

	private enum DodgeballTeleportRegionTags
	{
		Court = 0,
		Sidelines = 1
	}

	private class PlayerData
	{
		private SynchronizedField<bool> _onSidelines;

		public bool OnSidelines
		{
			get
			{
				return _onSidelines.Get();
			}
			set
			{
				_onSidelines.ForceSet(value);
			}
		}

		public PlayerData(PhotonPlayer player)
		{
			_onSidelines = new SynchronizedField<bool>(player, "ON_SIDELINES", false, SetterPermissionMode.ANYONE);
		}
	}

	[Header("Rounds")]
	[SerializeField]
	private int maxRounds = 5;

	private SynchronizedStateMachine dodgeballStateMachine;

	private SynchronizedTimer masterDodgeballStateTimer;

	private Dodgeball[] balls;

	private const float ROUND_NOTIFICATION_DURATION = 3f;

	private const float DEATH_NOTIFICATION_DURATION = 3f;

	private const float HIT_NOTIFICATION_OFFSET_DISTANCE = 0.35f;

	private Dictionary<int, PlayerData> playerData = new Dictionary<int, PlayerData>();

	private int ScoreToWin
	{
		get
		{
			return maxRounds / 2 + 1;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		dodgeballStateMachine = new SynchronizedStateMachine("DODGEBALL", SetterPermissionMode.MASTER);
		dodgeballStateMachine.AddState(ushort.MaxValue, null, null, null);
		dodgeballStateMachine.AddState(0, OnEnterRoundStartState, null, OnUpdateRoundStartState);
		dodgeballStateMachine.AddState(1, OnEnterRoundRunningState, null, OnUpdateRoundRunningState);
		dodgeballStateMachine.AddState(2, OnEnterRoundOverState, OnExitRoundOverState, OnUpdateRoundOverState);
		masterDodgeballStateTimer = new SynchronizedTimer(this, "DODGEBALL_TIMER", SetterPermissionMode.MASTER);
		balls = UnityEngine.Object.FindObjectsOfType<Dodgeball>();
		for (int i = 0; i < balls.Length; i++)
		{
			Dodgeball obj = balls[i];
			obj.DodgeballCatchEvent = (Dodgeball.DodgeballCatch)Delegate.Combine(obj.DodgeballCatchEvent, new Dodgeball.DodgeballCatch(OnDodgeballCatch));
			Dodgeball obj2 = balls[i];
			obj2.DodgeballPlayerOutEvent = (Dodgeball.DodgeballHitPlayer)Delegate.Combine(obj2.DodgeballPlayerOutEvent, new Dodgeball.DodgeballHitPlayer(OnDodgeballPlayerOut));
			Dodgeball obj3 = balls[i];
			obj3.DodgeballPlayerHitEvent = (Dodgeball.DodgeballHitPlayer)Delegate.Combine(obj3.DodgeballPlayerHitEvent, new Dodgeball.DodgeballHitPlayer(OnDodgeballPlayerHit));
			balls[i].SupportsKillzones = true;
		}
	}

	protected override void Initialize()
	{
		base.Initialize();
		dodgeballStateMachine.Initialize(this, ushort.MaxValue);
	}

	protected override void Update()
	{
		base.Update();
		dodgeballStateMachine.Update();
		masterDodgeballStateTimer.UpdateCallbacks();
	}

	protected override void ResetScene()
	{
		base.ResetScene();
		if (PhotonNetwork.isMasterClient)
		{
			MasterResetAllBalls();
		}
	}

	protected override void OnGameStart()
	{
		if (PhotonNetwork.isMasterClient)
		{
			dodgeballStateMachine.EnterState(0);
		}
		base.PresenceSettings.CurrentPresenceString = "Best " + ScoreToWin + " of " + maxRounds + " Rounds";
	}

	protected override void OnGameEnd()
	{
		if (PhotonNetwork.isMasterClient)
		{
			dodgeballStateMachine.EnterState(ushort.MaxValue);
		}
	}

	protected override void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
		base.InitializeLocalPlayer(localPlayerIsSpectator);
		if (!localPlayerIsSpectator)
		{
			LocalPlayerSetOnSidelines(false);
		}
	}

	protected override void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
		base.ResetLocalPlayer(localPlayerIsSpectator);
		if (!localPlayerIsSpectator)
		{
			LocalPlayerSetOnSidelines(false);
		}
	}

	private void OnEnterRoundStartState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			MasterDisableAllBalls();
			masterDodgeballStateTimer.StartTimer(5f);
		}
		masterDodgeballStateTimer.AddTimeRemainingCallback(3f, PlayRoundStartEffect);
		if (!base.TeamManager.IsPlayerSpectator(PhotonNetwork.player))
		{
			Player.LocalPlayer.PlayerLocomotion.TeleportEnabled = false;
		}
	}

	private void OnUpdateRoundStartState()
	{
		if (PhotonNetwork.isMasterClient && masterDodgeballStateTimer.TimerOver)
		{
			dodgeballStateMachine.EnterState(1);
		}
	}

	private void PlayRoundStartEffect()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "Round Starting", 3f);
	}

	private void OnEnterRoundRunningState(ushort previousStateId, ushort previousSubStateId)
	{
		if (!base.TeamManager.IsPlayerSpectator(PhotonNetwork.player))
		{
			Player.LocalPlayer.PlayerLocomotion.TeleportEnabled = true;
		}
		if (PhotonNetwork.isMasterClient)
		{
			MasterResetAllBalls();
		}
		base.FxManager.PlayFX(FxType.ROUND_START);
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Go!", 2f);
	}

	private void OnUpdateRoundRunningState()
	{
		if (!PhotonNetwork.isMasterClient)
		{
			return;
		}
		GameTeam[] allTeams = base.TeamManager.GetAllTeams();
		for (int i = 0; i < allTeams.Length; i++)
		{
			if (IsEntireTeamOnSidelines(allTeams[i]))
			{
				GameTeam anotherTeam = base.TeamManager.GetAnotherTeam(allTeams[i]);
				base.StatsManager.AddScore(anotherTeam, 1);
				dodgeballStateMachine.EnterState(2);
			}
		}
	}

	private void OnEnterRoundOverState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			masterDodgeballStateTimer.StartTimer(base.CombatManager.RespawnDuration + 3f);
		}
		masterDodgeballStateTimer.AddTimeElapsedCallback(base.CombatManager.RespawnDuration, PlayRoundOverEffect);
	}

	private void OnExitRoundOverState(ushort nextStateId, ushort nextSubStateId)
	{
		if (nextStateId == 0 && !base.TeamManager.IsPlayerSpectator(PhotonNetwork.player))
		{
			LocalPlayerSetOnSidelines(false);
			base.SpawnManager.LocalPlayerRequestRespawn();
		}
	}

	private void OnUpdateRoundOverState()
	{
		if (PhotonNetwork.isMasterClient && masterDodgeballStateTimer.TimerOver)
		{
			GameTeam winningTeam;
			if (base.StatsManager.GetWinningScore(out winningTeam) >= ScoreToWin)
			{
				MasterStopGame();
			}
			else
			{
				dodgeballStateMachine.EnterState(0);
			}
		}
	}

	private void PlayRoundOverEffect()
	{
		base.FxManager.PlayFX(FxType.ROUND_OVER);
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Round Over!", 3f);
	}

	private void OnDodgeballPlayerOut(Dodgeball dodgeball, Player thrower, Player outPlayer, Vector3 hitPoint)
	{
		if (dodgeball.Owner.isLocal && IsValidHit(thrower.PhotonPlayer, outPlayer.PhotonPlayer))
		{
			base.photonView.RPC("RpcMasterRequestPlayerOut", PhotonTargets.MasterClient, thrower.PhotonPlayer, outPlayer.PhotonPlayer, hitPoint);
		}
	}

	private void OnDodgeballPlayerHit(Dodgeball dodgeball, Player thrower, Player hitPlayer, Vector3 hitPoint)
	{
		if (dodgeball.Owner.isLocal && IsValidHit(thrower.PhotonPlayer, hitPlayer.PhotonPlayer))
		{
			base.photonView.RPC("RpcOnPlayerHit", PhotonTargets.All, thrower.PhotonPlayer, hitPlayer.PhotonPlayer, hitPoint);
		}
	}

	private void MasterRequestPlayerOut(PhotonPlayer thrower, PhotonPlayer outPlayer, Vector3 outPoint)
	{
		if (PhotonNetwork.isMasterClient && IsValidHit(thrower, outPlayer))
		{
			base.CombatManager.MasterSetPlayerHealth(outPlayer, 0);
			base.photonView.RPC("RpcOnPlayerOut", PhotonTargets.All, thrower, outPlayer, outPoint);
		}
	}

	private bool IsValidHit(PhotonPlayer thrower, PhotonPlayer outPlayer)
	{
		return dodgeballStateMachine.CurrentStateId == 1 && base.CombatManager.PlayerIsAlive(outPlayer) && !IsPlayerOnSidelines(thrower) && !IsPlayerOnSidelines(outPlayer) && !base.TeamManager.PlayersAreTeammates(thrower, outPlayer);
	}

	private void OnPlayerOut(PhotonPlayer thrower, PhotonPlayer outPlayer, Vector3 outPoint)
	{
		if (outPlayer.isLocal)
		{
			LocalPlayerSetOnSidelines(true);
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "You're Out!", "Hit by " + thrower.name, 3f);
			base.FxManager.PlayFX(FxType.PLAYER_OUT);
		}
		else if (thrower.isLocal)
		{
			Vector3 vector = 0.35f * ((Player.LocalPlayer.Head.transform.position - outPoint).normalized + Vector3.up);
			MenuNotification.PlayNext("Out!", 3f, null, false, outPoint + vector);
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "You Hit " + outPlayer.name, 3f);
			base.FxManager.PlayFX(FxType.PLAYER_HIT);
			Player.LocalPlayer.PlayerEvents.DodgeballOut();
		}
	}

	private void OnPlayerHit(PhotonPlayer thrower, PhotonPlayer hitPlayer, Vector3 hitPoint)
	{
		base.FxManager.PlayFX(FxType.PLAYER_DAMAGED, hitPoint);
	}

	private void OnDodgeballCatch(Dodgeball dodgeball, Player catcher, Player thrower, Vector3 catchPoint)
	{
		if (catcher.PhotonPlayer.isLocal && IsValidCatch(catcher.PhotonPlayer, thrower.PhotonPlayer))
		{
			base.photonView.RPC("RpcMasterRequestPlayerCatch", PhotonTargets.MasterClient, catcher.PhotonPlayer, thrower.PhotonPlayer, catchPoint);
		}
	}

	private void MasterRequestPlayerCatch(PhotonPlayer catcher, PhotonPlayer thrower, Vector3 catchPoint)
	{
		if (PhotonNetwork.isMasterClient && IsValidCatch(catcher, thrower))
		{
			base.photonView.RPC("RpcOnPlayerCatch", PhotonTargets.All, catcher, thrower, catchPoint);
		}
	}

	private bool IsValidCatch(PhotonPlayer catcher, PhotonPlayer thrower)
	{
		return dodgeballStateMachine.CurrentStateId == 1 && !IsPlayerOnSidelines(catcher) && !IsPlayerOnSidelines(thrower) && !base.TeamManager.PlayersAreTeammates(catcher, thrower);
	}

	private void OnPlayerCatch(PhotonPlayer catcher, PhotonPlayer thrower, Vector3 catchPoint)
	{
		Vector3 vector = 0.35f * ((Player.LocalPlayer.Head.transform.position - catchPoint).normalized + Vector3.up);
		MenuNotification.PlayNext("Catch!", 3f, null, false, catchPoint + vector);
	}

	private bool IsEntireTeamOnSidelines(GameTeam team)
	{
		PhotonPlayer[] teamPlayers = base.TeamManager.GetTeamPlayers(team);
		for (int i = 0; i < teamPlayers.Length; i++)
		{
			if (!IsPlayerOnSidelines(teamPlayers[i]))
			{
				return false;
			}
		}
		return true;
	}

	private PlayerData GetPlayerData(PhotonPlayer player)
	{
		PlayerData value;
		if (!playerData.TryGetValue(player.ID, out value))
		{
			value = new PlayerData(player);
			playerData[player.ID] = value;
		}
		return value;
	}

	private bool IsPlayerOnSidelines(PhotonPlayer player)
	{
		return GetPlayerData(player).OnSidelines;
	}

	private void LocalPlayerSetOnSidelines(bool isOnSidelines)
	{
		GetPlayerData(PhotonNetwork.player).OnSidelines = isOnSidelines;
		SpawnPointTagType localPlayerSpawnPointTag = SpawnPointTagType.TAG_1;
		TeleportRegionTagType localPlayerTeleportRegionTag = TeleportRegionTagType.TAG_1;
		if (isOnSidelines)
		{
			localPlayerSpawnPointTag = SpawnPointTagType.TAG_2;
			localPlayerTeleportRegionTag = TeleportRegionTagType.TAG_2;
		}
		base.SpawnManager.SetLocalPlayerSpawnPointTag(localPlayerSpawnPointTag);
		base.TeleportManager.SetLocalPlayerTeleportRegionTag(localPlayerTeleportRegionTag);
	}

	private void MasterResetAllBalls()
	{
		if (PhotonNetwork.isMasterClient)
		{
			for (int i = 0; i < balls.Length; i++)
			{
				balls[i].MasterResetToLastSpawnPosition();
			}
		}
	}

	private void MasterDisableAllBalls()
	{
		if (PhotonNetwork.isMasterClient)
		{
			for (int i = 0; i < balls.Length; i++)
			{
				balls[i].Disable();
			}
		}
	}

	[PunRPC]
	private void RpcMasterRequestPlayerOut(PhotonPlayer thrower, PhotonPlayer outPlayer, Vector3 outPoint)
	{
		MasterRequestPlayerOut(thrower, outPlayer, outPoint);
	}

	[PunRPC]
	private void RpcOnPlayerHit(PhotonPlayer thrower, PhotonPlayer hitPlayer, Vector3 hitPoint)
	{
		OnPlayerHit(thrower, hitPlayer, hitPoint);
	}

	[PunRPC]
	private void RpcOnPlayerOut(PhotonPlayer thrower, PhotonPlayer outPlayer, Vector3 outPoint)
	{
		OnPlayerOut(thrower, outPlayer, outPoint);
	}

	[PunRPC]
	private void RpcMasterRequestPlayerCatch(PhotonPlayer catcher, PhotonPlayer thrower, Vector3 catchPoint)
	{
		MasterRequestPlayerCatch(catcher, thrower, catchPoint);
	}

	[PunRPC]
	private void RpcOnPlayerCatch(PhotonPlayer catcher, PhotonPlayer thrower, Vector3 catchPoint)
	{
		OnPlayerCatch(catcher, thrower, catchPoint);
	}
}
