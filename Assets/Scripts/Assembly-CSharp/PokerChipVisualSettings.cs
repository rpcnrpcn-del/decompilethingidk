using System;
using UnityEngine;

public static class PokerChipVisualSettings
{
	private static PokerChipVisualConfig ConfigAsset = CustomSettingsLoader.LoadConfig<PokerChipVisualConfig>();

	private const string ConfigsAssetFilePath = "Assets/Activities/EventRoom/Configs/";

	private static Color[] chipColors = null;

	private static Color[] labelColors = null;

	private static string[] labels = null;

	public static Color LoadChipColor(PokerChip.PokerChipValue suit)
	{
		LazyLoadAsset();
		return chipColors[(int)suit];
	}

	public static Color LoadLabelColor(PokerChip.PokerChipValue value)
	{
		LazyLoadAsset();
		return labelColors[(int)value];
	}

	public static string LoadLabel(PokerChip.PokerChipValue value)
	{
		LazyLoadAsset();
		return labels[(int)value];
	}

	private static void LazyLoadAsset()
	{
		if (chipColors != null)
		{
			return;
		}
		PokerChip.PokerChipValue[] array = (PokerChip.PokerChipValue[])Enum.GetValues(typeof(PokerChip.PokerChipValue));
		labels = new string[array.Length];
		labelColors = new Color[array.Length];
		chipColors = new Color[array.Length];
		if (ConfigAsset != null && ConfigAsset.ValueConfigs != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				int value = (int)ConfigAsset.ValueConfigs[i].Value;
				labels[value] = ConfigAsset.ValueConfigs[i].Label;
				chipColors[value] = ConfigAsset.ValueConfigs[i].ChipColor;
				labelColors[value] = ConfigAsset.ValueConfigs[i].LabelColor;
			}
		}
	}
}
