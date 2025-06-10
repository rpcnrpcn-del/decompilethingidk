using UnityEngine;

public class Shotgun : Gun
{
	[Header("Spray")]
	[SerializeField]
	private float maxDistance = 3f;

	[SerializeField]
	private Transform sprayTargetRoot;

	private Transform[] sprayTargets;

	protected override void Awake()
	{
		base.Awake();
		sprayTargets = new Transform[sprayTargetRoot.childCount];
		for (int i = 0; i < sprayTargetRoot.childCount; i++)
		{
			sprayTargets[i] = sprayTargetRoot.GetChild(i);
		}
	}

	protected override void FireShot(Vector3 position, Quaternion rotation, Vector3 direction, float chargeAmount, bool isMisfire)
	{
		base.FireShot(position, rotation, direction, chargeAmount, isMisfire);
		if (!isMisfire)
		{
			float maxLifetime = maxDistance / bulletFireSpeed;
			sprayTargetRoot.localPosition = new Vector3(0f, 0f, maxDistance);
			for (int i = 0; i < sprayTargets.Length; i++)
			{
				FireBullet(position, rotation, (sprayTargets[i].position - position).normalized * bulletFireSpeed, 1f, maxLifetime);
			}
		}
	}
}
