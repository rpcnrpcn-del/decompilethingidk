using UnityEngine;

public class Killzone : MonoBehaviour
{
	public enum ToolSpawnLocation
	{
		LAST_SPAWN_POINT = 0,
		LAST_PICKUP_POINT = 1
	}

	[SerializeField]
	private bool killOnEnter = true;

	[SerializeField]
	private ToolSpawnLocation respawnMode = ToolSpawnLocation.LAST_PICKUP_POINT;

	public bool KillOnEnter
	{
		get
		{
			return killOnEnter;
		}
	}

	public ToolSpawnLocation ToolRespawnLocation
	{
		get
		{
			return respawnMode;
		}
	}
}
