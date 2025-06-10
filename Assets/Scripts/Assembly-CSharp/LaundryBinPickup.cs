using UnityEngine;

public class LaundryBinPickup : RigidbodyPickup
{
	[Header("Hold Configuration Parameters")]
	[SerializeField]
	private float springForce = 100f;

	[SerializeField]
	private float linearDrag = 20f;

	[SerializeField]
	private float angularDrag = 5f;

	[SerializeField]
	private float breakForce = float.PositiveInfinity;

	private float previousDrag;

	private float previousAngularDrag;

	private Vector3 previousPosition;

	private Quaternion previousRotation;

	private Vector3 trueVelocity;

	private Vector3 trueAngularVelocity;

	private Joint attachJoint;

	public override void Pickup(Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		Release();
		base.IsPickedUp = true;
		attachJoint = CreateJoint(target, targetSpacePickupPosition, targetSpacePickupRotation);
		previousDrag = base.Rigidbody.drag;
		previousAngularDrag = base.Rigidbody.angularDrag;
		base.Rigidbody.drag = linearDrag;
		base.Rigidbody.angularDrag = angularDrag;
		previousPosition = base.Rigidbody.position;
		previousRotation = base.Rigidbody.rotation;
		trueVelocity = Vector3.zero;
		trueAngularVelocity = Vector3.zero;
	}

	private void FixedUpdate()
	{
		if (base.IsPickedUp)
		{
			trueVelocity = (base.Rigidbody.position - previousPosition) / Time.fixedDeltaTime;
			trueAngularVelocity = UnityExtensions.AngularVelocityFromTo(previousRotation, base.Rigidbody.rotation) / Time.fixedDeltaTime;
			previousPosition = base.Rigidbody.position;
			previousRotation = base.Rigidbody.rotation;
		}
	}

	public override void Release()
	{
		base.Rigidbody.drag = previousDrag;
		base.Rigidbody.angularDrag = previousAngularDrag;
		if (attachJoint != null)
		{
			Object.Destroy(attachJoint);
			attachJoint = null;
		}
		base.Rigidbody.velocity = trueVelocity;
		base.Rigidbody.angularVelocity = trueAngularVelocity;
	}

	private Joint CreateJoint(Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		ConfigurableJoint configurableJoint = base.gameObject.AddComponent<ConfigurableJoint>();
		configurableJoint.autoConfigureConnectedAnchor = false;
		configurableJoint.axis = Vector3.up;
		configurableJoint.anchor = base.transform.InverseTransformPoint(target.position);
		configurableJoint.connectedBody = target;
		configurableJoint.connectedAnchor = Vector3.zero;
		configurableJoint.xMotion = ConfigurableJointMotion.Free;
		configurableJoint.yMotion = ConfigurableJointMotion.Locked;
		configurableJoint.zMotion = ConfigurableJointMotion.Locked;
		configurableJoint.angularXMotion = ConfigurableJointMotion.Free;
		configurableJoint.angularYMotion = ConfigurableJointMotion.Free;
		configurableJoint.angularZMotion = ConfigurableJointMotion.Free;
		JointDrive angularXDrive = configurableJoint.angularXDrive;
		angularXDrive.positionSpring = springForce;
		configurableJoint.angularXDrive = angularXDrive;
		configurableJoint.breakForce = breakForce;
		return configurableJoint;
	}
}
