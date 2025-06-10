using UnityEngine;

public class Ball : Tool
{
	public override void ApplyForce(Vector3 position, Vector3 force, bool playVFX, bool applyForceAtPosition = false)
	{
		Vector3 normalized = (base.Rigidbody.position - position).normalized;
		base.ApplyForce(position, normalized * force.magnitude, playVFX, applyForceAtPosition);
	}
}
