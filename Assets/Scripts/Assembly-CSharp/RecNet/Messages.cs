using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecNet
{
	public static class Messages
	{
		public delegate void MessageListUpdatedCallback(UpdateType updateType);

		public delegate void MessageAddedCallback(Message message);

		public enum UpdateType
		{
			Initial = 0,
			Add = 1,
			Delete = 2
		}

		private const string MESSAGES_API = "api/messages/";

		public static List<Message> MessageList = new List<Message>();

		public static event MessageListUpdatedCallback OnMessageListUpdated;

		public static event MessageAddedCallback OnMessageAdded;

		public static void RegisterPushNotificationCallbacks()
		{
			Core.RegisterPushNotificationConnectionCallback(OnPushNotificationChannelConnected);
			Core.RegisterPushNotificationHandler(Core.PushNotificationId.MessageReceived, OnMessageReceived);
			Core.RegisterPushNotificationHandler(Core.PushNotificationId.MessageDeleted, OnMessageDeleted);
		}

		public static IEnumerator SendVoteToKickRequest(ulong targetPlayerId, ulong toPlayerId)
		{
			return SendMessage(toPlayerId, Message.MessageType.VoteToKick, string.Empty);
		}

		public static IEnumerator SendGameInvite(ulong playerId, Core.ApiCallback callback = null)
		{
			return SendMessage(playerId, Message.MessageType.GameInvite, string.Empty, callback);
		}

		public static IEnumerator SendGameInviteDeclined(ulong playerId, Core.ApiCallback callback = null)
		{
			return SendMessage(playerId, Message.MessageType.GameInviteDeclined, string.Empty, callback);
		}

		public static IEnumerator SendGameJoinFailed(ulong playerId, int additionalPartySize, Core.ApiCallback callback = null)
		{
			return SendMessage(playerId, Message.MessageType.GameJoinFailed, additionalPartySize.ToString(), callback);
		}

		public static IEnumerator SendPartyActivitySwitch(ulong playerId, Core.ApiCallback callback = null)
		{
			return SendMessage(playerId, Message.MessageType.PartyActivitySwitch, string.Empty, callback);
		}

		private static IEnumerator SendMessage(ulong playerId, Message.MessageType messageType, string data, Core.ApiCallback callback = null)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("ToPlayerId", playerId.ToString());
			int num = (int)messageType;
			dictionary.Add("Type", num.ToString());
			dictionary.Add("Data", data);
			string requestUri = string.Format("{0}v2/send", "api/messages/");
			return Core.Post(requestUri, dictionary, callback);
		}

		public static IEnumerator DeleteMessage(long messageId, Core.ApiCallback callback = null)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Id", messageId.ToString());
			string requestUri = string.Format("{0}v2/delete", "api/messages/", messageId);
			return Core.Post(requestUri, dictionary, callback);
		}

		private static void RefreshList()
		{
			string requestUri = string.Format("{0}v2/get", "api/messages/");
			Core.Get(requestUri, delegate(string error, List<Message> newMessageList)
			{
				if (string.IsNullOrEmpty(error))
				{
					MessageList = newMessageList;
					RaiseMessageListUpdatedEvent(UpdateType.Initial);
				}
				else
				{
					Debug.LogError("Failed to refresh message list: " + error);
					RefreshList();
				}
			});
		}

		private static void OnPushNotificationChannelConnected()
		{
			RefreshList();
		}

		private static void OnMessageReceived(object messageDict)
		{
			try
			{
				Message message = new Message();
				message.Deserialize((Dictionary<string, object>)messageDict);
				int num = MessageList.FindIndex((Message m) => m.Id == message.Id);
				if (num == -1)
				{
					if (message.Type != Message.MessageType.VoteToKick || !MessageList.Exists((Message m) => m.Type == Message.MessageType.VoteToKick && m.FromPlayerId == message.FromPlayerId))
					{
						MessageList.Add(message);
					}
					RaiseMessageAddedEvent(message);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		private static void OnMessageDeleted(object messageDict)
		{
			try
			{
				Dictionary<string, object> dict = (Dictionary<string, object>)messageDict;
				long messageId = Util.GetKey<long>("Id", dict);
				int num = MessageList.FindIndex((Message m) => m.Id == messageId);
				if (num != -1)
				{
					MessageList.RemoveAt(num);
					RaiseMessageListUpdatedEvent(UpdateType.Delete);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		private static void RaiseMessageAddedEvent(Message message)
		{
			try
			{
				if (Messages.OnMessageAdded != null)
				{
					Messages.OnMessageAdded(message);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			finally
			{
				RaiseMessageListUpdatedEvent(UpdateType.Add);
			}
		}

		private static void RaiseMessageListUpdatedEvent(UpdateType updateType)
		{
			try
			{
				if (Messages.OnMessageListUpdated != null)
				{
					Messages.OnMessageListUpdated(updateType);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}
}
