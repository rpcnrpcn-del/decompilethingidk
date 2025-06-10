using System;
using UnityEngine;

[Serializable]
public class InvisibleWallVisualConfig : ScriptableObject
{
	public float ImpactDuration = 1f;

	public float MinImpactRadius = 0.1f;

	public float MaxImpactRadius = 1f;

	public float ForceToImpactRadius = 0.12f;

	public AnimationCurve ImpactRadiusGrowthCurve;

	public AnimationCurve ImpactAlphaCurve;
}
