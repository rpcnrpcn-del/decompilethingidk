using UnityEngine;

public class AxisConstrainedRigidbodyPickup : RigidbodyPickup
{
	public enum AllowedMovement
	{
		All = 2,
		None = 0,
		Positive = 1,
		Negative = -1
	}

	[Header("Movement constraints")]
	public AllowedMovement allowMovementOnX = AllowedMovement.All;

	public AllowedMovement allowMovementOnY = AllowedMovement.All;

	public AllowedMovement allowMovementOnZ = AllowedMovement.All;

	[Header("Rotation constraints")]
	public bool allowRotationOnX = true;

	public bool allowRotationOnY = true;

	public bool allowRotationOnZ = true;

	[Header("Spherical constraints")]
	public bool constrainToSphere;

	public Vector3 sphericalRestrictionCenter;

	public float sphericalRestrictionRadius;

	public override Vector3 TargetRigidbodyPosition
	{
		get
		{
			Vector3 position = base.transform.InverseTransformPoint(base.TargetRigidbodyPosition);
			if (allowMovementOnX != AllowedMovement.All)
			{
				if (allowMovementOnX == AllowedMovement.None)
				{
					position.x = 0f;
				}
				else
				{
					position.x = ((allowMovementOnX != AllowedMovement.Negative) ? Mathf.Max(0f, position.x) : Mathf.Min(0f, position.x));
				}
			}
			if (allowMovementOnY != AllowedMovement.All)
			{
				if (allowMovementOnY == AllowedMovement.None)
				{
					position.y = 0f;
				}
				else
				{
					position.y = ((allowMovementOnY != AllowedMovement.Negative) ? Mathf.Max(0f, position.y) : Mathf.Min(0f, position.y));
				}
			}
			if (allowMovementOnZ != AllowedMovement.All)
			{
				if (allowMovementOnZ == AllowedMovement.None)
				{
					position.z = 0f;
				}
				else
				{
					position.z = ((allowMovementOnZ != AllowedMovement.Negative) ? Mathf.Max(0f, position.z) : Mathf.Min(0f, position.z));
				}
			}
			position = base.transform.TransformPoint(position);
			if (constrainToSphere)
			{
				Vector3 vector = position - sphericalRestrictionCenter;
				if (vector.sqrMagnitude > sphericalRestrictionRadius * sphericalRestrictionRadius)
				{
					position = sphericalRestrictionCenter + vector.normalized * sphericalRestrictionRadius;
				}
			}
			return position;
		}
	}

	public override Quaternion TargetRigidbodyRotation
	{
		get
		{
			Vector3 eulerAngles = base.transform.InverseTransformRotation(base.TargetRigidbodyRotation).eulerAngles;
			if (!allowRotationOnX)
			{
				eulerAngles.x = 0f;
			}
			if (!allowRotationOnY)
			{
				eulerAngles.y = 0f;
			}
			if (!allowRotationOnZ)
			{
				eulerAngles.z = 0f;
			}
			return base.transform.TransformRotation(Quaternion.Euler(eulerAngles));
		}
	}

	public void SetAllConstraints(bool constrain)
	{
		AllowedMovement allowedMovement = ((!constrain) ? AllowedMovement.All : AllowedMovement.None);
		allowMovementOnX = (allowMovementOnY = (allowMovementOnZ = allowedMovement));
		allowRotationOnX = (allowRotationOnY = (allowRotationOnZ = !constrain));
		constrainToSphere = constrain;
	}
}
