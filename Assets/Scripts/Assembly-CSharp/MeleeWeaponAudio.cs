using UnityEngine;

public class MeleeWeaponAudio : ToolAudio
{
	[SerializeField]
	private RecRoomAudioClip onSwing;

	[SerializeField]
	private RecRoomAudioClip onHit;

	public void OnSwing(Transform audioPlayTransform)
	{
		AudioManager.Play3DSFX(onSwing, audioPlayTransform);
	}

	public void OnHit(Vector3 hitPoint)
	{
		AudioManager.Play3DSFX(onHit, hitPoint);
	}
}
