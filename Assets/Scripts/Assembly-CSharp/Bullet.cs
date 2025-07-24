using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	public delegate void PlayerImpact(Bullet thisBullet, Vector3 fireDirection, Player hitPlayer, Player.BodyPart hitBodyPart, float chargeAmount, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal);

	public delegate void EnemyImpact(Bullet thisBullet, Vector3 fireDirection, Enemy hitEnemy, float chargeAmount, Vector3 collisionForce, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal);

	public delegate void ToolImpact(Bullet thisBullet, Vector3 fireDirection, Tool hitTool, float chargeAmount, Vector3 collisionForce, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal);

	public delegate void OtherImpact(Bullet thisBullet, Vector3 fireDirection, float chargeAmount, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal);

	[Header("Physics")]
	[SerializeField]
	private float mass = 0.1f;

	[SerializeField]
	private float colliderRadius = 0.033f;

	[SerializeField]
	private bool applyGravity = true;

	[Header("Cleanup")]
	[SerializeField]
	private float selfDestructTime = 10f;

	[SerializeField]
	private Renderer[] renderers;

	private float fireTime;

	private WeaponRaycastCollider raycastCollider = new WeaponRaycastCollider();

	private BulletTrail trail;

	private Vector3 velocity = Vector3.zero;

	private Rigidbody ownerGunRigidbody;

	private float lifetime;

	private float chargeAmount = 1f;

	private Color _color;

	public Color Color
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
			Renderer[] array = renderers;
			foreach (Renderer renderer in array)
			{
				renderer.material.color = _color;
			}
			trail.Color = value;
		}
	}

	protected virtual Vector3 ColliderOrigin
	{
		get
		{
			return base.transform.position;
		}
	}

	public event Action<Bullet> DestroyEvent;

	public event PlayerImpact PlayerImpactEvent;

	public event EnemyImpact EnemyImpactEvent;

	public event ToolImpact ToolImpactEvent;

	public event OtherImpact OtherImpactEvent;

	public void Fire(Vector3 position, Quaternion rotation, Vector3 initialVelocity, float chargeAmount, Rigidbody gunRigidody)
	{
		Fire(position, rotation, initialVelocity, chargeAmount, gunRigidody, selfDestructTime);
	}

	public void Fire(Vector3 position, Quaternion rotation, Vector3 initialVelocity, float chargeAmount, Rigidbody gunRigidody, float maxLifetime)
	{
		base.enabled = true;
		lifetime = maxLifetime;
		this.chargeAmount = chargeAmount;
		base.transform.position = position;
		base.transform.rotation = rotation;
		velocity = initialVelocity;
		ownerGunRigidbody = gunRigidody;
		fireTime = Time.fixedTime;
		trail.enabled = true;
	}

	protected virtual void Awake()
	{
		base.enabled = false;
		trail = GetComponentInChildren<BulletTrail>();
		trail.enabled = false;
	}

	private void ReleaseBullet(Vector3 finalPosition)
	{
		base.transform.position = finalPosition;
		base.enabled = false;
		if (this.DestroyEvent != null)
		{
			this.DestroyEvent(this);
		}
		StartCoroutine(WaitForTrailToFinishCoroutine(finalPosition));
	}

	private IEnumerator WaitForTrailToFinishCoroutine(Vector3 finalPosition)
	{
		while (trail.Running)
		{
			yield return null;
		}
		trail.enabled = false;
		ObjectPool.Instance.Release(this);
	}

	private void FixedUpdate()
	{
		if (Time.fixedTime - fireTime >= lifetime)
		{
			ReleaseBullet(base.transform.position);
			return;
		}
		Vector3 colliderOrigin = ColliderOrigin;
		base.transform.position += velocity * Time.fixedDeltaTime;
		base.transform.LookAt(base.transform.position + velocity);
		if (applyGravity)
		{
			velocity += Physics.gravity * Time.fixedDeltaTime;
		}
		RaycastHit hit;
		if (raycastCollider.RunCollision(colliderOrigin, ColliderOrigin, colliderRadius, ownerGunRigidbody, out hit))
		{
			if (hit.distance == 0f)
			{
				ReleaseBullet(base.transform.position);
				return;
			}
			Vector3 vector = mass * velocity;
			Vector3 collisionForce = vector / Time.fixedDeltaTime;
			OnRaycastCollisionEnter(collisionForce, hit);
		}
	}

	private void OnRaycastCollisionEnter(Vector3 collisionForce, RaycastHit raycastCollision)
	{
		GameObject hitGameObject = ((!(raycastCollision.rigidbody != null)) ? raycastCollision.collider.gameObject : raycastCollision.rigidbody.gameObject);
		Player.BodyPart bodyPart;
		Player player = raycastCollision.collider.GetColliderPlayer(out bodyPart);
		Enemy enemy = raycastCollision.collider.GetColliderEnemy();
		Tool tool = raycastCollision.collider.GetColliderTool();

		/*MeleeWeapon meleeV1 = hitGameObject.GetComponent<MeleeWeapon>();
		MeleeWeaponV2 meleeV2 = hitGameObject.GetComponent<MeleeWeaponV2>();

        if (meleeV1 || meleeV2)
		{
			TrackedVelocity trackedVel = (meleeV1) ? meleeV1.TrackedVelocity : meleeV2.TrackedVelocity;
			Debug.LogFormat("Velocity: {0} | Magnitude: {1}", trackedVel.RecentLinearVelocity, trackedVel.RecentLinearVelocity.magnitude);
            if (trackedVel.RecentLinearVelocity.magnitude > 0.1f)
			{
				velocity *= -1;
				return;
			}
		}*/

		if (player)
		{
			OnPlayerHit(player, bodyPart, chargeAmount, hitGameObject, raycastCollision.point, raycastCollision.normal);
		}
		else if (enemy)
		{
			OnEnemyHit(enemy, chargeAmount, collisionForce, hitGameObject, raycastCollision.point, raycastCollision.normal);
		}
		else if (tool)
		{
			OnToolHit(tool, chargeAmount, collisionForce, hitGameObject, raycastCollision.point, raycastCollision.normal);
		}
		else
		{
			OnOtherHit(chargeAmount, hitGameObject, raycastCollision.point, raycastCollision.normal);
		}
		ReleaseBullet(raycastCollision.point);
	}

	private void OnPlayerHit(Player hitPlayer, Player.BodyPart hitBodyPart, float chargeAmount, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal)
	{
		if (this.PlayerImpactEvent != null)
		{
			this.PlayerImpactEvent(this, velocity.normalized, hitPlayer, hitBodyPart, chargeAmount, hitGameObject, impactPoint, surfaceNormal);
		}
	}

	private void OnEnemyHit(Enemy hitEnemy, float chargeAmount, Vector3 collisionForce, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal)
	{
		if (this.EnemyImpactEvent != null)
		{
			this.EnemyImpactEvent(this, velocity.normalized, hitEnemy, chargeAmount, collisionForce, hitGameObject, impactPoint, surfaceNormal);
		}
	}

	private void OnToolHit(Tool tool, float chargeAmount, Vector3 collisionForce, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal)
	{
		if (this.ToolImpactEvent != null)
		{
			this.ToolImpactEvent(this, velocity.normalized, tool, chargeAmount, collisionForce, hitGameObject, impactPoint, surfaceNormal);
		}
	}

	private void OnOtherHit(float chargeAmount, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal)
	{
		if (this.OtherImpactEvent != null)
		{
			this.OtherImpactEvent(this, velocity.normalized, chargeAmount, hitGameObject, impactPoint, surfaceNormal);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(ColliderOrigin, colliderRadius);
	}
}
