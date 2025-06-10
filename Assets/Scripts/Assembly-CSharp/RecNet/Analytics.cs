using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecNet
{
	public class Analytics
	{
		private const string ANALYTICS_API = "api/analytics/";

		public static void SendEvent(string category, string action, string label, float? value = null, float? value2 = null, float? value3 = null)
		{
			Event(category, action, label, value, value2, value3, delegate(string e)
			{
				if (!string.IsNullOrEmpty(e))
				{
					Debug.LogError("Recnet analytics failed to send event. " + e);
				}
			});
		}

		private static IEnumerator Event(string category, string action, string label, float? value, float? value2, float? value3, Core.ApiCallback callback)
		{
			string requestUri = string.Format("{0}v1/session/event", "api/analytics/");
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("SessionId", Core.SessionId.ToString());
			dictionary.Add("Category", category);
			dictionary.Add("Action", action);
			dictionary.Add("Label", label);
			if (value.HasValue)
			{
				dictionary.Add("Value", value.Value.ToString("F3"));
			}
			if (value2.HasValue)
			{
				dictionary.Add("Value2", value2.Value.ToString("F3"));
			}
			if (value3.HasValue)
			{
				dictionary.Add("Value3", value3.Value.ToString("F3"));
			}
			return Core.Post(requestUri, dictionary, callback);
		}
	}
}
