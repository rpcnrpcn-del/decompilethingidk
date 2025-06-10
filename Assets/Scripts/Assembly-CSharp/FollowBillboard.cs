using UnityEngine;

public class FollowBillboard : MonoBehaviour
{
	[SerializeField]
	private Vector3 viewSpaceTargetOffset = new Vector3(0f, 0.2f, 0f);

	public Transform Target;

	public Camera Camera;

	private void Update()
	{
		if (Target != null && Camera != null)
		{
			Vector3 up = Vector3.up;
			Vector3 normalized = Vector3.ProjectOnPlane(Camera.transform.forward, Vector3.up).normalized;
			Vector3 normalized2 = Vector3.Cross(normalized, up).normalized;
			base.transform.position = Target.position + normalized2 * viewSpaceTargetOffset.x + up * viewSpaceTargetOffset.y + normalized * viewSpaceTargetOffset.z;
			base.transform.rotation = Quaternion.LookRotation(normalized, up);
		}
	}
}
