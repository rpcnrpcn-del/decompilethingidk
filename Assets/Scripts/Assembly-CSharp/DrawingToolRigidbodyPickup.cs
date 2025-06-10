using UnityEngine;

public class DrawingToolRigidbodyPickup : RigidbodyPickup
{
	[SerializeField]
	private bool forceRotationNearSurface;

	[SerializeField]
	private AnimationCurve nearSurfaceRotationCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	public DrawingToolBase drawingTool;

	public override Quaternion TargetRigidbodyRotation
	{
		get
		{
			if (forceRotationNearSurface && drawingTool.CurrentSurface != null)
			{
				Quaternion targetRigidbodyRotation = base.TargetRigidbodyRotation;
				Vector3 fromDirection = targetRigidbodyRotation * Vector3.up;
				Vector3 normal = drawingTool.CurrentSurface.Normal;
				Quaternion quaternion = Quaternion.FromToRotation(fromDirection, normal);
				float time = drawingTool.CurrentSurface.DistanceToPlane(drawingTool.BrushTip.transform.position);
				float t = nearSurfaceRotationCurve.Evaluate(time);
				return Quaternion.Lerp(targetRigidbodyRotation, quaternion * targetRigidbodyRotation, t);
			}
			return base.TargetRigidbodyRotation;
		}
	}

	public override Vector3 TargetRigidbodyPosition
	{
		get
		{
			Vector3 targetRigidbodyPosition = base.TargetRigidbodyPosition;
			Vector2 onPlanePos;
			float distanceToPlane;
			if (drawingTool.CurrentSurface != null && drawingTool.CurrentSurface.IsOverPlane(targetRigidbodyPosition, out onPlanePos, out distanceToPlane) && distanceToPlane < 0f)
			{
				return drawingTool.CurrentSurface.transform.TransformPoint(onPlanePos);
			}
			return targetRigidbodyPosition;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		drawingTool = GetComponent<DrawingToolBase>();
	}
}
