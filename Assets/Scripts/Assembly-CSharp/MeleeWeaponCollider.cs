using System;
using UnityEngine;

public class MeleeWeaponCollider : MonoBehaviour
{
	public delegate void PlayerImpact(MeleeWeaponCollider thisCollider, Player hitPlayer, Player.BodyPart hitBodyPart, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal);

	public delegate void EnemyImpact(MeleeWeaponCollider thisCollider, Enemy hitEnemy, Vector3 collisionForce, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal);

	public delegate void ToolImpact(MeleeWeaponCollider thisCollider, Tool hitTool, Vector3 impactVelocity, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal);

	public delegate void OtherImpact(MeleeWeaponCollider thisCollider, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal);

	[SerializeField]
	private float radius = 0.05f;

	private Vector3 previousPosition = Vector3.zero;

	private WeaponRaycastCollider raycastCollider = new WeaponRaycastCollider();

	[NonSerialized]
	public Rigidbody weaponRigidbody;

	private RaycastHit[] hits = new RaycastHit[5];

	public event PlayerImpact PlayerImpactEvent;

	public event EnemyImpact EnemyImpactEvent;

	public event ToolImpact ToolImpactEvent;

	public event OtherImpact OtherImpactEvent;

	private void Start()
	{
		previousPosition = base.transform.position;
	}

	private void OnEnable()
	{
		previousPosition = base.transform.position;
	}

	private void FixedUpdate()
	{
		Vector3 vector = (base.transform.position - previousPosition) / Time.fixedDeltaTime;
		int num = raycastCollider.RunAllCollisions(previousPosition, base.transform.position, radius, weaponRigidbody, ref hits);
		for (int i = 0; i < num; i++)
		{
			if (!(hits[i].distance <= 0f))
			{
				GameObject hitGameObject = ((!(hits[i].rigidbody != null)) ? hits[i].collider.gameObject : hits[i].rigidbody.gameObject);
				Player player = null;
				Enemy enemy = null;
				Tool tool = null;
				Player.BodyPart bodyPart;
				if ((player = hits[i].collider.GetColliderPlayer(out bodyPart)) != null)
				{
					OnPlayerHit(player, bodyPart, hitGameObject, hits[i].point, hits[i].normal);
				}
				else if ((enemy = hits[i].collider.GetColliderEnemy()) != null)
				{
					OnEnemyHit(enemy, vector, hitGameObject, hits[i].point, hits[i].normal);
				}
				else if ((tool = hits[i].collider.GetColliderTool()) != null)
				{
					OnToolHit(tool, vector, hitGameObject, hits[i].point, hits[i].normal);
				}
				else
				{
					OnOtherHit(hitGameObject, hits[i].point, hits[i].normal);
				}
			}
		}
		previousPosition = base.transform.position;
	}

	private void OnPlayerHit(Player hitPlayer, Player.BodyPart hitBodyPart, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal)
	{
		if (this.PlayerImpactEvent != null)
		{
			this.PlayerImpactEvent(this, hitPlayer, hitBodyPart, hitGameObject, impactPoint, surfaceNormal);
		}
	}

	private void OnEnemyHit(Enemy hitEnemy, Vector3 collisionForce, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal)
	{
		if (this.EnemyImpactEvent != null)
		{
			this.EnemyImpactEvent(this, hitEnemy, collisionForce, hitGameObject, impactPoint, surfaceNormal);
		}
	}

	private void OnToolHit(Tool tool, Vector3 impactVelocity, GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal)
	{
		if (this.ToolImpactEvent != null)
		{
			this.ToolImpactEvent(this, tool, impactVelocity, hitGameObject, impactPoint, surfaceNormal);
		}
	}

	private void OnOtherHit(GameObject hitGameObject, Vector3 impactPoint, Vector3 surfaceNormal)
	{
		if (this.OtherImpactEvent != null)
		{
			this.OtherImpactEvent(this, hitGameObject, impactPoint, surfaceNormal);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position, radius);
	}
}
