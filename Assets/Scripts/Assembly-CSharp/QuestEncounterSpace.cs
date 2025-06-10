using System;
using Photon;
using UnityEngine;

public class QuestEncounterSpace : Photon.MonoBehaviour
{
	private SynchronizedField<bool> _hasStarted;

	[NonSerialized]
	public QuestRoom Room;

	[NonSerialized]
	public QuestManager QuestManager;

	private int spawnPointsHead;

	public bool HasStarted
	{
		get
		{
			return _hasStarted.Get();
		}
		set
		{
			_hasStarted.ForceSet(value);
		}
	}

	public EnemySpawnPoint[] SpawnPoints { get; private set; }

	public QuestEncounterTrigger Trigger { get; private set; }

	public event Action StartEvent;

	protected override void Awake()
	{
		base.Awake();
		_hasStarted = new SynchronizedField<bool>(this, "HAS_STARTED", false, SetterPermissionMode.MASTER, OnHasStartedChanged);
		SpawnPoints = GetComponentsInChildren<EnemySpawnPoint>();
		if (SpawnPoints == null || SpawnPoints.Length == 0)
		{
			Debug.LogError("Quest Encounter Space " + base.name + " is missing Enemy Spawn Points.  There must be at least one.");
			return;
		}
		QuestEncounterTrigger[] componentsInChildren = GetComponentsInChildren<QuestEncounterTrigger>();
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			Debug.LogError("Quest Encounter Space " + base.name + " is missing a QuestEncounterTrigger.  There must be at least one.");
			return;
		}
		Trigger = componentsInChildren[0];
		if (componentsInChildren.Length > 1)
		{
			Debug.Log("Quest Encounter Space " + base.name + " has more than one trigger. Remove extra triggers.");
		}
		Trigger.LocalPlayerTriggerEnterEvent += OnTriggerEvent;
		Trigger.enabled = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Trigger.LocalPlayerTriggerEnterEvent -= OnTriggerEvent;
	}

	private void OnHasStartedChanged()
	{
		Trigger.enabled = !HasStarted;
	}

	private void OnTriggerEvent()
	{
		if (!HasStarted && QuestManager != null)
		{
			QuestManager.RequestEncounterStart(this);
		}
	}

	public EnemySpawnPoint TryGetNextSpawnPoint(EnemyType enemyType)
	{
		EnemySpawnPoint result = null;
		for (int i = 0; i < SpawnPoints.Length; i++)
		{
			int num = spawnPointsHead;
			spawnPointsHead = (spawnPointsHead + 1) % SpawnPoints.Length;
			if (SpawnPoints[num].EnemyTypeIsSupported(enemyType) && !SpawnPoints[num].IsOnCooldown)
			{
				result = SpawnPoints[num];
				break;
			}
		}
		return result;
	}
}
