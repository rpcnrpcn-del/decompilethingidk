using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GAMiniJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.VR;
using WebSocketSharp;

namespace RecNet
{
	public class Core
	{
		public const bool PLAYIT_BUILD = false;

		public delegate void RawApiCallback(UnityWebRequest www);

		public delegate void ApiCallback(string error);

		public delegate void ApiCallback<T>(string error, T result);

		public enum PushNotificationId
		{
			RelationshipChanged = 1,
			MessageReceived = 2,
			MessageDeleted = 3
		}

		public delegate void PushNotificationHandler(object message);

		private class QueuedApiCall : CustomYieldInstruction
		{
			public string Uri;

			public Func<string, UnityWebRequest> ConstructRequest;

			public RawApiCallback Callback;

			public bool Done;

			public override bool keepWaiting
			{
				get
				{
					return !Done;
				}
			}

			public QueuedApiCall(string uri, Func<string, UnityWebRequest> constructRequest, RawApiCallback callback)
			{
				Uri = uri;
				ConstructRequest = constructRequest;
				Callback = callback;
				Done = false;
			}
		}

		private class PushNotificationChannel
		{
			private enum State
			{
				Disconnected = 0,
				Connecting = 1,
				PerformingHandshake = 2,
				Connected = 3,
				Disconnecting = 4
			}

			private struct PushNotification
			{
				public readonly PushNotificationId Id;

				public readonly object Msg;

				private PushNotification(PushNotificationId id, object msg)
				{
					Id = id;
					Msg = msg;
				}

				public static PushNotification Parse(string json)
				{
					Dictionary<string, object> dictionary = Json.Deserialize(json) as Dictionary<string, object>;
					return new PushNotification((PushNotificationId)Util.GetKey<int>("Id", dictionary), dictionary["Msg"]);
				}
			}

			private const string SERVERMESSAGE_API = "api/notification/v2";

			public static Dictionary<PushNotificationId, PushNotificationHandler> NotificationHandlers = new Dictionary<PushNotificationId, PushNotificationHandler>();

			private static WebSocket socket;

			private static int consecutiveConnectRetries = 0;

			private static float lastDisconnectTime = 0f;

			private static bool autoReconnect = false;

			private static State state = State.Disconnected;

			public static event Action OnConnect;

			public static IEnumerator Initialize(ApiCallback callback)
			{
				for (int i = 0; i < 3; i++)
				{
					if (i > 0)
					{
						yield return new WaitForSeconds(GetExponentialBackoffTime(i));
					}
					Connect();
					yield return new WaitUntil(() => state == State.Connected || state == State.Disconnected);
					if (state == State.Connected)
					{
						SafeInvoke(callback, null);
						yield break;
					}
				}
				SafeInvoke(callback, "Failed to connect to RecNet");
			}

			public static void Shutdown()
			{
				autoReconnect = false;
				Disconnect();
			}

			private static void Connect()
			{
				LazyInit();
				if (consecutiveConnectRetries > 0)
				{
					float exponentialBackoffTime = GetExponentialBackoffTime(consecutiveConnectRetries);
					if (Time.realtimeSinceStartup - lastDisconnectTime < exponentialBackoffTime)
					{
						QueueEvent(Connect);
						return;
					}
				}
				Debug.Log($"[RecNet Core] (1) Connecting to WebSocket... ({REC_NET_WEBSOCKET_URL})");
				socket = new WebSocket(REC_NET_WEBSOCKET_URL + "api/notification/v2");
				socket.SetCredentials("recroom@againstgrav.com", "recnet87", true);
				socket.OnOpen += DispatchOnUnityThread(OnOpen);
				socket.OnClose += DispatchOnUnityThread<CloseEventArgs>(OnClose);
				socket.OnError += DispatchOnUnityThread<ErrorEventArgs>(OnError);
				socket.OnMessage += DispatchOnUnityThread<MessageEventArgs>(OnMessage);
                Debug.Log($"[RecNet Core] (2) Connecting to WebSocket... ({REC_NET_WEBSOCKET_URL})");
                state = State.Connecting;
				socket.ConnectAsync();
			}

			private static void Disconnect()
			{
				if (socket != null && (socket.ReadyState == WebSocketState.Connecting || socket.ReadyState == WebSocketState.Open))
				{
					state = State.Disconnecting;
					socket.CloseAsync();
				}
			}

			private static void OnOpen(object sender, EventArgs e)
			{
				state = State.PerformingHandshake;
				socket.SendAsync(CreateHanshakeMessage(), DispatchOnUnityThread<bool>(OnSendHandshakeComplete));
			}

			private static string CreateHanshakeMessage()
			{
				string version = BuildSettings.Version;
				string ipAddress = Network.player.ipAddress;
				string value = VRSettings.loadedDeviceName + "_" + VRDevice.model;
				bool isDeveloper = SessionManager.IsDeveloper;
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("PlayerId", Profiles.LocalProfile.Id.ToString());
				dictionary.Add("AppVersion", version);
				dictionary.Add("IpAddress", ipAddress);
				dictionary.Add("VRDevice", value);
				dictionary.Add("IsDevelopment", isDeveloper.ToString());
				return Json.Serialize(dictionary);
			}

			private static void OnSendHandshakeComplete(bool success)
			{
				if (!success)
				{
					Disconnect();
				}
			}

			private static void OnClose(object sender, CloseEventArgs e)
			{
				if (e.Code != 1000)
				{
					Debug.LogErrorFormat("RECNET: PushNotification ChannelClosed: {0} {1}", e.Code, e.Reason);
				}
				state = State.Disconnected;
				socket = null;
				consecutiveConnectRetries++;
				lastDisconnectTime = Time.realtimeSinceStartup;
				if (autoReconnect)
				{
					Connect();
				}
			}

			private static void OnError(object sender, ErrorEventArgs e)
			{
				if (e.Exception != null)
				{
					Debug.LogException(e.Exception);
				}
				else
				{
					Debug.LogError(e.Message);
				}
			}

			private static bool ParseSessionId(string jsonStr, out long sessionId)
			{
				sessionId = 0L;
				try
				{
					Dictionary<string, object> dict = Json.Deserialize(jsonStr) as Dictionary<string, object>;
					sessionId = Util.GetKey<long>("SessionId", dict);
					return true;
				}
				catch (Exception ex)
				{
					Debug.LogError("Core ParseSessionId failed : " + jsonStr + ". Ex : " + ex);
				}
				return false;
			}

			private static void OnMessage(object sender, MessageEventArgs e)
			{
				if (state == State.PerformingHandshake)
				{
					long sessionId = 0L;
					if (ParseSessionId(e.Data, out sessionId) && sessionId > 0)
					{
						SessionId = sessionId;
						state = State.Connected;
						consecutiveConnectRetries = 0;
						autoReconnect = true;
						try
						{
							if (PushNotificationChannel.OnConnect != null)
							{
								PushNotificationChannel.OnConnect();
							}
							return;
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
							return;
						}
					}
					Disconnect();
				}
				else
				{
					if (state != State.Connected)
					{
						return;
					}
					try
					{
						PushNotification pushNotification = PushNotification.Parse(e.Data);
						PushNotificationHandler value;
						if (NotificationHandlers.TryGetValue(pushNotification.Id, out value))
						{
							value(pushNotification.Msg);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
					}
				}
			}
		}

		public const string PRODUCTION_SERVER_URL = "recroom.azurewebsites.net";

		private static string _RecNetHost = null;

		private static readonly string REC_NET_HTTP_URL = "http://" + REC_NET_HOST + "/";

		private static readonly string REC_NET_WEBSOCKET_URL = "ws://" + WS_REC_NET_HOST + "/";

		private const string REC_NET_USERNAME = "recroom@againstgrav.com";

		private const string REC_NET_PASSWORD = "recnet87";

		private const int API_RETRIES = 3;

		private const int MAX_BACKOFF_EXPONENT = 6;

		private static bool initialized = false;

		private static readonly string AUTHENTICATION_HEADER = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes("recroom@againstgrav.com:recnet87"));

		private static Queue<QueuedApiCall> callApiQueue = new Queue<QueuedApiCall>();

		private static Queue<Action> eventQueue = new Queue<Action>();

		public static string REC_NET_HOST
		{
			get
			{
				/*if (_RecNetHost == null)
				{
					string[] commandLineArgs = Environment.GetCommandLineArgs();
					foreach (string text in commandLineArgs)
					{
						if (text.StartsWith("+RecNetHostOverride:"))
						{
							_RecNetHost = text.Substring("+RecNetHostOverride:".Length);
							break;
						}
					}
					if (string.IsNullOrEmpty(_RecNetHost))
					{
						_RecNetHost = "recroom.azurewebsites.net";
					}
				}
				return _RecNetHost;*/
				if (!PlayerPrefs.HasKey("RecNet_Host"))
				{
					PlayerPrefs.SetString("RecNet_Host", "127.0.0.1:25565");
				}
				return (!PLAYIT_BUILD) ? PlayerPrefs.GetString("RecNet_Host", "127.0.0.1:25565") : "them-collaboration.gl.at.ply.gg:37450";
			}
        }
        public static string WS_REC_NET_HOST
        {
            get
            {
                /*if (_RecNetHost == null)
				{
					string[] commandLineArgs = Environment.GetCommandLineArgs();
					foreach (string text in commandLineArgs)
					{
						if (text.StartsWith("+RecNetHostOverride:"))
						{
							_RecNetHost = text.Substring("+RecNetHostOverride:".Length);
							break;
						}
					}
					if (string.IsNullOrEmpty(_RecNetHost))
					{
						_RecNetHost = "recroom.azurewebsites.net";
					}
				}
				return _RecNetHost;*/
                if (!PlayerPrefs.HasKey("WS_Host"))
                {
                    PlayerPrefs.SetString("WS_Host", "127.0.0.1:7777");
                }
                return (!PLAYIT_BUILD) ? PlayerPrefs.GetString("WS_Host", "127.0.0.1:7777") : "positive-catalog.gl.at.ply.gg:3808";
            }
        }

        public static long SessionId { get; private set; }

		public static IEnumerator Get(string requestUri, RawApiCallback callback = null)
		{
			return CallApi(requestUri, UnityWebRequest.Get, callback);
		}

		public static IEnumerator Get(string requestUri, ApiCallback callback = null)
		{
			return Get(requestUri, ErrorOnlyCallback(callback));
		}

		public static IEnumerator Get<T>(string requestUri, ApiCallback<T> callback) where T : IRecNetObject, new()
		{
			return Get(requestUri, ParseSingleCallback(callback));
		}

		public static IEnumerator Get<T>(string requestUri, ApiCallback<List<T>> callback) where T : IRecNetObject, new()
		{
			return Get(requestUri, ParseMultipleCallback(callback));
		}

		public static IEnumerator Get(string requestUri, List<KeyValuePair<string, string>> headers, RawApiCallback callback = null)
		{
			return CallApi(requestUri, delegate(string url)
			{
				UnityWebRequest unityWebRequest = UnityWebRequest.Get(url);
				foreach (KeyValuePair<string, string> header in headers)
				{
					unityWebRequest.SetRequestHeader(header.Key, header.Value);
				}
				return unityWebRequest;
			}, callback);
		}

		public static IEnumerator Get(string requestUri, List<KeyValuePair<string, string>> headers, ApiCallback callback)
		{
			return Get(requestUri, headers, ErrorOnlyCallback(callback));
		}

		public static IEnumerator Get<T>(string requestUri, List<KeyValuePair<string, string>> headers, ApiCallback<T> callback) where T : IRecNetObject, new()
		{
			return Get(requestUri, headers, ParseSingleCallback(callback));
		}

		public static IEnumerator Get<T>(string requestUri, List<KeyValuePair<string, string>> headers, ApiCallback<List<T>> callback) where T : IRecNetObject, new()
		{
			return Get(requestUri, headers, ParseMultipleCallback(callback));
		}

		public static IEnumerator Post(string requestUri, List<IMultipartFormSection> form, RawApiCallback callback = null)
		{
			throw new NotImplementedException("There is a bug in UnityWebRequest's handling of multipart-form requests. Use WWWForm instead!");
		}

		public static IEnumerator Post(string requestUri, List<IMultipartFormSection> form, ApiCallback callback)
		{
			return Post(requestUri, form, ErrorOnlyCallback(callback));
		}

		public static IEnumerator Post<T>(string requestUri, List<IMultipartFormSection> form, ApiCallback<T> callback) where T : IRecNetObject, new()
		{
			return Post(requestUri, form, ParseSingleCallback(callback));
		}

		public static IEnumerator Post<T>(string requestUri, List<IMultipartFormSection> form, ApiCallback<List<T>> callback) where T : IRecNetObject, new()
		{
			return Post(requestUri, form, ParseMultipleCallback(callback));
		}

		public static IEnumerator Post(string requestUri, Dictionary<string, string> form, RawApiCallback callback = null)
		{
			return CallApi(requestUri, (string url) => UnityWebRequest.Post(url, form), callback);
		}

		public static IEnumerator Post(string requestUri, Dictionary<string, string> form, ApiCallback callback)
		{
			return Post(requestUri, form, ErrorOnlyCallback(callback));
		}

		public static IEnumerator Post<T>(string requestUri, Dictionary<string, string> form, ApiCallback<T> callback) where T : IRecNetObject, new()
		{
			return Post(requestUri, form, ParseSingleCallback(callback));
		}

		public static IEnumerator Post<T>(string requestUri, Dictionary<string, string> form, ApiCallback<List<T>> callback) where T : IRecNetObject, new()
		{
			return Post(requestUri, form, ParseMultipleCallback(callback));
		}

		public static IEnumerator Post(string requestUri, WWWForm form, RawApiCallback callback = null)
		{
			return CallApi(requestUri, (string url) => UnityWebRequest.Post(url, form), callback);
		}

		public static IEnumerator Post(string requestUri, WWWForm form, ApiCallback callback)
		{
			return Post(requestUri, form, ErrorOnlyCallback(callback));
		}

		public static IEnumerator Post<T>(string requestUri, WWWForm form, ApiCallback<T> callback) where T : IRecNetObject, new()
		{
			return Post(requestUri, form, ParseSingleCallback(callback));
		}

		public static IEnumerator Post<T>(string requestUri, WWWForm form, ApiCallback<List<T>> callback) where T : IRecNetObject, new()
		{
			return Post(requestUri, form, ParseMultipleCallback(callback));
		}

		public static IEnumerator Post(string requestUri, string json, RawApiCallback callback = null)
        {
            return CallApi(requestUri, delegate(string url)
			{
				UnityWebRequest unityWebRequest = new UnityWebRequest(url, "POST");
				unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
				unityWebRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
				unityWebRequest.disposeDownloadHandlerOnDispose = true;
				unityWebRequest.disposeUploadHandlerOnDispose = true;
				unityWebRequest.SetRequestHeader("Content-Type", "text/json");
				return unityWebRequest;
			}, callback);
		}

		public static IEnumerator Post(string requestUri, string json, ApiCallback callback)
		{
			return Post(requestUri, json, ErrorOnlyCallback(callback));
		}

		public static IEnumerator Post<T>(string requestUri, string json, ApiCallback<T> callback) where T : IRecNetObject, new()
		{
			return Post(requestUri, json, ParseSingleCallback(callback));
		}

		public static IEnumerator Post<T>(string requestUri, string json, ApiCallback<List<T>> callback) where T : IRecNetObject, new()
        {
            return Post(requestUri, json, ParseMultipleCallback(callback));
		}

		public static IEnumerator InitializePushNotificationChannel(ApiCallback callback)
		{
			Relationships.RegisterPushNotificationCallbacks();
			Messages.RegisterPushNotificationCallbacks();
			return PushNotificationChannel.Initialize(callback);
		}

		public static void ShutdownPushNotificationChannel()
		{
			PushNotificationChannel.Shutdown();
		}

		public static void RegisterPushNotificationConnectionCallback(Action callback)
		{
			PushNotificationChannel.OnConnect += callback;
		}

		public static void RegisterPushNotificationHandler(PushNotificationId id, PushNotificationHandler callback)
		{
			if (PushNotificationChannel.NotificationHandlers.ContainsKey(id))
			{
				Dictionary<PushNotificationId, PushNotificationHandler> notificationHandlers = PushNotificationChannel.NotificationHandlers;
				PushNotificationId key = id;
				notificationHandlers[key] = (PushNotificationHandler)Delegate.Combine(notificationHandlers[key], callback);
			}
			else
			{
				PushNotificationChannel.NotificationHandlers.Add(id, callback);
			}
		}

		public static IEnumerator TestConnection(ApiCallback callback)
		{
			long responseCode = 0L;
			string error = null;
			yield return Get("api/versioncheck/v1", delegate(UnityWebRequest unityWebRequest)
			{
				responseCode = unityWebRequest.responseCode;
				error = GetError(unityWebRequest);
			});
			if (string.IsNullOrEmpty(error))
			{
				SafeInvoke(callback, null);
				yield break;
			}
			if (responseCode == 403)
			{
				SafeInvoke(callback, "Rec Room update required");
				yield break;
			}
			UnityWebRequest www = UnityWebRequest.Get("http://www.google.com/generate_204");
			yield return www.Send();
			if (www.responseCode == 204)
			{
				Debug.LogError("RecNet connectivity test failed: " + error);
				SafeInvoke(callback, "Failed to connect to RecNet");
			}
			else
			{
				Debug.LogError("Network connectivity test failed: " + GetError(www));
				SafeInvoke(callback, "No internet connection");
			}
		}

		private static void LazyInit()
		{
			if (!initialized)
			{
				GameObject gameObject = new GameObject();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				gameObject.hideFlags = HideFlags.HideInHierarchy;
				RecNetCore recNetCore = gameObject.AddComponent<RecNetCore>();
				recNetCore.StartCoroutine(ProcessCallApiQueue());
				recNetCore.StartCoroutine(ProcessEventQueue());
				initialized = true;
			}
		}

		private static float GetExponentialBackoffTime(int retryCount)
		{
			return (retryCount <= 0) ? 0f : ((float)(1 << Mathf.Min(retryCount - 1, 6)));
		}

		private static IEnumerator CallApi(string uri, Func<string, UnityWebRequest> onstructRequest, RawApiCallback callback)
		{
			LazyInit();
			QueuedApiCall queuedApiCall = new QueuedApiCall(uri, onstructRequest, callback);
			callApiQueue.Enqueue(queuedApiCall);
			return queuedApiCall;
		}

		private static IEnumerator ProcessCallApiQueue()
		{
			int consecutiveRetries = 0;
			float previousErrorTime = 0f;
			while (true)
			{
				yield return new WaitUntil(() => callApiQueue.Count > 0);
				QueuedApiCall apiCall = callApiQueue.Dequeue();
				UnityWebRequest www = null;
				byte[] wwwRequestBody = null;
				for (int retry = 0; retry < 3; retry++)
				{
					if (consecutiveRetries > 0)
					{
						float exponentialBackoff = GetExponentialBackoffTime(consecutiveRetries);
						float timeToWait = exponentialBackoff - (Time.realtimeSinceStartup - previousErrorTime);
						if (timeToWait > 0f)
						{
							yield return new WaitForSecondsRealtime(timeToWait);
						}
					}
					if (www != null)
					{
						www.Dispose();
					}
                    www = CreateRequest(apiCall);
					yield return www.Send();
					if (www.isError || www.responseCode == 429 || (www.responseCode >= 500 && www.responseCode < 600))
					{
						consecutiveRetries++;
						previousErrorTime = Time.realtimeSinceStartup;
						continue;
					}
					consecutiveRetries = 0;
					break;
				}
				SafeInvoke(apiCall.Callback, www);
				www.Dispose();
				apiCall.Done = true;
			}
		}

		private static UnityWebRequest CreateRequest(QueuedApiCall apiCall)
		{
			UnityWebRequest unityWebRequest = apiCall.ConstructRequest(REC_NET_HTTP_URL + apiCall.Uri);
			unityWebRequest.SetRequestHeader("Authorization", AUTHENTICATION_HEADER);
			unityWebRequest.SetRequestHeader("X-Rec-Room-Version", BuildSettings.Version);
			if (Profiles.LocalProfile != null)
			{
				unityWebRequest.SetRequestHeader("X-Rec-Room-Profile", Profiles.LocalProfile.Id.ToString());
			}
			return unityWebRequest;
		}

		private static void LogApiCall(QueuedApiCall apiCall, UnityWebRequest www, byte[] wwwRequestBody)
		{
			string text = string.Empty;
			if (www.uploadHandler != null && wwwRequestBody != null)
			{
				text = ((!(www.uploadHandler.contentType == "application/x-www-form-urlencoded") && !(www.uploadHandler.contentType == "text/json")) ? "<multipart_form_data>" : Encoding.UTF8.GetString(wwwRequestBody));
			}
			string message = string.Format("RECNET: {0} {1}\nHTTP STATUS {2}\nREQUEST: {3}\nRESPONSE: {4}", www.method, apiCall.Uri, www.responseCode, text, (!www.isError) ? www.downloadHandler.text : www.error);
			if (www.isError || www.responseCode >= 400)
			{
				Debug.LogError(message);
			}
			else
			{
				Debug.Log(message);
			}
		}

		private static RawApiCallback ErrorOnlyCallback(ApiCallback callback)
		{
			return delegate(UnityWebRequest www)
			{
				SafeInvoke(callback, GetError(www));
			};
		}

		private static RawApiCallback ParseSingleCallback<T>(ApiCallback<T> callback) where T : IRecNetObject, new()
		{
			return delegate(UnityWebRequest www)
			{
				ParseResponse(www, ParseSingle<T>, callback);
			};
		}

		private static RawApiCallback ParseMultipleCallback<T>(ApiCallback<List<T>> callback) where T : IRecNetObject, new()
		{
			return delegate(UnityWebRequest www)
			{
				ParseResponse(www, ParseMultiple<T>, callback);
			};
		}

		private static void ParseResponse<T>(UnityWebRequest www, Func<string, T> parser, ApiCallback<T> callback)
		{
			string text = GetError(www);
			T response = default(T);
			if (string.IsNullOrEmpty(text))
			{
				try
				{
					response = parser(www.downloadHandler.text);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					text = "Malformed Response";
					response = default(T);
				}
			}
			SafeInvoke(callback, text, response);
		}

		private static T ParseSingle<T>(string json) where T : IRecNetObject, new()
		{
			Dictionary<string, object> dict = Json.Deserialize(json) as Dictionary<string, object>;
			T result = new T();
			result.Deserialize(dict);
			return result;
		}

		private static List<T> ParseMultiple<T>(string json) where T : IRecNetObject, new()
		{
			List<object> list = Json.Deserialize(json) as List<object>;
			List<T> list2 = new List<T>(list.Count);
			foreach (object item2 in list)
			{
				Dictionary<string, object> dict = item2 as Dictionary<string, object>;
				T item = new T();
				item.Deserialize(dict);
				list2.Add(item);
			}
			return list2;
		}

		public static string GetError(UnityWebRequest www)
		{
			string result = null;
			if (www.isError)
			{
				result = www.error;
			}
			else if (www.responseCode >= 400)
			{
				result = "HTTP Error " + www.responseCode;
			}
			return result;
		}

		public static void SafeInvoke(RawApiCallback callback, UnityWebRequest www)
		{
			try
			{
				if (callback != null)
				{
					callback(www);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		public static void SafeInvoke(ApiCallback callback, string error)
		{
			try
			{
				if (callback != null)
				{
					callback(error);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		public static void SafeInvoke<T>(ApiCallback<T> callback, string error, T response)
		{
			try
			{
				if (callback != null)
				{
					callback(error, response);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		private static EventHandler DispatchOnUnityThread(EventHandler eventHandler)
		{
			return delegate(object sender, EventArgs eventArgs)
			{
				QueueEvent(delegate
				{
					eventHandler(sender, eventArgs);
				});
			};
		}

		private static EventHandler<TEventArgs> DispatchOnUnityThread<TEventArgs>(EventHandler<TEventArgs> eventHandler) where TEventArgs : EventArgs
		{
			return delegate(object sender, TEventArgs eventArgs)
			{
				QueueEvent(delegate
				{
					eventHandler(sender, eventArgs);
				});
			};
		}

		private static Action<T> DispatchOnUnityThread<T>(Action<T> callback)
		{
			return delegate(T arg)
			{
				QueueEvent(delegate
				{
					callback(arg);
				});
			};
		}

		private static void QueueEvent(Action callback)
		{
			lock (eventQueue)
			{
				eventQueue.Enqueue(callback);
			}
		}

		private static IEnumerator ProcessEventQueue()
		{
			while (true)
			{
				yield return new WaitUntil(() => eventQueue.Count > 0);
				Action callback;
				lock (eventQueue)
				{
					callback = eventQueue.Dequeue();
				}
				try
				{
					callback();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
	}
}
