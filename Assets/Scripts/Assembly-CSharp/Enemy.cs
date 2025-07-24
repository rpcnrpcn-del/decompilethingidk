using Photon;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(RigidbodySteering))]
public abstract class Enemy : Photon.MonoBehaviour
{
	public delegate void PlayerHitEventHandler(Weapon weapon, Enemy enemy, Player hitPlayer, Player.BodyPart hitBodyPart, Vector3 impactPoint);

	public delegate void Death(Enemy thisEnemy);

	public delegate void HealthChange(Enemy thisEnemy, int newHealth, int maxHealth);

	[SerializeField]
	private EnemyType enemyType = EnemyType.INVALID;

	[Header("Health")]
	[SerializeField]
	private int maxHealth = 100;

	[Header("VFX")]
	[SerializeField] PooledParticle DamageVFX;

	protected Rigidbody rigidbody;

	protected RigidbodySteering steering;

	protected SynchronizedField<int> _health;

	protected SynchronizedField<bool> _isAlive;

	protected SynchronizedStateMachine behaviorStateMachine;

	protected SynchronizedTimer behaviorTimer;

	public EnemyType EnemyType
	{
		get
		{
			return enemyType;
		}
	}

	public bool IsAlive
	{
		get
		{
			return _isAlive.Get();
		}
		protected set
		{
			_isAlive.ForceSet(value);
		}
	}

	public int Health
	{
		get
		{
			return _health.Get();
		}
		protected set
		{
			_health.ForceSet(Mathf.Clamp(value, 0, maxHealth));
		}
	}

	public int MaxHealth
	{
		get
		{
			return maxHealth;
		}
	}

	public event PlayerHitEventHandler PlayerHitEvent;

	public event HealthChange HealthChangeEvent;

	public event Death DeathEvent;

	protected override void Awake()
	{
		base.Awake();
		rigidbody = GetComponent<Rigidbody>();
		steering = GetComponent<RigidbodySteering>();
		_health = new SynchronizedField<int>(this, "HEALTH", maxHealth, SetterPermissionMode.AUTHORITY, OnHealthChanged);
		_isAlive = new SynchronizedField<bool>(this, "IS_ALIVE", true, SetterPermissionMode.AUTHORITY, OnIsAliveChanged);
		behaviorStateMachine = new SynchronizedStateMachine("BEHAVIOR", SetterPermissionMode.AUTHORITY);
		behaviorTimer = new SynchronizedTimer(this, "BEHAVIOR_TIMER", SetterPermissionMode.AUTHORITY);
		EnemyCollider[] componentsInChildren = GetComponentsInChildren<EnemyCollider>(true);
		EnemyCollider[] array = componentsInChildren;
		foreach (EnemyCollider enemyCollider in array)
		{
			enemyCollider.ThisEnemy = this;
		}
	}

	protected virtual void Update()
	{
		behaviorStateMachine.Update();
		behaviorTimer.UpdateCallbacks();
	}

	protected virtual void OnTookDamage(int damage, PhotonPlayer attacker)
	{
	}

	public virtual void AuthorityApplyDamage(int damage, Vector3 collisionForce, Vector3 collisionPosition)
	{
		if (base.hasAuthority)
		{
			base.photonView.RPC("RpcOnTookDamage", PhotonTargets.All, base.authority, damage);
			Health -= damage;
			rigidbody.AddForceAtPosition(collisionForce, collisionPosition);
		}
	}

	protected virtual void OnHealthChanged()
	{
		if (this.HealthChangeEvent != null)
		{
			this.HealthChangeEvent(this, Health, maxHealth);
		}
	}

	protected virtual void OnIsAliveChanged()
	{
		if (!IsAlive && this.DeathEvent != null)
		{
			this.DeathEvent(this);
		}
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
		HandleCollision(collision);
	}

	protected virtual void OnCollisionStay(Collision collision)
	{
		HandleCollision(collision);
	}

	protected virtual void HandleCollision(Collision collision)
	{
		Vector3 point = ((collision.contacts.Length <= 0) ? base.transform.position : collision.contacts[0].point);
		Tool colliderTool = collision.GetColliderTool();
		if (colliderTool != null)
		{
			OnToolCollisionEnter(colliderTool, point, collision);
			return;
		}
		Player.BodyPart bodyPart;
		Player colliderPlayer = collision.GetColliderPlayer(out bodyPart);
		if (colliderPlayer != null)
		{
			OnPlayerCollisionEnter(colliderPlayer, bodyPart, point, collision);
		}
	}

	protected virtual void OnToolCollisionEnter(Tool hitTool, Vector3 point, Collision collision)
	{
		if (!base.hasAuthority && hitTool.IsHeld && hitTool.isLocal)
		{
			base.photonView.TransferOwnership(hitTool.photonView.ownerId);
		}
	}

	protected virtual void OnPlayerCollisionEnter(Player hitPlayer, Player.BodyPart bodyPart, Vector3 point, Collision collision)
	{
		if (!base.hasAuthority && hitPlayer.isLocal)
		{
			base.photonView.TransferOwnership(hitPlayer.photonView.ownerId);
		}
		switch (bodyPart)
		{
		case Player.BodyPart.LeftHand:
			hitPlayer.LeftHand.Vibrate(50, 1000);
			break;
		case Player.BodyPart.RightHand:
			hitPlayer.RightHand.Vibrate(50, 1000);
			break;
		}
		PlayerHit(null, this, hitPlayer, bodyPart, point);
	}

	protected void PlayerHit(Weapon weapon, Enemy enemy, Player hitPlayer, Player.BodyPart hitBodyPart, Vector3 impactPoint)
	{
		if (this.PlayerHitEvent != null)
		{
			this.PlayerHitEvent(weapon, enemy, hitPlayer, hitBodyPart, impactPoint);
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		Killzone colliderKillzone = collider.GetColliderKillzone();
		if (colliderKillzone != null)
		{
			OnKillzoneTriggerEnter(colliderKillzone);
		}
	}

	protected virtual void OnKillzoneTriggerEnter(Killzone hitKillzone)
	{
		if (base.hasAuthority && IsAlive)
		{
			Health = 0;
			IsAlive = false;
		}
	}

	[PunRPC]
	protected void RpcOnTookDamage(PhotonPlayer damagingPlayer, int damage)
	{
        OnTookDamage(damage, damagingPlayer);
		if (damage > 0 && damage < this.maxHealth) // prevent the nuking of ears n stuff
			VFX_Damage();
    }

	private void VFX_Damage()
	{
        if (DamageVFX)
		{
			PooledParticle pooledParticle = ObjectPool.Instance.Acquire(DamageVFX);
			if (pooledParticle != null)
			{
				pooledParticle.transform.position = this.transform.position + (Vector3.up/2);
				pooledParticle.transform.rotation = Quaternion.identity;
				pooledParticle.Play();
			}
		}
    }
}
