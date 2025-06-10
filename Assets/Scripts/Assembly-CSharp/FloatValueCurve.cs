using System;
using UnityEngine;

[Serializable]
public class FloatValueCurve : ValueCurve<float>
{
	protected override float Interpolate(float lhs, float rhs, float t)
	{
		return Mathf.Lerp(lhs, rhs, t);
	}
}
