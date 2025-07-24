using System;
using System.Collections.Generic;
using UnityEngine;
using SwingState = MeleeWeapon.SwingState;

public class MeleeWeaponV2 : Weapon
{
	[Header("")]
	[SerializeField] private PooledParticle ImpactVFX;

	[Header("Physics")]
	[SerializeField]
	private float swingingImpactMass = 0.1f;

	[SerializeField]
	private float defaultImpactMass = 0.01f;

	private const float SWING_START_SPEED = 3f;

	private const float SWING_STOP_SPEED = 1.75f;

	private const float SWING_START_DISPLACEMENT = 0.075f;

	private const float HIT_HAPTIC_LENGTH = 0.2f;

	private SwingState swingState;

	private float previousSwingSpeed;

	private float previousSwingDisplacement;

	private float swingDisableStartTime;

	private float swingDisableDuration;

	private MeleeWeaponAudio meleeWeaponAudio;

	private MeleeWeaponCollider[] meleeColliders;

	private TrailRenderer trail;

	private TrackedVelocity swingTrackedVelocity;

	private Dictionary<Enemy, float> enemyLastHitTimes = new Dictionary<Enemy, float>();

	private Dictionary<Player, float> playerLastHitTimes = new Dictionary<Player, float>();

	private Dictionary<Tool, float> toolLastHitTimes = new Dictionary<Tool, float>();

	private float otherLastHitTime;

	float TrailOriginalTime;
	bool TrailStopNow;

	protected bool IsSwinging
	{
		get
		{
			return swingState == SwingState.SWINGING;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		DefaultCollisionLayer = Layers.DynamicPhysicsIgnoreEnemyPhysics;
		PickupCollisionLayer = Layers.DynamicPhysicsIgnoreEnemyAndPlayerPhysics;
		Transform pickupTransform = GetPickupTransform(PlayerHand.HandType.Right);
		if (pickupTransform == null)
		{
			pickupTransform = base.transform;
		}
		meleeWeaponAudio = GetComponent<MeleeWeaponAudio>();
		swingTrackedVelocity = pickupTransform.gameObject.AddComponent<TrackedVelocity>();
		TrackedVelocity trackedVelocity = swingTrackedVelocity;
		trackedVelocity.UpdateEvent = (Action<TrackedVelocity>)Delegate.Combine(trackedVelocity.UpdateEvent, new Action<TrackedVelocity>(UpdateSwing));

		trail = GetComponentInChildren<TrailRenderer>();
		TrailOriginalTime = trail.time;
		TrailStopNow = true;

        meleeColliders = GetComponentsInChildren<MeleeWeaponCollider>();
		for (int i = 0; i < meleeColliders.Length; i++)
		{
			meleeColliders[i].weaponRigidbody = base.Rigidbody;
			meleeColliders[i].PlayerImpactEvent += OnMeleeColliderPlayerImpact;
			meleeColliders[i].EnemyImpactEvent += OnMeleeColliderEnemyImpact;
			meleeColliders[i].ToolImpactEvent += OnMeleeColliderToolImpact;
			meleeColliders[i].OtherImpactEvent += OnMeleeColliderOtherImpact;
			meleeColliders[i].enabled = false;
		}
		base.ToolRenderer.AccentColorUpdateEvent += OnAccentColorUpdate;
		base.ToolRenderer.VisibleChangeEvent += OnVisibilityChange;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (swingTrackedVelocity != null)
		{
			TrackedVelocity trackedVelocity = swingTrackedVelocity;
			trackedVelocity.UpdateEvent = (Action<TrackedVelocity>)Delegate.Remove(trackedVelocity.UpdateEvent, new Action<TrackedVelocity>(UpdateSwing));
		}
		for (int i = 0; i < meleeColliders.Length; i++)
		{
			meleeColliders[i].PlayerImpactEvent -= OnMeleeColliderPlayerImpact;
			meleeColliders[i].EnemyImpactEvent -= OnMeleeColliderEnemyImpact;
			meleeColliders[i].ToolImpactEvent -= OnMeleeColliderToolImpact;
			meleeColliders[i].OtherImpactEvent -= OnMeleeColliderOtherImpact;
		}
		base.ToolRenderer.AccentColorUpdateEvent -= OnAccentColorUpdate;
		base.ToolRenderer.VisibleChangeEvent -= OnVisibilityChange;
	}

	public override void Pickup(Player player, Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		base.Pickup(player, target, targetSpacePickupPosition, targetSpacePickupRotation);
		for (int i = 0; i < meleeColliders.Length; i++)
		{
			meleeColliders[i].enabled = true;
		}
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		base.Release(player, linearVelocity, angularVelocity);
		for (int i = 0; i < meleeColliders.Length; i++)
		{
			meleeColliders[i].enabled = false;
		}
	}

	public override bool OnPlayerTryingToRotateInPlace(float angle, float duration)
	{
		TemporarilyDisableSwing(duration);
		return base.OnPlayerTryingToRotateInPlace(angle, duration);
	}

	public override void OnPlayerTeleport(float duration)
	{
		TemporarilyDisableSwing(duration);
		base.OnPlayerTeleport(duration);
	}

	private void UpdateSwing(TrackedVelocity swingMeasurementPoint)
	{
		float magnitude = swingMeasurementPoint.LatestVelocity.magnitude;
		if (magnitude < 0.001f)
		{
			magnitude = previousSwingSpeed;
		}
		float num = swingMeasurementPoint.LatestDisplacement.magnitude;
		switch (swingState)
		{
		case SwingState.NOT_SWINGING:
			swingMeasurementPoint.ClearDisplacement();
			num = 0f;
			if (magnitude >= 3f && previousSwingSpeed < 3f)
			{
				swingState = SwingState.PREPARING_TO_SWING;
			}
			break;
		case SwingState.PREPARING_TO_SWING:
		{
			bool flag = num >= previousSwingDisplacement;
			if (magnitude <= 1.75f || !flag)
			{
				swingState = SwingState.NOT_SWINGING;
			}
			else if (num >= 0.075f)
			{
				swingState = SwingState.SWINGING;
				meleeWeaponAudio.OnSwing(swingTrackedVelocity.transform);
			}
			break;
		}
		case SwingState.SWINGING:
			if (num < 0.075f || magnitude <= 1.75f)
			{
				swingState = SwingState.NOT_SWINGING;
			}
			break;
		case SwingState.DISABLED:
			if (Time.time - swingDisableStartTime > swingDisableDuration)
			{
				swingState = SwingState.NOT_SWINGING;
			}
			break;
		}
		TrailStopNow = swingState != SwingState.SWINGING;
		previousSwingSpeed = magnitude;
		previousSwingDisplacement = num;
	}

	private void TemporarilyDisableSwing(float duration)
	{
		swingState = SwingState.DISABLED;
		swingDisableStartTime = Time.time;
		swingDisableDuration = duration;
	}

	private void OnAccentColorUpdate()
	{
		trail.startColor = base.ToolRenderer.AccentColor;
		trail.endColor = base.ToolRenderer.AccentColor * new Color(1,1,1,0);
	}

	private void OnVisibilityChange()
	{
		trail.enabled = base.ToolRenderer.Visible;
	}

	private bool PlayerHitOnCooldown(Player player)
	{
		bool result = false;
		float value = 0f;
		if (playerLastHitTimes.TryGetValue(player, out value))
		{
			result = Time.time - value <= 0.25f;
		}
		return result;
	}

	private void UpdateLastPlayerHitTime(Player player)
	{
		if (!playerLastHitTimes.ContainsKey(player))
		{
			playerLastHitTimes.Add(player, Time.time);
		}
		else
		{
			playerLastHitTimes[player] = Time.time;
		}
	}

	private void OnMeleeColliderPlayerImpact(MeleeWeaponCollider collider, Player hitPlayer, Player.BodyPart hitBodyPart, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal)
	{
		if (base.Owner != hitPlayer && !PlayerHitOnCooldown(hitPlayer))
		{
			PlayImpactAudio(impactPoint);
			PlayImpactHaptics();
			OnPlayerImpact(base.Owner, hitPlayer, hitBodyPart, hitGameObject, impactPoint, Vector3.zero, surfaceNormal);
		}
		UpdateLastPlayerHitTime(hitPlayer);
	}

	private bool EnemyHitOnCooldown(Enemy enemy)
	{
		bool result = false;
		float value = 0f;
		if (enemyLastHitTimes.TryGetValue(enemy, out value))
		{
			result = Time.time - value <= 0.25f;
		}
		return result;
	}

	private void UpdateLastEnemyHitTime(Enemy enemy)
	{
		if (!enemyLastHitTimes.ContainsKey(enemy))
		{
			enemyLastHitTimes.Add(enemy, Time.time);
		}
		else
		{
			enemyLastHitTimes[enemy] = Time.time;
		}
	}

	private void OnMeleeColliderEnemyImpact(MeleeWeaponCollider collider, Enemy hitEnemy, Vector3 impactVelocity, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal)
	{
		if (!EnemyHitOnCooldown(hitEnemy))
		{
			int damage = 0;
			float num = defaultImpactMass;
			if (IsSwinging)
			{
				damage = GetEnemyImpactDamage(hitEnemy.EnemyType, 1f);
				num = swingingImpactMass;
			}
			PlayImpactAudio(impactPoint);
			PlayImpactHaptics();
			Vector3 collisionForce = impactVelocity * num / Time.fixedDeltaTime;
			OnEnemyImpact(base.Owner, hitEnemy, damage, collisionForce, hitGameObject, impactPoint, Vector3.zero, surfaceNormal);
		}
		UpdateLastEnemyHitTime(hitEnemy);
	}

	private bool ToolHitOnCooldown(Tool tool)
	{
		bool result = false;
		float value = 0f;
		if (toolLastHitTimes.TryGetValue(tool, out value))
		{
			result = Time.time - value <= 0.25f;
		}
		return result;
	}

	private void UpdateLastToolHitTime(Tool tool)
	{
		if (!toolLastHitTimes.ContainsKey(tool))
		{
			toolLastHitTimes.Add(tool, Time.time);
		}
		else
		{
			toolLastHitTimes[tool] = Time.time;
		}
	}

	private void OnMeleeColliderToolImpact(MeleeWeaponCollider collider, Tool hitTool, Vector3 impactVelocity, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal)
	{
		if (this != hitTool && !ToolHitOnCooldown(hitTool))
		{
			PlayImpactVFX(impactPoint);
            PlayImpactAudio(impactPoint);
			PlayImpactHaptics();
			float num = ((!IsSwinging) ? defaultImpactMass : swingingImpactMass);
			Vector3 collisionForce = impactVelocity * num / Time.fixedDeltaTime;
			OnToolImpact(base.Owner, hitTool, collisionForce, hitGameObject, impactPoint, Vector3.zero, surfaceNormal);
		}
		UpdateLastToolHitTime(hitTool);
	}

	private bool OtherHitInCooldown()
	{
		return Time.time - otherLastHitTime <= 0.25f;
	}

	private void UpdateLastOtherHitTime()
	{
		otherLastHitTime = Time.time;
	}

	private void OnMeleeColliderOtherImpact(MeleeWeaponCollider collider, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal)
	{
		if (!OtherHitInCooldown())
		{
			PlayImpactAudio(impactPoint);
			PlayImpactHaptics();
			OnOtherImpact(base.Owner, hitGameObject, impactPoint, Vector3.zero, surfaceNormal);
		}
		UpdateLastOtherHitTime();
	}

	private void PlayImpactHaptics()
	{
		if (IsSwinging)
		{
			base.HolderHand.Vibrate(0.2f);
		}
	}

	private void PlayImpactVFX(Vector3 hitPos)
	{
		if (IsSwinging && ImpactVFX != null)
		{
			PooledParticle pooledParticle = ObjectPool.Instance.Acquire<PooledParticle>(ImpactVFX);
			if (pooledParticle != null)
			{
				pooledParticle.transform.position = hitPos;
				pooledParticle.transform.rotation = Quaternion.identity;
				pooledParticle.Play();
			}
		}
	}

	private void PlayImpactAudio(Vector3 impactPoint)
	{
		if (IsSwinging)
		{
			meleeWeaponAudio.OnHit(impactPoint);
		}
	}

	private void LateUpdate()
	{
		if (TrailStopNow)
			trail.time = Mathf.Lerp(trail.time, 0, Time.deltaTime * 25);
        else
            trail.time = TrailOriginalTime;
    }
}
