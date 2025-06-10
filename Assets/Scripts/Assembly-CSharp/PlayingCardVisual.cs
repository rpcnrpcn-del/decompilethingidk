using UnityEngine;
using UnityEngine.UI;

public class PlayingCardVisual : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private Image[] suitImages;

	[SerializeField]
	private Text[] valueLabels;

	public PlayingCard.PlayingCardSuit Suit
	{
		set
		{
			Sprite sprite = PlayingCardVisualSettings.LoadSuitImage(value);
			for (int i = 0; i < suitImages.Length; i++)
			{
				suitImages[i].sprite = sprite;
			}
			Color color = PlayingCardVisualSettings.LoadSuitColor(value);
			for (int j = 0; j < valueLabels.Length; j++)
			{
				valueLabels[j].color = color;
			}
		}
	}

	public PlayingCard.PlayingCardValue Value
	{
		set
		{
			string text = PlayingCardVisualSettings.LoadValueLabel(value);
			for (int i = 0; i < valueLabels.Length; i++)
			{
				valueLabels[i].text = text;
			}
		}
	}
}
