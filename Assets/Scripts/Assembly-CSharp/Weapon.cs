using System;
using UnityEngine;

public class Weapon : Tool
{
	public delegate void PlayerImpact(Weapon weapon, Player shooter, Player hitPlayer, Player.BodyPart hitPlayerBodyPart, Vector3 impactPoint);

	public delegate void EnemyImpact(Weapon weapon, Player shooter, Enemy hitEnemy, Vector3 impactPoint);

	[Serializable]
	private struct PlayerDamageSetting
	{
		public Player.BodyPart BodyPart;

		public int Damage;
	}

	[Serializable]
	private struct EnemyDamageSetting
	{
		public EnemyType EnemyType;

		[SerializeField]
		private int MinDamage;

		[SerializeField]
		private int MaxDamage;

		public int GetDamageAmount(float normalizedDamageAmount)
		{
			return (int)Mathf.Lerp(MinDamage, MaxDamage, normalizedDamageAmount);
		}
	}

	[SerializeField]
	protected bool weaponHitsEnabled = true;

	[Header("Damage")]
	[SerializeField]
	private PlayerDamageSetting[] damageSettings;

	[SerializeField]
	private EnemyDamageSetting[] enemyDamageSettings;

	[Header("Impact")]
	[SerializeField]
	private ImpactDecal impactDecalPrefab;

	[SerializeField]
	private Vector2 minMaxImpactDecalScale = new Vector2(0.8f, 1.2f);

	[SerializeField]
	private float randomImpactSplatOffset;

	[SerializeField]
	protected PooledParticle impactPlayerParticlePrefab;

	[SerializeField]
	protected PooledParticle impactOtherParticlePrefab;

	private GameManager gameManager;

	public event PlayerImpact PlayerImpactEvent;

	public event EnemyImpact EnemyImpactEvent;

	protected override void Start()
	{
		base.Start();
		gameManager = RecRoomSceneManager.Instance.GameManager;
		if (SpawnedFromPool)
		{
			AutoLock = true;
		}
	}

	public int GetPlayerImpactDamage(Player.BodyPart hitBodyPart)
	{
		int result = 0;
		if (damageSettings != null)
		{
			for (int i = 0; i < damageSettings.Length; i++)
			{
				if (damageSettings[i].BodyPart == hitBodyPart)
				{
					result = damageSettings[i].Damage;
					break;
				}
			}
		}
		return result;
	}

	protected int GetEnemyImpactDamage(EnemyType hitEnemyType, float normalizedDamageAmount)
	{
		int result = 0;
		if (enemyDamageSettings != null)
		{
			for (int i = 0; i < enemyDamageSettings.Length; i++)
			{
				if (enemyDamageSettings[i].EnemyType == hitEnemyType)
				{
					result = enemyDamageSettings[i].GetDamageAmount(normalizedDamageAmount);
					break;
				}
			}
		}
		return result;
	}

	protected virtual void OnPlayerImpact(Player shooter, Player player, Player.BodyPart bodyPart, GameObject hitGameObject, Vector3 position, Vector3 attackDirection, Vector3 surfaceNormal)
	{
		if (this.PlayerImpactEvent != null)
		{
			this.PlayerImpactEvent(this, shooter, player, bodyPart, position);
		}
		SpawnImpactDecal(position, surfaceNormal, hitGameObject);
	}

	protected virtual void OnEnemyImpact(Player shooter, Enemy enemy, int damage, Vector3 collisionForce, GameObject hitGameObject, Vector3 position, Vector3 attackDirection, Vector3 surfaceNormal)
	{
		if (this.EnemyImpactEvent != null)
		{
			this.EnemyImpactEvent(this, shooter, enemy, position);
		}
		if (base.hasAuthority && !enemy.hasAuthority)
		{
			enemy.photonView.TransferOwnership(PhotonNetwork.player);
		}
		if (enemy.hasAuthority)
		{
			enemy.AuthorityApplyDamage(damage, collisionForce, position);
		}
		SpawnImpactDecal(position, surfaceNormal, hitGameObject);
	}

	protected virtual void OnToolImpact(Player shooter, Tool tool, Vector3 collisionForce, GameObject hitGameObject, Vector3 position, Vector3 attackDirection, Vector3 surfaceNormal)
	{
		Weapon component = tool.GetComponent<Weapon>();
		if (component != null && component.IsHeld)
		{
			if (weaponHitsEnabled && component.Owner.PhotonPlayer.ID == component.HolderId)
			{
				OnPlayerImpact(shooter, component.Owner, Player.BodyPart.None, hitGameObject, position, attackDirection, surfaceNormal);
			}
		}
		else
		{
			if (OwnershipTransferAllowed && base.hasAuthority && !tool.IsHeld)
			{
				tool.photonView.TransferOwnership(PhotonNetwork.player);
			}
			if (tool.hasAuthority)
			{
				tool.Rigidbody.AddForceAtPosition(collisionForce, position);
				tool.OnToolForceApplied(shooter, position, collisionForce);
			}
			if (tool.ToolAudio != null)
			{
				tool.ToolAudio.OnApplyForce(base.authority, position);
			}
			if (shooter.isLocal)
			{
				PaintballGrenade paintballGrenade = tool as PaintballGrenade;
				if (paintballGrenade != null)
				{
					paintballGrenade.StartCooking(shooter, !(this is PaintballGrenade));
				}
			}
		}
		OnOtherImpact(shooter, hitGameObject, position, attackDirection, surfaceNormal);
	}

	protected virtual void OnOtherImpact(Player shooter, GameObject hitGameObject, Vector3 position, Vector3 attackDirection, Vector3 surfaceNormal)
	{
		SpawnImpactDecal(position, surfaceNormal, hitGameObject);
	}

	protected void SpawnImpactDecal(Vector3 position, Vector3 normal, GameObject hitGameObject)
	{
		SpawnImpactDecal(position, normal, hitGameObject, minMaxImpactDecalScale);
	}

	protected void SpawnImpactDecal(Vector3 position, Vector3 normal, GameObject hitGameObject, Vector2 minMaxDecalScale)
	{
		if (SingletonMonoBehaviour<SettingsManager>.Instance.PersonalBubble)
		{
			PlayerHead component = hitGameObject.GetComponent<PlayerHead>();
			if (component != null && component.isLocal && base.Owner.PhotonPlayer != null && !base.Owner.isLocal)
			{
				bool flag = gameManager != null && gameManager.CurrentState == GameStates.GAME_RUNNING;
				bool flag2 = gameManager != null && gameManager.TeamManager.PlayersAreTeammates(component.Player.PhotonPlayer, base.Owner.PhotonPlayer);
				if (!flag || flag2)
				{
					return;
				}
			}
		}
		ImpactDecal impactDecal = ObjectPool.Instance.Acquire(impactDecalPrefab);
		if (impactDecal != null)
		{
			if (randomImpactSplatOffset > 0f)
			{
				Vector3 vector = UnityEngine.Random.insideUnitCircle * randomImpactSplatOffset;
				vector = Quaternion.LookRotation(normal) * vector;
				position += vector;
			}
			impactDecal.Color = base.ToolRenderer.AccentColor;
			impactDecal.Splat(position, normal, hitGameObject, minMaxDecalScale.x, minMaxDecalScale.y);
		}
	}

	protected void PlayImpactParticles(PooledParticle particlePrefab, Vector3 position, Vector3 normal)
	{
		if (!(particlePrefab != null))
		{
			return;
		}
		PooledParticle pooledParticle = ObjectPool.Instance.Acquire(particlePrefab);
		if (pooledParticle != null)
		{
			for (int i = 0; i < pooledParticle.AllParticleSystems.Length; i++)
			{
				ParticleSystem.MainModule main = pooledParticle.AllParticleSystems[i].main;
				main.startColor = base.ToolRenderer.AccentColor;
			}
			pooledParticle.transform.position = position;
			pooledParticle.transform.rotation = Quaternion.LookRotation(normal, Vector3.up);
			pooledParticle.Play();
		}
	}
}
