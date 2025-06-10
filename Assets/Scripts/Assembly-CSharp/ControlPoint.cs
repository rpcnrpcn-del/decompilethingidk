using UnityEngine;

public class ControlPoint
{
	public Vector3 Position = Vector3.zero;

	public Vector3 Tangent = Vector3.zero;

	public Vector3 Normal = Vector3.zero;

	public Vector3 Binormal
	{
		get
		{
			return Vector3.Cross(Tangent, Normal).normalized;
		}
	}

	public ControlPoint()
	{
		Position = Vector3.zero;
		Tangent = Vector3.zero;
		Normal = Vector3.zero;
	}

	public ControlPoint(ControlPoint other)
	{
		Position = other.Position;
		Tangent = other.Tangent;
		Normal = other.Normal;
	}

	public static ControlPoint Interpolate(ControlPoint lhs, ControlPoint rhs, float t)
	{
		Quaternion a = Quaternion.LookRotation(lhs.Tangent, lhs.Normal);
		Quaternion b = Quaternion.LookRotation(rhs.Tangent, rhs.Normal);
		Quaternion quaternion = Quaternion.Slerp(a, b, t);
		Vector3 position = Vector3.Lerp(lhs.Position, rhs.Position, t);
		ControlPoint controlPoint = new ControlPoint();
		controlPoint.Position = position;
		controlPoint.Tangent = quaternion * Vector3.forward;
		controlPoint.Normal = quaternion * Vector3.up;
		return controlPoint;
	}

	public static ControlPoint InterpolatePositionOnly(ControlPoint lhs, ControlPoint rhs, float t)
	{
		Vector3 position = Vector3.Lerp(lhs.Position, rhs.Position, t);
		ControlPoint controlPoint = new ControlPoint();
		controlPoint.Position = position;
		controlPoint.Tangent = lhs.Tangent;
		controlPoint.Normal = lhs.Normal;
		return controlPoint;
	}

	public static ControlPoint InterpolateRotationOnly(ControlPoint lhs, ControlPoint rhs, float t)
	{
		Quaternion a = Quaternion.LookRotation(lhs.Tangent, lhs.Normal);
		Quaternion b = Quaternion.LookRotation(rhs.Tangent, rhs.Normal);
		Quaternion quaternion = Quaternion.Slerp(a, b, t);
		ControlPoint controlPoint = new ControlPoint();
		controlPoint.Position = lhs.Position;
		controlPoint.Tangent = quaternion * Vector3.forward;
		controlPoint.Normal = quaternion * Vector3.up;
		return controlPoint;
	}
}
