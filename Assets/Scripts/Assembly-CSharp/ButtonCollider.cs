using UnityEngine;
using UnityEngine.UI;

public class ButtonCollider : MonoBehaviour
{
	public Button Button;

	public float consequtiveClickInterval = 1f;

	private float lastTriggerEnterTime;

	private void OnTriggerEnter(Collider collider)
	{
		if (!Button.IsInteractable() || !(Time.time - lastTriggerEnterTime > consequtiveClickInterval))
		{
			return;
		}
		Player.BodyPart bodyPart;
		Player colliderPlayer = collider.GetColliderPlayer(out bodyPart);
		if (colliderPlayer != null && bodyPart.IsHand())
		{
			PlayerHand playerHand = ((bodyPart != Player.BodyPart.LeftHand) ? colliderPlayer.RightHand : colliderPlayer.LeftHand);
			Button.Select();
			if (playerHand != null && playerHand.IsVisible)
			{
				lastTriggerEnterTime = Time.time;
				Button.onClick.Invoke();
			}
		}
	}
}
