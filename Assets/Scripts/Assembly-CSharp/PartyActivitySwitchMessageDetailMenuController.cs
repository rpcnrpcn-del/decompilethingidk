using RecNet;

public class PartyActivitySwitchMessageDetailMenuController : MessageDetailMenuController
{
	public void AcceptMessage()
	{
		if (base.Message != null)
		{
			RecRoomSceneManager.Instance.JoinPlayer(base.Message.FromPlayerId, false);
		}
		CloseAndDeleteMessage();
	}

	public void DeclineMessage()
	{
		if (base.Message != null)
		{
			Messages.SendGameInviteDeclined(base.Message.FromPlayerId);
		}
		CloseAndDeleteMessage();
	}
}
