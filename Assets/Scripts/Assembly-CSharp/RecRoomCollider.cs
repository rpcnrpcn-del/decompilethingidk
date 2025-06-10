using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class RecRoomCollider : MonoBehaviour
{
	[SerializeField]
	private float kinematicBounciness = 0.6f;

	[SerializeField]
	private ColliderImpactType impactType;

	[HideInInspector]
	public Rigidbody ThisRigidbody;

	[HideInInspector]
	public TrackedVelocity ThisTrackedVelocity;

	protected float boundingRadius;

	private const int MAX_RAYCAST_HIT = 256;

	private static Collider[] hits = new Collider[256];

	public Collider ThisCollider { get; private set; }

	public bool IsTriggerByDefault { get; private set; }

	public float KinematicBounciness
	{
		get
		{
			return kinematicBounciness;
		}
	}

	public ColliderImpactType ImpactType
	{
		get
		{
			return impactType;
		}
	}

	private void Awake()
	{
		ThisCollider = GetComponent<Collider>();
		IsTriggerByDefault = ThisCollider != null && ThisCollider.isTrigger;
	}

	private void OnEnable()
	{
		if (ThisCollider != null)
		{
			SphereCollider sphereCollider = ThisCollider as SphereCollider;
			if (sphereCollider != null)
			{
				boundingRadius = sphereCollider.radius;
			}
			else
			{
				boundingRadius = ThisCollider.bounds.extents.magnitude;
			}
		}
	}

	public virtual Vector3 GetCollisionNormal(Collision collision)
	{
		Vector3 vector = Vector3.zero;
		if (ThisRigidbody != null)
		{
			if (ThisRigidbody.isKinematic && ThisTrackedVelocity != null)
			{
				vector = ThisTrackedVelocity.RecentLinearVelocity;
			}
			else if (!ThisRigidbody.isKinematic)
			{
				vector = ThisRigidbody.velocity;
			}
		}
		if (vector.sqrMagnitude < Mathf.Epsilon)
		{
			vector = collision.relativeVelocity.normalized;
		}
		else
		{
			vector.Normalize();
		}
		Vector3 vector2 = Vector3.zero;
		if (collision.contacts != null && collision.contacts.Length > 0)
		{
			ContactPoint contactPoint = collision.contacts[0];
			vector2 = contactPoint.normal;
			Vector3 vector3 = -vector2;
			Vector3 origin = contactPoint.point - vector3;
			RaycastHit hitInfo;
			if (Physics.Raycast(origin, vector3, out hitInfo, 2f, 1 << base.gameObject.layer) && Vector3.Dot(hitInfo.normal, vector2) >= 0f)
			{
				vector2 = hitInfo.normal;
			}
		}
		if (vector.sqrMagnitude > Mathf.Epsilon && Vector3.Dot(vector2, vector) < 0.3f)
		{
			vector2 = vector;
		}
		return vector2;
	}

	public bool IsOverlapping(Rigidbody otherRigidbody)
	{
		int num = Physics.OverlapSphereNonAlloc(ThisCollider.bounds.center, boundingRadius, hits, 1 << otherRigidbody.gameObject.layer);
		for (int i = 0; i < num; i++)
		{
			if (hits[i].gameObject == otherRigidbody.gameObject)
			{
				return true;
			}
		}
		return false;
	}
}
