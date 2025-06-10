using System;
using UnityEngine;

[Serializable]
public class PokerChipVisualConfig : ScriptableObject
{
	[Serializable]
	public struct ValueConfig
	{
		public PokerChip.PokerChipValue Value;

		public string Label;

		public Color ChipColor;

		public Color LabelColor;
	}

	[SerializeField]
	public ValueConfig[] ValueConfigs;
}
