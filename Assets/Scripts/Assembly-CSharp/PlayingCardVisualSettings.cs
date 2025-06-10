using System;
using UnityEngine;

public static class PlayingCardVisualSettings
{
	private static PlayingCardVisualConfig ConfigAsset = CustomSettingsLoader.LoadConfig<PlayingCardVisualConfig>();

	private const string ConfigsAssetFilePath = "Assets/Activities/EventRoom/Configs/";

	private static Sprite[] suitImages = null;

	private static Color[] suitColors = null;

	private static string[] valueLabels = null;

	public static Sprite LoadSuitImage(PlayingCard.PlayingCardSuit suit)
	{
		LazyLoadAsset();
		return suitImages[(int)suit];
	}

	public static Color LoadSuitColor(PlayingCard.PlayingCardSuit suit)
	{
		LazyLoadAsset();
		return suitColors[(int)suit];
	}

	public static string LoadValueLabel(PlayingCard.PlayingCardValue value)
	{
		LazyLoadAsset();
		return valueLabels[(int)value];
	}

	private static void LazyLoadAsset()
	{
		if (suitImages == null)
		{
			PlayingCard.PlayingCardSuit[] array = (PlayingCard.PlayingCardSuit[])Enum.GetValues(typeof(PlayingCard.PlayingCardSuit));
			suitImages = new Sprite[array.Length];
			suitColors = new Color[array.Length];
			if (ConfigAsset != null && ConfigAsset.SuitConfigs != null)
			{
				for (int i = 0; i < ConfigAsset.SuitConfigs.Length; i++)
				{
					int suit = (int)ConfigAsset.SuitConfigs[i].Suit;
					suitColors[suit] = ConfigAsset.SuitConfigs[i].Color;
					if (ConfigAsset.SuitConfigs[i].Sprite != null)
					{
						suitImages[suit] = LoadSprite(ConfigAsset.SuitConfigs[i].Sprite.name);
					}
				}
			}
		}
		if (valueLabels != null)
		{
			return;
		}
		PlayingCard.PlayingCardValue[] array2 = (PlayingCard.PlayingCardValue[])Enum.GetValues(typeof(PlayingCard.PlayingCardValue));
		valueLabels = new string[array2.Length];
		if (ConfigAsset != null && ConfigAsset.ValueConfigs != null)
		{
			for (int j = 0; j < array2.Length; j++)
			{
				valueLabels[(int)ConfigAsset.ValueConfigs[j].Value] = ConfigAsset.ValueConfigs[j].Label;
			}
		}
	}

	private static Sprite LoadSprite(string name)
	{
		return (Sprite)Resources.Load(name, typeof(Sprite));
	}
}
