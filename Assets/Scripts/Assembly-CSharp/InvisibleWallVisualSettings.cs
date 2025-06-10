using UnityEngine;

public static class InvisibleWallVisualSettings
{
	private static InvisibleWallVisualConfig ConfigAsset = CustomSettingsLoader.LoadConfig<InvisibleWallVisualConfig>();

	private const string ConfigsAssetFilePath = "Assets/Core/Content/Configs/";

	public static float ImpactDuration
	{
		get
		{
			if (ConfigAsset != null)
			{
				return ConfigAsset.ImpactDuration;
			}
			return 0f;
		}
	}

	public static float ConvertImpactVelocityToImpactRadius(Vector3 impactVelocity)
	{
		if (ConfigAsset != null)
		{
			return Mathf.Clamp(impactVelocity.magnitude * ConfigAsset.ForceToImpactRadius, ConfigAsset.MinImpactRadius, ConfigAsset.MaxImpactRadius);
		}
		return 0f;
	}

	public static float EvaluateImpactGrowthCurve(float t)
	{
		if (ConfigAsset != null)
		{
			return ConfigAsset.ImpactRadiusGrowthCurve.Evaluate(t);
		}
		return 0f;
	}

	public static float EvaluateImpactAlphaCurve(float t)
	{
		if (ConfigAsset != null)
		{
			return ConfigAsset.ImpactAlphaCurve.Evaluate(t);
		}
		return 0f;
	}
}
