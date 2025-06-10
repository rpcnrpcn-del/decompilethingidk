using UnityEngine;

[DisallowMultipleComponent]
public class ToolAudio : MonoBehaviour
{
	[SerializeField]
	private RecRoomAudioClip onPickup;

	[SerializeField]
	private RecRoomAudioClip onRelease;

	[SerializeField]
	private RecRoomAudioClip onDrop;

	[SerializeField]
	private RecRoomAudioClip onReset;

	[SerializeField]
	private RecRoomAudioClip onApplyForceLocal;

	[SerializeField]
	private RecRoomAudioClip onApplyForceRemote;

	private float lastApplyForceTime;

	public void OnPickup()
	{
		AudioManager.Play3DSFX(onPickup, base.transform.position);
	}

	public void OnRelease(Vector3 releaseVelocity)
	{
		RecRoomAudioClip clip = ((!(releaseVelocity.magnitude < 0.5f)) ? onRelease : onDrop);
		AudioManager.Play3DSFX(clip, base.transform.position);
	}

	public void OnReset()
	{
		AudioManager.Play3DSFX(onReset, base.transform.position);
	}

	public void OnApplyForce(PhotonPlayer photonPlayer, Vector3 position)
	{
		if (Time.time - lastApplyForceTime > 0.25f)
		{
			lastApplyForceTime = Time.time;
			AudioManager.Play3DSFX((photonPlayer == null || !photonPlayer.isLocal) ? onApplyForceRemote : onApplyForceLocal, position);
		}
	}
}
