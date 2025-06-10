using System.Collections.Generic;
using Photon;
using UnityEngine;

public class PlayerFollowCameraManager : Photon.MonoBehaviour
{
	[SerializeField]
	private UIPlayerCamera uIPlayerCameraPrefab;

	[SerializeField]
	private Vector2 uiCameraResolutionOverride = new Vector2(320f, 270f);

	[SerializeField]
	private PlayerFollowCameraTV[] screens;

	[SerializeField]
	private Texture emptyTexture;

	private List<UIPlayerCamera> uIPlayerCameras = new List<UIPlayerCamera>();

	private int activeCameraIndex;

	private void Start()
	{
		for (int i = 0; i < screens.Length; i++)
		{
			UIPlayerCamera uIPlayerCamera = Object.Instantiate(uIPlayerCameraPrefab);
			uIPlayerCamera.Resolution = uiCameraResolutionOverride;
			uIPlayerCameras.Add(uIPlayerCamera);
			screens[i].ClearScreen(emptyTexture);
		}
		Canvas[] componentsInChildren = GetComponentsInChildren<Canvas>(true);
		foreach (Canvas canvas in componentsInChildren)
		{
			canvas.worldCamera = ViveControllerInput.Instance.ControllerCamera;
		}
		Invoke("ReAssignCameras", 1f);
		PUNNetworkManager.Instance.OnRecRoomPlayerConnected += PUNNetworkManager_OnRecRoomPlayerConnected;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (PUNNetworkManager.Instance != null)
		{
			PUNNetworkManager.Instance.OnRecRoomPlayerConnected -= PUNNetworkManager_OnRecRoomPlayerConnected;
		}
	}

	private void Update()
	{
		for (int i = 0; i < uIPlayerCameras.Count; i++)
		{
			uIPlayerCameras[i].Camera.gameObject.SetActive(i == activeCameraIndex);
		}
		activeCameraIndex = (activeCameraIndex + 1) % uIPlayerCameras.Count;
	}

	private void ReAssignCameras()
	{
		int i;
		for (i = 0; i < PhotonNetwork.playerList.Length; i++)
		{
			screens[i].SetupCamera(PhotonNetwork.playerList[i].TagObject as Player, uIPlayerCameras[i]);
		}
		for (; i < screens.Length; i++)
		{
			screens[i].ClearScreen(emptyTexture);
		}
	}

	private void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
	}

	private void PUNNetworkManager_OnRecRoomPlayerConnected(Player newPlayer)
	{
		ReAssignCameras();
	}

	private void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
	{
		if (base.isActiveAndEnabled)
		{
			ReAssignCameras();
		}
	}
}
