using UnityEngine;

public class PersonalMuteZone : MonoBehaviour
{
	[SerializeField]
	private BoxCollider boxCollider;

	private Player thisPlayer;

	private Vector3 boxSize = Vector3.one;

	[Tooltip("Delay before mute is active, this is to filter out quick swipe of hand in front of face without meaning to mute.")]
	[SerializeField]
	private float muteDelay = 1f;

	private bool isHandMuted;

	private float inMuteZoneDuration;

	private const int MAX_RAYCAST_HIT = 256;

	private static Collider[] hits = new Collider[256];

	private void Awake()
	{
		thisPlayer = GetComponentInParent<Player>();
		boxSize = boxCollider.size.MultiplyComponents(boxCollider.transform.lossyScale) / 2f;
		base.enabled = thisPlayer.isLocal;
	}

	private void FixedUpdate()
	{
		if (SingletonMonoBehaviour<SettingsManager>.Instance.VoiceChat != VoiceChat.AlwaysOn)
		{
			return;
		}
		bool flag = false;
		Vector3 center = boxCollider.transform.TransformPoint(boxCollider.center);
		int num = Physics.OverlapBoxNonAlloc(center, boxSize, hits, boxCollider.transform.rotation, 1049088);
		for (int i = 0; i < num; i++)
		{
			if (flag)
			{
				break;
			}
			flag |= IsHandInMuteZone(hits[i]);
		}
		if (flag)
		{
			inMuteZoneDuration += Time.deltaTime;
			if (!thisPlayer.Mute && inMuteZoneDuration >= muteDelay)
			{
				isHandMuted = true;
				Player.LocalPlayer.Mute = true;
				ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "Muted", 1.5f);
			}
		}
		else
		{
			inMuteZoneDuration = 0f;
			if (isHandMuted)
			{
				Player.LocalPlayer.Mute = false;
				isHandMuted = false;
				ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "Unmuted", 1.5f);
			}
		}
	}

	private bool IsHandInMuteZone(Collider col)
	{
		Player.BodyPart bodyPart;
		Player colliderPlayer = col.GetColliderPlayer(out bodyPart);
		if (colliderPlayer != null && colliderPlayer.isLocal && bodyPart.IsHand())
		{
			PlayerHand hand = colliderPlayer.GetHand(bodyPart);
			if (hand != null && hand.IsVisible)
			{
				return true;
			}
		}
		return false;
	}
}
