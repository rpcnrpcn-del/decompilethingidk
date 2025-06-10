using UnityEngine;

public abstract class CastCollider
{
	protected Collider collider;

	protected Rigidbody rigidbody;

	private Vector3 positionRigidbodySpace;

	private Quaternion rotationRigidbodySpace;

	private const int MAX_RAYCAST_HITS = 256;

	private static RaycastHit[] hits = new RaycastHit[256];

	public CastCollider(Rigidbody rigidbody, Collider collider, Vector3 colliderCenter)
	{
		this.rigidbody = rigidbody;
		this.collider = collider;
		Vector3 position = this.collider.transform.TransformPoint(colliderCenter);
		positionRigidbodySpace = this.rigidbody.transform.InverseTransformPoint(position);
		rotationRigidbodySpace = this.rigidbody.transform.InverseTransformRotation(collider.transform.rotation);
	}

	public int Cast(Vector3 initialPosition, Quaternion orientation, Vector3 direction, float distance, int layerMask, CastColliderHit[] results)
	{
		int num = 0;
		initialPosition = Matrix4x4.TRS(initialPosition, orientation, rigidbody.transform.lossyScale).MultiplyPoint(positionRigidbodySpace);
		orientation *= rotationRigidbodySpace;
		if (collider.gameObject.activeSelf && collider.enabled)
		{
			int num2 = CastNoAlloc(initialPosition, orientation, direction, distance, layerMask, hits);
			for (int i = 0; i < num2; i++)
			{
				if (num >= results.Length)
				{
					break;
				}
				if (hits[i].collider.enabled && hits[i].collider.gameObject.activeSelf && (!(hits[i].point.magnitude < Mathf.Epsilon) || !(hits[i].distance < Mathf.Epsilon)))
				{
					results[num].Rigidbody = hits[i].rigidbody;
					results[num].Collider = hits[i].collider;
					results[num].Point = hits[i].point;
					results[num].Distance = hits[i].distance;
					results[num].Normal = hits[i].normal;
					results[num].CastColliderNormal = GetColliderNormal(initialPosition, orientation, initialPosition + direction * hits[i].distance);
					results[num].CastCollider = collider;
					num++;
				}
			}
		}
		return num;
	}

	protected abstract int CastNoAlloc(Vector3 origin, Quaternion orientation, Vector3 direction, float distance, int layerMask, RaycastHit[] results);

	protected abstract Vector3 GetColliderNormal(Vector3 origin, Quaternion orientation, Vector3 collisionPoint);
}
