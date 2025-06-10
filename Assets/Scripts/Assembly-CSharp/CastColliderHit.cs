using UnityEngine;

public struct CastColliderHit
{
	public Rigidbody Rigidbody;

	public Collider Collider;

	public Vector3 Point;

	public float Distance;

	public Vector3 Normal;

	public Vector3 CastColliderNormal;

	public Collider CastCollider;
}
