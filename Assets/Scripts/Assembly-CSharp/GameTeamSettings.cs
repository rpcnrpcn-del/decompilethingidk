using UnityEngine;

public static class GameTeamSettings
{
	private static GameTeamConfig ConfigAsset = CustomSettingsLoader.LoadConfig<GameTeamConfig>();

	private const string ConfigsAssetFilePath = "Assets/Core/Content/Configs/";

	private const string MissingTeamName = "MISSING_TEAM_NAME";

	private static readonly Color MissingTeamColor = Color.white;

	public static Color GetTeamColor(GameTeam team)
	{
		if (ConfigAsset != null)
		{
			for (int i = 0; i < ConfigAsset.TeamConfigs.Length; i++)
			{
				if (ConfigAsset.TeamConfigs[i].Team == team)
				{
					return ConfigAsset.TeamConfigs[i].Color;
				}
			}
		}
		return MissingTeamColor;
	}

	public static string GetTeamName(GameTeam team)
	{
		if (ConfigAsset != null)
		{
			for (int i = 0; i < ConfigAsset.TeamConfigs.Length; i++)
			{
				if (ConfigAsset.TeamConfigs[i].Team == team)
				{
					return ConfigAsset.TeamConfigs[i].Name;
				}
			}
		}
		return "MISSING_TEAM_NAME";
	}
}
