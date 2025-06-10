using UnityEngine;

[RequireComponent(typeof(ToolCleanup))]
public class Cutout : Tool
{
	private ToolCleanup cleanup;

	protected override void Awake()
	{
		base.Awake();
		cleanup = GetComponent<ToolCleanup>();
		cleanup.TestRigidbody = true;
		cleanup.TestRotation = true;
		cleanup.MinDisplacementBeforeCleanup = 1f;
		cleanup.MinPlayerDistanceBeforeCleanup = 0f;
	}
}
