using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarColor : MonoBehaviour
{
	public enum ColorType
	{
		Skin = 0,
		Hair = 1
	}

	public static List<AvatarColor> All = new List<AvatarColor>();

	[Tooltip("Matching guid value from the OutfitManager Color Vault")]
	public string GuidString = string.Empty;

	public ColorType Type = ColorType.Hair;

	private Image image;

	private Outline outline;

	private bool _selected;

	public Color Color { get; private set; }

	public bool Selected
	{
		get
		{
			return _selected;
		}
		private set
		{
			if (_selected != value)
			{
				_selected = value;
				if (outline != null)
				{
					outline.enabled = _selected;
				}
			}
		}
	}

	private IEnumerator Start()
	{
		ColorVault vault = ((Type != ColorType.Hair) ? OutfitManager.Instance.SkinColorVault : OutfitManager.Instance.HairColorVault);
		ColorVault.ColorToGuid colorToGuid = vault.Find(GuidString);
		if (colorToGuid != null)
		{
			Color = colorToGuid.color;
			image = GetComponent<Image>();
			if (image != null)
			{
				image.color = Color;
				All.Add(this);
			}
			outline = GetComponent<Outline>();
		}
		else
		{
			Debug.LogError(string.Concat(this, " has an invalid color guid '", GuidString, ". Check to make sure you have the exact matching guid from the OutfitManager."));
			base.gameObject.SetActive(false);
		}
		while (Player.LocalPlayer == null || Player.LocalPlayer.PlayerOutfit == null || Player.LocalPlayer.PlayerOutfit.HairColor == null || Player.LocalPlayer.PlayerOutfit.SkinColor == null)
		{
			yield return null;
		}
		if (image != null && Player.LocalPlayer != null)
		{
			ColorVault.ColorToGuid colorToGuid2 = ((Type != ColorType.Hair) ? Player.LocalPlayer.PlayerOutfit.SkinColor : Player.LocalPlayer.PlayerOutfit.HairColor);
			Selected = colorToGuid2.guidString == GuidString;
		}
	}

	private void OnDestroy()
	{
		if (All != null && image != null)
		{
			All.Remove(this);
		}
	}

	public static void SetSelected(ColorType type, ColorVault.ColorToGuid colorToGuid)
	{
		foreach (AvatarColor item in All)
		{
			if (item.Type == type)
			{
				item.Selected = item.GuidString == colorToGuid.guidString;
			}
		}
	}
}
