using System.Collections;
using UnityEngine;

namespace RecNet
{
	public class Config
	{
		private const string CONFIG_API = "api/config/";

		private static RecRoomConfig instance;

		public static string MessageOfTheDay
		{
			get
			{
				return instance.MessageOfTheDay;
			}
		}

		public static MatchmakingConfigParams MatchmakingParams
		{
			get
			{
				return instance.MatchmakingParams;
			}
		}

		public static ProgressionManager.Objective[][] DailyObjectives
		{
			get
			{
				return instance.DailyObjectives;
			}
		}

		public static string GetConfigSetting(string key)
		{
			string value;
			return (!instance.ConfigTable.TryGetValue(key, out value)) ? null : value;
		}

		public static IEnumerator DownloadConfigSettings(Core.ApiCallback callback)
		{
			string requestUri = string.Format("{0}v2", "api/config/");
			return Core.Get(requestUri, delegate(string error, RecRoomConfig config)
			{
				instance = config;
				if (!string.IsNullOrEmpty(error))
				{
					Debug.LogError("Failed to download config settings: " + error);
					Core.SafeInvoke(callback, "Failed to connect to RecNet");
				}
				else
				{
					Core.SafeInvoke(callback, null);
				}
			});
		}
	}
}
