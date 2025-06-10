using UnityEngine;
using UnityEngine.UI;

public class PlayerFollowCameraTV : MonoBehaviour
{
	public Text PlayerNameText;

	public Transform UIRoot;

	public TeleportationPortal TeleportationPortal;

	public Renderer TargetRenderer;

	public bool IsActive
	{
		get
		{
			return UIPlayerCamera != null && UIPlayerCamera.IsActive;
		}
	}

	public UIPlayerCamera UIPlayerCamera { get; private set; }

	private void Awake()
	{
		ClearScreen();
	}

	public void SetupCamera(Player player, UIPlayerCamera uIPlayerCamera)
	{
		UIPlayerCamera = uIPlayerCamera;
		UIPlayerCamera.Setup(player, TargetRenderer, TeleportationPortal, UIRoot);
		PlayerNameText.text = ((!(player != null)) ? string.Empty : player.PlayerName);
	}

	public void ClearScreen(Texture emptyTexture = null)
	{
		TargetRenderer.material.mainTexture = emptyTexture;
		TeleportationPortal.gameObject.SetActive(false);
		UIRoot.gameObject.SetActive(false);
		if (UIPlayerCamera != null)
		{
			UIPlayerCamera.Setup(null);
			UIPlayerCamera = null;
		}
	}

	public void Button_ToggleFollowCameraMode()
	{
		if (UIPlayerCamera != null)
		{
			UIPlayerCamera.CycleFollowMode();
		}
	}
}
