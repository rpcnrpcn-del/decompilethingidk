using Photon;
using UnityEngine;

public class PaddleballGoal : Photon.MonoBehaviour
{
	public delegate void Score(PaddleballGoal thisGoal, Vector3 scorePoint);

	[SerializeField]
	private GameTeam team = GameTeam.INVALID;

	public Score ScoreEvent;

	public GameTeam Team
	{
		get
		{
			return team;
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (base.hasAuthority)
		{
			Tool colliderTool = collider.GetColliderTool();
			PaddleballBall paddleballBall = ((!(colliderTool != null)) ? null : colliderTool.GetComponent<PaddleballBall>());
			if (paddleballBall != null && ScoreEvent != null)
			{
				ScoreEvent(this, paddleballBall.transform.position);
			}
		}
	}
}
