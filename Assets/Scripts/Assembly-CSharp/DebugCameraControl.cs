using System.Collections;
using UnityEngine;
using UnityEngine.VR;

public class DebugCameraControl : MonoBehaviour
{
	[SerializeField]
	private KeyCode exitKey = KeyCode.Escape;

	[SerializeField]
	private float yawSpeed = 360f;

	[SerializeField]
	private float pitchSpeed = 180f;

	[SerializeField]
	private float rollSpeed = 180f;

	[SerializeField]
	private float movementSpeed = 6f;

	[SerializeField]
	private float upDownSpeed = 6f;

	private bool GameViewFocused
	{
		get
		{
			return !Cursor.visible;
		}
		set
		{
			Cursor.lockState = (value ? CursorLockMode.Locked : CursorLockMode.None);
			Cursor.visible = !value;
		}
	}

	private void Awake()
	{
		/*if (false)
		{
			GameViewFocused = false;
		}
		else
		{
			Object.Destroy(this);
		}*/
		if (BootSequence.ForceVR)
		{
			if (!Application.isEditor || VRDevice.isPresent)
				Destroy(this);
		}
	}

	private void Update()
	{
		if (GameViewFocused)
		{
			Vector3 vector = ReadMovementInput();
			Vector3 vector2 = base.transform.right * vector.x + base.transform.forward * vector.z;
			vector2 = Vector3.ProjectOnPlane(Vector3.ClampMagnitude(vector2, 1f), Vector3.up) * movementSpeed;
			vector2 += Vector3.up * vector.y * upDownSpeed;
			base.transform.position += vector2 * Time.deltaTime;
			Vector3 vector3 = ReadLookInput();
			float yAngle = vector3.y * yawSpeed * Time.deltaTime;
			base.transform.Rotate(0f, yAngle, 0f, Space.World);
			float xAngle = (0f - vector3.x) * pitchSpeed * Time.deltaTime;
			base.transform.Rotate(xAngle, 0f, 0f, Space.Self);
			float zAngle = (0f - vector3.z) * rollSpeed * Time.deltaTime;
			base.transform.Rotate(0f, 0f, zAngle, Space.Self);
			if (Input.GetKeyDown(exitKey))
			{
				GameViewFocused = false;
			}
		}
		else if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
		{
			StartCoroutine(DelayedSetGameViewFocused(true));
		}
	}

	private IEnumerator DelayedSetGameViewFocused(bool value)
	{
		yield return new WaitForSeconds(0.1f);
		GameViewFocused = value;
	}

	private Vector3 ReadMovementInput()
	{
		return new Vector3(Input.GetAxis("Movement Right"), Input.GetAxis("Movement Up"), Input.GetAxis("Movement Forward"));
	}

	private Vector3 ReadLookInput()
	{
		return new Vector3(Input.GetAxis("Camera Pitch"), Input.GetAxis("Camera Yaw"), 0f);
	}
}
