using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : GameManager
{
	private enum QuestGameState
	{
		INVALID = 0,
		ROOM_RUNNING = 1,
		ROOM_CLEARED = 2
	}

	private enum QuestPlayerStats
	{
		GOLD = 0
	}

	[Serializable]
	private struct RunFlowRoom
	{
		public string Name;

		public QuestRoom.QuestRoomType RoomType;
	}

	[Header("Run Flow")]
	[SerializeField]
	private RunFlowRoom[] runFlowRooms;

	[Header("Health and Damage")]
	[SerializeField]
	private float invincibilityAfterHit = 2f;

	[SerializeField]
	private HealthBar healthBarPrefab;

	[SerializeField]
	private float monochromeTimeOnHit = 1f;

	[SerializeField]
	private PlayerHandGestures.Gesture reviveGesture;

	[Header("Room Completion Timing")]
	[SerializeField]
	private float roomClearedDuration = 4f;

	private QuestRoom[] rooms;

	private QuestRoom startingRoom;

	private SynchronizedStateMachine questStateMachine;

	private SynchronizedTimer masterQuestStateTimer;

	private SynchronizedField<int> _enemiesAlive;

	private SynchronizedField<int> _roomsCompleted;

	private SynchronizedField<int> _currentRoomId;

	private SynchronizedField<int> _previousRoomId;

	private Dictionary<Player, float> playerHitTimes;

	private Dictionary<PhotonPlayer, HealthBar> healthBars;

	private int EnemiesAlive
	{
		get
		{
			return _enemiesAlive.Get();
		}
		set
		{
			_enemiesAlive.ForceSet(value);
		}
	}

	private QuestRoom PreviousRoom
	{
		get
		{
			PhotonView photonView = PhotonView.Find(_previousRoomId.Get());
			return (!(photonView != null)) ? null : photonView.GetComponent<QuestRoom>();
		}
		set
		{
			_previousRoomId.ForceSet((!(value != null)) ? (-1) : value.photonView.viewID);
		}
	}

	private QuestRoom CurrentRoom
	{
		get
		{
			PhotonView photonView = PhotonView.Find(_currentRoomId.Get());
			return (!(photonView != null)) ? null : photonView.GetComponent<QuestRoom>();
		}
		set
		{
			_currentRoomId.ForceSet((value == null) ? (-1) : value.photonView.viewID);
		}
	}

	private int RoomsCompleted
	{
		get
		{
			return _roomsCompleted.Get();
		}
		set
		{
			_roomsCompleted.ForceSet(value);
		}
	}

	private bool CurrentRoomAllEncounterSpacesStarted
	{
		get
		{
			bool result = true;
			QuestRoom currentRoom = CurrentRoom;
			if (currentRoom != null)
			{
				for (int i = 0; i < currentRoom.EncounterSpaces.Length; i++)
				{
					if (!currentRoom.EncounterSpaces[i].HasStarted)
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		rooms = UnityEngine.Object.FindObjectsOfType<QuestRoom>();
		if (rooms == null || rooms.Length == 0)
		{
			Debug.LogError("Quest Manager has no rooms.  At least one room is required.");
		}
		else
		{
			QuestRoom[] array = rooms;
			foreach (QuestRoom questRoom in array)
			{
				questRoom.PlayerSpawnPointsEnabled = false;
			}
		}
		startingRoom = FindRoom(QuestRoom.QuestRoomType.STARTING_ROOM);
		if (startingRoom == null)
		{
			Debug.LogError("Quest scene does not have a room marked as starting room.  There must be a starting room.");
		}
		else
		{
			startingRoom.PlayerSpawnPointsEnabled = true;
		}
		questStateMachine = new SynchronizedStateMachine("QUEST", SetterPermissionMode.MASTER);
		questStateMachine.AddState(0, null, null, null);
		questStateMachine.AddState(1, OnEnterRoomRunningState, null, OnUpdateRoomRunningState);
		questStateMachine.AddState(2, OnEnterRoomClearedState, OnExitRoomClearedState, OnUpdateRoomClearedState);
		masterQuestStateTimer = new SynchronizedTimer(this, "QUEST_TIMER", SetterPermissionMode.MASTER);
		_enemiesAlive = new SynchronizedField<int>(this, "ENEMIES_ALIVE", 0, SetterPermissionMode.MASTER);
		_roomsCompleted = new SynchronizedField<int>(this, "ROOMS_COMPLETED", 0, SetterPermissionMode.MASTER);
		_currentRoomId = new SynchronizedField<int>(this, "CURRENT_ROOM_ID", -1, SetterPermissionMode.MASTER);
		_previousRoomId = new SynchronizedField<int>(this, "PREVIOUS_ROOM_ID", -1, SetterPermissionMode.MASTER);
		playerHitTimes = new Dictionary<Player, float>();
		healthBars = new Dictionary<PhotonPlayer, HealthBar>();
	}

	protected override void Initialize()
	{
		base.Initialize();
		questStateMachine.Initialize(this, 0);
	}

	protected override void Update()
	{
		base.Update();
		questStateMachine.Update();
		masterQuestStateTimer.UpdateCallbacks();
	}

	protected override void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
		base.InitializeLocalPlayer(localPlayerIsSpectator);
		if (!localPlayerIsSpectator)
		{
			Player.LocalPlayer.PlayerUI.NameVisible = true;
			Player.LocalPlayer.BothHands.ForEach(delegate(PlayerHand x)
			{
				x.Gestures.GestureDetected += OnHandGestureDetected;
			});
		}
	}

	protected override void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
		base.ResetLocalPlayer(localPlayerIsSpectator);
		if (!localPlayerIsSpectator)
		{
			Player.LocalPlayer.PlayerUI.NameVisible = false;
			Player.LocalPlayer.BothHands.ForEach(delegate(PlayerHand x)
			{
				x.Gestures.GestureDetected -= OnHandGestureDetected;
			});
			SingletonMonoBehaviour<CameraRig>.Instance.MonochromeImageEffect.FadeOut(0f);
		}
	}

	protected override void OnGameStart()
	{
		if (PhotonNetwork.isMasterClient)
		{
			RoomsCompleted = 0;
			PreviousRoom = null;
			CurrentRoom = startingRoom;
			PhotonPlayer[] activePlayers = base.TeamManager.GetActivePlayers();
			foreach (PhotonPlayer player in activePlayers)
			{
				base.CombatManager.MasterResetPlayerHealth(player);
			}
			base.photonView.RPC("RpcOnMultipleHealthChanges", PhotonTargets.All);
			questStateMachine.EnterState(1);
		}
	}

	protected override void OnGameEnd()
	{
		startingRoom.DoorsLocked = true;
		startingRoom.DoorTriggerActive = false;
		startingRoom.PlayerSpawnPointsEnabled = true;
		PhotonPlayer[] activePlayers = base.TeamManager.GetActivePlayers();
		foreach (PhotonPlayer key in activePlayers)
		{
			healthBars[key].Active = false;
		}
		if (!PhotonNetwork.isMasterClient)
		{
			return;
		}
		questStateMachine.EnterState(0);
		Enemy[] array = UnityEngine.Object.FindObjectsOfType<Enemy>();
		for (int j = 0; j < array.Length; j++)
		{
			if (!array[j].hasAuthority)
			{
				array[j].photonView.TransferOwnership(PhotonNetwork.player);
			}
			if (array[j].IsAlive && array[j].Health > 0)
			{
				array[j].AuthorityApplyDamage(array[j].MaxHealth, Vector3.zero, Vector3.zero);
			}
		}
	}

	private void InitializeHealthBar(PhotonPlayer photonPlayer)
	{
		Player player = photonPlayer.ToPlayer();
		HealthBar value = null;
		if (player != null && !healthBars.TryGetValue(photonPlayer, out value))
		{
			value = UnityEngine.Object.Instantiate(healthBarPrefab, player.PlayerUI.LabelTransform, false);
			value.Initialize(photonPlayer, base.CombatManager.GetPlayerHealth(photonPlayer));
			value.Active = base.IsGameRunning;
			healthBars[photonPlayer] = value;
		}
	}

	public void RequestEncounterStart(QuestEncounterSpace encounterSpace)
	{
		if (!encounterSpace.HasStarted && CanStartEncounter(encounterSpace))
		{
			base.photonView.RPC("RpcMasterRequestEncounterStart", PhotonTargets.MasterClient, encounterSpace.photonView.viewID);
		}
	}

	private void MasterRequestEncounterStart(QuestEncounterSpace encounterSpace)
	{
		if (PhotonNetwork.isMasterClient && CanStartEncounter(encounterSpace))
		{
			encounterSpace.HasStarted = true;
			QuestEncounter encounter = CurrentRoom.PossibleEncounters[UnityEngine.Random.Range(0, CurrentRoom.PossibleEncounters.Length)];
			MasterSpawnEncounter(encounter, encounterSpace);
		}
	}

	private bool CanStartEncounter(QuestEncounterSpace encounterSpace)
	{
		return CurrentRoom != null && encounterSpace.Room == CurrentRoom && !encounterSpace.HasStarted;
	}

	private void MasterSpawnEncounter(QuestEncounter encounter, QuestEncounterSpace encounterSpace)
	{
		if (!PhotonNetwork.isMasterClient)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < encounter.Enemies.Length; i++)
		{
			EnemyType enemyType = encounter.Enemies[i];
			EnemySpawnPoint enemySpawnPoint = encounterSpace.TryGetNextSpawnPoint(enemyType);
			if (enemySpawnPoint != null)
			{
				SpawnEnemy(enemyType, enemySpawnPoint);
				num++;
			}
		}
		EnemiesAlive += num;
	}

	private QuestRoom MasterGetNextRoom(QuestRoom roomToAvoid = null)
	{
		QuestRoom result = null;
		if (PhotonNetwork.isMasterClient)
		{
			int roomsCompleted = RoomsCompleted;
			if (roomsCompleted >= 0 && roomsCompleted < runFlowRooms.Length)
			{
				QuestRoom.QuestRoomType roomType = runFlowRooms[roomsCompleted].RoomType;
				return FindRoom(roomType);
			}
		}
		return result;
	}

	private QuestRoom FindRoom(QuestRoom.QuestRoomType roomType)
	{
		QuestRoom[] array = rooms;
		foreach (QuestRoom questRoom in array)
		{
			if (questRoom.RoomType == roomType)
			{
				return questRoom;
			}
		}
		return null;
	}

	private void SpawnEnemy(EnemyType enemyType, EnemySpawnPoint spawnPoint)
	{
		string enemyPrefabName = EnemyPrefabSettings.GetEnemyPrefabName(enemyType);
		GameObject gameObject = PhotonNetwork.InstantiateSceneObject(enemyPrefabName, spawnPoint.transform.position, spawnPoint.transform.rotation, 0, null);
		spawnPoint.MasterUse();
		if (gameObject != null)
		{
			Enemy component = gameObject.GetComponent<Enemy>();
			if (component != null)
			{
				PhotonNetwork.RPC(base.photonView, "RpcEnemySpawned", PhotonTargets.All, false, component.photonView.viewID);
			}
		}
	}

	private void OnEnemySpawned(Enemy spawnedEnemy)
	{
		if (spawnedEnemy != null)
		{
			spawnedEnemy.DeathEvent += OnEnemyDied;
			spawnedEnemy.PlayerHitEvent += OnEnemyHitPlayer;
		}
	}

	private void OnEnemyDied(Enemy deadEnemy)
	{
		if (!(deadEnemy == null))
		{
			deadEnemy.DeathEvent -= OnEnemyDied;
			deadEnemy.PlayerHitEvent -= OnEnemyHitPlayer;
			if (deadEnemy.hasAuthority)
			{
				PhotonNetwork.Destroy(deadEnemy.photonView);
			}
			if (PhotonNetwork.isMasterClient)
			{
				EnemiesAlive--;
			}
		}
	}

	protected override void OnTeamChange()
	{
		base.OnTeamChange();
		PhotonPlayer[] activePlayers = base.TeamManager.GetActivePlayers();
		foreach (PhotonPlayer photonPlayer in activePlayers)
		{
			HealthBar value = null;
			if (!healthBars.TryGetValue(photonPlayer, out value))
			{
				InitializeHealthBar(photonPlayer);
			}
		}
	}

	private void OnPlayerHealthChanged(PhotonPlayer player)
	{
		healthBars[player].Active = base.IsGameRunning;
		healthBars[player].UpdateHealth(base.CombatManager.GetPlayerHealth(player));
		if (!PhotonNetwork.isMasterClient)
		{
			return;
		}
		bool flag = true;
		PhotonPlayer[] activePlayers = base.TeamManager.GetActivePlayers();
		foreach (PhotonPlayer player2 in activePlayers)
		{
			if (base.CombatManager.PlayerIsAlive(player2))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			MasterStopGame();
		}
		else if (base.CombatManager.GetPlayerHealth(player) == 0)
		{
			base.photonView.RPC("RpcOnPlayerDown", PhotonTargets.All, player);
		}
	}

	private void OnPlayerRevived(PhotonPlayer revivedPlayer, PhotonPlayer revivingPlayer)
	{
		OnPlayerHealthChanged(revivedPlayer);
		if (revivedPlayer.isLocal)
		{
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "You've been revived by " + revivingPlayer.name + "!", 2f);
			SingletonMonoBehaviour<CameraRig>.Instance.MonochromeImageEffect.FadeOut(0.5f);
		}
	}

	private void OnEnemyHitPlayer(Weapon weapon, Enemy enemy, Player hitPlayer, Player.BodyPart hitBodyPart, Vector3 impactPoint)
	{
		if (hitPlayer.PhotonPlayer.isLocal && IsValidPlayerHit(enemy.photonView, hitPlayer.PhotonPlayer))
		{
			base.photonView.RPC("RpcMasterRequestPlayerHit", PhotonTargets.MasterClient, (!(weapon != null)) ? (-1) : weapon.photonView.viewID, enemy.photonView.viewID, hitPlayer.PhotonPlayer, (int)hitBodyPart, impactPoint);
		}
	}

	private void MasterRequestPlayerHit(Weapon weapon, Enemy enemy, PhotonPlayer hitPlayer, Player.BodyPart hitBodyPart, Vector3 impactPoint)
	{
		if (PhotonNetwork.isMasterClient && IsValidPlayerHit(enemy.photonView, hitPlayer))
		{
			playerHitTimes[hitPlayer.ToPlayer()] = Time.time;
			int num = ((!(weapon != null)) ? 1 : weapon.GetPlayerImpactDamage(hitBodyPart));
			base.CombatManager.MasterAddPlayerHealth(hitPlayer, -num);
			playerHitTimes[hitPlayer.ToPlayer()] = Time.time;
			base.photonView.RPC("RpcOnPlayerHit", PhotonTargets.All, (!(weapon != null)) ? (-1) : weapon.photonView.viewID, enemy.photonView.viewID, hitPlayer, hitBodyPart, impactPoint);
		}
	}

	private void OnPlayerHit(Weapon weapon, Enemy enemy, PhotonPlayer hitPlayer, Player.BodyPart hitBodyPart, Vector3 impactPoint)
	{
		base.FxManager.PlayFX(FxType.PLAYER_DAMAGED, impactPoint);
		OnPlayerHealthChanged(hitPlayer);
		if (!hitPlayer.isLocal)
		{
			return;
		}
		if (base.CombatManager.GetPlayerHealth(hitPlayer) > 0)
		{
			StartCoroutine(PlayLocalPlayerOnHitEffects());
		}
		foreach (PlayerHand bothHand in hitPlayer.ToPlayer().BothHands)
		{
			bothHand.Vibrate(200, 1000);
		}
	}

	private IEnumerator PlayLocalPlayerOnHitEffects()
	{
		SingletonMonoBehaviour<CameraRig>.Instance.MonochromeImageEffect.FadeIn(0.1f);
		yield return new WaitForSeconds(monochromeTimeOnHit);
		SingletonMonoBehaviour<CameraRig>.Instance.MonochromeImageEffect.FadeOut(0.5f);
	}

	private bool IsValidPlayerHit(PhotonView enemyPhotonView, PhotonPlayer photonPlayerHit)
	{
		bool result = true;
		float value = -1f;
		if (playerHitTimes.TryGetValue(photonPlayerHit.ToPlayer(), out value) && Time.time - value < invincibilityAfterHit)
		{
			result = false;
		}
		return result;
	}

	private void OnPlayerDown(PhotonPlayer downedPlayer)
	{
		if (downedPlayer.isLocal)
		{
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "You're Down!", "Shake a buddy's hand to revive.", 5f);
			SingletonMonoBehaviour<CameraRig>.Instance.MonochromeImageEffect.FadeIn(0.2f);
		}
		else
		{
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, downedPlayer.name + " is Down!", "Shake their hand to revive them.", 3f);
		}
	}

	private void OnHandGestureDetected(PlayerHand localHand, PlayerHand otherHand, PlayerHandGestures.Gesture gesture)
	{
		if (gesture == reviveGesture && !base.CombatManager.PlayerIsAlive(Player.LocalPlayer.PhotonPlayer) && otherHand != null && base.CombatManager.PlayerIsAlive(otherHand.ThisPlayer.PhotonPlayer))
		{
			base.photonView.RPC("RpcMasterRequestRevivePlayer", PhotonTargets.MasterClient, Player.LocalPlayer.PhotonPlayer, otherHand.ThisPlayer.PhotonPlayer);
		}
	}

	private void MasterRequestPlayerRevive(PhotonPlayer playerToRevive, PhotonPlayer revivingPlayer)
	{
		if (PhotonNetwork.isMasterClient)
		{
			base.CombatManager.MasterResetPlayerHealth(playerToRevive);
			base.photonView.RPC("RpcOnPlayerRevived", PhotonTargets.All, playerToRevive, revivingPlayer);
		}
	}

	public int GetPlayerGold(PhotonPlayer player)
	{
		return base.StatsManager.GetPlayerStat(player, PlayerStatType.STAT_1);
	}

	public void MasterAddPlayerGold(PhotonPlayer player, int deltaGold)
	{
		if (PhotonNetwork.isMasterClient)
		{
			base.StatsManager.MasterAddPlayerStat(player, PlayerStatType.STAT_1, deltaGold);
		}
	}

	public void MasterSetPlayerGold(PhotonPlayer player, int gold)
	{
		if (PhotonNetwork.isMasterClient)
		{
			base.StatsManager.MasterSetPlayerStat(player, PlayerStatType.STAT_1, gold);
		}
	}

	private void OnPickupGold(int gold, Vector3 pickupLocation)
	{
		MenuNotification.PlayNext("+" + gold, 1.5f, null, false, pickupLocation);
	}

	private void OnEnterRoomRunningState(ushort previousStateId, ushort previousSubStateId)
	{
		if (CurrentRoom == null)
		{
			Debug.LogError("Current Room is null.  Tried to load invalid room.");
			if (PhotonNetwork.isMasterClient)
			{
				MasterStopGame();
			}
		}
		QuestRoom currentRoom = CurrentRoom;
		currentRoom.PlayerSpawnPointsEnabled = true;
		currentRoom.DoorsLocked = true;
		currentRoom.DoorTriggerActive = false;
		currentRoom.LocalPlayerEnterDoorTriggerEvent += OnLocalPlayerEnterCurrentRoomDoorTrigger;
		for (int i = 0; i < currentRoom.EncounterSpaces.Length; i++)
		{
			currentRoom.EncounterSpaces[i].QuestManager = this;
			if (PhotonNetwork.isMasterClient)
			{
				currentRoom.EncounterSpaces[i].HasStarted = false;
			}
		}
	}

	private void OnUpdateRoomRunningState()
	{
		bool flag = EnemiesAlive <= 0;
		CurrentRoom.DoorTriggerActive = flag;
		if (PhotonNetwork.isMasterClient)
		{
			bool flag2 = CurrentRoom.AllPlayersInDoorTrigger(base.TeamManager.GetActivePlayers());
			if (flag && flag2)
			{
				questStateMachine.EnterState(2);
			}
		}
	}

	private void OnEnterRoomClearedState(ushort previousStateId, ushort previousSubStateId)
	{
		if (PhotonNetwork.isMasterClient)
		{
			RoomsCompleted++;
			PreviousRoom = CurrentRoom;
			CurrentRoom = MasterGetNextRoom(CurrentRoom);
			masterQuestStateTimer.StartTimer(roomClearedDuration);
		}
	}

	private void OnUpdateRoomClearedState()
	{
		if (PhotonNetwork.isMasterClient && masterQuestStateTimer.TimerOver)
		{
			if (CurrentRoom == null)
			{
				MasterStopGame();
			}
			else
			{
				questStateMachine.EnterState(1);
			}
		}
	}

	private void OnExitRoomClearedState(ushort nextStateId, ushort nextSubStateId)
	{
		QuestRoom previousRoom = PreviousRoom;
		QuestRoom currentRoom = CurrentRoom;
		if (previousRoom != null && currentRoom != null)
		{
			previousRoom.LocalPlayerEnterDoorTriggerEvent -= OnLocalPlayerEnterCurrentRoomDoorTrigger;
			previousRoom.DoorTriggerActive = false;
			previousRoom.PlayerSpawnPointsEnabled = false;
			previousRoom.DoorsLocked = false;
			PlayDoorsUnlockedEffect();
		}
	}

	private void PlayDoorsUnlockedEffect()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Door Unlocked!", "Continue to the next room!", 5f);
	}

	private void OnLocalPlayerEnterCurrentRoomDoorTrigger()
	{
		if (EnemiesAlive > 0)
		{
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "There Are Still Enemies Out There!", null, 3f);
		}
		else
		{
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Gather All Players To Unlock The Next Room!", null, 3f);
		}
	}

	[PunRPC]
	private void RpcEnemySpawned(int enemyId)
	{
		PhotonView photonView = PhotonView.Find(enemyId);
		OnEnemySpawned(photonView.GetComponent<Enemy>());
	}

	[PunRPC]
	private void RpcSubscribeToEnemyEvents(int[] enemyIds)
	{
		if (enemyIds != null)
		{
			for (int i = 0; i < enemyIds.Length; i++)
			{
				PhotonView photonView = PhotonView.Find(enemyIds[i]);
				OnEnemySpawned(photonView.GetComponent<Enemy>());
			}
		}
	}

	[PunRPC]
	private void RpcMasterRequestPlayerHit(int weaponId, int enemyId, PhotonPlayer photonPlayerHit, int hitBodyPartId, Vector3 impactPoint)
	{
		PhotonView photonView = PhotonView.Find(weaponId);
		Weapon weapon = ((!(photonView != null)) ? null : photonView.GetComponent<Weapon>());
		PhotonView photonView2 = PhotonView.Find(enemyId);
		Enemy enemy = ((!(photonView2 != null)) ? null : photonView2.GetComponent<Enemy>());
		MasterRequestPlayerHit(weapon, enemy, photonPlayerHit, (Player.BodyPart)hitBodyPartId, impactPoint);
	}

	[PunRPC]
	private void RpcOnPlayerHit(int weaponId, int enemyId, PhotonPlayer hitPlayer, Player.BodyPart hitBodyPart, Vector3 impactPoint)
	{
		PhotonView photonView = PhotonView.Find(enemyId);
		Enemy enemy = ((!(photonView != null)) ? null : photonView.GetComponent<Enemy>());
		PhotonView photonView2 = PhotonView.Find(weaponId);
		Weapon weapon = ((!(photonView2 != null)) ? null : photonView2.GetComponent<Weapon>());
		OnPlayerHit(weapon, enemy, hitPlayer, hitBodyPart, impactPoint);
	}

	[PunRPC]
	private void RpcOnMultipleHealthChanges()
	{
		PhotonPlayer[] activePlayers = base.TeamManager.GetActivePlayers();
		foreach (PhotonPlayer player in activePlayers)
		{
			OnPlayerHealthChanged(player);
		}
	}

	[PunRPC]
	private void RpcOnPlayerDown(PhotonPlayer photonPlayer)
	{
		OnPlayerDown(photonPlayer);
	}

	[PunRPC]
	private void RpcMasterRequestEncounterStart(int encounterSpaceViewId)
	{
		PhotonView photonView = PhotonView.Find(encounterSpaceViewId);
		QuestEncounterSpace questEncounterSpace = ((!(photonView != null)) ? null : photonView.GetComponent<QuestEncounterSpace>());
		if (questEncounterSpace != null)
		{
			MasterRequestEncounterStart(questEncounterSpace);
		}
	}

	[PunRPC]
	private void RpcMasterRequestRevivePlayer(PhotonPlayer playerToRevive, PhotonPlayer revivingPlayer)
	{
		MasterRequestPlayerRevive(playerToRevive, revivingPlayer);
	}

	[PunRPC]
	private void RpcOnPlayerRevived(PhotonPlayer revivedPlayer, PhotonPlayer revivingPlayer)
	{
		OnPlayerRevived(revivedPlayer, revivingPlayer);
	}

	[PunRPC]
	private void RpcMasterRequestPickupGold(PhotonPlayer[] receivingPlayers, int gold, Vector3 pickupLocation)
	{
		foreach (PhotonPlayer photonPlayer in receivingPlayers)
		{
			MasterAddPlayerGold(photonPlayer, gold);
			base.photonView.RPC("RpcOnPickupGold", photonPlayer, gold, pickupLocation);
		}
	}

	[PunRPC]
	private void RpcOnPickupGold(int gold, Vector3 pickupLocation)
	{
		OnPickupGold(gold, pickupLocation);
	}

	protected override void OnPhotonPlayerConnected(PhotonPlayer otherPlayer)
	{
		base.OnPhotonPlayerConnected(otherPlayer);
		if (!PhotonNetwork.isMasterClient)
		{
			return;
		}
		Enemy[] array = UnityEngine.Object.FindObjectsOfType<Enemy>();
		int[] array2 = null;
		if (array.Length > 0)
		{
			array2 = array.Select((Enemy x) => x.photonView.viewID).ToArray();
		}
		base.photonView.RPC("RpcSubscribeToEnemyEvents", otherPlayer, array2);
	}
}
