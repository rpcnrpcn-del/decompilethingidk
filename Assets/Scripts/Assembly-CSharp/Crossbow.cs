using UnityEngine;

public class Crossbow : Gun
{
	[Header("Crossbow")]
	[SerializeField]
	private FloatValueCurve speedOverDrawTime;

	[SerializeField]
	private Transform boltNockingTransform;

	[SerializeField]
	private Transform maxPullTransform;

	private bool chargingShot;

	private float chargeStartTime;

	private CrossbowBolt chargingBolt;

	private float maxDrawDistance;

	protected override void Awake()
	{
		base.Awake();
		if (boltNockingTransform != null && maxPullTransform != null)
		{
			maxDrawDistance = (maxPullTransform.position - boltNockingTransform.position).magnitude;
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (chargingBolt != null)
		{
			chargingBolt.transform.position = boltNockingTransform.position + (maxPullTransform.position - boltNockingTransform.position) / maxDrawDistance * Mathf.Min(maxDrawDistance, maxDrawDistance * (Time.time - chargeStartTime) / speedOverDrawTime.Duration);
			chargingBolt.transform.rotation = boltNockingTransform.rotation;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (chargingShot && Time.time - chargeStartTime >= speedOverDrawTime.Duration && Time.time - chargeStartTime - Time.fixedDeltaTime < speedOverDrawTime.Duration)
		{
			base.HolderHand.Vibrate(500, 1000);
		}
	}

	public override void OnInputDown()
	{
		if (base.hasAuthority)
		{
			if (maximumFiringRate >= 0f && base.TimeSinceLastFire >= 1f / maximumFiringRate)
			{
				ChargeShot();
			}
			else
			{
				gunAudio.OnMisfire();
			}
		}
	}

	public override void OnInputUp()
	{
		base.OnInputUp();
		if (base.hasAuthority && chargingShot)
		{
			float chargeAmount = Mathf.InverseLerp(chargeStartTime, chargeStartTime + speedOverDrawTime.Duration, Time.time);
			Fire(chargeAmount);
			chargingShot = false;
		}
	}

	private void ChargeShot()
	{
		chargingShot = true;
		chargeStartTime = Time.time;
		chargingBolt = AcquireBullet() as CrossbowBolt;
		chargingBolt.transform.position = boltNockingTransform.position;
		chargingBolt.transform.rotation = boltNockingTransform.rotation;
	}

	protected override void FireShot(Vector3 position, Quaternion rotation, Vector3 direction, float chargeAmount, bool isMisfire)
	{
		base.FireShot(position, rotation, direction, chargeAmount, isMisfire);
		if (!isMisfire)
		{
			Vector3 velocity = direction * speedOverDrawTime.Evaluate(chargeAmount);
			FireBullet(position, rotation, velocity, chargeAmount);
		}
	}

	protected override void FireBullet(Vector3 position, Quaternion rotation, Vector3 velocity, float chargeAmount)
	{
		if (chargingBolt != null)
		{
			if (chargingBolt.maxChargedParticleTrail != null && chargeAmount >= 0.99f)
			{
				chargingBolt.maxChargedParticleTrail.Play();
			}
			chargingBolt.Fire(position, rotation, velocity, chargeAmount, base.Rigidbody);
			chargingBolt = null;
		}
	}
}
