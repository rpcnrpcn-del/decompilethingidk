using System;

public static class EnemyPrefabSettings
{
	private static EnemyPrefabConfig ConfigAsset = CustomSettingsLoader.LoadConfig<EnemyPrefabConfig>();

	private const string ConfigsAssetFilePath = "Assets/Activities/Quest/Configs/";

	private static string[] enemyPrefabNames = null;

	public static string GetEnemyPrefabName(EnemyType enemyType)
	{
		LazyLoadAsset();
		return enemyPrefabNames[(int)enemyType];
	}

	private static void LazyLoadAsset()
	{
		if (enemyPrefabNames != null)
		{
			return;
		}
		EnemyType[] array = (EnemyType[])Enum.GetValues(typeof(EnemyType));
		enemyPrefabNames = new string[array.Length];
		if (ConfigAsset != null && ConfigAsset.EnemyConfigs != null)
		{
			for (int i = 0; i < ConfigAsset.EnemyConfigs.Length; i++)
			{
				int type = (int)ConfigAsset.EnemyConfigs[i].Type;
				enemyPrefabNames[type] = ConfigAsset.EnemyConfigs[i].Prefab.name;
			}
		}
	}
}
