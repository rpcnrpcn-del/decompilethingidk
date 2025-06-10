using System;
using UnityEngine;

[Serializable]
public class ColorValueCurve : ValueCurve<Color>
{
	protected override Color Interpolate(Color lhs, Color rhs, float t)
	{
		return Color.Lerp(lhs, rhs, t);
	}
}
