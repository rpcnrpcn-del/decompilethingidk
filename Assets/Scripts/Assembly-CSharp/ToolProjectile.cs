using UnityEngine;

public class ToolProjectile : Tool
{
	[SerializeField]
	private Transform colliderOriginTransform;

	[SerializeField]
	private float colliderRadius = 0.05f;

	private BulletTrail trail;

	protected override void Awake()
	{
		base.Awake();
		base.enabled = false;
		trail = GetComponentInChildren<BulletTrail>();
		if (trail != null)
		{
			trail.enabled = false;
		}
	}
}
