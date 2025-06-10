using UnityEngine;

public class FaceCamera : MonoBehaviour
{
	private void Update()
	{
		if (SingletonMonoBehaviour<CameraRig>.Instance != null)
		{
			Vector3 vector = base.transform.position - SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.position;
			Vector3 normalized = Vector3.ProjectOnPlane(vector, Vector3.up).normalized;
			if (normalized != Vector3.zero)
			{
				base.transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
			}
		}
	}
}
