using UnityEngine;

public class PlayerAfterImageParticles : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem[] particleSystems;

	[SerializeField]
	private float speed = 5f;

	[SerializeField]
	private float emissionRate = 100f;

	[SerializeField]
	private Color defaultColor = new Color(0.45f, 0.45f, 0.45f, 1f);

	public Color Color
	{
		set
		{
			ParticleSystem[] array = particleSystems;
			foreach (ParticleSystem particleSystem in array)
			{
				ParticleSystem.MainModule main = particleSystem.main;
				main.startColor = value;
			}
		}
	}

	public void Run(Vector3 origin, Vector3 destination)
	{
		base.transform.position = origin;
		Vector3 vector = destination - origin;
		base.transform.rotation = Quaternion.LookRotation(-vector.normalized);
		float num = vector.magnitude / speed;
		ParticleSystem[] array = particleSystems;
		foreach (ParticleSystem particleSystem in array)
		{
			ParticleSystem.MainModule main = particleSystem.main;
			main.startLifetime = num;
			main.startSpeed = speed;
			particleSystem.Play();
		}
		Object.Destroy(base.gameObject, num);
	}

	private void Awake()
	{
		ParticleSystem[] array = particleSystems;
		foreach (ParticleSystem particleSystem in array)
		{
			ParticleSystem.EmissionModule emission = particleSystem.emission;
			emission.rateOverTime = new ParticleSystem.MinMaxCurve
			{
				constantMax = emissionRate
			};
		}
		Color = defaultColor;
	}
}
