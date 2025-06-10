using UnityEngine;
using UnityEngine.VR;

public class HandheldCameraRemoteTool : Tool
{
	private enum DollyMode
	{
		Stationary = 0,
		Linear = 1,
		Orbit = 2
	}

	[Header("Camera Remote Settings")]
	[SerializeField]
	private HandheldCameraTool camera;

	[SerializeField]
	private DollyMode mode;

	[SerializeField]
	private float dollyTime;

	[SerializeField]
	private Transform startPosition;

	[Tooltip("Only valid for linear dolly mode")]
	[SerializeField]
	private Transform endPosition;

	[Tooltip("Only valid for orbit dolly mode")]
	[SerializeField]
	private Transform orbitCenter;

	private bool initialized;

	private bool recording;

	private void Update()
	{
		if (!initialized)
		{
			initialized = true;
			if (SingletonMonoBehaviour<ToolCleanupManager>.Instance != null)
			{
				SingletonMonoBehaviour<ToolCleanupManager>.Instance.RemoveTool(camera);
			}
			camera.Rigidbody.isKinematic = true;
			camera.RemoteControl = true;
			camera.OpenScreen(true);
		}
		if (base.hasAuthority)
		{
			Vector3 position;
			Quaternion rotation;
			ComputeCameraView(Time.time, out position, out rotation);
			camera.transform.position = position;
			camera.transform.rotation = rotation;
		}
	}

	private void ComputeCameraView(float elapsedTime, out Vector3 position, out Quaternion rotation)
	{
		position = startPosition.position;
		rotation = startPosition.rotation;
		float num = elapsedTime / dollyTime;
		switch (mode)
		{
		case DollyMode.Linear:
			if (!(endPosition != null))
			{
				break;
			}
			if (num > 1f)
			{
				int num2 = Mathf.FloorToInt(num);
				num -= (float)num2;
				if (num2 % 2 == 1)
				{
					num = 1f - num;
				}
			}
			position = Vector3.Lerp(startPosition.position, endPosition.position, num);
			rotation = Quaternion.Slerp(startPosition.rotation, endPosition.rotation, num);
			break;
		case DollyMode.Orbit:
			if (orbitCenter != null)
			{
				Quaternion quaternion = Quaternion.AngleAxis(360f * num, Vector3.up);
				Vector3 vector = orbitCenter.InverseTransformPoint(startPosition.position);
				Quaternion quaternion2 = orbitCenter.InverseTransformRotation(startPosition.rotation);
				position = orbitCenter.TransformPoint(quaternion * vector);
				rotation = orbitCenter.TransformRotation(quaternion * quaternion2);
			}
			break;
		}
	}

	public override void Pickup(Player player, Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		base.Pickup(player, target, targetSpacePickupPosition, targetSpacePickupRotation);
		camera.photonView.TransferOwnership(player.PhotonPlayer);
	}

	public override void OnInputDown()
	{
		base.OnInputDown();
		if (base.hasAuthority)
		{
			if (!recording)
			{
				recording = true;
				VRSettings.renderScale = 0.5f;
				camera.StartCapturing();
			}
			else
			{
				recording = false;
				VRSettings.renderScale = 1f;
				camera.FinishCapturing();
			}
		}
	}
}
