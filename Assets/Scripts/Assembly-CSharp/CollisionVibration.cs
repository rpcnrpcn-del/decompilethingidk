using UnityEngine;

public class CollisionVibration : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
		PlayerHand playerHand = null;
		Player.BodyPart bodyPart;
		Player colliderPlayer = collision.GetColliderPlayer(out bodyPart);
		Tool colliderTool = collision.GetColliderTool();
		if (colliderPlayer != null)
		{
			switch (bodyPart)
			{
			case Player.BodyPart.LeftHand:
				playerHand = colliderPlayer.LeftHand;
				break;
			case Player.BodyPart.RightHand:
				playerHand = colliderPlayer.RightHand;
				break;
			}
		}
		else if (colliderTool != null && colliderTool.IsHeld && colliderTool.Owner != null)
		{
			if (colliderTool.Owner.LeftHand.Tool == colliderTool)
			{
				playerHand = colliderTool.Owner.LeftHand;
			}
			else if (colliderTool.Owner.RightHand.Tool == colliderTool)
			{
				playerHand = colliderTool.Owner.RightHand;
			}
		}
		if (playerHand != null)
		{
			playerHand.Vibrate(50, 1000);
		}
	}
}
