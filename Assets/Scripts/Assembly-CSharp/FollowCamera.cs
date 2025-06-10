using UnityEngine;

[RequireComponent(typeof(Interpolator))]
public class FollowCamera : MonoBehaviour
{
	public float DistanceFromPlayer = 1f;

	public bool FacePlayer = true;

	public Vector3 PositionOffset = Vector3.zero;

	public Vector3 RotationOffset = Vector3.zero;

	public bool StayAtHeadHeight;

	public float OuterFollowAngle = 45f;

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
		if (!SingletonMonoBehaviour<CameraRig>.IsInitialized)
		{
			return;
		}
		Transform transform = SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform;
		if (!(transform != null))
		{
			return;
		}
		Vector3 vector = ((!StayAtHeadHeight) ? transform.forward : Vector3.Cross(transform.right, Vector3.up));
		float num = Vector3.Angle(vector, base.transform.position - transform.position);
		if (num >= OuterFollowAngle)
		{
			interpolator.TargetPosition = transform.position + vector * DistanceFromPlayer + base.transform.rotation * PositionOffset;
			if (FacePlayer)
			{
				interpolator.TargetRotation = Quaternion.LookRotation(transform.position - interpolator.TargetPosition) * Quaternion.Euler(RotationOffset);
			}
		}
	}

	public void ForceUpdate()
	{
		Update();
		interpolator.SnapToTarget();
	}
}
