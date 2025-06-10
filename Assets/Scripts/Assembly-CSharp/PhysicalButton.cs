using UnityEngine;

public class PhysicalButton : MonoBehaviour
{
	public delegate void Push(PhysicalButton thisButton, Player pushPlayer);

	[SerializeField]
	private float instanceLimitDuration = 0.5f;

	[SerializeField]
	private bool canOnlyPushWithHand = true;

	[SerializeField]
	private RecRoomAudioClip onButtonPressAudio;

	private float lastButtonPressTime;

	public event Push PushEvent;

	private void OnTriggerEnter(Collider collider)
	{
		Player.BodyPart bodyPart;
		Player colliderPlayer = collider.GetColliderPlayer(out bodyPart);
		if (colliderPlayer != null && (!canOnlyPushWithHand || bodyPart == Player.BodyPart.LeftHand || bodyPart == Player.BodyPart.RightHand) && Time.time - lastButtonPressTime > instanceLimitDuration)
		{
			lastButtonPressTime = Time.time;
			if (onButtonPressAudio != null)
			{
				AudioManager.Play3DSFX(onButtonPressAudio, base.transform.position);
			}
			if (this.PushEvent != null)
			{
				this.PushEvent(this, colliderPlayer);
			}
		}
	}
}
