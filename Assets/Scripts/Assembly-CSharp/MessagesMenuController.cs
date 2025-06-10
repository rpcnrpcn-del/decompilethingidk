using System;
using System.Collections.Generic;
using System.Linq;
using RecNet;
using UnityEngine;

public class MessagesMenuController : MenuController
{
	[Serializable]
	private class MessageDetailMenuControllerMapping
	{
		public Message.MessageType MessageType;

		public MessageDetailMenuController MenuController;
	}

	[SerializeField]
	private MessageSummaryView messagePanelPrefab;

	[SerializeField]
	private RectTransform messageList;

	[SerializeField]
	private RectTransform noMessagesPanel;

	[SerializeField]
	private List<MessageDetailMenuControllerMapping> messageDetailMenuControllers;

	private List<MessageSummaryView> messagePanels = new List<MessageSummaryView>();

	private MessageDetailMenuController messageDetailMenu;

	public event Action<MessagesMenuController> MessageReceived;

	public override void Initialize(PlayerMenu playerMenu)
	{
		base.Initialize(playerMenu);
		Messages.OnMessageListUpdated += OnMessageListUpdated;
		foreach (MessageDetailMenuControllerMapping messageDetailMenuController in messageDetailMenuControllers)
		{
			messageDetailMenuController.MenuController.Visible = false;
		}
		messagePanelPrefab.gameObject.SetActive(false);
		PUNNetworkManager.Instance.PhotonPlayerDisconnected += OnPhotonPlayerDisconnected;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.Owner != null)
		{
			Profiles.OnProfileUpdated += OnProfileUpdated;
			PlayerPresenceManager.OnPlayerPresenceUpdated += OnPlayerPresenceUpdated;
			Images.OnProfileImageUpdated += OnProfileImageUpdated;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (base.Owner != null)
		{
			Profiles.OnProfileUpdated -= OnProfileUpdated;
			PlayerPresenceManager.OnPlayerPresenceUpdated -= OnPlayerPresenceUpdated;
			Images.OnProfileImageUpdated -= OnProfileImageUpdated;
		}
	}

	private void OnDestroy()
	{
		Messages.OnMessageListUpdated -= OnMessageListUpdated;
		PUNNetworkManager.Instance.PhotonPlayerDisconnected -= OnPhotonPlayerDisconnected;
	}

	public override void Refresh()
	{
		base.Refresh();
		if (base.Owner != null)
		{
			RefreshMessageList();
		}
	}

	private void RemoveStaleMessages()
	{
		Message[] array = Messages.MessageList.ToArray();
		foreach (Message message in array)
		{
			if (message.Type == Message.MessageType.VoteToKick)
			{
				if (Player.Find(message.FromPlayerId) == null)
				{
					Messages.DeleteMessage(message.Id);
				}
			}
			else if ((message.Type == Message.MessageType.GameInvite || message.Type == Message.MessageType.GameInviteDeclined || message.Type == Message.MessageType.GameJoinFailed || message.Type == Message.MessageType.PartyActivitySwitch) && Player.Find(message.FromPlayerId) != null)
			{
				Messages.DeleteMessage(message.Id);
			}
		}
	}

	private void RefreshMessageList()
	{
		RemoveStaleMessages();
		int count = Messages.MessageList.Count;
		while (count > messagePanels.Count)
		{
			MessageSummaryView messageSummaryView = UnityEngine.Object.Instantiate(messagePanelPrefab);
			messageSummaryView.transform.SetParent(messageList, false);
			messageSummaryView.gameObject.SetActive(true);
			messageSummaryView.OnSelected += OnMessagePanelSelected;
			messagePanels.Add(messageSummaryView);
		}
		for (int i = 0; i < count; i++)
		{
			messagePanels[i].gameObject.SetActive(true);
			messagePanels[i].Message = Messages.MessageList[count - 1 - i];
			RefreshPanel(messagePanels[i]);
		}
		for (int j = count; j < messagePanels.Count; j++)
		{
			messagePanels[j].gameObject.SetActive(false);
		}
		noMessagesPanel.gameObject.SetActive(count == 0);
		if (messageDetailMenu != null && messageDetailMenu.Visible && !Messages.MessageList.Any((Message m) => m.Id == messageDetailMenu.Message.Id))
		{
			messageDetailMenu.Visible = false;
		}
		if (Messages.MessageList.Count > 0)
		{
			List<ulong> list = Messages.MessageList.Select((Message m) => m.FromPlayerId).ToList();
			Profiles.RefreshCachedProfiles(list);
			PlayerPresenceManager.RefreshCachedPlayerPresences(list);
		}
	}

	private void RefreshPanel(MessageSummaryView messagePanel)
	{
		Profile profileFromCache = Profiles.GetProfileFromCache(messagePanel.Message.FromPlayerId);
		messagePanel.SenderName = ((profileFromCache == null) ? null : profileFromCache.DisplayName);
		messagePanel.Description = GetMessageDescription(messagePanel.Message);
		RefreshPanelSenderImage(messagePanel);
		if (messageDetailMenu != null && messageDetailMenu.Visible && messageDetailMenu.Message.Id == messagePanel.Message.Id)
		{
			messageDetailMenu.SetMessage(messagePanel.Message);
		}
	}

	private string GetMessageDescription(Message message)
	{
		switch (message.Type)
		{
		case Message.MessageType.GameInvite:
		case Message.MessageType.PartyActivitySwitch:
			return "Game Invite";
		case Message.MessageType.GameInviteDeclined:
			return "Game Invite Declined";
		case Message.MessageType.GameJoinFailed:
		{
			int num = (int)message.Details;
			return (num != 0) ? ("Failed To Join Your Game (+" + num + " party members)") : "Failed To Join Your Game";
		}
		case Message.MessageType.FriendInvite:
			return "Friend Invite";
		case Message.MessageType.VoteToKick:
			return "Vote to kick?";
		default:
			Debug.LogError("Unknown message type");
			return "<unknown>";
		}
	}

	private void RefreshPanelSenderImage(MessageSummaryView messagePanel)
	{
		Profile profileFromCache = Profiles.GetProfileFromCache(messagePanel.Message.FromPlayerId);
		Texture2D texture2D = Images.GetProfileImageFromCache(messagePanel.Message.FromPlayerId);
		if (texture2D == null || profileFromCache == null || !profileFromCache.Verified)
		{
			texture2D = Player.LocalPlayer.DefaultProfileImage;
		}
		messagePanel.SenderProfileImage = texture2D;
		if (messageDetailMenu != null && messageDetailMenu.Message.Id == messagePanel.Message.Id)
		{
			messageDetailMenu.RefreshSenderImage();
		}
	}

	private void OnMessageListUpdated(Messages.UpdateType updateType)
	{
		if (base.Owner != null)
		{
			RefreshMessageList();
			if (updateType == Messages.UpdateType.Add && this.MessageReceived != null)
			{
				this.MessageReceived(this);
			}
		}
	}

	private void OnProfileImageUpdated(ulong id, Texture2D image)
	{
		foreach (MessageSummaryView item in GetSummariesForMessagesFromPlayer(id))
		{
			RefreshPanelSenderImage(item);
		}
	}

	private void OnProfileUpdated(ulong id, Profile profile)
	{
		foreach (MessageSummaryView item in GetSummariesForMessagesFromPlayer(id))
		{
			RefreshPanel(item);
		}
	}

	private void OnPlayerPresenceUpdated(ulong id, PlayerPresence playerPresence)
	{
		foreach (MessageSummaryView item in GetSummariesForMessagesFromPlayer(id))
		{
			RefreshPanel(item);
		}
	}

	private void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		ulong result;
		if (!ulong.TryParse(player.userId, out result))
		{
			return;
		}
		bool flag = false;
		foreach (MessageSummaryView item in GetSummariesForMessagesFromPlayer(result).ToList())
		{
			if (item.Message.Type == Message.MessageType.VoteToKick)
			{
				if (messageDetailMenu != null && messageDetailMenu.Visible && messageDetailMenu.Message.Id == item.Message.Id)
				{
					messageDetailMenu.CloseMessage();
				}
				messagePanels.Remove(item);
				if (item.Message != null)
				{
					Messages.DeleteMessage(item.Message.Id);
				}
				flag = true;
			}
		}
		if (flag)
		{
			Refresh();
		}
	}

	private IEnumerable<MessageSummaryView> GetSummariesForMessagesFromPlayer(ulong id)
	{
		foreach (MessageSummaryView messagePanel in messagePanels)
		{
			if (!messagePanel.gameObject.activeSelf)
			{
				break;
			}
			if (messagePanel.Message.FromPlayerId == id)
			{
				yield return messagePanel;
			}
		}
	}

	private void OnMessagePanelSelected(long messageId)
	{
		Message message = Messages.MessageList.FirstOrDefault((Message m) => m.Id == messageId);
		if (message != null)
		{
			ShowMessageDetails(message);
		}
	}

	public void ShowLatestMessage()
	{
		Message message = Messages.MessageList.LastOrDefault();
		if (message != null)
		{
			ShowMessageDetails(message);
		}
	}

	private void ShowMessageDetails(Message message)
	{
		if (messageDetailMenu != null)
		{
			messageDetailMenu.Visible = false;
		}
		MessageDetailMenuControllerMapping messageDetailMenuControllerMapping = messageDetailMenuControllers.FirstOrDefault((MessageDetailMenuControllerMapping m) => m.MessageType == message.Type);
		if (messageDetailMenuControllerMapping != null)
		{
			messageDetailMenu = messageDetailMenuControllerMapping.MenuController;
			messageDetailMenu.SetMessage(message);
			subMenu = messageDetailMenu;
			messageDetailMenu.Visible = true;
		}
	}
}
