using UnityEngine;

public class Dart : StickyTool
{
	[Header("Drag Configuration")]
	[SerializeField]
	private Transform centerOfMass;

	[SerializeField]
	private Transform tail;

	[SerializeField]
	private float tailDrag = 0.1f;

	[SerializeField]
	private AnimationCurve dragByAngle;

	[SerializeField]
	private float scaleVelocityAtRelease = 1.25f;

	[SerializeField]
	private float scaleAngularVelocityAtRelease;

	[Header("Sticking Requirements")]
	[SerializeField]
	private float minSpeedToStick = 1f;

	[SerializeField]
	private float maxAngleToVelocityToStick = 45f;

	[SerializeField]
	private float maxAngleToSurfaceToStick = 60f;

	protected override void Start()
	{
		base.Start();
		base.Rigidbody.centerOfMass = base.transform.InverseTransformPoint(centerOfMass.position);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!base.Rigidbody.isKinematic && !base.Rigidbody.IsSleeping())
		{
			Vector3 vector = base.transform.InverseTransformDirection(base.Rigidbody.angularVelocity);
			Quaternion quaternion = UnityExtensions.QuaternionFromAngularVelocity(vector * Time.fixedDeltaTime);
			Vector3 vector2 = base.transform.InverseTransformPoint(tail.position);
			Vector3 direction = (quaternion * vector2 - vector2) / Time.fixedDeltaTime;
			Vector3 toDirection = base.Rigidbody.velocity + base.transform.TransformDirection(direction);
			float angle;
			Vector3 axis;
			Quaternion.FromToRotation(base.Rigidbody.rotation * Vector3.forward, toDirection).ToAngleAxis(out angle, out axis);
			float num = toDirection.sqrMagnitude * tailDrag * dragByAngle.Evaluate(angle);
			base.Rigidbody.AddTorque(axis * num);
		}
	}

	protected override void OnCollisionEnter(Collision collision)
	{
		base.OnCollisionEnter(collision);
		if (base.hasAuthority && !base.IsHeld && collision.relativeVelocity.magnitude > minSpeedToStick && Vector3.Angle(collision.relativeVelocity, -base.transform.forward) < maxAngleToVelocityToStick && Vector3.Angle(collision.contacts[0].normal, -base.transform.forward) < maxAngleToSurfaceToStick)
		{
			PhotonView componentInParent = collision.gameObject.GetComponentInParent<PhotonView>();
			AttachToObject(componentInParent, true);
		}
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		linearVelocity *= scaleVelocityAtRelease;
		angularVelocity *= scaleAngularVelocityAtRelease;
		base.Release(player, linearVelocity, angularVelocity);
	}
}
