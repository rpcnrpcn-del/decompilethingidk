public class VoteToKickMessageDetailMenuController : MessageDetailMenuController
{
	public void AcceptMessage()
	{
		if (base.Message != null)
		{
			foreach (Player item in Player.All)
			{
				if (item.PlayerId == base.Message.FromPlayerId)
				{
					item.PlayerModeration.VoteToKickPlayer();
					break;
				}
			}
		}
		CloseAndDeleteMessage();
	}

	public void DeclineMessage()
	{
		if (base.Message != null)
		{
		}
		CloseAndDeleteMessage();
	}
}
