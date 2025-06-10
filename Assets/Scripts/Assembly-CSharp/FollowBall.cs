using UnityEngine;

public class FollowBall : MonoBehaviour
{
	[SerializeField]
	private GameObject ball;

	private Vector3 offset;

	private void Start()
	{
		offset = base.transform.position - ball.transform.position;
	}

	private void LateUpdate()
	{
		base.transform.position = ball.transform.position + offset;
	}
}
