using UnityEngine;

public class Pistol : Gun
{
	protected override void FireShot(Vector3 position, Quaternion rotation, Vector3 direction, float chargeAmount, bool isMisfire)
	{
		base.FireShot(position, rotation, direction, chargeAmount, isMisfire);
		if (!isMisfire)
		{
			FireBullet(position, rotation, direction * bulletFireSpeed, 1f);
		}
	}
}
