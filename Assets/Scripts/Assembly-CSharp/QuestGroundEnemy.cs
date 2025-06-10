using UnityEngine;

public class QuestGroundEnemy : QuestEnemy
{
	private enum AliveBehaviorSubState
	{
		FIND_TARGET = 0,
		FOLLOW_TARGET = 1,
		ATTACK_TARGET = 2
	}

	[Header("Movement")]
	[SerializeField]
	private float movementSpeed = 4f;

	[SerializeField]
	private float desiredTargetDistance = 1f;

	[Header("Attack")]
	[SerializeField]
	private float attackDuration = 1.5f;

	private static readonly Vector3[] surfaceQueryOffsets = new Vector3[2]
	{
		new Vector3(0f, 0f, 0.1f),
		new Vector3(0f, 0f, -0.1f)
	};

	protected override void Awake()
	{
		base.Awake();
		behaviorStateMachine.AddState(1, 0, null, null, OnUpdateFindTargetState);
		behaviorStateMachine.AddState(1, 1, OnEnterFollowTargetState, OnExitFollowTargetState, OnUpdateFollowTargetState);
		behaviorStateMachine.AddState(1, 2, OnEnterAttackTargetState, null, OnUpdateAttackTargetState);
	}

	protected override void Start()
	{
		base.Start();
		behaviorStateMachine.Initialize(this, 1, 0);
	}

	private void OnUpdateFindTargetState()
	{
		if (base.hasAuthority)
		{
			PhotonPlayer photonPlayer = null;
			if (base.LastAttacker != null)
			{
				photonPlayer = base.LastAttacker;
				base.LastAttacker = null;
			}
			else
			{
				photonPlayer = GetRandomPlayer();
			}
			if (photonPlayer != null)
			{
				base.TargetPlayer = photonPlayer;
				behaviorStateMachine.EnterState(1, 1);
			}
		}
	}

	private void OnEnterFollowTargetState(ushort previousStateId, ushort previousSubStateId)
	{
		steering.MaxLinearSpeed = movementSpeed * base.MovementSpeedVarietyScalar;
	}

	private void OnUpdateFollowTargetState()
	{
		if (!base.hasAuthority)
		{
			return;
		}
		if (base.TargetPlayer == null || !gameManager.CombatManager.PlayerIsAlive(base.TargetPlayer))
		{
			behaviorStateMachine.EnterState(1, 0);
			return;
		}
		Vector3 targetPosition = GetTargetPosition();
		if (PathIsInvalid(targetPosition))
		{
			CalculateNewPath(base.transform.position, targetPosition);
		}
		if (PathCompleted(base.transform.position))
		{
			behaviorStateMachine.EnterState(1, 2);
		}
		else
		{
			steering.Seek(GetNextPathNode(base.transform.position));
		}
	}

	private void OnExitFollowTargetState(ushort nextStateId, ushort nextSubStateId)
	{
		steering.StopSeeking();
		if (base.hasAuthority)
		{
			ClearPath();
		}
	}

	private Vector3 GetTargetPosition()
	{
		Vector3 vector = Vector3.zero;
		if (base.TargetPlayer != null)
		{
			vector = GetPlayerPosition(base.TargetPlayer);
			Vector3 normalized = Vector3.ProjectOnPlane(vector - base.transform.position, Vector3.up).normalized;
			vector += normalized * desiredTargetDistance;
		}
		return vector;
	}

	private void OnEnterAttackTargetState(ushort previousStateId, ushort previousSubStateId)
	{
		if (base.hasAuthority)
		{
			behaviorTimer.StartTimer(attackDuration);
		}
	}

	private void OnUpdateAttackTargetState()
	{
		if (base.hasAuthority && behaviorTimer.TimerOver)
		{
			behaviorStateMachine.EnterState(1, 1);
		}
	}

	protected override void AuthorityEnterStateAfterRecoil()
	{
		behaviorStateMachine.EnterState(1, 0);
	}
}
