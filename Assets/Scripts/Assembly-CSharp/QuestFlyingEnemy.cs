using UnityEngine;

public class QuestFlyingEnemy : QuestEnemy
{
	private enum AliveBehaviorSubState
	{
		FIND_TARGET = 0,
		FOLLOW_TARGET = 1,
		CHARGE_ATTACK = 2,
		ATTACK_TARGET = 3,
		FLEE = 4,
		POST_FLEE = 5
	}

	[Header("Hovering")]
	[SerializeField]
	private float hoverHeight = 3f;

	[Header("Behavior Timing")]
	[SerializeField]
	private float attackChargeDuration = 1f;

	[SerializeField]
	private float attackDuration = 3f;

	[SerializeField]
	private float postFleeDuration = 0.5f;

	[Header("Behavior Movement Speeds")]
	[SerializeField]
	private float followPlayerSpeed = 3.5f;

	[SerializeField]
	private float fleeSpeed = 6f;

	[Header("Behavior Distances")]
	[SerializeField]
	private float desiredAttackDistance = 3f;

	[SerializeField]
	private float maxFleeDistance = 6f;

	[Header("Projectile")]
	[SerializeField]
	private Bullet projectilePrefab;

	[SerializeField]
	private Color projectileColor = Color.red;

	[SerializeField]
	private Transform projectileOriginTransform;

	[SerializeField]
	private float projectileSpeed = 2f;

	protected override void Awake()
	{
		base.Awake();
		behaviorStateMachine.AddState(1, 0, null, null, OnUpdateFindTargetState);
		behaviorStateMachine.AddState(1, 1, OnEnterFollowTargetState, OnExitFollowTargetState, OnUpdateFollowTargetState);
		behaviorStateMachine.AddState(1, 2, OnEnterChargeAttackState, OnExitChargeAttackState, OnUpdateChargeAttackState);
		behaviorStateMachine.AddState(1, 3, OnEnterAttackTargetState, OnExitAttackTargetState, OnUpdateAttackTargetState);
		behaviorStateMachine.AddState(1, 4, OnEnterFleeState, OnExitFleeState, OnUpdateFleeState);
		behaviorStateMachine.AddState(1, 5, OnEnterPostFleeState, OnExitPostFleeState, OnUpdatePostFleeState);
	}

	protected override void Start()
	{
		base.Start();
		rigidbody.constraints = RigidbodyConstraints.None;
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
		if (base.hasAuthority)
		{
			steering.MaxLinearSpeed = followPlayerSpeed * base.MovementSpeedVarietyScalar;
			rigidbody.ClearVelocity();
		}
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
		Vector3 surfacePosition = GetSurfacePosition();
		Vector3 playerPosition = GetPlayerPosition(base.TargetPlayer);
		if (PathIsInvalid(playerPosition))
		{
			CalculateNewPath(surfacePosition, playerPosition);
		}
		bool flag = (playerPosition - surfacePosition).sqrMagnitude <= desiredAttackDistance * desiredAttackDistance;
		Vector3 vector = playerPosition - rigidbody.position;
		Vector3 hitPoint;
		bool flag2 = !TryGetRaycastHitPoint(rigidbody.position, vector.normalized, vector.magnitude, out hitPoint);
		if ((flag && flag2) || PathCompleted(surfacePosition))
		{
			behaviorStateMachine.EnterState(1, 2);
		}
		else
		{
			steering.Seek(GetNextPathNode(surfacePosition) + Vector3.up * hoverHeight);
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

	private Vector3 GetSurfacePosition()
	{
		Vector3 hitPoint;
		if (!TryGetRaycastHitPoint(rigidbody.position, -Vector3.up, float.MaxValue, out hitPoint))
		{
			return rigidbody.position;
		}
		return hitPoint;
	}

	private bool TryGetRaycastHitPoint(Vector3 origin, Vector3 direction, float distance, out Vector3 hitPoint)
	{
		bool result = false;
		RaycastHit hitInfo;
		if (Physics.Raycast(origin, direction, out hitInfo, distance, 2048))
		{
			hitPoint = hitInfo.point;
			result = true;
		}
		else
		{
			hitPoint = origin + direction * distance;
		}
		return result;
	}

	private void OnEnterChargeAttackState(ushort previousStateId, ushort previousSubStateId)
	{
		if (base.hasAuthority)
		{
			behaviorTimer.StartTimer(attackChargeDuration);
		}
	}

	private void OnUpdateChargeAttackState()
	{
		if (base.hasAuthority)
		{
			if (base.TargetPlayer != null)
			{
				steering.Look((GetPlayerHead(base.TargetPlayer).position - base.transform.position).normalized);
			}
			if (behaviorTimer.TimerOver)
			{
				behaviorStateMachine.EnterState(1, 3);
			}
		}
	}

	private void OnExitChargeAttackState(ushort nextStateId, ushort nextSubStateId)
	{
		steering.StopLooking();
	}

	private void OnEnterAttackTargetState(ushort previousStateId, ushort previousSubStateId)
	{
		if (base.hasAuthority)
		{
			behaviorTimer.StartTimer(attackDuration);
			if (base.TargetPlayer != null)
			{
				AuthorityFireProjectile(projectileOriginTransform.position, GetPlayerHeadPosition(base.TargetPlayer));
			}
		}
		rigidbody.ClearVelocity();
	}

	private void OnUpdateAttackTargetState()
	{
		if (base.hasAuthority)
		{
			if (base.TargetPlayer != null)
			{
				steering.Look((GetPlayerHead(base.TargetPlayer).position - base.transform.position).normalized);
			}
			if (behaviorTimer.TimerOver)
			{
				behaviorStateMachine.EnterState(1, 4);
			}
		}
	}

	private void OnExitAttackTargetState(ushort nextStateId, ushort nextSubStateId)
	{
		steering.StopLooking();
	}

	private void OnEnterFleeState(ushort previousStateId, ushort previousSubStateId)
	{
		steering.MaxLinearSpeed = fleeSpeed * base.MovementSpeedVarietyScalar;
		if (base.hasAuthority)
		{
			Vector3 surfacePosition = GetSurfacePosition();
			CalculateNewPath(surfacePosition, GetFleePosition());
		}
	}

	private void OnUpdateFleeState()
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
		Vector3 surfacePosition = GetSurfacePosition();
		if (PathCompleted(surfacePosition))
		{
			behaviorStateMachine.EnterState(1, 5);
		}
		else
		{
			steering.Seek(GetNextPathNode(surfacePosition) + Vector3.up * hoverHeight);
		}
	}

	private void OnExitFleeState(ushort nextStateId, ushort nextSubStateId)
	{
		steering.StopAll();
		if (base.hasAuthority)
		{
			ClearPath();
		}
	}

	private Vector3 GetFleePosition()
	{
		Vector2 insideUnitCircle = Random.insideUnitCircle;
		Vector3 vector = new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y) * maxFleeDistance;
		return GetPathPosition(rigidbody.position + vector);
	}

	private void OnEnterPostFleeState(ushort previousStateId, ushort previousSubStateId)
	{
		if (base.hasAuthority)
		{
			behaviorTimer.StartTimer(postFleeDuration);
		}
	}

	private void OnUpdatePostFleeState()
	{
		if (base.hasAuthority && behaviorTimer.TimerOver)
		{
			behaviorStateMachine.EnterState(1, 1);
		}
	}

	private void OnExitPostFleeState(ushort nextStateId, ushort nextSubStateId)
	{
	}

	protected override void OnRecoilStart()
	{
		base.OnRecoilStart();
		rigidbody.useGravity = true;
	}

	protected override void OnRecoilEnd()
	{
		base.OnRecoilEnd();
		rigidbody.useGravity = false;
	}

	protected override void AuthorityEnterStateAfterRecoil()
	{
		behaviorStateMachine.EnterState(1, 0);
	}

	protected override void OnDeath()
	{
		base.OnDeath();
		rigidbody.useGravity = true;
	}

	private void AuthorityFireProjectile(Vector3 originPosition, Vector3 targetPosition)
	{
		if (base.hasAuthority)
		{
			base.photonView.RPC("RpcFireProjectile", PhotonTargets.All, originPosition, targetPosition);
		}
	}

	private void FireProjectile(Vector3 origin, Vector3 target)
	{
		Vector3 normalized = (target - origin).normalized;
		Bullet bullet = AcquireProjectile();
		if (bullet != null)
		{
			bullet.Fire(origin, Quaternion.LookRotation(normalized, Vector3.up), normalized * projectileSpeed, 1f, rigidbody);
		}
	}

	private Bullet AcquireProjectile()
	{
		Bullet bullet = null;
		if (projectilePrefab != null)
		{
			bullet = ObjectPool.Instance.Acquire(projectilePrefab);
			if (bullet != null)
			{
				bullet.DestroyEvent += OnProjectileDestroy;
				bullet.PlayerImpactEvent += OnProjectilePlayerImpact;
				bullet.EnemyImpactEvent += OnProjectileEnemyImpact;
				bullet.ToolImpactEvent += OnProjectileToolImpact;
				bullet.OtherImpactEvent += OnProjectileOtherImpact;
				bullet.Color = projectileColor;
			}
		}
		return bullet;
	}

	private void OnProjectileDestroy(Bullet projectile)
	{
		projectile.DestroyEvent -= OnProjectileDestroy;
		projectile.PlayerImpactEvent -= OnProjectilePlayerImpact;
		projectile.EnemyImpactEvent -= OnProjectileEnemyImpact;
		projectile.ToolImpactEvent -= OnProjectileToolImpact;
		projectile.OtherImpactEvent -= OnProjectileOtherImpact;
	}

	private void OnProjectilePlayerImpact(Bullet bullet, Vector3 fireDirection, Player player, Player.BodyPart bodyPart, float chargeAmount, GameObject hitGameObject, Vector3 position, Vector3 surfaceNormal)
	{
		PlayerHit(null, this, player, bodyPart, position);
	}

	private void OnProjectileEnemyImpact(Bullet bullet, Vector3 fireDirection, Enemy enemy, float chargeAmount, Vector3 collisionForce, GameObject hitGameObject, Vector3 position, Vector3 surfaceNormal)
	{
	}

	private void OnProjectileToolImpact(Bullet bullet, Vector3 fireDirection, Tool hitTool, float chargeAmount, Vector3 collisionForce, GameObject hitGameObject, Vector3 position, Vector3 surfaceNormal)
	{
	}

	private void OnProjectileOtherImpact(Bullet bullet, Vector3 fireDirection, float chargeAmount, GameObject hitGameObject, Vector3 position, Vector3 surfaceNormal)
	{
	}

	[PunRPC]
	private void RpcFireProjectile(Vector3 origin, Vector3 target)
	{
		FireProjectile(origin, target);
	}
}
