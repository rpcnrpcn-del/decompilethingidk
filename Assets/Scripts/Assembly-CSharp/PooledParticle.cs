using UnityEngine;

public class PooledParticle : MonoBehaviour
{
	private float maxDuration;

	public ParticleSystem PrimaryParticleSystem { get; private set; }

	public ParticleSystem[] AllParticleSystems { get; private set; }

	private void Awake()
	{
		PrimaryParticleSystem = GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main = PrimaryParticleSystem.main;
		main.playOnAwake = false;
		AllParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
		maxDuration = 0f;
		ParticleSystem[] allParticleSystems = AllParticleSystems;
		foreach (ParticleSystem particleSystem in allParticleSystems)
		{
			maxDuration = Mathf.Max(maxDuration, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
		}
	}

	private void DeferredHide()
	{
		ObjectPool.Instance.Release(this);
	}

	public void Play()
	{
		PrimaryParticleSystem.Play();
		Invoke("DeferredHide", maxDuration);
	}
}
