using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecNet
{
	public class GameSessions
	{
		private const string GAME_SESSIONS_API = "api/gamesessions/v1/";

		public static IEnumerator GetAllGameSessionsFromServer(Core.ApiCallback<List<GameSession>> callback)
		{
			string requestUri = string.Format("{0}?v={1}", "api/gamesessions/v1/", BuildSettings.Version);
			return Core.Get(requestUri, callback);
		}

		public static IEnumerator GetGameSessionFromServer(string id, Core.ApiCallback<GameSession> callback)
		{
			string requestUri = string.Format("{0}{1}", "api/gamesessions/v1/", WWW.EscapeURL(id));
			return Core.Get(requestUri, callback);
		}
	}
}
