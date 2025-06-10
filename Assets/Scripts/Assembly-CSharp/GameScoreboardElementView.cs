using UnityEngine;
using UnityEngine.UI;

public class GameScoreboardElementView : MonoBehaviour
{
	public struct ElementColors
	{
		public Color TextColor;

		public Color BackgroundColor;

		public float DefaultBackgroundAlpha;

		public float HighlightBackgroundAlpha;
	}

	[SerializeField]
	protected Image[] backgroundImages;

	protected Text[] allText;

	public virtual ElementColors Colors { get; set; }

	protected virtual void Awake()
	{
		allText = GetComponentsInChildren<Text>();
	}

	protected void ApplyColors(bool useHighlightAlpha)
	{
		Color backgroundColor = Colors.BackgroundColor;
		backgroundColor.a = ((!useHighlightAlpha) ? Colors.DefaultBackgroundAlpha : Colors.HighlightBackgroundAlpha);
		if (backgroundImages != null)
		{
			for (int i = 0; i < backgroundImages.Length; i++)
			{
				backgroundImages[i].color = backgroundColor;
			}
		}
		for (int j = 0; j < allText.Length; j++)
		{
			allText[j].color = Colors.TextColor;
		}
	}
}
