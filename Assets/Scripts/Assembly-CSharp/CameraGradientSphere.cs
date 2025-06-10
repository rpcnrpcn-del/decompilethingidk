using UnityEngine;

public class CameraGradientSphere : MonoBehaviour
{
	private void Update()
	{
		base.transform.rotation = Quaternion.LookRotation(Vector3.up);
	}
}
