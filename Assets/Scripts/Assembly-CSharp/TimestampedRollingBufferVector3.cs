using UnityEngine;

public class TimestampedRollingBufferVector3 : TimestampedRollingBuffer<Vector3>
{
	protected override Vector3 ZeroValue()
	{
		return Vector3.zero;
	}

	protected override Vector3 Interpolate(Vector3 lhs, Vector3 rhs, float t)
	{
		return Vector3.Lerp(lhs, rhs, t);
	}

	protected override Vector3 Scale(Vector3 value, float t)
	{
		return value * t;
	}

	protected override Vector3 Sum(Vector3 lhs, Vector3 rhs)
	{
		return lhs + rhs;
	}
}
