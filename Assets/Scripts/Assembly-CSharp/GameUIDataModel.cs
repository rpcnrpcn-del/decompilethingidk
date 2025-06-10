using System;

public class GameUIDataModel
{
	public string Name = string.Empty;

	public string PresenceString = string.Empty;

	public bool InviteOnly;

	public bool SupportsPreviousGameResults;

	public bool SupportsTeamSwitching;

	public bool SupportsModeSwitching;

	public bool SupportsScore;

	public string WinCondition;

	public string ModeName;

	public string[] StatNames;

	public GameUITeamDataModel[] TeamModels;

	public GameUILocalPlayerDataModel LocalPlayer;

	public static void DeepCopy(GameUIDataModel source, GameUIDataModel dest)
	{
		dest.Name = source.Name;
		dest.PresenceString = source.PresenceString;
		dest.InviteOnly = source.InviteOnly;
		dest.SupportsPreviousGameResults = source.SupportsPreviousGameResults;
		dest.SupportsTeamSwitching = source.SupportsTeamSwitching;
		dest.SupportsModeSwitching = source.SupportsModeSwitching;
		dest.SupportsScore = source.SupportsScore;
		dest.ModeName = source.ModeName;
		if (dest.StatNames == null || dest.StatNames.Length != source.StatNames.Length)
		{
			dest.StatNames = new string[source.StatNames.Length];
		}
		Array.Copy(source.StatNames, dest.StatNames, source.StatNames.Length);
		if (dest.TeamModels == null || source.TeamModels.Length != dest.TeamModels.Length)
		{
			dest.TeamModels = new GameUITeamDataModel[source.TeamModels.Length];
		}
		for (int i = 0; i < dest.TeamModels.Length; i++)
		{
			if (dest.TeamModels[i] == null)
			{
				dest.TeamModels[i] = new GameUITeamDataModel();
			}
			GameUITeamDataModel.DeepCopy(source.TeamModels[i], dest.TeamModels[i]);
		}
		if (dest.LocalPlayer == null)
		{
			dest.LocalPlayer = new GameUILocalPlayerDataModel();
		}
		GameUILocalPlayerDataModel.DeepCopy(source.LocalPlayer, dest.LocalPlayer);
	}
}
