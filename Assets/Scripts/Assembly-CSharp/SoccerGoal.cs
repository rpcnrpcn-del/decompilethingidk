using Photon;
using UnityEngine;

public class SoccerGoal : Photon.MonoBehaviour
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
