using UnityEngine;

public class Dartboard : MonoBehaviour
{
	public float bullseyeRadius = 0.07f;

	public float innerRadius = 0.15f;

	public float outerRadius = 0.3f;

	public PooledParticle HitParticleOuter;

	public PooledParticle HitParticleInner;

	public PooledParticle HitParticleBullseye;

	public RecRoomAudioClip HitAudio;

	private void OnTriggerEnter(Collider collider)
	{
		Tool colliderTool = collider.GetColliderTool();
		Dart dart = ((!(colliderTool != null)) ? null : colliderTool.GetComponent<Dart>());
		if (!(dart != null))
		{
			return;
		}
		float num = Vector2.Distance(base.transform.position, collider.transform.position);
		PooledParticle pooledParticle = ObjectPool.Instance.Acquire(HitParticleOuter);
		if (num < bullseyeRadius)
		{
			pooledParticle = ObjectPool.Instance.Acquire(HitParticleBullseye);
		}
		else if (num < innerRadius)
		{
			pooledParticle = ObjectPool.Instance.Acquire(HitParticleInner);
		}
		if (num < outerRadius)
		{
			if (pooledParticle != null)
			{
				pooledParticle.transform.position = collider.transform.position;
				pooledParticle.transform.rotation = collider.transform.rotation;
				pooledParticle.Play();
			}
			AudioManager.Play3DSFX(HitAudio, base.transform);
		}
	}
}
