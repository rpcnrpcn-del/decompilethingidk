using System;
using Photon;
using UnityEngine;

public class QuestRoom : Photon.MonoBehaviour
{
	public enum QuestRoomType
	{
		INVALID = -1,
		STARTING_ROOM = 0,
		ROOM_1 = 1,
		ROOM_2 = 2,
		ROOM_3 = 3,
		ROOM_4 = 4,
		ROOM_5 = 5,
		ROOM_6 = 6,
		ROOM_7 = 7,
		ROOM_8 = 8,
		ROOM_9 = 9,
		ROOM_10 = 10
	}

	[SerializeField]
	private QuestRoomType roomType = QuestRoomType.INVALID;

	[SerializeField]
	private QuestEncounter[] possibleEncounters;

	private QuestRoomDoorTrigger doorTrigger;

	private GameSpawnPoint[] playerSpawnPoints;

	private bool _spawnPointsEnabled;

	public QuestRoomType RoomType
	{
		get
		{
			return roomType;
		}
	}

	public QuestEncounterSpace[] EncounterSpaces { get; private set; }

	public QuestDoor[] Doors { get; private set; }

	public QuestEncounter[] PossibleEncounters
	{
		get
		{
			return possibleEncounters;
		}
	}

	public bool DoorsLocked
	{
		get
		{
			return Doors != null && Doors[0].IsLocked;
		}
		set
		{
			for (int i = 0; i < Doors.Length; i++)
			{
				Doors[i].IsLocked = value;
			}
		}
	}

	public bool DoorTriggerActive
	{
		get
		{
			return doorTrigger != null && doorTrigger.Active;
		}
		set
		{
			if (doorTrigger != null)
			{
				doorTrigger.Active = value;
			}
		}
	}

	public bool PlayerSpawnPointsEnabled
	{
		get
		{
			return _spawnPointsEnabled;
		}
		set
		{
			_spawnPointsEnabled = value;
			OnSpawnPointEnabledUpdated();
		}
	}

	public event Action LocalPlayerEnterDoorTriggerEvent;

	protected override void Awake()
	{
		base.Awake();
		EncounterSpaces = GetComponentsInChildren<QuestEncounterSpace>();
		if (EncounterSpaces != null)
		{
			for (int i = 0; i < EncounterSpaces.Length; i++)
			{
				EncounterSpaces[i].Room = this;
			}
		}
		playerSpawnPoints = base.transform.GetComponentsInChildren<GameSpawnPoint>(true);
		if (playerSpawnPoints == null || playerSpawnPoints.Length == 0)
		{
			Debug.LogError("Quest Room is missing player spawn points.");
		}
		OnSpawnPointEnabledUpdated();
		doorTrigger = GetComponentInChildren<QuestRoomDoorTrigger>();
		doorTrigger.LocalPlayerTriggerEnterEvent += OnLocalPlayerTriggerDoor;
		DoorTriggerActive = false;
		Doors = base.transform.GetComponentsInChildren<QuestDoor>(true);
		DoorsLocked = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		doorTrigger.LocalPlayerTriggerEnterEvent -= OnLocalPlayerTriggerDoor;
	}

	public bool AllPlayersInDoorTrigger(PhotonPlayer[] players)
	{
		bool result = true;
		foreach (PhotonPlayer photonPlayer in players)
		{
			if (!doorTrigger.ContainsPoint(photonPlayer.ToPlayer().Head.transform.position))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void OnLocalPlayerTriggerDoor()
	{
		if (this.LocalPlayerEnterDoorTriggerEvent != null)
		{
			this.LocalPlayerEnterDoorTriggerEvent();
		}
	}

	private void OnSpawnPointEnabledUpdated()
	{
		if (playerSpawnPoints != null)
		{
			GameSpawnPoint[] array = playerSpawnPoints;
			foreach (GameSpawnPoint gameSpawnPoint in array)
			{
				gameSpawnPoint.SpawnPointEnabled = PlayerSpawnPointsEnabled;
			}
		}
	}
}
