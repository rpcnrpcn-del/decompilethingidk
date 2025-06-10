using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Joint))]
public class JointMaxStretch : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Max distance this bone can stretch from it's parent before resetting to it's original offset")]
	private float maxStretch = 0.2f;

	private Rigidbody rigidbody;

	private Joint joint;

	private Vector3 defaultPosition;

	private Quaternion defaultRotation;

	private float maxDistanceFromConnectedBody;

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		joint = GetComponent<Joint>();
		defaultPosition = joint.connectedBody.transform.InverseTransformPoint(base.transform.position);
		defaultRotation = joint.connectedBody.transform.InverseTransformRotation(base.transform.rotation);
	}

	private void Update()
	{
		if (Vector3.Distance(base.transform.position, joint.connectedBody.transform.position) > defaultPosition.magnitude + maxStretch)
		{
			base.transform.position = joint.connectedBody.transform.TransformPoint(defaultPosition);
			base.transform.rotation = joint.connectedBody.transform.TransformRotation(defaultRotation);
			rigidbody.ClearVelocity();
		}
	}
}
