using UnityEngine;

public class PlayerScoreUI : MonoBehaviour
{
	public Player Player;

	[SerializeField]
	private FollowBillboard followBillboard;

	[SerializeField]
	private TextMesh textMesh;

	public int Score
	{
		set
		{
			textMesh.text = "SCORE:" + value;
		}
	}

	public Transform PlayerHead
	{
		set
		{
			followBillboard.Target = value;
		}
	}

	public Camera LocalCamera
	{
		set
		{
			followBillboard.Camera = value;
		}
	}
}
