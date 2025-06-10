using UnityEngine;

public class SlidingDrawerPickup : RigidbodyPickup
{
	[Header("Hold Configuration Parameters")]
	[SerializeField]
	private float springForce = 100f;

	[SerializeField]
	private float linearDrag = 20f;

	[SerializeField]
	private float angularDrag = 5f;

	[SerializeField]
	private float breakForce = 50f;

	private float previousDrag;

	private float previousAngularDrag;

	private Joint attachJoint;

	public override void Pickup(Rigidbody hand, Vector3 handSpacePickupPosition, Quaternion handSpacePickupRotation)
	{
		Release();
		base.IsPickedUp = true;
		attachJoint = CreateJoint(hand, handSpacePickupPosition, handSpacePickupRotation);
		previousDrag = base.Rigidbody.drag;
		previousAngularDrag = base.Rigidbody.angularDrag;
		base.Rigidbody.drag = linearDrag;
		base.Rigidbody.angularDrag = angularDrag;
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
	}

	private Joint CreateJoint(Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		SpringJoint springJoint = base.gameObject.AddComponent<SpringJoint>();
		springJoint.autoConfigureConnectedAnchor = false;
		springJoint.anchor = -targetSpacePickupPosition;
		springJoint.connectedBody = target;
		springJoint.connectedAnchor = Vector3.zero;
		springJoint.spring = springForce;
		springJoint.breakForce = breakForce;
		return springJoint;
	}
}
