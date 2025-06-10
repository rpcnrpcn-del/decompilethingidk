using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AutoDestroyParticleSystem : MonoBehaviour
{
	private ParticleSystem effect;

	private void Awake()
	{
		effect = GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		if (!effect || !effect.isPlaying)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
