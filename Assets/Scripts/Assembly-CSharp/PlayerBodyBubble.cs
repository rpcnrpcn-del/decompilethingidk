using UnityEngine;

public class PlayerBodyBubble : MonoBehaviour
{
	[SerializeField]
	private float playerHeadHeight = 0.4f;

	private float radius;

	private const int MAX_RAYCAST_HITS = 256;

	private static RaycastHit[] hits = new RaycastHit[256];

	public Player ThisPlayer { get; set; }

	public float Radius
	{
		get
		{
			return radius;
		}
		set
		{
			radius = value;
			base.transform.localScale = radius * 2f * Vector3.one;
		}
	}

	public Vector3 PlayerHeadOffset
	{
		get
		{
			return Vector3.up * (playerHeadHeight - Radius);
		}
	}

	private void FixedUpdate()
	{
		base.transform.position = ThisPlayer.Head.transform.position + PlayerHeadOffset;
	}

	public Tool RunTeleportCollision(Vector3 teleportStart, Vector3 teleportEnd, float hitForce)
	{
		Vector3 vector = teleportEnd - teleportStart;
		float magnitude = vector.magnitude;
		if (magnitude < Mathf.Epsilon)
		{
			vector = Vector3.zero;
		}
		else
		{
			vector /= magnitude;
		}
		Tool tool = null;
		Vector3 origin = teleportStart + ThisPlayer.Head.HeightOffset + PlayerHeadOffset;
		int num = Physics.SphereCastNonAlloc(origin, radius, vector, hits, magnitude, 226894848, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < num; i++)
		{
			tool = hits[i].collider.GetColliderTool();
			if (tool != null && tool.SupportsTeleportHits)
			{
				tool.ApplyForce(hits[i].point, vector * hitForce, false);
				break;
			}
		}
		return tool;
	}
}
