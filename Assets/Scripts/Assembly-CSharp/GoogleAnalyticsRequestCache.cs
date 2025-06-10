using System;
using System.Collections.Generic;
using GAMiniJSON;
using UnityEngine;

public class GoogleAnalyticsRequestCache
{
	private const string DATA_SPLITTER = "|";

	private const string RQUEST_DATA_SPLITTER = "%rps%";

	private const string GA_DATA_CACHE_KEY = "GoogleAnalyticsRequestCache";

	public static string SavedData
	{
		get
		{
			if (PlayerPrefs.HasKey("GoogleAnalyticsRequestCache"))
			{
				return PlayerPrefs.GetString("GoogleAnalyticsRequestCache");
			}
			return string.Empty;
		}
		set
		{
			PlayerPrefs.SetString("GoogleAnalyticsRequestCache", value);
		}
	}

	public static List<GACachedRequest> CurrenCachedRequests
	{
		get
		{
			if (SavedData == string.Empty)
			{
				return new List<GACachedRequest>();
			}
			try
			{
				List<GACachedRequest> list = new List<GACachedRequest>();
				List<object> list2 = Json.Deserialize(SavedData) as List<object>;
				foreach (object item in list2)
				{
					List<object> list3 = item as List<object>;
					GACachedRequest gACachedRequest = new GACachedRequest();
					int num = 1;
					foreach (object item2 in list3)
					{
						string text = item2 as string;
						switch (num)
						{
						case 1:
							gACachedRequest.RequestBody = text;
							break;
						case 2:
							gACachedRequest.TimeCreated = Convert.ToInt64(text);
							break;
						}
						num++;
					}
					list.Add(gACachedRequest);
				}
				return list;
			}
			catch (Exception ex)
			{
				Clear();
				Debug.LogError(ex.Message);
				return new List<GACachedRequest>();
			}
		}
	}

	public static void SaveRequest(string cache)
	{
		GACachedRequest item = new GACachedRequest(cache, DateTime.Now.Ticks);
		List<GACachedRequest> currenCachedRequests = CurrenCachedRequests;
		currenCachedRequests.Add(item);
		Debug.Log(currenCachedRequests.Count);
		CacheRequests(currenCachedRequests);
	}

	public static void SendCashedRequests()
	{
		List<GACachedRequest> currenCachedRequests = CurrenCachedRequests;
		foreach (GACachedRequest item in currenCachedRequests)
		{
			string requestBody = item.RequestBody;
			if (GoogleAnalyticsSettings.Instance.IsQueueTimeEnabled)
			{
				requestBody = requestBody + "&qt" + item.Delay;
				GoogleAnalytics.SendSkipCache(requestBody);
			}
		}
		Clear();
	}

	public static void Clear()
	{
		PlayerPrefs.DeleteKey("GoogleAnalyticsRequestCache");
	}

	public static void CacheRequests(List<GACachedRequest> requests)
	{
		List<List<string>> list = new List<List<string>>();
		foreach (GACachedRequest request in requests)
		{
			List<string> list2 = new List<string>();
			list2.Add(request.RequestBody);
			list2.Add(request.TimeCreated.ToString());
			list.Add(list2);
		}
		SavedData = Json.Serialize(list);
	}
}
