using UnityEngine;

public class UnrollingScreen : Tool
{
	[SerializeField]
	private Transform screenTop;

	[SerializeField]
	private Transform screenBottom;

	[SerializeField]
	private SlideshowScreen screen;

	[SerializeField]
	private float screenVisibilityDistance = 0.1f;

	private Vector3 screenDownDirection;

	protected override void Awake()
	{
		base.Awake();
		screenDownDirection = (screenBottom.position - screenTop.position).normalized;
	}

	protected override void FixedUpdate()
	{
		if (base.hasAuthority)
		{
			Vector3 lhs = screenTop.position - base.Rigidbody.position;
			if (Vector3.Dot(lhs, screenDownDirection) > 0f)
			{
				base.Rigidbody.position = screenTop.position;
			}
			Vector3 lhs2 = screenBottom.position - base.Rigidbody.position;
			if (Vector3.Dot(lhs2, screenDownDirection) < 0f)
			{
				base.Rigidbody.position = screenBottom.position;
				lhs2 = Vector3.zero;
			}
			float magnitude = lhs2.magnitude;
			if (magnitude <= screenVisibilityDistance && !screen.IsVisible)
			{
				screen.SetVisibility(true);
			}
			else if (magnitude > screenVisibilityDistance && screen.IsVisible)
			{
				screen.SetVisibility(false);
			}
		}
	}
}
