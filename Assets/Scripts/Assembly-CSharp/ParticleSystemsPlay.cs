using UnityEngine;

public class ParticleSystemsPlay : MonoBehaviour
{
	[SerializeField]
	private bool destroyOnDone = true;

	[SerializeField]
	private float destroyLifeTimeOverride = 5f;

	private void Start()
	{
		float num = destroyLifeTimeOverride;
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			num = Mathf.Max(num, particleSystem.main.startLifetime.constantMax);
			particleSystem.Clear();
			particleSystem.Play();
		}
		if (destroyOnDone)
		{
			Object.Destroy(base.gameObject, num);
		}
	}
}
