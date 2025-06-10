using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecNet
{
	public static class Relationships
	{
		public delegate void RelationshipListUpdatedCallback();

		private const string RELATIONSHIPS_API = "api/relationships/v2/";

		public static List<Relationship> RelationshipList = new List<Relationship>();

		public static event RelationshipListUpdatedCallback RelationshipListUpdated;

		public static void RegisterPushNotificationCallbacks()
		{
			Core.RegisterPushNotificationConnectionCallback(OnPushNotificationConnectCallback);
			Core.RegisterPushNotificationHandler(Core.PushNotificationId.RelationshipChanged, OnRelationshipChanged);
		}

		public static IEnumerator AddFriend(ulong playerId, Core.ApiCallback callback = null)
		{
			string requestUri = string.Format("{0}addfriend?id={1}", "api/relationships/v2/", playerId);
			return Core.Get(requestUri, ParseRelationshipCallback(callback));
		}

		public static IEnumerator RemoveFriend(ulong playerId, Core.ApiCallback callback = null)
		{
			string requestUri = string.Format("{0}removefriend?id={1}", "api/relationships/v2/", playerId);
			return Core.Get(requestUri, ParseRelationshipCallback(callback));
		}

		public static IEnumerator SendFriendRequest(ulong playerId, Core.ApiCallback callback = null)
		{
			string requestUri = string.Format("{0}sendfriendrequest?id={1}", "api/relationships/v2/", playerId);
			return Core.Get(requestUri, ParseRelationshipCallback(callback));
		}

		public static IEnumerator AcceptFriendRequest(ulong playerId, Core.ApiCallback callback = null)
		{
			string requestUri = string.Format("{0}acceptfriendrequest?id={1}", "api/relationships/v2/", playerId);
			return Core.Get(requestUri, ParseRelationshipCallback(callback));
		}

		public static IEnumerator BlockPlayer(ulong playerId, Core.ApiCallback callback = null)
		{
			string requestUri = string.Format("{0}blockplayer?id={1}", "api/relationships/v2/", playerId);
			return Core.Get(requestUri, ParseRelationshipCallback(callback));
		}

		public static IEnumerator UnblockPlayer(ulong playerId, Core.ApiCallback callback = null)
		{
			string requestUri = string.Format("{0}unblockplayer?id={1}", "api/relationships/v2/", playerId);
			return Core.Get(requestUri, ParseRelationshipCallback(callback));
		}

		private static void RefreshList()
		{
			string requestUri = string.Format("{0}get", "api/relationships/v2/");
			Core.Get(requestUri, delegate(string error, List<Relationship> newRelationshipList)
			{
				if (string.IsNullOrEmpty(error))
				{
					RelationshipList = newRelationshipList;
					RaiseRelationshipListUpdatedEvent();
				}
				else
				{
					Debug.LogError("Failed to refresh relationship list: " + error);
					RefreshList();
				}
			});
		}

		private static Core.ApiCallback<Relationship> ParseRelationshipCallback(Core.ApiCallback callback)
		{
			return delegate(string error, Relationship relationship)
			{
				if (string.IsNullOrEmpty(error))
				{
					AddRelationshipToCache(relationship);
				}
				Core.SafeInvoke(callback, error);
			};
		}

		private static void OnPushNotificationConnectCallback()
		{
			RefreshList();
		}

		private static void OnRelationshipChanged(object message)
		{
			try
			{
				Relationship relationship = new Relationship();
				relationship.Deserialize((Dictionary<string, object>)message);
				AddRelationshipToCache(relationship);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		private static void AddRelationshipToCache(Relationship relation)
		{
			int num = RelationshipList.FindIndex((Relationship r) => r.PlayerID == relation.PlayerID);
			if (num == -1)
			{
				RelationshipList.Add(relation);
			}
			else
			{
				RelationshipList[num] = relation;
			}
			RaiseRelationshipListUpdatedEvent();
		}

		private static void RaiseRelationshipListUpdatedEvent()
		{
			try
			{
				if (Relationships.RelationshipListUpdated != null)
				{
					Relationships.RelationshipListUpdated();
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}
}
