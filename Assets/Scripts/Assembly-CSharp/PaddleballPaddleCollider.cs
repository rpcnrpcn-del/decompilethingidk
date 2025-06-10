using UnityEngine;

public class PaddleballPaddleCollider : ToolCollider
{
	public override Vector3 GetCollisionNormal(Collision collision)
	{
		Vector3 recentLinearVelocity = ThisTrackedVelocity.RecentLinearVelocity;
		if (recentLinearVelocity.magnitude < 0.5f)
		{
			return (!(Vector3.Dot(collision.relativeVelocity, base.transform.forward) < 0f)) ? base.transform.forward : (-base.transform.forward);
		}
		return (!(Vector3.Dot(recentLinearVelocity, base.transform.forward) < 0f)) ? base.transform.forward : (-base.transform.forward);
	}
}
