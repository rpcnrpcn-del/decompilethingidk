using UnityEngine;

public class WheelConstraintRigidbodyPickup : RigidbodyPickup
{
	[SerializeField]
	private Transform constraintRoot;

	[SerializeField]
	private float minRotationAngle = 45f;

	[SerializeField]
	private float maxRotationAngle = 135f;

	private float lastValidRollAngle;

	private float lastValidRollAngleSign = 1f;

	private float pickupRollOffset;

	private Transform targetTransform
	{
		get
		{
			if (constraintRoot != null)
			{
				return constraintRoot;
			}
			return base.Rigidbody.transform;
		}
	}

	public override Vector3 TargetRigidbodyPosition
	{
		get
		{
			return targetTransform.position;
		}
	}

	public override Quaternion TargetRigidbodyRotation
	{
		get
		{
			if (pickupHand == null)
			{
				return targetTransform.rotation;
			}
			return targetTransform.TransformRotation(ConstrainedTargetRotation());
		}
	}

	private void Start()
	{
		base.Rigidbody.isKinematic = true;
		StartAttachToHandCoroutine();
	}

	public override void Pickup(Rigidbody hand, Vector3 handSpacePickupPosition, Quaternion handSpacePickupRotation)
	{
		pickupHand = hand;
		float rollAngle = GetRollAngle(base.Rigidbody.transform.up);
		float rollAngle2 = GetRollAngle(GetCurrentHandDirection());
		pickupRollOffset = rollAngle2 - rollAngle;
		lastValidRollAngleSign = Mathf.Sign(rollAngle2);
	}

	public override void Release()
	{
		pickupHand = null;
	}

	private Quaternion ConstrainedTargetRotation()
	{
		float rollAngle = GetRollAngle(GetCurrentHandDirection());
		float num = rollAngle - pickupRollOffset;
		if (num >= minRotationAngle && num <= maxRotationAngle && Mathf.Sign(rollAngle) == lastValidRollAngleSign)
		{
			lastValidRollAngle = num;
		}
		return Quaternion.AngleAxis(lastValidRollAngle, Vector3.forward);
	}

	private Vector3 GetCurrentHandDirection()
	{
		Vector3 result = pickupHand.position - targetTransform.position;
		if (result.sqrMagnitude <= Mathf.Epsilon)
		{
			return base.Rigidbody.transform.up;
		}
		return result;
	}

	private float GetRollAngle(Vector3 direction)
	{
		return Vector3.up.AngleSignedVector3(targetTransform.InverseTransformDirection(direction).normalized, Vector3.forward);
	}
}
