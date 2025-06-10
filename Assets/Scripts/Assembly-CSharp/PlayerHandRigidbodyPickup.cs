using UnityEngine;

public class PlayerHandRigidbodyPickup : RigidbodyPickup
{
	public override void Pickup(Rigidbody hand, Vector3 handSpacePickupPosition, Quaternion handSpacePickupRotation)
	{
		Release();
		pickupHand = hand;
		localPickupPosition = handSpacePickupPosition;
		localPickupRotation = handSpacePickupRotation;
		if (networkTransform != null)
		{
			networkTransform.SynchronizationEnabled = base.hasAuthority;
		}
		StartAttachToHandCoroutine();
	}

	public override void Release()
	{
		pickupHand = null;
		localPickupPosition = Vector3.zero;
		localPickupRotation = Quaternion.identity;
		if (networkTransform != null)
		{
			networkTransform.SynchronizationEnabled = true;
			if (!base.hasAuthority)
			{
				networkTransform.StartDynamicTimeExtrapolation();
			}
		}
		ClearAttachToHandCoroutine();
	}
}
