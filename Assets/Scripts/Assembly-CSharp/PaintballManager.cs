using System;
using UnityEngine;

public class PaintballManager : GameManager
{
	public enum PaintballMode
	{
		CaptureTheFlag = 0,
		TeamBattle = 1,
		None = 2
	}

	private enum PaintballGameStates
	{
		CTF_ROUND_RUNNING = 0,
		CTF_FLAG_CAPTURED = 1,
		TEAM_BATTLE_ROUND_RUNNING = 2,
		INVALID = 65535
	}

	private enum PaintballPlayerStats
	{
		HITS = 0,
		OUTS = 1,
		FLAG_CAPTURES = 2
	}

	[Header("Paintball")]
	[Header("Team Battle")]
	[SerializeField]
	private int teamBattleScoreToWin = 50;

	[Header("Capture The Flag")]
	[SerializeField]
	private int ctfScoreToWin = 3;

	[Header("Audio")]
	[SerializeField]
	private float audioFXMinInterval = 1.5f;

	private const float HIT_NOTIFICATION_OFFSET_DISTANCE = 0.35f;

	private const float DEATH_NOTIFICATION_DURATION = 3f;

	private const float CTF_CAPTURED_FLAG_STATE_DURATION = 3f;

	private SynchronizedStateMachine paintballStateMachine;

	private SynchronizedTimer masterPaintballStateTimer;

	private FlagTool[] flags;

	protected override void Awake()
	{
		base.Awake();
		paintballStateMachine = new SynchronizedStateMachine("PAINTBALL", SetterPermissionMode.MASTER);
		paintballStateMachine.AddState(ushort.MaxValue, null, null, null);
		paintballStateMachine.AddState(0, OnEnterCTFRoundRunningState, null, null);
		paintballStateMachine.AddState(1, OnEnterCTFFlagCapturedState, null, OnUpdateCTFFlagCapturedState);
		paintballStateMachine.AddState(2, OnEnterTeamBattleRoundRunningState, null, OnUpdateTeamBattleRoundRunningState);
		masterPaintballStateTimer = new SynchronizedTimer(this, "PAINTBALL_TIMER", SetterPermissionMode.MASTER);
		flags = UnityEngine.Object.FindObjectsOfType<FlagTool>();
		for (int i = 0; i < flags.Length; i++)
		{
			FlagTool obj = flags[i];
			obj.FlagAtGoalEvent = (FlagTool.FlagAtGoal)Delegate.Combine(obj.FlagAtGoalEvent, new FlagTool.FlagAtGoal(OnFlagAtGoal));
			FlagTool obj2 = flags[i];
			obj2.FlagPickupEvent = (FlagTool.FlagPickup)Delegate.Combine(obj2.FlagPickupEvent, new FlagTool.FlagPickup(OnFlagPickup));
			FlagTool obj3 = flags[i];
			obj3.FlagReleaseEvent = (FlagTool.FlagRelease)Delegate.Combine(obj3.FlagReleaseEvent, new FlagTool.FlagRelease(OnFlagRelease));
			FlagTool obj4 = flags[i];
			obj4.FlagResetEvent = (FlagTool.FlagReset)Delegate.Combine(obj4.FlagResetEvent, new FlagTool.FlagReset(OnFlagReset));
		}
		Weapon[] array = UnityEngine.Object.FindObjectsOfType<Weapon>();
		for (int j = 0; j < array.Length; j++)
		{
			array[j].PlayerImpactEvent += OnWeaponPlayerImpact;
		}
	}

	protected override void Initialize()
	{
		base.Initialize();
		paintballStateMachine.Initialize(this, ushort.MaxValue);
	}

	protected override void Update()
	{
		base.Update();
		paintballStateMachine.Update();
		masterPaintballStateTimer.UpdateCallbacks();
	}

	protected override void ResetScene()
	{
		if (PhotonNetwork.isMasterClient)
		{
			MasterDisableAllFlags();
		}
	}

	protected override void OnGameStart()
	{
		GameMode mode = base.ModeManager.GetMode();
		if (PhotonNetwork.isMasterClient)
		{
			switch (mode)
			{
			case GameMode.MODE_1:
				paintballStateMachine.EnterState(0);
				break;
			case GameMode.MODE_2:
				paintballStateMachine.EnterState(2);
				break;
			}
		}
		string modeName = base.ModeManager.GetModeName(mode);
		switch (mode)
		{
		case GameMode.MODE_1:
			base.PresenceSettings.CurrentPresenceString = modeName + " - First to " + ctfScoreToWin + " Flags";
			break;
		case GameMode.MODE_2:
			base.PresenceSettings.CurrentPresenceString = modeName + " - First to " + teamBattleScoreToWin + " Hits";
			break;
		}
	}

	protected override void OnGameEnd()
	{
		if (PhotonNetwork.isMasterClient)
		{
			paintballStateMachine.EnterState(ushort.MaxValue);
		}
	}

	protected override void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
		base.InitializeLocalPlayer(localPlayerIsSpectator);
		if (!localPlayerIsSpectator)
		{
			Player.LocalPlayer.SupportsOutOfBounds = true;
			Player.LocalPlayer.OutOfBoundsEvent += OnLocalPlayerOutOfBounds;
		}
	}

	protected override void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
		base.ResetLocalPlayer(localPlayerIsSpectator);
		if (!localPlayerIsSpectator)
		{
			Player.LocalPlayer.SupportsOutOfBounds = false;
			Player.LocalPlayer.OutOfBoundsEvent -= OnLocalPlayerOutOfBounds;
		}
	}

	private void OnWeaponPlayerImpact(Weapon weapon, Player shooter, Player hitPlayer, Player.BodyPart hitBodyPart, Vector3 impactPoint)
	{
		if ((shooter.PhotonPlayer.isLocal || hitPlayer.PhotonPlayer.isLocal) && IsValidPlayerHit(shooter.PhotonPlayer, hitPlayer.PhotonPlayer))
		{
			base.photonView.RPC("RpcMasterRequestPlayerHit", PhotonTargets.MasterClient, weapon.photonView.viewID, shooter.PhotonPlayer, hitPlayer.PhotonPlayer, (int)hitBodyPart, impactPoint);
		}
	}

	private void MasterRequestPlayerHit(Weapon weapon, PhotonPlayer shooter, PhotonPlayer hitPlayer, Player.BodyPart hitBodyPart, Vector3 impactPoint)
	{
		if (!PhotonNetwork.isMasterClient || !IsValidPlayerHit(shooter, hitPlayer))
		{
			return;
		}
		int playerImpactDamage = weapon.GetPlayerImpactDamage(hitBodyPart);
		base.CombatManager.MasterAddPlayerHealth(hitPlayer, -playerImpactDamage);
		if (!base.CombatManager.PlayerIsAlive(hitPlayer))
		{
			base.StatsManager.MasterAddPlayerStat(hitPlayer, PlayerStatType.STAT_2, 1);
			base.StatsManager.MasterAddPlayerStat(shooter, PlayerStatType.STAT_1, 1);
			if (paintballStateMachine.CurrentStateId == 2)
			{
				base.StatsManager.MasterAddScore(base.TeamManager.GetPlayerTeam(shooter), 1);
			}
			base.photonView.RPC("RpcOnPlayerHit", PhotonTargets.All, shooter, hitPlayer, hitBodyPart, impactPoint);
		}
	}

	private void OnPlayerHit(PhotonPlayer shooter, PhotonPlayer hitPlayer, Player.BodyPart hitBodyPart, Vector3 impactPoint)
	{
		base.FxManager.PlayFX(FxType.PLAYER_DAMAGED, impactPoint);
		if (hitPlayer.isLocal)
		{
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "You're Out!", "Hit by " + shooter.name, 3f);
			base.FxManager.PlayFX(FxType.PLAYER_OUT);
		}
		else if (shooter.isLocal)
		{
			Vector3 vector = 0.35f * Vector3.up;
			MenuNotification.PlayNext("Hit!", 3f, null, false, hitPlayer.ToPlayer().Head.transform.position + vector);
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "You Hit " + hitPlayer.name, 3f);
			base.FxManager.PlayFX(FxType.PLAYER_HIT);
			Player.LocalPlayer.PlayerEvents.PaintballHit();
		}
	}

	private bool IsValidPlayerHit(PhotonPlayer shooter, PhotonPlayer hitPlayer)
	{
		return paintballStateMachine.CurrentStateId != ushort.MaxValue && base.CombatManager.PlayerIsAlive(hitPlayer) && !base.TeamManager.IsPlayerSpectator(shooter) && !base.TeamManager.IsPlayerSpectator(hitPlayer) && !base.TeamManager.PlayersAreTeammates(shooter, hitPlayer);
	}

	private void OnFlagAtGoal(FlagTool flag, Player capturePlayer)
	{
		if (capturePlayer.PhotonPlayer.isLocal && IsValidFlagCapture(flag, capturePlayer.PhotonPlayer))
		{
			base.photonView.RPC("RpcMasterRequestFlagCapture", PhotonTargets.MasterClient, flag.photonView.viewID, capturePlayer.PhotonPlayer);
		}
	}

	private void OnFlagPickup(FlagTool flag, Player pickupPlayer)
	{
		if (pickupPlayer.PhotonPlayer.isLocal)
		{
			if (IsValidFlagReturn(flag, pickupPlayer.PhotonPlayer))
			{
				flag.AuthorityResetToLastSpawnPosition();
				base.photonView.RPC("RpcMasterRequestFlagReturn", PhotonTargets.MasterClient, flag.photonView.viewID, pickupPlayer.PhotonPlayer.ID);
			}
			else if (IsValidFlagPickup(flag, pickupPlayer.PhotonPlayer))
			{
				base.photonView.RPC("RpcMasterRequestFlagTaken", PhotonTargets.MasterClient, flag.photonView.viewID, pickupPlayer.PhotonPlayer);
			}
		}
	}

	private void OnFlagRelease(FlagTool flag, Player droppingPlayer)
	{
		if (droppingPlayer.PhotonPlayer.isLocal && IsValidFlagDrop(flag, droppingPlayer.PhotonPlayer))
		{
			base.photonView.RPC("RpcMasterRequestFlagDrop", PhotonTargets.MasterClient, flag.photonView.viewID, droppingPlayer.PhotonPlayer);
		}
	}

	private void OnFlagReset(FlagTool flag)
	{
		if (PhotonNetwork.isMasterClient && IsValidFlagReturn(flag, null))
		{
			base.photonView.RPC("RpcMasterRequestFlagReturn", PhotonTargets.MasterClient, flag.photonView.viewID, PhotonPlayer.Invalid);
		}
	}

	private void MasterRequestFlagCapture(FlagTool flag, PhotonPlayer capturer)
	{
		if (PhotonNetwork.isMasterClient && IsValidFlagCapture(flag, capturer))
		{
			base.StatsManager.AddScore(base.TeamManager.GetPlayerTeam(capturer), 1);
			base.StatsManager.AddPlayerStat(capturer, PlayerStatType.STAT_3, 1);
			paintballStateMachine.EnterState(1);
			base.photonView.RPC("RpcOnFlagCaptured", PhotonTargets.All, flag.photonView.viewID, capturer);
		}
	}

	private void MasterRequestFlagReturn(FlagTool flag, PhotonPlayer returner)
	{
		if (PhotonNetwork.isMasterClient && IsValidFlagReturn(flag, returner))
		{
			base.photonView.RPC("RpcOnFlagReturn", PhotonTargets.All, flag.photonView.viewID);
		}
	}

	private void MasterRequestFlagTaken(FlagTool flag, PhotonPlayer pickupPlayer)
	{
		if (PhotonNetwork.isMasterClient && IsValidFlagPickup(flag, pickupPlayer))
		{
			base.photonView.RPC("RpcOnFlagTaken", PhotonTargets.All, flag.photonView.viewID);
		}
	}

	private void MasterRequestFlagDrop(FlagTool flag, PhotonPlayer dropper)
	{
		if (PhotonNetwork.isMasterClient && IsValidFlagDrop(flag, dropper))
		{
			base.photonView.RPC("RpcOnFlagDrop", PhotonTargets.All, flag.photonView.viewID, dropper);
		}
	}

	private void OnFlagCaptured(FlagTool flag, PhotonPlayer capturingPlayer)
	{
		GameTeam playerTeam = base.TeamManager.GetPlayerTeam(capturingPlayer);
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, base.TeamManager.GetTeamName(playerTeam) + " Scores!", "Scored by " + capturingPlayer.name, 3f);
		switch (playerTeam)
		{
		case GameTeam.TEAM_1:
			base.FxManager.PlayFX(FxType.TEAM_1_SCORE);
			break;
		case GameTeam.TEAM_2:
			base.FxManager.PlayFX(FxType.TEAM_2_SCORE);
			break;
		}
		if (capturingPlayer.isLocal)
		{
			Player.LocalPlayer.PlayerEvents.PaintballFlagCapture();
			base.TeleportManager.RemoveLocalPlayerCustomTeleportCooldown(TeleportCooldownType.ACTIVITY_COOLDOWN_2);
		}
	}

	private void OnFlagTaken(FlagTool flag)
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, base.TeamManager.GetTeamName(flag.Team) + " Flag Taken!", 1.5f);
		if (flag.Team == GameTeam.TEAM_1)
		{
			base.FxManager.PlayRateLimitedFX(FxType.TEAM_1_FLAG_TAKEN, audioFXMinInterval);
		}
		else if (flag.Team == GameTeam.TEAM_2)
		{
			base.FxManager.PlayRateLimitedFX(FxType.TEAM_2_FLAG_TAKEN, audioFXMinInterval);
		}
		if (flag.IsHeld && flag.HolderHand != null && flag.HolderHand.ThisPlayer.isLocal)
		{
			base.TeleportManager.SetLocalPlayerCustomTeleportCooldown(TeleportCooldownType.ACTIVITY_COOLDOWN_2);
		}
	}

	private void OnFlagReturn(FlagTool flag)
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, base.TeamManager.GetTeamName(flag.Team) + " Flag Returned", 1.5f);
		if (flag.Team == GameTeam.TEAM_1)
		{
			base.FxManager.PlayFX(FxType.TEAM_1_FLAG_RETURNED);
		}
		else if (flag.Team == GameTeam.TEAM_2)
		{
			base.FxManager.PlayFX(FxType.TEAM_2_FLAG_RETURNED);
		}
	}

	private void OnFlagDropped(FlagTool flag, PhotonPlayer dropper)
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, base.TeamManager.GetTeamName(flag.Team) + " Flag Dropped", 1.5f);
		if (flag.Team == GameTeam.TEAM_1)
		{
			base.FxManager.PlayRateLimitedFX(FxType.TEAM_1_FLAG_DROPPED, audioFXMinInterval);
		}
		else if (flag.Team == GameTeam.TEAM_2)
		{
			base.FxManager.PlayRateLimitedFX(FxType.TEAM_2_FLAG_DROPPED, audioFXMinInterval);
		}
		if (dropper.ToPlayer() != null && dropper.isLocal)
		{
			base.TeleportManager.RemoveLocalPlayerCustomTeleportCooldown(TeleportCooldownType.ACTIVITY_COOLDOWN_2);
		}
	}

	private bool IsValidFlagCapture(FlagTool flag, PhotonPlayer capturer)
	{
		GameTeam playerTeam = base.TeamManager.GetPlayerTeam(capturer);
		FlagTool teamFlag = GetTeamFlag(playerTeam);
		return !base.TeamManager.IsPlayerSpectator(capturer) && paintballStateMachine.CurrentStateId == 0 && playerTeam != flag.Team && teamFlag.AtHomeGoal;
	}

	private bool IsValidFlagDrop(FlagTool flag, PhotonPlayer droppingPlayer)
	{
		return !base.TeamManager.IsPlayerSpectator(droppingPlayer) && paintballStateMachine.CurrentStateId == 0 && base.TeamManager.GetPlayerTeam(droppingPlayer) != flag.Team;
	}

	private bool IsValidFlagPickup(FlagTool flag, PhotonPlayer pickupPlayer)
	{
		return !base.TeamManager.IsPlayerSpectator(pickupPlayer) && paintballStateMachine.CurrentStateId == 0 && base.TeamManager.GetPlayerTeam(pickupPlayer) != flag.Team;
	}

	private bool IsValidFlagReturn(FlagTool flag, PhotonPlayer returner)
	{
		return (returner == null || (!base.TeamManager.IsPlayerSpectator(returner) && base.TeamManager.GetPlayerTeam(returner) == flag.Team)) && paintballStateMachine.CurrentStateId == 0;
	}

	private FlagTool GetTeamFlag(GameTeam team)
	{
		FlagTool result = null;
		for (int i = 0; i < flags.Length; i++)
		{
			if (flags[i].Team == team)
			{
				result = flags[i];
				break;
			}
		}
		return result;
	}

	private void AuthorityResetAllFlags()
	{
		for (int i = 0; i < flags.Length; i++)
		{
			if (!flags[i].AtHomeGoal)
			{
				flags[i].AuthorityResetToLastSpawnPosition();
			}
		}
	}

	private void SetAllFlagsPickupDisabled()
	{
		for (int i = 0; i < flags.Length; i++)
		{
			flags[i].PlayerInteractionRestriction.ForceRestricted = true;
		}
	}

	private void SetAllFlagsPickupEnabled()
	{
		for (int i = 0; i < flags.Length; i++)
		{
			flags[i].PlayerInteractionRestriction.ForceRestricted = false;
		}
	}

	private void MasterDisableAllFlags()
	{
		if (PhotonNetwork.isMasterClient)
		{
			for (int i = 0; i < flags.Length; i++)
			{
				flags[i].Disable();
			}
		}
	}

	private void OnEnterCTFRoundRunningState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			base.GameTimerManager.Resume();
		}
		AuthorityResetAllFlags();
		SetAllFlagsPickupEnabled();
	}

	private void OnEnterCTFFlagCapturedState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			base.GameTimerManager.Pause();
			masterPaintballStateTimer.StartTimer(3f);
		}
		SetAllFlagsPickupDisabled();
		AuthorityResetAllFlags();
	}

	private void OnUpdateCTFFlagCapturedState()
	{
		if (PhotonNetwork.isMasterClient && masterPaintballStateTimer.TimerOver)
		{
			GameTeam winningTeam = GameTeam.INVALID;
			if (base.StatsManager.GetWinningScore(out winningTeam) >= ctfScoreToWin)
			{
				MasterStopGame();
			}
			else
			{
				paintballStateMachine.EnterState(0);
			}
		}
	}

	private void OnEnterTeamBattleRoundRunningState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			MasterDisableAllFlags();
		}
	}

	private void OnUpdateTeamBattleRoundRunningState()
	{
		GameTeam winningTeam = GameTeam.INVALID;
		if (PhotonNetwork.isMasterClient && base.StatsManager.GetWinningScore(out winningTeam) >= teamBattleScoreToWin)
		{
			MasterStopGame();
		}
	}

	private void OnLocalPlayerOutOfBounds()
	{
		base.photonView.RPC("RpcMasterPlayerOutOfBounds", PhotonTargets.MasterClient, PhotonNetwork.player);
	}

	private void MasterPlayerOutOfBounds(PhotonPlayer player)
	{
		if (base.CombatManager.PlayerIsAlive(player))
		{
			base.CombatManager.MasterSetPlayerHealth(player, 0);
			base.photonView.RPC("RpcOnPlayerOutOfBounds", PhotonTargets.All, player);
		}
	}

	private void OnPlayerOutOfBounds(PhotonPlayer player)
	{
		if (player.isLocal)
		{
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "You're Out!", "Out of Bounds!", 3f);
		}
	}

	[PunRPC]
	private void RpcMasterRequestPlayerHit(int weaponPhotonViewId, PhotonPlayer shooter, PhotonPlayer hitPlayer, int hitPlayerBodyPartId, Vector3 impactPoint)
	{
		PhotonView photonView = PhotonView.Find(weaponPhotonViewId);
		Weapon weapon = ((!(photonView != null)) ? null : photonView.GetComponent<Weapon>());
		if (weapon != null)
		{
			MasterRequestPlayerHit(weapon, shooter, hitPlayer, (Player.BodyPart)hitPlayerBodyPartId, impactPoint);
		}
	}

	[PunRPC]
	private void RpcOnPlayerHit(PhotonPlayer shooter, PhotonPlayer hitPlayer, Player.BodyPart hitBodyPart, Vector3 impactPoint)
	{
		OnPlayerHit(shooter, hitPlayer, hitBodyPart, impactPoint);
	}

	[PunRPC]
	private void RpcMasterRequestFlagCapture(int flagToolPhotonViewId, PhotonPlayer capturingPlayer)
	{
		PhotonView photonView = PhotonView.Find(flagToolPhotonViewId);
		FlagTool flagTool = ((!(photonView != null)) ? null : photonView.GetComponent<FlagTool>());
		if (capturingPlayer != null && flagTool != null)
		{
			MasterRequestFlagCapture(flagTool, capturingPlayer);
		}
	}

	[PunRPC]
	private void RpcMasterRequestFlagReturn(int flagToolPhotonViewId, int capturerPhotonPlayerId)
	{
		PhotonPlayer returner = PhotonPlayer.Find(capturerPhotonPlayerId);
		PhotonView photonView = PhotonView.Find(flagToolPhotonViewId);
		FlagTool flagTool = ((!(photonView != null)) ? null : photonView.GetComponent<FlagTool>());
		if (flagTool != null)
		{
			MasterRequestFlagReturn(flagTool, returner);
		}
	}

	[PunRPC]
	private void RpcMasterRequestFlagTaken(int flagToolPhotonViewId, PhotonPlayer capturer)
	{
		PhotonView photonView = PhotonView.Find(flagToolPhotonViewId);
		FlagTool flagTool = ((!(photonView != null)) ? null : photonView.GetComponent<FlagTool>());
		if (flagTool != null)
		{
			MasterRequestFlagTaken(flagTool, capturer);
		}
	}

	[PunRPC]
	private void RpcMasterRequestFlagDrop(int flagToolPhotonViewId, PhotonPlayer capturer)
	{
		PhotonView photonView = PhotonView.Find(flagToolPhotonViewId);
		FlagTool flagTool = ((!(photonView != null)) ? null : photonView.GetComponent<FlagTool>());
		if (flagTool != null)
		{
			MasterRequestFlagDrop(flagTool, capturer);
		}
	}

	[PunRPC]
	private void RpcOnFlagCaptured(int flagToolPhotonViewId, PhotonPlayer capturingPlayer)
	{
		PhotonView photonView = PhotonView.Find(flagToolPhotonViewId);
		FlagTool flagTool = ((!(photonView != null)) ? null : photonView.GetComponent<FlagTool>());
		if (capturingPlayer != null && flagTool != null)
		{
			OnFlagCaptured(flagTool, capturingPlayer);
		}
	}

	[PunRPC]
	private void RpcOnFlagReturn(int flagToolPhotonViewId)
	{
		PhotonView photonView = PhotonView.Find(flagToolPhotonViewId);
		FlagTool flagTool = ((!(photonView != null)) ? null : photonView.GetComponent<FlagTool>());
		if (flagTool != null)
		{
			OnFlagReturn(flagTool);
		}
	}

	[PunRPC]
	private void RpcOnFlagTaken(int flagToolPhotonViewId)
	{
		PhotonView photonView = PhotonView.Find(flagToolPhotonViewId);
		FlagTool flagTool = ((!(photonView != null)) ? null : photonView.GetComponent<FlagTool>());
		if (flagTool != null)
		{
			OnFlagTaken(flagTool);
		}
	}

	[PunRPC]
	private void RpcOnFlagDrop(int flagToolPhotonViewId, PhotonPlayer dropper)
	{
		PhotonView photonView = PhotonView.Find(flagToolPhotonViewId);
		FlagTool flagTool = ((!(photonView != null)) ? null : photonView.GetComponent<FlagTool>());
		if (flagTool != null)
		{
			OnFlagDropped(flagTool, dropper);
		}
	}

	[PunRPC]
	private void RpcMasterPlayerOutOfBounds(PhotonPlayer player)
	{
		MasterPlayerOutOfBounds(player);
	}

	[PunRPC]
	private void RpcOnPlayerOutOfBounds(PhotonPlayer player)
	{
		OnPlayerOutOfBounds(player);
	}
}
