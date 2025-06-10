using RecNet;

public class GameInviteMessageDetailMenuController : MessageDetailMenuController
{
	public void AcceptMessage()
	{
		if (base.Message != null)
		{
			AnalyticsHelper.GameInviteMessageResponse(base.Message, true);
			base.PlayerMenu.RunJoinPlayer(base.Message.FromPlayerId);
		}
		CloseAndDeleteMessage();
	}

	public void DeclineMessage()
	{
		if (base.Message != null)
		{
			AnalyticsHelper.GameInviteMessageResponse(base.Message, false);
			Messages.SendGameInviteDeclined(base.Message.FromPlayerId);
		}
		CloseAndDeleteMessage();
	}
}
