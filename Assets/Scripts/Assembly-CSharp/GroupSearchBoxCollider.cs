using UnityEngine;

public class GroupSearchBoxCollider : GroupSearch
{
	[SerializeField]
	private BoxCollider overlapCollider;

	protected override void Awake()
	{
		base.Awake();
		overlapCollider.isTrigger = true;
		overlapCollider.enabled = false;
	}

	protected override void GetOverlapGeometry(out Vector3 center, out Vector3 size)
	{
		center = overlapCollider.center;
		size = overlapCollider.size;
	}
}
