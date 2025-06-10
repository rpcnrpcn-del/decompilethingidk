using RecNet;

public static class RecnetAnalyticsHelper
{
	private static bool IsReady
	{
		get
		{
			return Profiles.LocalProfile != null;
		}
	}

	public static void CricketStart()
	{
		if (IsReady)
		{
			string currentSceneName = RecRoomSceneManager.CurrentSceneName;
			Analytics.SendEvent(currentSceneName, "cricket", "started", null, PhotonNetwork.countOfPlayers, PhotonNetwork.countOfRooms);
		}
	}

	public static void CricketEnd(bool anotherPlayerJoined, float aloneDuration)
	{
		if (IsReady)
		{
			string currentSceneName = RecRoomSceneManager.CurrentSceneName;
			Analytics.SendEvent(currentSceneName, "cricket", (!anotherPlayerJoined) ? "changed activity" : "someone joined", aloneDuration, PhotonNetwork.countOfPlayers, PhotonNetwork.countOfRooms);
		}
	}

	public static void VoteKicked(float userPercent, int userCount, int duration)
	{
		if (IsReady)
		{
			string label = ((!(RecRoomSceneManager.Instance != null) || !(RecRoomSceneManager.Instance.GameManager != null)) ? string.Empty : RecRoomSceneManager.Instance.GameManager.CurrentState.ToString());
			Analytics.SendEvent(RecRoomSceneManager.CurrentSceneName, "VoteKicked", label, userPercent, userCount, duration);
		}
	}

	public static void VoteKickInitiated(Player sourcePlayer, Player targetPlayer)
	{
		if (IsReady)
		{
			Analytics.SendEvent(RecRoomSceneManager.CurrentSceneName, "VoteKickInitiator", sourcePlayer.PlayerName, sourcePlayer.PlayerId, targetPlayer.PlayerId);
		}
	}
}
