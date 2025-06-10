using UnityEngine;

public class SphereCastCollider : CastCollider
{
	private float colliderRadius;

	public SphereCastCollider(Rigidbody rigidbody, Collider collider, Vector3 colliderCenter)
		: base(rigidbody, collider, colliderCenter)
	{
		Vector3 lossyScale = collider.transform.lossyScale;
		colliderRadius = Mathf.Max(lossyScale.x, lossyScale.y, lossyScale.z) * (collider as SphereCollider).radius;
	}

	protected override int CastNoAlloc(Vector3 origin, Quaternion orientation, Vector3 direction, float distance, int layerMask, RaycastHit[] results)
	{
		return Physics.SphereCastNonAlloc(origin, colliderRadius, direction, results, distance, layerMask, QueryTriggerInteraction.Ignore);
	}

	protected override Vector3 GetColliderNormal(Vector3 origin, Quaternion orientation, Vector3 collisionPoint)
	{
		return (collisionPoint - origin).normalized;
	}
}
