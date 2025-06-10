using UnityEngine;

public class BoxCastCollider : CastCollider
{
	private Vector3 colliderSize = Vector3.one;

	private Vector3[] localSpaceNormals;

	private float[] sideExtents;

	public BoxCastCollider(Rigidbody rigidbody, Collider collider, Vector3 colliderCenter)
		: base(rigidbody, collider, colliderCenter)
	{
		colliderSize = Vector3.Scale(collider.transform.lossyScale, (collider as BoxCollider).size);
		localSpaceNormals = new Vector3[6];
		localSpaceNormals[0] = Vector3.right;
		localSpaceNormals[1] = -Vector3.right;
		localSpaceNormals[2] = Vector3.up;
		localSpaceNormals[3] = -Vector3.up;
		localSpaceNormals[4] = Vector3.forward;
		localSpaceNormals[5] = -Vector3.forward;
		sideExtents = new float[6];
		sideExtents[0] = colliderSize.x / 2f;
		sideExtents[1] = sideExtents[0];
		sideExtents[2] = colliderSize.y / 2f;
		sideExtents[3] = sideExtents[2];
		sideExtents[4] = colliderSize.z / 2f;
		sideExtents[5] = sideExtents[4];
	}

	protected override int CastNoAlloc(Vector3 origin, Quaternion orientation, Vector3 direction, float distance, int layerMask, RaycastHit[] results)
	{
		return Physics.BoxCastNonAlloc(origin, colliderSize / 2f, direction, results, orientation, distance, layerMask, QueryTriggerInteraction.Ignore);
	}

	protected override Vector3 GetColliderNormal(Vector3 origin, Quaternion orientation, Vector3 collisionPoint)
	{
		Vector3 vector = collisionPoint - origin;
		Vector3 lhs = Quaternion.Inverse(orientation) * vector;
		int num = -1;
		float num2 = float.MinValue;
		for (int i = 0; i < localSpaceNormals.Length; i++)
		{
			float num3 = Vector3.Dot(lhs, localSpaceNormals[i]) - sideExtents[i];
			if (num3 > num2)
			{
				num = i;
				num2 = num3;
			}
		}
		return orientation * localSpaceNormals[num];
	}
}
