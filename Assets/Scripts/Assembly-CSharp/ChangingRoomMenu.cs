using UnityEngine;

public class ChangingRoomMenu : MonoBehaviour
{
	private void Start()
	{
		if (ViveControllerInput.Instance != null)
		{
			GetComponent<Canvas>().worldCamera = ViveControllerInput.Instance.ControllerCamera;
		}
	}

	public void Button_ResetOutfitSelection()
	{
		if (Player.LocalPlayer != null)
		{
			Player.LocalPlayer.PlayerOutfit.RestoreSelection();
			AnalyticsHelper.AvatarChangingRoomResetButton();
		}
	}

	public void Button_RandomizeOutfitSelection()
	{
		if (Player.LocalPlayer != null && OutfitManager.Instance != null)
		{
			OutfitManager.Instance.RandomizePlayerOutfit();
		}
	}

	public void Button_ChangeAvatarColor(AvatarColor avatarColor)
	{
		if (Player.LocalPlayer != null)
		{
			if (avatarColor.Type == AvatarColor.ColorType.Skin)
			{
				Player.LocalPlayer.PlayerOutfit.SetSkinColor(avatarColor.GuidString);
			}
			else if (avatarColor.Type == AvatarColor.ColorType.Hair)
			{
				Player.LocalPlayer.PlayerOutfit.SetHairColor(avatarColor.GuidString);
			}
			else
			{
				Debug.LogError("Button_ChangeAvatarColor cannot handle the avatarColor.Type : " + avatarColor.Type);
			}
		}
	}
}
