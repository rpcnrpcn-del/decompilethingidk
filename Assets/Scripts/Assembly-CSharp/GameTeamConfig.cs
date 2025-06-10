using System;
using UnityEngine;

[Serializable]
public class GameTeamConfig : ScriptableObject
{
	[Serializable]
	public struct TeamConfig
	{
		public GameTeam Team;

		public Color Color;

		public string Name;
	}

	[SerializeField]
	public TeamConfig[] TeamConfigs;
}
