using UnityEngine;

public class Billboard : MonoBehaviour
{
	private enum LockMode
	{
		None = 0,
		ParentUp = 1,
		GlobalUp = 2
	}

	[SerializeField]
	private Vector3 localForward = Vector3.forward;

	[SerializeField]
	private Vector3 localUp = Vector3.up;

	[SerializeField]
	private Vector3 parentUp = Vector3.up;

	[SerializeField]
	[Tooltip("None: No lock. Billboard directly to camera\nParent up: Local up aligned to parent up\nGlobal up: Local up aligned to global up")]
	private LockMode lockMode;

	private void OnWillRenderObject()
	{
		Vector3 vector = (base.transform.position - Camera.current.transform.position).normalized;
		Vector3 vector2 = Vector3.up;
		if (base.transform.parent != null)
		{
			vector = base.transform.parent.InverseTransformDirection(vector);
			vector2 = base.transform.parent.InverseTransformDirection(vector2);
		}
		switch (lockMode)
		{
		case LockMode.ParentUp:
			vector2 = parentUp;
			vector = Vector3.ProjectOnPlane(vector, vector2);
			break;
		case LockMode.GlobalUp:
			vector = Vector3.ProjectOnPlane(vector, vector2);
			break;
		}
		base.transform.localRotation = Quaternion.LookRotation(vector, vector2) * Quaternion.Inverse(Quaternion.LookRotation(localForward, localUp));
	}
}
