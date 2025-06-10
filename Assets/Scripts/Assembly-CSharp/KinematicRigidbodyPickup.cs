public class KinematicRigidbodyPickup : RigidbodyPickup
{
	public override void Release()
	{
		base.Release();
		base.Rigidbody.isKinematic = true;
		base.Rigidbody.ClearVelocity();
	}
}
