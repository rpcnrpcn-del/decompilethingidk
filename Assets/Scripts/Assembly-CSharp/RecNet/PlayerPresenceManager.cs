using System;
using System.Collections.Generic;
using System.Linq;
using GAMiniJSON;
using UnityEngine;

namespace RecNet
{
	public static class PlayerPresenceManager
	{
		public delegate void PlayerPresenceUpdatedCallback(ulong id, PlayerPresence playerPresence);

		private static Dictionary<ulong, PlayerPresence> playerPresenceCache = new Dictionary<ulong, PlayerPresence>();

		private const string GAME_PRESENCE_API = "api/presence/";

		public static event PlayerPresenceUpdatedCallback OnPlayerPresenceUpdated;

		public static PlayerPresence GetPlayerPresenceFromCache(ulong id)
		{
			PlayerPresence value;
			playerPresenceCache.TryGetValue(id, out value);
			return value;
		}

		public static void RefreshCachedPlayerPresence(ulong profileId)
		{
			string requestUri = string.Format("{0}v1/{1}", "api/presence/", profileId);
			Core.Get(requestUri, delegate(string error, PlayerPresence playerPresence)
			{
				if (string.IsNullOrEmpty(error))
				{
					playerPresenceCache[profileId] = playerPresence;
					RaisePlayerPresenceUpdatedEvent(profileId, playerPresence);
				}
				else
				{
					Debug.LogError("Failed to refresh player presence: " + error);
				}
			});
		}

		public static void RefreshCachedPlayerPresences(List<ulong> profileIds)
		{
			string requestUri = string.Format("{0}v1/list", "api/presence/");
			string json = Json.Serialize(profileIds);
			Core.Post(requestUri, json, delegate(string error, List<PlayerPresence> playerPresenceList)
			{
				if (string.IsNullOrEmpty(error))
				{
					foreach (ulong id in profileIds)
					{
						PlayerPresence playerPresence = playerPresenceList.FirstOrDefault((PlayerPresence p) => p.PlayerId == id);
						if (playerPresence == null)
						{
							playerPresenceCache.Remove(id);
						}
						else
						{
							playerPresenceCache[id] = playerPresence;
						}
						RaisePlayerPresenceUpdatedEvent(id, playerPresence);
					}
					return;
				}
				Debug.LogError("Failed to refresh player presence: " + error);
			});
		}

		public static void UpdatePlayerPresence(PlayerPresence playerPresence)
		{
			string requestUri = string.Format("{0}v2", "api/presence/");
			Dictionary<string, object> obj = playerPresence.Serialize();
			string json = Json.Serialize(obj);
			Core.Post(requestUri, json, delegate(string error)
			{
				if (!string.IsNullOrEmpty(error))
				{
					Debug.LogError("Failed to update player presence: " + error);
				}
			});
		}

		private static void RaisePlayerPresenceUpdatedEvent(ulong id, PlayerPresence playerPresence)
		{
			try
			{
				if (PlayerPresenceManager.OnPlayerPresenceUpdated != null)
				{
					PlayerPresenceManager.OnPlayerPresenceUpdated(id, playerPresence);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}
}
