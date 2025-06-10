using UnityEngine;

public class AdjustablePodiumPickup : RigidbodyPickup
{
	[SerializeField]
	private float localMinYOffset = -0.9f;

	[SerializeField]
	private float localMaxYOffset;

	[SerializeField]
	private Transform podiumRoot;

	public override Vector3 TargetRigidbodyPosition
	{
		get
		{
			if (pickupHand == null || podiumRoot == null)
			{
				return base.TargetRigidbodyPosition;
			}
			Vector3 position = podiumRoot.InverseTransformPoint(base.TargetRigidbodyPosition);
			float y = podiumRoot.InverseTransformPoint(pickupHand.position).y;
			float y2 = Mathf.Clamp(y, localMinYOffset, localMaxYOffset);
			position.y = y2;
			position.x = 0f;
			position.z = 0f;
			return podiumRoot.TransformPoint(position);
		}
	}

	public override Quaternion TargetRigidbodyRotation
	{
		get
		{
			return podiumRoot.transform.rotation;
		}
	}

	private void Start()
	{
		base.Rigidbody.isKinematic = true;
	}

	public override void Release()
	{
		base.Release();
		base.Rigidbody.isKinematic = true;
	}
}
