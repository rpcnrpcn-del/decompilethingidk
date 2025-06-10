using System;
using UnityEngine;

[Serializable]
public class PlayingCardVisualConfig : ScriptableObject
{
	[Serializable]
	public struct SuitConfig
	{
		public PlayingCard.PlayingCardSuit Suit;

		public Sprite Sprite;

		public Color Color;
	}

	[Serializable]
	public struct ValueConfig
	{
		public PlayingCard.PlayingCardValue Value;

		public string Label;
	}

	[SerializeField]
	public SuitConfig[] SuitConfigs;

	[SerializeField]
	public ValueConfig[] ValueConfigs;
}
