using UnityEngine;

public class PhysicsButtonVisual : MonoBehaviour
{
	[SerializeField]
	private float releaseSpeed = 0.5f;

	[SerializeField]
	private Transform unpressedTransform;

	[SerializeField]
	private Transform pressedTransform;

	private Rigidbody rigidbody;

	private Vector3 buttonDownDirection;

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		buttonDownDirection = (pressedTransform.position - unpressedTransform.position).normalized;
		rigidbody.position = unpressedTransform.position;
		rigidbody.velocity = Vector3.zero;
	}

	private void FixedUpdate()
	{
		Vector3 lhs = unpressedTransform.position - rigidbody.position;
		if (Vector3.Dot(lhs, buttonDownDirection) >= 0f - Mathf.Epsilon)
		{
			rigidbody.position = unpressedTransform.position;
			rigidbody.velocity = Vector3.zero;
			return;
		}
		rigidbody.velocity = -buttonDownDirection * releaseSpeed;
		Vector3 lhs2 = pressedTransform.position - rigidbody.position;
		if (Vector3.Dot(lhs2, buttonDownDirection) <= Mathf.Epsilon)
		{
			rigidbody.position = pressedTransform.position;
		}
	}
}
