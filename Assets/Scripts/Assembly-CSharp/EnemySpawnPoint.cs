using UnityEngine;

public class EnemySpawnPoint : SpawnPoint
{
	[SerializeField]
	private EnemyType supportedEnemyType = EnemyType.INVALID;

	protected override Color GizmoColor
	{
		get
		{
			return Color.green;
		}
	}

	public bool EnemyTypeIsSupported(EnemyType enemyType)
	{
		return supportedEnemyType == EnemyType.INVALID || enemyType == supportedEnemyType;
	}
}
