using System;
using UnityEngine;

public class CollisionParticles : MonoBehaviour
{
	[Serializable]
	public class ParticlesToLayer
	{
		public string Name;

		public ParticleSystem ParticleSystemPrefab;

		public LayerMask Layers;

		public string Tag = string.Empty;

		[Tooltip("Per particle cooldown to limit the number of consequtive plays. Set it to the particle duration")]
		public float InstanceLimitDuration;

		private ParticleSystem particleSystemInstance;

		public float LastInstanceTime { get; set; }

		public ParticleSystem GetParticleSystem()
		{
			if (particleSystemInstance == null)
			{
				particleSystemInstance = UnityEngine.Object.Instantiate(ParticleSystemPrefab);
			}
			return particleSystemInstance;
		}

		public bool CanPlay(GameObject go, LayerMask layerMask)
		{
			if (Time.time - LastInstanceTime > InstanceLimitDuration && (Layers.value & (int)layerMask) == (int)layerMask && (string.IsNullOrEmpty(Tag) || go.CompareTag(Tag)))
			{
				LastInstanceTime = Time.time;
				return true;
			}
			return false;
		}
	}

	private Tool thisTool;

	[SerializeField]
	private ParticlesToLayer[] particleMappings;

	private void Awake()
	{
		thisTool = GetComponent<Tool>();
	}

	private void OnDestroy()
	{
		ParticlesToLayer[] array = particleMappings;
		foreach (ParticlesToLayer particlesToLayer in array)
		{
			ParticleSystem particleSystem = particlesToLayer.GetParticleSystem();
			if (particleSystem != null)
			{
				particleSystem.Stop();
				UnityEngine.Object.Destroy(particleSystem.gameObject);
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (thisTool != null && thisTool.IsHeld)
		{
			return;
		}
		int num = 1 << collision.gameObject.layer;
		ParticlesToLayer[] array = particleMappings;
		foreach (ParticlesToLayer particlesToLayer in array)
		{
			if (particlesToLayer.CanPlay(collision.gameObject, num))
			{
				ParticleSystem particleSystem = particlesToLayer.GetParticleSystem();
				if (particleSystem != null)
				{
					particleSystem.Stop();
					Debug.Log("OnCollision enter" + collision);
					particleSystem.transform.position = base.transform.position;
					particleSystem.Play();
				}
				break;
			}
		}
	}
}
