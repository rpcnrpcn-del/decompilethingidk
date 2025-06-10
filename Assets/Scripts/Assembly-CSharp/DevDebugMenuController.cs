using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DevDebugMenuController : ActivityMapSelectionMenuController
{
	[Header("Dev Menu")]
	[SerializeField]
	private Text masterNameText;

	[SerializeField]
	private Text playerNameText;

	[SerializeField]
	private Button leaveGameButton;

	private GameObject tutorialCamera;

	public override void Refresh()
	{
		base.Refresh();
		if (PUNNetworkManager.Instance != null && PhotonNetwork.connected && PhotonNetwork.player != null && PhotonNetwork.masterClient != null)
		{
			if (masterNameText != null)
			{
				masterNameText.text = "Master : " + PhotonNetwork.masterClient.name;
			}
			if (playerNameText != null)
			{
				playerNameText.text = "Player : " + PhotonNetwork.player.name;
			}
		}
		GameManager gameManager = RecRoomSceneManager.Instance.GameManager;
		leaveGameButton.interactable = gameManager != null && gameManager.IsGameRunning && !gameManager.TeamManager.IsPlayerSpectator(Player.LocalPlayer.PhotonPlayer);
	}

	public void ButtonPress_Debug_SwapHands()
	{
		SingletonMonoBehaviour<CameraRig>.Instance.SwapHands();
	}

	public void ButtonPress_Debug_ChangeMasterClient()
	{
		string text = PhotonNetwork.masterClient.name;
		PhotonPlayer photonPlayer = PUNNetworkManager.Instance.DebugChangeMasterClient();
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "New Master : " + photonPlayer.name, "Previous : " + text, 2f);
	}

	public void ButtonPress_Debug_RandomizeOutfits()
	{
		if (OutfitManager.IsInitialized)
		{
			OutfitManager.Instance.RandomizePlayerOutfit();
		}
	}

	public void ButtonPress_Debug_ModeratorOutfit()
	{
		if (Player.LocalPlayer != null)
		{
			Player.LocalPlayer.PlayerOutfit.DebugEquipModeratorOutfits();
		}
	}

	public void ButtonPress_RandomName(int randomNameTypeIndex)
	{
		if (SingletonMonoBehaviour<DebugManager>.Instance != null)
		{
			SingletonMonoBehaviour<DebugManager>.Instance.SetRandomName((DebugManager.RandomName)randomNameTypeIndex);
		}
	}

	public void ButtonPress_ModeratorScene()
	{
		PhotonNetwork.LeaveRoom();
		SceneManager.LoadScene("moderator");
	}

	public void ButtonPress_SpawnTutorialCamera()
	{
		if (tutorialCamera != null)
		{
			PhotonNetwork.Destroy(tutorialCamera);
			tutorialCamera = null;
		}
		Vector3 forward = base.Owner.Head.transform.forward;
		forward.y = -0.5f;
		Vector3 position = base.Owner.Head.transform.position + forward;
		Quaternion rotation = Quaternion.Euler(0f, base.Owner.Head.transform.rotation.eulerAngles.y, 0f);
		tutorialCamera = PhotonNetwork.Instantiate("[TutorialCamera]", position, rotation, 0);
	}

	public void ButtonPress_DestroyTutorialCamera()
	{
		if (tutorialCamera != null)
		{
			PhotonNetwork.Destroy(tutorialCamera);
			tutorialCamera = null;
		}
	}
}
