using UnityEngine;

public class DiscGolfTee : MonoBehaviour
{
	[SerializeField]
	private GameTeam team = GameTeam.INVALID;

	[SerializeField]
	private DiscGolfHole hole = DiscGolfHole.INVALID;

	public GameTeam Team
	{
		get
		{
			return team;
		}
	}

	public DiscGolfHole Hole
	{
		get
		{
			return hole;
		}
	}
}
