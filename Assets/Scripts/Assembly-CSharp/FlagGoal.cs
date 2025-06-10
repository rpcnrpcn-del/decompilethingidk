using UnityEngine;

public class FlagGoal : MonoBehaviour
{
	[SerializeField]
	private GameTeam team = GameTeam.INVALID;

	[SerializeField]
	private float radius = 0.5f;

	public GameTeam Team
	{
		get
		{
			return team;
		}
	}

	public float Radius
	{
		get
		{
			return radius;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position, radius);
	}
}
