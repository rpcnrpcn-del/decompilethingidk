using RecNet;

public class FriendInviteMessageDetailMenuController : MessageDetailMenuController
{
	public void AcceptMessage()
	{
		if (base.Message != null)
		{
			Relationships.AcceptFriendRequest(base.Message.FromPlayerId);
		}
		CloseAndDeleteMessage();
	}

	public void DeclineMessage()
	{
		if (base.Message != null)
		{
			Relationships.RemoveFriend(base.Message.FromPlayerId);
		}
		CloseAndDeleteMessage();
	}
}
