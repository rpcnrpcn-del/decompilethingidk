using UnityEngine;

public class SceneSpawnPoint : SpawnPoint
{
	[SerializeField]
	private string previousActivityName;

	public string SupportedPreviousActivityName
	{
		get
		{
			return previousActivityName;
		}
	}

	protected override Color GizmoColor
	{
		get
		{
			return Color.red;
		}
	}
}
