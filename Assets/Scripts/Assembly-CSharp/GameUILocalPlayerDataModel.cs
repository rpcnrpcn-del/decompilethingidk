public class GameUILocalPlayerDataModel
{
	public bool IsSpectator = true;

	public bool CanSwitchTeams;

	public bool CanJoinGame;

	public bool CanLeaveGame;

	public static void DeepCopy(GameUILocalPlayerDataModel source, GameUILocalPlayerDataModel dest)
	{
		dest.IsSpectator = source.IsSpectator;
		dest.CanSwitchTeams = source.CanSwitchTeams;
		dest.CanJoinGame = source.CanJoinGame;
		dest.CanLeaveGame = source.CanLeaveGame;
	}
}
