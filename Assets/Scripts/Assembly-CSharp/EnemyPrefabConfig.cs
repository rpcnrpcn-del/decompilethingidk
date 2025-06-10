using System;
using UnityEngine;

[Serializable]
public class EnemyPrefabConfig : ScriptableObject
{
	[Serializable]
	public struct EnemyConfig
	{
		public EnemyType Type;

		public Enemy Prefab;
	}

	[SerializeField]
	public EnemyConfig[] EnemyConfigs;
}
