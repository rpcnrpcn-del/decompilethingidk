using System.Collections;
using UnityEngine;

public class Gun : Weapon
{
	[Header("Firing")]
	[SerializeField]
	protected Bullet bulletPrefab;

	[SerializeField]
	protected float bulletFireSpeed = 10f;

	[Header("Visuals")]
	[SerializeField]
	protected PooledParticle muzzleFlashPrefab;

	[SerializeField]
	protected bool overrideMuzzleFlashStartColor = true;

	[SerializeField]
	protected Transform muzzleFlashOrigin;

	[SerializeField]
	protected PulsingBeam pulsingBeam;

	[Header("Barrel")]
	[SerializeField]
	protected Transform barrelTransform;

	[Header("Automatic")]
	[SerializeField]
	protected float maximumFiringRate = 10f;

	[SerializeField]
	protected float automaticFiringRate = 10f;

	[Header("Misfire")]
	[SerializeField]
	protected BoxCollider worldOverlapCollider;

	[Header("Reload")]
	[SerializeField]
	protected bool hasReloadEffect;

	[SerializeField]
	protected float reloadDelay = 0.5f;

	protected static readonly int triggerId = Animator.StringToHash("Trigger");

	protected static readonly int recoilId = Animator.StringToHash("Recoil");

	protected static readonly int reloadId = Animator.StringToHash("Reload");

	protected GunAudio gunAudio;

	protected Animator animator;

	protected float triggerAmount;

	protected float lastFireTime;

	private Coroutine delayedPumpCoroutine;

	protected float TimeSinceLastFire
	{
		get
		{
			return Time.time - lastFireTime;
		}
	}

	private bool IsOutOfBounds
	{
		get
		{
			return (worldOverlapCollider != null && worldOverlapCollider.CheckOverlap(2048, QueryTriggerInteraction.Ignore)) || !ActivityBounds.PointInBounds(base.transform.position);
		}
	}

	private bool IsMisfire
	{
		get
		{
			return IsOutOfBounds || (base.IsHeld && base.Owner != null && (base.Owner.IsOutOfBounds || base.Owner.IsOverlappingWorld));
		}
	}

	protected override void Awake()
	{
		base.Awake();
		gunAudio = GetComponent<GunAudio>();
		animator = GetComponentInChildren<Animator>();
		if (barrelTransform == null)
		{
			barrelTransform = base.transform;
		}
	}

	protected virtual void Update()
	{
		if (base.hasAuthority)
		{
			triggerAmount = InputAmount;
			animator.SetFloat(triggerId, triggerAmount);
		}
	}

	public override void OnInputDown()
	{
		base.OnInputDown();
		if (base.hasAuthority)
		{
			if (maximumFiringRate >= 0f && TimeSinceLastFire >= 1f / maximumFiringRate)
			{
				Fire();
			}
			else
			{
				gunAudio.OnMisfire();
			}
		}
	}

	public override void OnInputPressed()
	{
		base.OnInputPressed();
		if (base.hasAuthority && automaticFiringRate > 0f && TimeSinceLastFire >= 1f / automaticFiringRate)
		{
			Fire();
		}
	}

	public void Fire(float chargeAmount = 1f)
	{
		lastFireTime = Time.time;
		base.photonView.RPC("RpcFireShot", PhotonTargets.All, barrelTransform.position, barrelTransform.rotation, barrelTransform.forward, chargeAmount, IsMisfire);
	}

	protected virtual void FireShot(Vector3 position, Quaternion rotation, Vector3 direction, float chargeAmount, bool isMisfire)
	{
		if (isMisfire)
		{
			gunAudio.OnMisfire();
			return;
		}
		animator.SetTrigger(recoilId);
		gunAudio.OnFire();
		if (muzzleFlashPrefab != null)
		{
			PooledParticle pooledParticle = ObjectPool.Instance.Acquire(muzzleFlashPrefab);
			if (pooledParticle != null)
			{
				ParticleSystem.MainModule main = pooledParticle.PrimaryParticleSystem.main;
				if (overrideMuzzleFlashStartColor)
				{
					main.startColor = base.ToolRenderer.AccentColor;
				}
				pooledParticle.transform.position = muzzleFlashOrigin.position;
				pooledParticle.transform.rotation = muzzleFlashOrigin.rotation;
				pooledParticle.Play();
			}
		}
		if (hasReloadEffect)
		{
			PlayDelayedReload();
		}
	}

	protected virtual void FireBullet(Vector3 position, Quaternion rotation, Vector3 velocity, float chargeAmount)
	{
		Bullet bullet = AcquireBullet();
		if (bullet != null)
		{
			bullet.Fire(position, rotation, velocity, chargeAmount, base.Rigidbody);
		}
	}

	protected void FireBullet(Vector3 position, Quaternion rotation, Vector3 velocity, float chargeAmount, float maxLifetime)
	{
		Bullet bullet = AcquireBullet();
		if (bullet != null)
		{
			bullet.Fire(position, rotation, velocity, 1f, base.Rigidbody, maxLifetime);
		}
	}

	protected Bullet AcquireBullet()
	{
		Bullet bullet = null;
		if (bulletPrefab != null)
		{
			bullet = ObjectPool.Instance.Acquire(bulletPrefab);
			if (bullet != null)
			{
				bullet.DestroyEvent += OnBulletDestroy;
				bullet.PlayerImpactEvent += OnBulletPlayerImpact;
				bullet.EnemyImpactEvent += OnBulletEnemyImpact;
				bullet.ToolImpactEvent += OnBulletToolImpact;
				bullet.OtherImpactEvent += OnBulletOtherImpact;
				bullet.Color = base.ToolRenderer.AccentColor;
			}
		}
		return bullet;
	}

	private void OnBulletDestroy(Bullet bullet)
	{
		bullet.DestroyEvent -= OnBulletDestroy;
		bullet.PlayerImpactEvent -= OnBulletPlayerImpact;
		bullet.EnemyImpactEvent -= OnBulletEnemyImpact;
		bullet.ToolImpactEvent -= OnBulletToolImpact;
		bullet.OtherImpactEvent -= OnBulletOtherImpact;
	}

	protected void OnBulletPlayerImpact(Bullet bullet, Vector3 fireDirection, Player player, Player.BodyPart bodyPart, float chargeAmount, GameObject hitGameObject, Vector3 position, Vector3 surfaceNormal)
	{
		OnPlayerImpact(base.Owner, player, bodyPart, hitGameObject, position, fireDirection, surfaceNormal);
	}

	protected void OnBulletEnemyImpact(Bullet bullet, Vector3 fireDirection, Enemy enemy, float chargeAmount, Vector3 collisionForce, GameObject hitGameObject, Vector3 position, Vector3 surfaceNormal)
	{
		int enemyImpactDamage = GetEnemyImpactDamage(enemy.EnemyType, chargeAmount);
		OnEnemyImpact(base.Owner, enemy, enemyImpactDamage, collisionForce, hitGameObject, position, fireDirection, surfaceNormal);
	}

	protected void OnBulletToolImpact(Bullet bullet, Vector3 fireDirection, Tool hitTool, float chargeAmount, Vector3 collisionForce, GameObject hitGameObject, Vector3 position, Vector3 surfaceNormal)
	{
		OnToolImpact(base.Owner, hitTool, collisionForce, hitGameObject, position, fireDirection, surfaceNormal);
	}

	protected void OnBulletOtherImpact(Bullet bullet, Vector3 fireDirection, float chargeAmount, GameObject hitGameObject, Vector3 position, Vector3 surfaceNormal)
	{
		OnOtherImpact(base.Owner, hitGameObject, position, fireDirection, surfaceNormal);
	}

	protected override void OnPlayerImpact(Player shooter, Player player, Player.BodyPart bodyPart, GameObject hitGameObject, Vector3 position, Vector3 attackDirection, Vector3 surfaceNormal)
	{
		gunAudio.OnBulletHitPlayer(position);
		PlayImpactParticles(impactPlayerParticlePrefab, position, -attackDirection);
		base.OnPlayerImpact(shooter, player, bodyPart, hitGameObject, position, attackDirection, surfaceNormal);
	}

	protected override void OnEnemyImpact(Player shooter, Enemy enemy, int damage, Vector3 collisionForce, GameObject hitGameObject, Vector3 position, Vector3 attackDirection, Vector3 surfaceNormal)
	{
		gunAudio.OnBulletHitPlayer(position);
		PlayImpactParticles(impactPlayerParticlePrefab, position, -attackDirection);
		base.OnEnemyImpact(shooter, enemy, damage, collisionForce, hitGameObject, position, attackDirection, surfaceNormal);
	}

	protected override void OnOtherImpact(Player shooter, GameObject hitGameObject, Vector3 position, Vector3 attackDirection, Vector3 surfaceNormal)
	{
		gunAudio.OnBulletHitEnvironment(position);
		PlayImpactParticles(impactOtherParticlePrefab, position, -attackDirection);
		base.OnOtherImpact(shooter, hitGameObject, position, attackDirection, surfaceNormal);
	}

	private void PlayDelayedReload()
	{
		StopDelayedReload();
		delayedPumpCoroutine = StartCoroutine(PlayDelayedReloadCoroutine());
	}

	private void StopDelayedReload()
	{
		if (delayedPumpCoroutine != null)
		{
			StopCoroutine(delayedPumpCoroutine);
		}
	}

	private IEnumerator PlayDelayedReloadCoroutine()
	{
		yield return new WaitForSeconds(reloadDelay);
		gunAudio.OnReload();
		if (animator != null)
		{
			animator.SetTrigger(reloadId);
		}
		delayedPumpCoroutine = null;
	}

	[PunRPC]
	public void RpcFireShot(Vector3 position, Quaternion rotation, Vector3 velocity, float chargeAmount, bool misFire)
	{
		FireShot(position, rotation, velocity, chargeAmount, IsMisfire);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isReading)
		{
			triggerAmount = (float)stream.ReceiveNext();
			if (animator.isActiveAndEnabled)
			{
				animator.SetFloat(triggerId, triggerAmount);
			}
		}
		else
		{
			stream.SendNext(triggerAmount);
		}
	}
}
