using UnityEngine;

public class GunAudio : ToolAudio
{
	[SerializeField]
	private RecRoomAudioClip onFire;

	[SerializeField]
	private RecRoomAudioClip onReload;

	[SerializeField]
	private RecRoomAudioClip onMisfire;

	[SerializeField]
	private RecRoomAudioClip onBulletHitPlayer;

	[SerializeField]
	private RecRoomAudioClip onBulletHitEnvironment;

	public void OnFire()
	{
		AudioManager.Play3DSFX(onFire, base.transform.position);
	}

	public void OnReload()
	{
		AudioManager.Play3DSFX(onReload, base.transform.position);
	}

	public void OnMisfire()
	{
		AudioManager.Play3DSFX(onMisfire, base.transform.position);
	}

	public void OnBulletHitPlayer(Vector3 position)
	{
		AudioManager.Play3DSFX(onBulletHitPlayer, position);
	}

	public void OnBulletHitEnvironment(Vector3 position)
	{
		AudioManager.Play3DSFX(onBulletHitEnvironment, position);
	}
}
