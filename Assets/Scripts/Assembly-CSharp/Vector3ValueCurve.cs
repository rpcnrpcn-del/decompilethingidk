using System;
using UnityEngine;

[Serializable]
public class Vector3ValueCurve : ValueCurve<Vector3>
{
	protected override Vector3 Interpolate(Vector3 lhs, Vector3 rhs, float t)
	{
		return Vector3.Lerp(lhs, rhs, t);
	}
}
