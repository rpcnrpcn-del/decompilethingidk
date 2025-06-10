using UnityEngine;

[RequireComponent(typeof(Interpolator))]
public class FollowTarget : MonoBehaviour
{
	public Transform Target;

	public float DistanceFromTarget = 1f;

	[Tooltip("If the target is farther than this than force update position.")]
	public float SnapDistance = 5f;

	public bool FaceTarget = true;

	public bool FaceTargetXZOnly = true;

	public Vector2 DistanceBoundaries = new Vector2(0.75f, 1.5f);

	public Vector3 PositionOffset = Vector3.zero;

	public Vector3 RotationOffset = Vector3.zero;

	public bool StayAtTargetHeight;

	public float OuterFollowAngle = 35f;

	private Interpolator interpolator;

	private void Awake()
	{
		interpolator = GetComponent<Interpolator>();
	}

	private void OnEnable()
	{
		ForceUpdate();
	}

	private void Update()
	{
		UpdateTransform();
	}

	private void UpdateTransform(bool force = false)
	{
		if (!(Target != null) || !(interpolator != null))
		{
			return;
		}
		Vector3 vector = ((!StayAtTargetHeight) ? Target.forward : Vector3.Cross(Target.right, Vector3.up));
		float num = Vector3.Angle(vector, base.transform.position - Target.position);
		float magnitude = (base.transform.position - Target.position).magnitude;
		bool flag = magnitude < DistanceBoundaries.x || magnitude > DistanceBoundaries.y;
		if (!force && !flag && !(num >= OuterFollowAngle))
		{
			return;
		}
		interpolator.TargetPosition = Target.position + vector * DistanceFromTarget + base.transform.rotation * PositionOffset;
		if (FaceTarget)
		{
			Vector3 forward = Target.position - interpolator.TargetPosition;
			if (FaceTargetXZOnly)
			{
				forward.y = 0f;
			}
			interpolator.TargetRotation = Quaternion.LookRotation(forward) * Quaternion.Euler(RotationOffset);
		}
		if (magnitude > SnapDistance)
		{
			interpolator.SnapToTarget();
		}
	}

	public void ForceUpdate()
	{
		UpdateTransform(true);
		interpolator.SnapToTarget();
	}
}
