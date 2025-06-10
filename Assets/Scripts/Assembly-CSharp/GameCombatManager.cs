using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameCombatManager : IGameComponent
{
	private enum LocalPlayerLifeStates
	{
		ALIVE = 0,
		DEAD = 1,
		RESPAWNING = 2,
		INVALID = 65535
	}

	private class PlayerData
	{
		private SynchronizedField<int> _health;

		public int Health
		{
			get
			{
				return _health.Get();
			}
			set
			{
				_health.ForceSet(value);
			}
		}

		public PlayerData(PhotonPlayer player, int defaultHealth)
		{
			_health = new SynchronizedField<int>(player, "HEALTH", defaultHealth, SetterPermissionMode.ANYONE);
		}
	}

	[Header("Health")]
	[SerializeField]
	private int defaultHealth = 100;

	[Header("Respawn")]
	[SerializeField]
	private bool autoRespawnWhenDead = true;

	[SerializeField]
	private float respawnDelay = 3f;

	private GameManager gameManager;

	private StateMachine localPlayerLifeStateMachine;

	private Timer localPlayerLifeStateTimer;

	private Dictionary<int, PlayerData> playerData = new Dictionary<int, PlayerData>();

	public float RespawnDuration
	{
		get
		{
			return respawnDelay + Player.RespawnDuration;
		}
	}

	public void OnAwake(GameManager gameManager)
	{
		this.gameManager = gameManager;
		localPlayerLifeStateMachine = new StateMachine();
		localPlayerLifeStateMachine.AddState(ushort.MaxValue, OnEnterLocalPlayerInvalidState, null, null);
		localPlayerLifeStateMachine.AddState(0, OnEnterLocalPlayerAliveState, null, OnUpdateLocalPlayerAliveState);
		localPlayerLifeStateMachine.AddState(1, OnEnterLocalPlayerDeadState, null, OnUpdateLocalPlayerDeadState);
		localPlayerLifeStateMachine.AddState(2, OnEnterLocalPlayerRespawnState, null, OnUpdateLocalPlayerRespawnState);
		localPlayerLifeStateMachine.Initialize(ushort.MaxValue);
		localPlayerLifeStateTimer = new Timer();
	}

	public void OnStart()
	{
	}

	public void OnDestroy()
	{
	}

	public void OnUpdate()
	{
		localPlayerLifeStateMachine.Update();
		localPlayerLifeStateTimer.UpdateCallbacks();
	}

	public void OnPlayerDisconnected(PhotonPlayer player)
	{
	}

	public void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
		if (!localPlayerIsSpectator)
		{
			localPlayerLifeStateMachine.EnterState(0);
		}
	}

	public void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
		if (!localPlayerIsSpectator)
		{
			localPlayerLifeStateMachine.EnterState(ushort.MaxValue);
		}
	}

	private PlayerData GetPlayerData(PhotonPlayer player)
	{
		PlayerData value;
		if (!playerData.TryGetValue(player.ID, out value))
		{
			value = new PlayerData(player, defaultHealth);
			playerData[player.ID] = value;
		}
		return value;
	}

	public int GetPlayerHealth(PhotonPlayer player)
	{
		return GetPlayerData(player).Health;
	}

	public bool PlayerIsAlive(PhotonPlayer player)
	{
		return GetPlayerHealth(player) > 0;
	}

	public bool PlayersAreAlive(PhotonPlayer player1, PhotonPlayer player2)
	{
		return PlayerIsAlive(player1) && PlayerIsAlive(player2);
	}

	public void MasterAddPlayerHealth(PhotonPlayer player, int deltaHealth)
	{
		if (PhotonNetwork.isMasterClient)
		{
			int playerHealth = GetPlayerHealth(player);
			MasterSetPlayerHealth(player, playerHealth + deltaHealth);
		}
	}

	public void MasterSetPlayerHealth(PhotonPlayer player, int health)
	{
		if (PhotonNetwork.isMasterClient)
		{
			GetPlayerData(player).Health = health;
		}
	}

	public void MasterResetPlayerHealth(PhotonPlayer player)
	{
		GetPlayerData(player).Health = defaultHealth;
	}

	private void LocalPlayerResetHealth()
	{
		GetPlayerData(PhotonNetwork.player).Health = defaultHealth;
	}

	private void OnEnterLocalPlayerInvalidState(ushort previousStateId, ushort previousSubStateId)
	{
		if (Player.LocalPlayer != null)
		{
			Player.LocalPlayer.CanInteractWithTools = true;
			Player.LocalPlayer.PlayerLocomotion.TeleportEnabled = true;
		}
	}

	private void OnEnterLocalPlayerAliveState(ushort previousStateId, ushort previousSubStateId)
	{
		LocalPlayerResetHealth();
		Player.LocalPlayer.CanInteractWithTools = true;
		Player.LocalPlayer.PlayerLocomotion.TeleportEnabled = true;
	}

	private void OnUpdateLocalPlayerAliveState()
	{
		if (!PlayerIsAlive(PhotonNetwork.player))
		{
			localPlayerLifeStateMachine.EnterState(1);
		}
	}

	private void OnEnterLocalPlayerDeadState(ushort previousStateId, ushort previousSubStateId)
	{
		Player.LocalPlayer.CanInteractWithTools = false;
		Player.LocalPlayer.PlayerLocomotion.TeleportEnabled = false;
		if (autoRespawnWhenDead)
		{
			localPlayerLifeStateTimer.StartTimer(respawnDelay);
		}
	}

	private void OnUpdateLocalPlayerDeadState()
	{
		if (gameManager.CurrentState != GameStates.GAME_RUNNING)
		{
			localPlayerLifeStateMachine.EnterState(0);
		}
		else if (autoRespawnWhenDead)
		{
			if (localPlayerLifeStateTimer.TimerOver)
			{
				localPlayerLifeStateMachine.EnterState(2);
			}
		}
		else if (GetPlayerHealth(Player.LocalPlayer.PhotonPlayer) > 0)
		{
			localPlayerLifeStateMachine.EnterState(0);
		}
	}

	private void OnEnterLocalPlayerRespawnState(ushort previousStateId, ushort previousSubStateId)
	{
		Player.LocalPlayer.CanInteractWithTools = false;
		Player.LocalPlayer.PlayerLocomotion.TeleportEnabled = false;
		gameManager.SpawnManager.LocalPlayerRequestRespawn();
		localPlayerLifeStateTimer.StartTimer(Player.RespawnDuration);
	}

	private void OnUpdateLocalPlayerRespawnState()
	{
		if (localPlayerLifeStateTimer.TimerOver)
		{
			localPlayerLifeStateMachine.EnterState(0);
		}
	}
}
