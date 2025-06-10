using UnityEngine;

public class TreeLeafTrigger : MonoBehaviour
{
	[SerializeField]
	private PooledParticle leafParticlesPrefab;

	[SerializeField]
	private bool playParticles = true;

	private void OnTriggerEnter(Collider collider)
	{
		if (playParticles && leafParticlesPrefab != null)
		{
			PooledParticle pooledParticle = ObjectPool.Instance.Acquire(leafParticlesPrefab);
			if (pooledParticle != null)
			{
				pooledParticle.transform.position = collider.transform.position;
				pooledParticle.transform.rotation = Quaternion.identity;
				pooledParticle.Play();
			}
		}
	}
}
