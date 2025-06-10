using UnityEngine;

public class Basketball : Ball
{
	[Header("Basketball")]
	public ParticleSystem FireTrail;

	public PooledParticle PassedRimParticle;

	public RecRoomAudioClip PassedRimAudio;

	public void PassedRim()
	{
		AudioManager.Play3DSFX(PassedRimAudio, base.transform);
		PooledParticle pooledParticle = ObjectPool.Instance.Acquire(PassedRimParticle);
		if (pooledParticle != null)
		{
			pooledParticle.transform.position = base.transform.position;
			pooledParticle.transform.rotation = base.transform.rotation;
			pooledParticle.Play();
		}
		if (FireTrail.gameObject.activeSelf)
		{
			CancelInvoke("HideFireTrail");
		}
		FireTrail.gameObject.SetActive(true);
		FireTrail.Play();
		Invoke("HideFireTrail", 1f);
	}

	private void HideFireTrail()
	{
		FireTrail.gameObject.SetActive(false);
	}
}
