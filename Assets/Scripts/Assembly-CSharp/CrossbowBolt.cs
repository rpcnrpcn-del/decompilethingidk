using UnityEngine;

public class CrossbowBolt : Bullet
{
	[SerializeField]
	private Transform colliderOriginTransform;

	public ParticleSystem maxChargedParticleTrail;

	protected override Vector3 ColliderOrigin
	{
		get
		{
			return (!(colliderOriginTransform != null)) ? base.transform.position : colliderOriginTransform.position;
		}
	}
}
