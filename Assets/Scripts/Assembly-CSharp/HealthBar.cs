using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class HealthBar : MonoBehaviour
{
	[SerializeField]
	private Image healthChunkPrefab;

	[SerializeField]
	private Image koGraphicImage;

	[SerializeField]
	private Image reviveNotificationImage;

	private RectTransform thisRectTransform;

	private List<Image> healthChunks;

	private bool _active = true;

	public int MaxHealth { get; set; }

	public int CurrentHealth { get; set; }

	public PhotonPlayer ThisPlayer { get; private set; }

	public bool Active
	{
		get
		{
			return _active;
		}
		set
		{
			_active = value;
			base.gameObject.SetActive(_active);
		}
	}

	public void Initialize(PhotonPlayer ownerPlayer, int maxHealth)
	{
		if (!(healthChunkPrefab == null))
		{
			if (thisRectTransform == null)
			{
				thisRectTransform = GetComponent<RectTransform>();
			}
			if (healthChunks == null)
			{
				healthChunks = new List<Image>();
			}
			for (int i = 0; i < maxHealth; i++)
			{
				Image image = Object.Instantiate(healthChunkPrefab, thisRectTransform, false);
				image.rectTransform.localPosition = thisRectTransform.pivot + Vector2.left * image.rectTransform.rect.width * i;
				healthChunks.Add(image);
			}
			ThisPlayer = ownerPlayer;
		}
	}

	public void UpdateHealth(int newHealth)
	{
		for (int i = 0; i < healthChunks.Count; i++)
		{
			healthChunks[i].enabled = i < newHealth;
		}
		if (koGraphicImage != null)
		{
			koGraphicImage.gameObject.SetActive(newHealth <= 0);
			ThrobbingUIElement component = koGraphicImage.GetComponent<ThrobbingUIElement>();
			if (component != null)
			{
				component.Active = koGraphicImage.gameObject.activeSelf;
			}
		}
		if (ThisPlayer != null && !ThisPlayer.isLocal && reviveNotificationImage != null)
		{
			reviveNotificationImage.gameObject.SetActive(newHealth <= 0);
		}
	}
}
