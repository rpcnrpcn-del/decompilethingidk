using System.Collections;
using System.Collections.Generic;
using GAMiniJSON;
using UnityEngine;

namespace RecNet
{
	public static class Settings
	{
		private const string SETTINGS_API = "api/settings/";

		public static IEnumerator DowloadLocalPlayerSettings(Core.ApiCallback<List<Setting>> callback)
		{
			string requestUri = string.Format("{0}v2/", "api/settings/");
			yield return Core.Get(requestUri, delegate(string error, List<Setting> settings)
			{
				if (string.IsNullOrEmpty(error))
				{
					Core.SafeInvoke(callback, null, settings);
				}
				else
				{
					Debug.LogError("Failed to download player settings: " + error);
					Core.SafeInvoke(callback, "Failed to download player settings", null);
				}
			});
		}

		public static IEnumerator StoreLocalPlayerSetting(Setting setting, Core.ApiCallback callback)
		{
			string action = ((!string.IsNullOrEmpty(setting.Value)) ? "set" : "remove");
			string requestUri = string.Format("{0}v2/{1}", "api/settings/", action);
			Dictionary<string, object> dict = setting.Serialize();
			string json = Json.Serialize(dict);
			yield return Core.Post(requestUri, json, callback);
		}
	}
}
