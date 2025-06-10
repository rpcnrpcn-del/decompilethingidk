using UnityEngine;

public class PlayerHandCollider : PlayerCollider
{
	private BoxCollider boxCollider;

	private Tool heldTool;

	private Tool lastReleasedTool;

	private float timeOfLastContactWithReleasedTool;

	private const float TIME_SINCE_LAST_CONTACT_THRESHOLD = 0.1f;

	private const int MAX_RAYCAST_HIT = 256;

	private static Collider[] hits = new Collider[256];

	private void Update()
	{
		if (lastReleasedTool == null || IsTrigger)
		{
			return;
		}
		Vector3 center = base.transform.TransformPoint(boxCollider.center);
		int num = Physics.OverlapBoxNonAlloc(center, 1.05f * boxCollider.size / 2f, hits, base.transform.rotation, 226894848);
		for (int i = 0; i < num; i++)
		{
			Tool colliderTool = hits[i].GetColliderTool();
			if (colliderTool != null && colliderTool == lastReleasedTool)
			{
				timeOfLastContactWithReleasedTool = Time.unscaledTime;
				break;
			}
		}
		if (Time.unscaledTime - timeOfLastContactWithReleasedTool > 0.1f)
		{
			lastReleasedTool = null;
			boxCollider.enabled = true;
		}
	}

	private void Awake()
	{
		boxCollider = GetComponent<BoxCollider>();
	}

	public void DisableHandToolCollisions(Tool tool)
	{
		if (!IsTrigger && !(tool == heldTool) && !(tool == null))
		{
			heldTool = tool;
			lastReleasedTool = null;
			boxCollider.enabled = false;
		}
	}

	public void ReEnableHandToolCollisions(Tool tool)
	{
		if (IsTrigger || (tool != null && tool == heldTool))
		{
			heldTool = null;
			lastReleasedTool = tool;
			timeOfLastContactWithReleasedTool = Time.unscaledTime;
		}
	}
}
