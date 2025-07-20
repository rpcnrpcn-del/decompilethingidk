using System;
using System.Collections;
using System.Collections.Generic;
using AmplitudeMiniJSON;
using UnityEngine;
using UnityEngine.Networking;

namespace AmplitudeAnalytics
{
	public class AmplitudeAnalyticsClient : MonoBehaviour
	{
		private enum QuitState
		{
			Running = 0,
			WaitingForFlush = 1,
			Flushed = 2
		}

		private class AnalyticsCache
		{
			private static List<Dictionary<string, object>> queuedEvents = new List<Dictionary<string, object>>();

			public static int TotalCachedEvents
			{
				get
				{
					return queuedEvents.Count;
				}
			}

			public static List<Dictionary<string, object>> GetBatch(int batchSize)
			{
				int num = Mathf.Min(batchSize, queuedEvents.Count);
				if (num > 0)
				{
					return queuedEvents.GetRange(0, num);
				}
				return null;
			}

			public static void SaveEvent(AmplitudeAnalyticsEvent anEvent, bool skipSaveToDisk = false)
			{
				SaveEvent(anEvent.ToJsonDictionary(), skipSaveToDisk);
			}

			public static void SaveEvent(Dictionary<string, object> anEvent, bool skipSaveToDisk = false)
			{
				queuedEvents.Add(anEvent);
				if (!skipSaveToDisk)
				{
					SaveToDisk();
				}
			}

			public static void RemoveEventList(List<Dictionary<string, object>> events)
			{
				foreach (Dictionary<string, object> @event in events)
				{
					RemoveEvent(@event, true);
				}
				SaveToDisk();
			}

			public static void RemoveEvent(Dictionary<string, object> anEvent, bool skipSaveToDisk = false)
			{
				queuedEvents.Remove(anEvent);
				if (!skipSaveToDisk)
				{
					SaveToDisk();
				}
			}

			public static void SaveToDisk()
			{
				string value = Json.Serialize(queuedEvents);
				PlayerPrefs.SetString("queued_events", value);
				PlayerPrefs.Save();
			}

			public static void LoadFromDisk()
			{
				string json = PlayerPrefs.GetString("queued_events");
				List<object> list = Json.Deserialize(json) as List<object>;
				if (list == null)
				{
					return;
				}
				foreach (object item2 in list)
				{
					Dictionary<string, object> item = item2 as Dictionary<string, object>;
					queuedEvents.Add(item);
				}
			}

			public static void RemoveList(List<Dictionary<string, object>> eventParams)
			{
				foreach (Dictionary<string, object> eventParam in eventParams)
				{
					RemoveEvent(eventParam, true);
				}
				SaveToDisk();
			}
		}

		[Serializable]
		public class Settings
		{
			public string ApiURL;

			public string ApiKey;

			public float BatchIntervalSeconds = 30f;

			public bool verboseLogging;
		}

		public static AmplitudeAnalyticsClient Instance;

		[SerializeField]
		private Settings settings;

		private long lastEventId = -1L;

		private long lastIdentifyId = -1L;

		private long lastEventTime = -1L;

		private long previousSessionId = -1L;

		private bool backoffUpload;

		private bool usingForegroundTracking;

		private bool trackingSessionEvents;

		private bool inForeground;

		private bool flushEventsOnClose = true;

		private bool initialized;

		private float lastFlushTime = float.MinValue;

		private static string userId = string.Empty;

		private QuitState quitState;

		private static long? _sessionId;

		private int eventsThisSecond;

		private int batchesThisSecond;

		private float throttleTime;

		private bool flushRunning;

		private static int SequenceNumber
		{
			get
			{
				int num = 0;
				if (PlayerPrefs.HasKey("amplitude_sequence_number"))
				{
					num = PlayerPrefs.GetInt("amplitude_sequence_number");
				}
				int value = ((num < 2147483646) ? (num + 1) : 0);
				PlayerPrefs.SetInt("amplitude_sequence_number", value);
				return num;
			}
		}

		public static long SessionId
		{
			get
			{
				if (!_sessionId.HasValue)
				{
					_sessionId = UTCMillisSinceEpoch();
				}
				return _sessionId.Value;
			}
		}

		public void ForceSettings(Settings forcedSettings)
		{
			Debug.Log("Forcing settings for Amplitude");
			settings = forcedSettings;
		}

		private void Awake()
		{
			Instance = this;
			AnalyticsCache.LoadFromDisk();
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}

		private void Start()
		{
			lastFlushTime = float.MinValue;
		}

		private void Update()
		{
			if (initialized)
			{
				throttleTime += Time.deltaTime;
				if (throttleTime >= 1f)
				{
					eventsThisSecond = 0;
					batchesThisSecond = 0;
					throttleTime = 0f;
				}
				if (!flushRunning && (Time.realtimeSinceStartup >= lastFlushTime + settings.BatchIntervalSeconds || AnalyticsCache.TotalCachedEvents >= 10))
				{
					flushRunning = true;
					StartCoroutine(Flush());
				}
			}
		}

		private void OnDestroy()
		{
			if (quitState == QuitState.Flushed)
			{
				Environment.Exit(0);
			}
		}

		private void OnApplicationQuit()
		{
			StandaloneApplicationQuit();
		}

		public IEnumerator Initialize(AmplitudeAnalyticsEvent startEvent)
		{
			flushRunning = true;
			yield return Flush();
			initialized = true;
			LogEvent(startEvent);
		}

		private void InEditorApplicationQuit()
		{
			AmplitudeAnalyticsEvent analyticsEvent = new AmplitudeAnalyticsEvent("session_end", SessionId, SequenceNumber, userId);
			LogEventAsync(analyticsEvent);
		}

		private void StandaloneApplicationQuit()
		{
			// wtf rec room why you cancel my quitting
			/*if (quitState == QuitState.Running)
			{
				AmplitudeAnalyticsEvent analyticsEvent = new AmplitudeAnalyticsEvent("session_end", SessionId, SequenceNumber, userId);
				LogEventAsync(analyticsEvent);
				Application.CancelQuit();
				StartCoroutine(WaitForFlushAndQuit());
			}*/
		}

		private IEnumerator WaitForFlushAndQuit()
		{
			/*float timeout = 3f;
			quitState = QuitState.WaitingForFlush;
			while (flushRunning && timeout > 0f)
			{
				timeout -= Time.unscaledDeltaTime;
				yield return null;
			}
			flushRunning = true;
			yield return Flush(timeout);
			quitState = QuitState.Flushed;
			Application.Quit();*/
			yield break;
		}

		public static long UTCMillisSinceEpoch()
		{
			return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
		}

		public static AmplitudeAnalyticsEvent Event(string event_type)
		{
			//return new AmplitudeAnalyticsEvent(event_type, SessionId, SequenceNumber, userId);
			return null;
		}

		public AmplitudeAnalyticsEvent InitializeEvent(string userId)
		{
			//return InitializeEvent(userId, Constants.DefaultUnityDeviceInfo);
            return null;
        }

		public AmplitudeAnalyticsEvent InitializeEvent(string userId, Func<AmplitudeAnalyticsEvent.DeviceInfo> deviceInfoFunc)
		{
			/*AmplitudeAnalyticsClient.userId = userId;
			return new AmplitudeAnalyticsEvent("session_start", SessionId, SequenceNumber, userId).WithDeviceInfo(deviceInfoFunc());*/
            return null;
        }

		private IEnumerator LogEventRoutine(AmplitudeAnalyticsEvent analyticsEvent)
		{
			yield break;
			//return PostJson(settings.ApiURL, analyticsEvent.ToJsonDictionary(), OnLogSingleResponse);
		}

		public void LogEvent(AmplitudeAnalyticsEvent analyticsEvent)
		{
			/*if (!initialized)
			{
				throw new InvalidOperationException("You must initialize before you can log events");
			}
			if (eventsThisSecond < 1000)
			{
				eventsThisSecond++;
				StartCoroutine(LogEventRoutine(analyticsEvent));
			}
			else
			{
				Debug.LogWarning("Too many analytics events being sent. Saving for later");
				AnalyticsCache.SaveEvent(analyticsEvent);
			}*/
		}

		public void LogEventAsync(AmplitudeAnalyticsEvent analyticsEvent)
		{
			/*if (!initialized)
			{
				throw new InvalidOperationException("You must initialize before you can log events");
			}
			AnalyticsCache.SaveEvent(analyticsEvent);*/
		}

		private bool IsTimedOut(float startTime, float timeout)
		{
			/*if (timeout <= 0f)
			{
				return false;
			}
			float num = Time.realtimeSinceStartup - startTime;
			return num > timeout;*/
			return false;
		}

		private IEnumerator Flush(float timeout = -1f)
		{
			/*lastFlushTime = Time.realtimeSinceStartup;
			float startTime = Time.realtimeSinceStartup;
			bool shouldSendBatch = true;
			if (batchesThisSecond >= 100)
			{
				Debug.LogWarning("Too many batches being sent. Skipping flush for now");
				shouldSendBatch = false;
			}
			if (eventsThisSecond >= 1000)
			{
				Debug.LogWarning("Too many events being sent. Skipping flush for now");
				shouldSendBatch = false;
			}
			if (!shouldSendBatch)
			{
				yield break;
			}
			int totals = AnalyticsCache.TotalCachedEvents;
			bool erroredOut = false;
			bool timedOut = false;
			int batchSize = 0;
			while (true)
			{
				List<Dictionary<string, object>> batch2;
				List<Dictionary<string, object>> batch = (batch2 = AnalyticsCache.GetBatch(10));
				if (batch2 == null || erroredOut)
				{
					break;
				}
				bool flag;
				timedOut = (flag = IsTimedOut(startTime, timeout));
				if (flag)
				{
					break;
				}
				batchSize = batch.Count;
				yield return PostJson(settings.ApiURL, batch, delegate(long responseCode, string responseBody, List<Dictionary<string, object>> eventParams)
				{
					if (responseCode >= 200 && responseCode < 300)
					{
						AnalyticsCache.RemoveList(eventParams);
					}
					else if (responseCode == 413)
					{
						AnalyticsCache.RemoveList(eventParams);
						Debug.LogWarning("Received 413 error from Analytics request");
					}
					else
					{
						Debug.LogWarning(string.Format("AnalyticsRequest failed: [{0}] {1}", responseCode, responseBody));
						erroredOut = true;
						if (responseCode == 400 && responseBody.Contains("invalid api_key"))
						{
							Debug.LogError("Bad api key. Dropping messages");
							AnalyticsCache.RemoveList(eventParams);
						}
					}
				});
			}
			if (batchSize > 0 && settings.verboseLogging)
			{
				Debug.Log(string.Format("[{0}] Flush complete: {1} event sent. Finished with error: {2}. TimedOut: {3}", string.Format("{0:s}", DateTime.Now), batchSize, erroredOut, timedOut));
			}
			flushRunning = false;*/
			yield break;
		}

		private IEnumerator PostJson<T>(string url, T eventParams, Action<long, string, T> onResponse)
		{
			/*WWWForm form = new WWWForm();
			form.AddField("api_key", settings.ApiKey);
			string jsonString = Json.Serialize(eventParams);
			form.AddField("event", jsonString);
			UnityWebRequest request = UnityWebRequest.Post(url, form);
			request.downloadHandler = new DownloadHandlerBuffer();
			yield return request.Send();
			onResponse(request.responseCode, request.downloadHandler.text, eventParams);*/
			yield break;
		}

		private void OnLogSingleResponse(long responseCode, string responseBody, Dictionary<string, object> eventParams)
		{
			/*if (responseCode >= 200 && responseCode < 300)
			{
				AnalyticsCache.RemoveEvent(eventParams);
			}
			else if (responseCode != 413)
			{
				Debug.LogWarning("AnalyticsRequest failed: " + responseBody);
			}*/
		}

		private void OnLogMultiResponse(long responseCode, string responseBody, List<Dictionary<string, object>> eventParams)
		{
		}
	}
}
