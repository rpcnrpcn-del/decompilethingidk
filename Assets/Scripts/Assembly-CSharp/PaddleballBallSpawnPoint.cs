using UnityEngine;

public class PaddleballBallSpawnPoint : MonoBehaviour
{
	[SerializeField]
	private GameTeam team = GameTeam.INVALID;

	public GameTeam Team
	{
		get
		{
			return team;
		}
	}
}
