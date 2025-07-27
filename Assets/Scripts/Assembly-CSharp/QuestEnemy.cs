using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class QuestEnemy : Enemy
{
	protected enum BehaviorState
	{
		INVALID = 0,
		ALIVE = 1,
		RECOIL = 2,
		DEAD = 3
	}

	[Header("Target Search")]
	[SerializeField]
	protected float targetSearchRadius = 20f;

	[Header("Combat Behavior")]
	[SerializeField]
	private float recoilDuration = 1f;

	[SerializeField]
	private float deathDuration = 2f;

	protected GameManager gameManager;

	protected SynchronizedField<float> _movementSpeedVarietyScalar;

	protected SynchronizedField<int> _targetPlayerId;

	protected SynchronizedField<int> _lastAttackerId;

	protected int pathNodeCount;

	protected int pathNodeIndex;

	protected Vector3 pathTargetPosition = Vector3.zero;

	protected Vector3[] pathNodes;

	protected NavMeshPath navMeshPath;

	private List<PhotonPlayer> playerScratchSpace = new List<PhotonPlayer>();

	protected LootDropper lootDrops;

	protected List<PhotonPlayer> damageInstancesReceived;

	protected const RigidbodyConstraints RECOIL_RIGIDBODY_CONSTRAINTS = (RigidbodyConstraints)80;

	protected float MovementSpeedVarietyScalar
	{
		get
		{
			return _movementSpeedVarietyScalar.Get();
		}
		set
		{
			_movementSpeedVarietyScalar.ForceSet(value);
		}
	}

	protected PhotonPlayer TargetPlayer
	{
		get
		{
			return PhotonPlayer.Find(_targetPlayerId.Get());
		}
		set
		{
			_targetPlayerId.ForceSet((value == null) ? PhotonPlayer.Invalid : value.ID);
		}
	}

	protected PhotonPlayer LastAttacker
	{
		get
		{
			return PhotonPlayer.Find(_lastAttackerId.Get());
		}
		set
		{
			_lastAttackerId.ForceSet((value == null) ? PhotonPlayer.Invalid : value.ID);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_movementSpeedVarietyScalar = new SynchronizedField<float>(this, "SPEED", 1f, SetterPermissionMode.AUTHORITY);
		_targetPlayerId = new SynchronizedField<int>(this, "PLAYER_TARGET_ID", -1, SetterPermissionMode.AUTHORITY);
		_lastAttackerId = new SynchronizedField<int>(this, "LAST_ATTACKER_ID", -1, SetterPermissionMode.AUTHORITY);
		behaviorStateMachine.AddState(0, null, null, null);
		behaviorStateMachine.AddState(1, OnEnterAliveState, OnExitAliveState, OnUpdateAliveState);
		behaviorStateMachine.AddState(2, OnEnterRecoilState, OnExitRecoilState, OnUpdateRecoilState);
		behaviorStateMachine.AddState(3, OnEnterDeadState, null, OnUpdateDeadState);
		navMeshPath = new NavMeshPath();
		pathNodes = new Vector3[16];
		pathNodeIndex = 0;
		pathNodeCount = 0;
		damageInstancesReceived = new List<PhotonPlayer>();
		lootDrops = GetComponent<LootDropper>();
	}

	protected virtual void Start()
	{
		gameManager = RecRoomSceneManager.Instance.GameManager;
		if (base.hasAuthority)
		{
			MovementSpeedVarietyScalar = Random.Range(0.9f, 1.1f);
		}
		rigidbody.constraints = RigidbodyConstraints.None;
	}

	protected bool PathIsInvalid(Vector3 targetPosition)
	{
		return PathIsInvalid() || (targetPosition - pathTargetPosition).sqrMagnitude > 1f;
	}

	protected bool PathCompleted(Vector3 currentPosition)
	{
		return !PathIsInvalid() && (pathNodeIndex >= pathNodeCount || (currentPosition - pathNodes[pathNodeCount - 1]).sqrMagnitude <= 0.1f);
	}

	protected void ClearPath()
	{
		pathNodeCount = 0;
		pathNodeIndex = -1;
	}

	protected void CalculateNewPath(Vector3 currentPosition, Vector3 targetPosition)
	{
		NavMesh.CalculatePath(currentPosition, targetPosition, -1, navMeshPath);
		if (navMeshPath.status != NavMeshPathStatus.PathInvalid)
		{
			pathNodeCount = navMeshPath.GetCornersNonAlloc(pathNodes);
			pathNodeIndex = 0;
			pathTargetPosition = targetPosition;
		}
	}

	protected Vector3 GetNextPathNode(Vector3 currentPosition)
	{
		if ((pathNodes[pathNodeIndex] - currentPosition).sqrMagnitude <= 0.25f)
		{
			pathNodeIndex++;
		}
		return pathNodes[pathNodeIndex];
	}

	protected Vector3 GetPathPosition(Vector3 position)
	{
		NavMeshHit hit;
		if (NavMesh.SamplePosition(position, out hit, float.MaxValue, -1))
		{
			return hit.position;
		}
		return position;
	}

	private bool PathIsInvalid()
	{
		return navMeshPath.status == NavMeshPathStatus.PathInvalid || pathNodeCount <= 0;
	}

	protected virtual void OnRecoilStart()
	{
		steering.StopAll();
		rigidbody.constraints = (RigidbodyConstraints)80;
	}

	protected virtual void OnRecoilEnd()
	{
		rigidbody.ClearVelocity();
		rigidbody.constraints = RigidbodyConstraints.None;
	}

	protected abstract void AuthorityEnterStateAfterRecoil();

	private void OnEnterRecoilState(ushort previousStateId, ushort previousSubStateId)
	{
		OnRecoilStart();
		if (base.hasAuthority)
		{
			behaviorTimer.StartTimer(recoilDuration);
		}
	}

	private void OnUpdateRecoilState()
	{
		if (base.hasAuthority && behaviorTimer.TimerOver)
		{
			AuthorityEnterStateAfterRecoil();
		}
	}

	private void OnExitRecoilState(ushort nextStateId, ushort nextSubStateId)
	{
		OnRecoilEnd();
	}

	protected virtual void OnAliveStart()
	{
	}

	protected virtual void OnAliveEnd()
	{
	}

	private void OnEnterAliveState(ushort previousStateId, ushort previousSubStateId)
	{
		OnAliveStart();
	}

	private void OnUpdateAliveState()
	{
		if (base.hasAuthority && base.Health <= 0)
		{
			behaviorStateMachine.EnterState(3);
		}
	}

	private void OnExitAliveState(ushort nextStateId, ushort nextSubStateId)
	{
		OnAliveEnd();
	}

	protected override void OnTookDamage(int damage, PhotonPlayer attacker)
	{
		base.OnTookDamage(damage, attacker);
		if (base.hasAuthority)
		{
			LastAttacker = attacker;
		}
		if (!damageInstancesReceived.Contains(attacker))
		{
			damageInstancesReceived.Add(attacker);
		}
	}

	protected override void OnHealthChanged()
	{
		base.OnHealthChanged();
		if (base.hasAuthority)
		{
			if (base.Health <= 0 && behaviorStateMachine.CurrentStateId != 3)
			{
				behaviorStateMachine.EnterState(3);
			}
			else if (base.Health > 0 && behaviorStateMachine.CurrentStateId != 2)
			{
				behaviorStateMachine.EnterState(2);
			}
		}
	}

	protected virtual void OnDeath()
	{
		steering.StopAll();
		rigidbody.constraints = RigidbodyConstraints.None;
		if (lootDrops != null && damageInstancesReceived.Count > 0)
		{
			lootDrops.DropLoot(damageInstancesReceived.ToArray());
		}
	}

	private void OnEnterDeadState(ushort previousStateId, ushort previousSubStateId)
	{
		OnDeath();
		if (base.hasAuthority)
		{
			behaviorTimer.StartTimer(deathDuration);
		}
	}

	private void OnUpdateDeadState()
	{
		if (base.hasAuthority && behaviorTimer.TimerOver)
		{
			base.IsAlive = false;
			behaviorStateMachine.EnterState(0);
		}
	}

	protected PhotonPlayer GetRandomPlayer(bool onlySearchActivePlayers = true)
	{
		PhotonPlayer[] array = null;
		array = ((!onlySearchActivePlayers) ? PhotonNetwork.playerList : ((!(gameManager != null)) ? null : gameManager.TeamManager.GetActivePlayers()));
		PhotonPlayer result = null;
		if (array != null && array.Length > 0)
		{
			playerScratchSpace.Clear();
			float num = targetSearchRadius * targetSearchRadius;
			for (int i = 0; i < array.Length; i++)
			{
				if ((GetPlayerPosition(array[i]) - base.transform.position).sqrMagnitude <= num)
				{
					playerScratchSpace.Add(array[i]);
				}
			}
			if (playerScratchSpace.Count > 0)
			{
				result = playerScratchSpace[Random.Range(0, playerScratchSpace.Count - 1)];
			}
		}
		return result;
	}

	protected PhotonPlayer GetClosestPlayer(bool onlySearchInXZPlane, bool onlySearchActivePlayers = true)
	{
		PhotonPlayer result = null;
		float num = float.MaxValue;
		PhotonPlayer[] array = null;
		array = ((!onlySearchActivePlayers) ? PhotonNetwork.playerList : ((!(gameManager != null)) ? null : gameManager.TeamManager.GetActivePlayers()));
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 vector = GetPlayerPosition(array[i]) - base.transform.position;
				if (onlySearchInXZPlane)
				{
					vector = Vector3.ProjectOnPlane(vector, Vector3.up);
				}
				float sqrMagnitude = vector.sqrMagnitude;
				if (sqrMagnitude <= num)
				{
					num = sqrMagnitude;
					result = array[i];
				}
			}
		}
		return result;
	}

	protected Vector3 GetPlayerPosition(PhotonPlayer photonPlayer)
	{
		Player player = ((photonPlayer == null) ? null : photonPlayer.ToPlayer());
		return (!(player != null)) ? Vector3.zero : player.CurrentFloorPosition;
	}

	protected Vector3 GetPlayerPosition(PhotonPlayer photonPlayer, Vector3 offset)
	{
		Player player = ((photonPlayer == null) ? null : photonPlayer.ToPlayer());
		if (player == null)
		{
			return Vector3.zero;
		}
		Vector3 normalized = Vector3.ProjectOnPlane(player.Head.transform.forward, Vector3.up).normalized;
		Vector3 up = Vector3.up;
		Vector3 normalized2 = Vector3.Cross(up, normalized).normalized;
		return player.CurrentFloorPosition + offset.x * normalized2 + offset.y * up + offset.z * normalized;
	}

	protected Vector3 GetPlayerForward(PhotonPlayer photonPlayer)
	{
		Player player = ((photonPlayer == null) ? null : photonPlayer.ToPlayer());
		return (!(player != null)) ? Vector3.forward : Vector3.ProjectOnPlane(player.Head.transform.forward, Vector3.up).normalized;
	}

	protected Transform GetPlayerHead(PhotonPlayer photonPlayer)
	{
		Player player = ((photonPlayer == null) ? null : photonPlayer.ToPlayer());
		return (!(player != null)) ? null : player.Head.transform;
	}

	protected Vector3 GetPlayerHeadPosition(PhotonPlayer photonPlayer)
	{
		Player player = ((photonPlayer == null) ? null : photonPlayer.ToPlayer());
		return (!(player != null)) ? Vector3.zero : player.Head.transform.position;
	}

	private void OnDrawGizmos()
	{
		if (pathNodeCount > 0 && pathNodeIndex < pathNodeCount)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(base.transform.position, pathNodes[pathNodeIndex]);
			Gizmos.color = Color.red;
			for (int i = 0; i < pathNodeCount - 1; i += 2)
			{
				Gizmos.DrawLine(pathNodes[i], pathNodes[i + 1]);
			}
		}
	}
}
