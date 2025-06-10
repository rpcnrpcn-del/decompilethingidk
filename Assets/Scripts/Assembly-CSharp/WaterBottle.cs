using UnityEngine;

public class WaterBottle : Tool
{
	[SerializeField]
	private ParticleSystem waterParticles;

	[SerializeField]
	private float lifeSpan = 10f;

	private float defaultLifeSpan = 5f;

	protected override void Start()
	{
		base.Start();
		defaultLifeSpan = lifeSpan;
	}

	private void Update()
	{
		ParticleSystem.EmissionModule emission = waterParticles.emission;
		bool flag = Vector3.Dot(base.transform.up, Vector3.up) < 0f;
		bool flag2 = lifeSpan > 0f && flag;
		if (emission.enabled != flag2)
		{
			emission.enabled = flag2;
		}
		if (flag2)
		{
			lifeSpan -= Time.deltaTime;
		}
		else if (!flag && lifeSpan <= 0f)
		{
			lifeSpan = defaultLifeSpan;
		}
	}
}
