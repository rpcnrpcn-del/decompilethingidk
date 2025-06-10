using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon;
using RecNet;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PUNNetworkManager : Photon.MonoBehaviour
{
	[Serializable]
	private class ActivityInfo
	{
		public string Name;

		public string FriendlyName;

		public int MaxPlayers = 8;

		public bool CanJoinInProgress = true;
	}

	[Serializable]
	private class ActivityGroupInfo
	{
		public string Name;

		public string FriendlyName;

		public string[] Activities;
	}

	private class RuntimeActivityGroupInfo
	{
		public string Name;

		public string FriendlyName;

		public List<ActivityInfo> Activities;
	}

	private enum RandomRoomMatchMode
	{
		PreferFullRooms = 0,
		PreferEmptyRooms = 1,
		AnyRoom = 2
	}

	public delegate void InitializeCallback(string error);

	public static PUNNetworkManager Instance;

	public const string VR_REQUIRED_SCENE_NAME = "vr_device_required";

	public const string LOCKER_ROOM_SCENE_NAME = "lockerroom";

	public const string DORM_ROOM_SCENE_NAME = "dormroom";

	public const string MODERATOR_ROOM_SCENE_NAME = "moderator";

	public const string EMPTY_SCENE_NAME = "empty";

	public const string ANY_ACTIVITY_GROUP_NAME = "any_activity";

	public const string ROOM_PROP_KEY_ACTIVITY = "C1";

	public const string ROOM_PROP_KEY_PRIVATE = "C2";

	public const string ROOM_PROP_KEY_AVAILABLE_SPACE = "C3";

	public const string ROOM_PROP_KEY_GAME_IN_PROGRESS = "C4";

	public const string ROOM_PROP_KEY_ROOM_NAME = "C5";

	public const string ROOM_PROP_KEY_PLAYERIDS = "C6";

	private const string RICH_JOIN_COMMAND_PREFIX = "+join:";

	public const int DEFAULT_ROOM_SIZE = 8;

	private const float JOIN_PLAYER_TIMEOUT = 20f;

	private const int SERIALIZE_RATE = 30;

	public static readonly ExitGames.Client.Photon.Hashtable PhotonRoomPropertiesBugWorkaround = new ExitGames.Client.Photon.Hashtable { { "BUG", 1 } };

	private const float NETWORK_TIMEOUT = 60f;

	public AnimationCurve DynamicNetworkExtrapolationCurve;

	[SerializeField]
	private ActivityInfo[] activities;

	[SerializeField]
	private ActivityGroupInfo[] activityGroups;

	private Dictionary<string, ActivityInfo> activityMap;

	private Dictionary<string, RuntimeActivityGroupInfo> activityGroupMap;

	[Header("Network Disconnect Message")]
	[SerializeField]
	private string disconnectAlertTitle;

	[SerializeField]
	private string timeoutCouldntConnectMessage;

	[SerializeField]
	private string timeoutDisconnectMessage;

	[SerializeField]
	private string genericDisconnectMessage;

	[Header("Join Fail Message")]
	[SerializeField]
	private string joinTailTitle;

	[SerializeField]
	private string fullGameJoinFailMessage;

	[SerializeField]
	private string genericJoinFailMessage;

	private bool initialized;

	private DisconnectCause? photonConnectionFailureDuringInitCause;

	private bool intentionallyDisconnecting;

	private bool roomJoinFailed;

	private short roomJoinErrorCode;

	private bool friendListUpdated;

	public bool IsInLockerRoom
	{
		get
		{
			return IsInActivity("lockerroom");
		}
	}

	public bool IsInDormRoom
	{
		get
		{
			return IsInActivity("dormroom");
		}
	}

	public int AvailableSpaceInRoom
	{
		get
		{
			return PhotonNetwork.inRoom ? ((int)PhotonNetwork.room.customProperties["C3"]) : 0;
		}
	}

	public bool IsActivityInviteOnly
	{
		get
		{
			return PhotonNetwork.inRoom && (int)PhotonNetwork.room.customProperties["C2"] == 1;
		}
	}

	public string RichJoinCommand
	{
		get
		{
			return "+join:" + PhotonNetwork.player.userId;
		}
	}

	public event Action<Player> OnRecRoomPlayerConnected;

	public event Action<PhotonPlayer> PhotonPlayerDisconnected;

	protected override void Awake()
	{
		base.Awake();
		Instance = this;
		GenerateActivityMap();
		Profiles.LocalProfileDownloaded += Profile_LocalProfileDownloaded;
	}

	private void GenerateActivityMap()
	{
		activityMap = new Dictionary<string, ActivityInfo>();
		ActivityInfo[] array = activities;
		foreach (ActivityInfo activityInfo in array)
		{
			activityMap[activityInfo.Name] = activityInfo;
		}
		activityGroupMap = new Dictionary<string, RuntimeActivityGroupInfo>();
		ActivityGroupInfo[] array2 = activityGroups;
		foreach (ActivityGroupInfo activityGroupInfo in array2)
		{
			activityGroupMap[activityGroupInfo.Name] = new RuntimeActivityGroupInfo
			{
				Name = activityGroupInfo.Name,
				FriendlyName = activityGroupInfo.FriendlyName,
				Activities = activityGroupInfo.Activities.Select((string a) => activityMap[a]).ToList()
			};
		}
	}

	private void Profile_LocalProfileDownloaded(Profile profile)
	{
		if (profile == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(profile.DisplayName))
		{
			if (PhotonNetwork.player != null && PhotonNetwork.player.name != profile.DisplayName)
			{
				PhotonNetwork.player.name = profile.DisplayName;
				AnalyticsHelper.UserName(profile.DisplayName);
			}
		}
		else
		{
			Debug.LogError("Punnetworkmanager Profile_LocalProfileDownloaded has an empty DisplayName.");
		}
	}

	public IEnumerator Initialize(InitializeCallback callback)
	{
		PhotonNetwork.sendRate = 30;
		PhotonNetwork.sendRateOnSerialize = 30;
		PhotonNetwork.BackgroundTimeout = 60f;
		PhotonNetwork.UseRpcMonoBehaviourCache = true;
		PhotonNetwork.automaticallySyncScene = true;
		PhotonNetwork.AuthValues = new AuthenticationValues(Profiles.LocalProfile.Id.ToString());
		PhotonNetwork.lobby = new TypedLobby("lobby", LobbyType.SqlLobby);
		PhotonNetwork.autoJoinLobby = false;
		if (Microphone.devices.Length > 0)
		{
			string text = Microphone.devices.FirstOrDefault((string d) => d.Contains("Rift"));
			if (PlatformManager.Instance.CurrentHardwareType == PlatformManager.HardwareType.OCULUS && text != null)
			{
				PhotonVoiceNetwork.MicrophoneDevice = text;
			}
			else
			{
				PhotonVoiceNetwork.MicrophoneDevice = Microphone.devices[0];
			}
		}
		PhotonNetwork.ConnectUsingSettings(BuildSettings.Version);
		yield return new WaitUntil(delegate
		{
			int result;
			if (!PhotonNetwork.connectedAndReady)
			{
				DisconnectCause? disconnectCause2 = photonConnectionFailureDuringInitCause;
				result = (disconnectCause2.HasValue ? 1 : 0);
			}
			else
			{
				result = 1;
			}
			return (byte)result != 0;
		});
		DisconnectCause? disconnectCause = photonConnectionFailureDuringInitCause;
		if (disconnectCause.HasValue)
		{
			callback(string.Format("Unable to connect to Rec Room game servers (Error: {0})", photonConnectionFailureDuringInitCause.Value));
			yield break;
		}
		InvokeRepeating("UpdatePresence", 60f, 60f);
		initialized = true;
		callback(null);
	}

	private RandomRoomMatchMode GetRandomRoomMatchMode()
	{
		MatchmakingConfigParams matchmakingParams = Config.MatchmakingParams;
		float max = Mathf.Max(1f, matchmakingParams.PreferEmptyRoomsFrequency + matchmakingParams.PreferFullRoomsFrequency);
		float num = UnityEngine.Random.Range(0f, max);
		if (num <= matchmakingParams.PreferFullRoomsFrequency)
		{
			return RandomRoomMatchMode.PreferFullRooms;
		}
		if (num - matchmakingParams.PreferFullRoomsFrequency <= matchmakingParams.PreferEmptyRoomsFrequency)
		{
			return RandomRoomMatchMode.PreferEmptyRooms;
		}
		return RandomRoomMatchMode.AnyRoom;
	}

	public void SetGameInProgress(bool gameInProgress)
	{
		if (PhotonNetwork.isMasterClient && PhotonNetwork.inRoom)
		{
			PhotonNetwork.room.SetCustomProperties("C4", gameInProgress ? 1 : 0);
		}
	}

	private bool IsInActivity(string activity)
	{
		return SceneManagerHelper.ActiveSceneName == activity;
	}

	private bool CanJoinInProgress(string activityOrGroup)
	{
		ActivityInfo value;
		if (activityMap.TryGetValue(activityOrGroup, out value))
		{
			return value.CanJoinInProgress;
		}
		RuntimeActivityGroupInfo value2;
		if (activityGroupMap.TryGetValue(activityOrGroup, out value2))
		{
			return value2.Activities.Any((ActivityInfo a) => a.CanJoinInProgress);
		}
		return true;
	}

	private int GetMaxPlayers(string activityOrGroup, int roomSize)
	{
		ActivityInfo value;
		RuntimeActivityGroupInfo value2;
		int b = (activityMap.TryGetValue(activityOrGroup, out value) ? value.MaxPlayers : ((!activityGroupMap.TryGetValue(activityOrGroup, out value2)) ? 8 : value2.Activities.Max((ActivityInfo a) => a.MaxPlayers)));
		return Mathf.Min(roomSize, b);
	}

	public string GetActivityFriendlyName(string activityOrGroup)
	{
		ActivityInfo value;
		if (activityMap.TryGetValue(activityOrGroup, out value))
		{
			return value.FriendlyName;
		}
		RuntimeActivityGroupInfo value2;
		if (activityGroupMap.TryGetValue(activityOrGroup, out value2))
		{
			return value2.FriendlyName;
		}
		return null;
	}

	public bool IsOnline(PlayerPresence playerPresence)
	{
		return playerPresence != null && !string.IsNullOrEmpty(playerPresence.GameSessionId) && playerPresence.AppVersion == PhotonNetwork.gameVersion;
	}

	public string GetPlayerStatusString(PlayerPresence playerPresence)
	{
		if (!IsOnline(playerPresence))
		{
			return "Offline";
		}
		string text = null;
		text = ((playerPresence.Activity == "dormroom") ? "Hanging out in their Dorm Room" : ((!(playerPresence.Activity == "lockerroom")) ? ("Playing " + GetActivityFriendlyName(playerPresence.Activity)) : "Hanging out in the Locker Room"));
		if (playerPresence.GameSessionId == PhotonNetwork.room.name)
		{
			text += " with you";
		}
		if (playerPresence.Private)
		{
			text += " [INVITE ONLY]";
		}
		return text;
	}

	private void OnConnectionFail(DisconnectCause cause)
	{
		Debug.LogError("Connection failed: " + cause);
		HandleConnectionFailure(true, cause);
	}

	private void OnFailedToConnectToPhoton(DisconnectCause cause)
	{
		Debug.LogError("Failed to connect: " + cause);
		HandleConnectionFailure(false, cause);
	}

	private void OnApplicationQuit()
	{
		intentionallyDisconnecting = true;
	}

	private void OnDisconnectedFromPhoton()
	{
		if (intentionallyDisconnecting)
		{
			intentionallyDisconnecting = false;
			return;
		}
		SingletonMonoBehaviour<SessionManager>.Instance.AlertTitle = disconnectAlertTitle;
		SingletonMonoBehaviour<SessionManager>.Instance.AlertMessage = string.Format(genericDisconnectMessage, -1);
		Debug.LogError("Unexpected Photon disconnect");
		SwitchActivity("dormroom", null, false, 0, true);
	}

	private void HandleConnectionFailure(bool wasConnected, DisconnectCause cause)
	{
		if (!initialized)
		{
			photonConnectionFailureDuringInitCause = cause;
			return;
		}
		string format = ((cause != DisconnectCause.DisconnectByClientTimeout && cause != DisconnectCause.DisconnectByServerTimeout && cause != DisconnectCause.ExceptionOnConnect) ? genericDisconnectMessage : ((!wasConnected) ? timeoutCouldntConnectMessage : timeoutDisconnectMessage));
		SingletonMonoBehaviour<SessionManager>.Instance.AlertTitle = disconnectAlertTitle;
		SingletonMonoBehaviour<SessionManager>.Instance.AlertMessage = string.Format(format, (int)cause);
		intentionallyDisconnecting = true;
		PhotonNetwork.Disconnect();
		SwitchActivity("dormroom", null, false, 0, true);
	}

	public void SwitchActivity(string activityOrGroup, string[] expectedUsers = null, bool createPrivateRoom = false, int roomSize = 0, bool offlineMode = false)
	{
		StopAllCoroutines();
		StartCoroutine(SwitchActivityCoroutine(activityOrGroup, expectedUsers, createPrivateRoom, roomSize, offlineMode));
	}

	public void JoinRoom(string roomName, string[] expectedUsers = null, string fallbackRoomName = null)
	{
		StopAllCoroutines();
		StartCoroutine(JoinRoomCoroutine(roomName, expectedUsers, fallbackRoomName));
	}

	public void JoinPlayer(ulong playerId, string[] expectedUsers = null)
	{
		StopAllCoroutines();
		StartCoroutine(JoinPlayerCoroutine(playerId, expectedUsers));
	}

	private void OnPhotonRandomJoinFailed(object[] codeAndMsg)
	{
		roomJoinErrorCode = (short)codeAndMsg[0];
		roomJoinFailed = true;
	}

	private void OnPhotonJoinRoomFailed(object[] codeAndMsg)
	{
		roomJoinErrorCode = (short)codeAndMsg[0];
		roomJoinFailed = true;
	}

	private void OnPhotonCreateRoomFailed(object[] codeAndMsg)
	{
		roomJoinErrorCode = (short)codeAndMsg[0];
		roomJoinFailed = true;
	}

	private void OnUpdatedFriendList()
	{
		friendListUpdated = true;
	}

	private IEnumerator SwitchActivityCoroutine(string activityOrGroup, string[] expectedUsers = null, bool createPrivateRoom = false, int roomSize = 0, bool offlineMode = false)
	{
		if (activityOrGroup == "dormroom")
		{
			createPrivateRoom = true;
			roomSize = 1;
		}
		if (roomSize <= 0)
		{
			roomSize = 8;
		}
		string previousRoomName = ((PhotonNetwork.room == null) ? null : PhotonNetwork.room.name);
		yield return LeaveRoomCoroutine();
		yield return InitializeNewConnection(offlineMode);
		if (!createPrivateRoom && !offlineMode)
		{
			yield return JoinRandomRoomCoroutine(activityOrGroup, expectedUsers, previousRoomName, roomSize);
		}
		if (PhotonNetwork.connectionStateDetailed != ClientState.Joined)
		{
			yield return CreateRoomCoroutine(activityOrGroup, expectedUsers, createPrivateRoom, roomSize);
		}
		if (PhotonNetwork.connectionStateDetailed != ClientState.Joined)
		{
			Debug.LogError("Unable to join or create a room! Falling back to dormroom.");
			SwitchActivity("dormroom");
		}
		else
		{
			SendPartyActivitySwitchMessages(expectedUsers);
		}
	}

	private IEnumerator InitializeNewConnection(bool offlineMode)
	{
		if (offlineMode != PhotonNetwork.offlineMode)
		{
			if (!PhotonNetwork.offlineMode && PhotonNetwork.connected)
			{
				intentionallyDisconnecting = true;
				PhotonNetwork.Disconnect();
				while (PhotonNetwork.connectedAndReady)
				{
					yield return null;
				}
			}
			PhotonNetwork.offlineMode = offlineMode;
		}
		if (!offlineMode && !PhotonNetwork.connected)
		{
			PhotonNetwork.ConnectUsingSettings(BuildSettings.Version);
			while (!PhotonNetwork.connectedAndReady)
			{
				yield return null;
			}
		}
	}

	private IEnumerator LeaveRoomCoroutine()
	{
		if (PhotonNetwork.connectionStateDetailed == ClientState.Uninitialized || PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated)
		{
			yield break;
		}
		if (PhotonNetwork.offlineMode)
		{
			PhotonNetwork.DestroyAll();
		}
		if (PhotonNetwork.room != null)
		{
			PhotonNetwork.LeaveRoom();
			yield return new WaitUntil(() => PhotonNetwork.connectionStateDetailed == ClientState.ConnectedToMaster || PhotonNetwork.connectionStateDetailed == ClientState.Disconnected || PhotonNetwork.connectionStateDetailed == ClientState.PeerCreated);
		}
		PhotonVoiceNetwork.Disconnect();
		SceneManager.LoadScene("empty");
	}

	private void SendPartyActivitySwitchMessages(string[] expectedUsers)
	{
		if (expectedUsers == null)
		{
			return;
		}
		foreach (string s in expectedUsers)
		{
			ulong result;
			if (ulong.TryParse(s, out result))
			{
				Messages.SendPartyActivitySwitch(result);
			}
		}
	}

	private IEnumerator JoinRandomRoomCoroutine(string activityOrGroup, string[] expectedUsers, string excludedRoomName, int roomSize)
	{
		int minAvailableSpace = 1 + ((expectedUsers != null) ? expectedUsers.Length : 0);
		int maxAvailableSpace = GetMaxPlayers(activityOrGroup, roomSize) - 1;
		switch (GetRandomRoomMatchMode())
		{
		case RandomRoomMatchMode.PreferFullRooms:
		{
			int limit2 = Mathf.Min(minAvailableSpace + 3, maxAvailableSpace);
			for (int i = minAvailableSpace; i < limit2; i++)
			{
				if (PhotonNetwork.connectionStateDetailed == ClientState.Joined)
				{
					break;
				}
				yield return JoinRandomRoomCoroutine(activityOrGroup, expectedUsers, excludedRoomName, minAvailableSpace, i);
			}
			break;
		}
		case RandomRoomMatchMode.PreferEmptyRooms:
		{
			int limit = Mathf.Max(maxAvailableSpace - 3, minAvailableSpace);
			int step = maxAvailableSpace;
			while (step > limit && PhotonNetwork.connectionStateDetailed != ClientState.Joined)
			{
				yield return JoinRandomRoomCoroutine(activityOrGroup, expectedUsers, excludedRoomName, step, maxAvailableSpace);
				step--;
			}
			break;
		}
		}
		if (PhotonNetwork.connectionStateDetailed != ClientState.Joined)
		{
			yield return JoinRandomRoomCoroutine(activityOrGroup, expectedUsers, excludedRoomName, minAvailableSpace, maxAvailableSpace);
		}
	}

	private IEnumerator JoinRandomRoomCoroutine(string activityOrGroup, string[] expectedUsers, string excludedRoomName, int minAvailableSpace, int maxAvailableSpace)
	{
		yield return JoinRandomRoomCoroutine(activityOrGroup, expectedUsers, excludedRoomName, minAvailableSpace, maxAvailableSpace, true);
		if (PhotonNetwork.connectionStateDetailed != ClientState.Joined && CanJoinInProgress(activityOrGroup))
		{
			yield return JoinRandomRoomCoroutine(activityOrGroup, expectedUsers, excludedRoomName, minAvailableSpace, maxAvailableSpace, false);
		}
	}

	private IEnumerator JoinRandomRoomCoroutine(string activityOrGroup, string[] expectedUsers, string excludedRoomName, int minAvailableSpace, int maxAvailableSpace, bool excludeInProgress)
	{
		yield return new WaitUntil(() => PhotonNetwork.connectionStateDetailed == ClientState.ConnectedToMaster || PhotonNetwork.connectionStateDetailed == ClientState.JoinedLobby);
		string sqlLobbyFilter = string.Format("{0} = {1} AND {2} >= {3} AND {2} <= {4}", "C2", 0, "C3", minAvailableSpace, maxAvailableSpace);
		RuntimeActivityGroupInfo groupInfo;
		if (activityGroupMap.TryGetValue(activityOrGroup, out groupInfo))
		{
			string arg = (from a in groupInfo.Activities
				where a.CanJoinInProgress || excludeInProgress
				select string.Format("\"{0}\"", a.Name)).Aggregate((string a, string b) => string.Format("{0},{1}", a, b));
			sqlLobbyFilter += string.Format(" AND {0} IN ({1})", "C1", arg);
		}
		else
		{
			sqlLobbyFilter += string.Format(" AND {0} = \"{1}\"", "C1", activityOrGroup);
		}
		if (!string.IsNullOrEmpty(excludedRoomName))
		{
			sqlLobbyFilter += string.Format(" AND {0} <> \"{1}\"", "C5", excludedRoomName);
		}
		if (!string.IsNullOrEmpty(SingletonMonoBehaviour<SessionManager>.Instance.BlockedRoomName))
		{
			sqlLobbyFilter += string.Format(" AND {0} <> \"{1}\"", "C5", SingletonMonoBehaviour<SessionManager>.Instance.BlockedRoomName);
		}
		if (excludeInProgress)
		{
			sqlLobbyFilter += string.Format(" AND {0} = {1}", "C4", 0);
		}
		Debug.Log("Searching for match: " + sqlLobbyFilter);
		roomJoinFailed = false;
		if (PhotonNetwork.JoinRandomRoom(null, 0, MatchmakingMode.RandomMatching, PhotonNetwork.lobby, sqlLobbyFilter, expectedUsers))
		{
			yield return new WaitUntil(() => PhotonNetwork.connectionStateDetailed == ClientState.Joined || roomJoinFailed);
		}
	}

	private IEnumerator CreateRoomCoroutine(string activityOrGroup, string[] expectedUsers, bool createPrivateRoom, int roomSize)
	{
		yield return new WaitUntil(() => PhotonNetwork.connectionStateDetailed == ClientState.ConnectedToMaster || PhotonNetwork.connectionStateDetailed == ClientState.JoinedLobby);
		string roomName = Guid.NewGuid().ToString();
		int playerCount = 1 + ((expectedUsers != null) ? expectedUsers.Length : 0);
		RuntimeActivityGroupInfo groupInfo;
		if (activityGroupMap.TryGetValue(activityOrGroup, out groupInfo))
		{
			int index = UnityEngine.Random.Range(0, groupInfo.Activities.Count);
			activityOrGroup = groupInfo.Activities[index].Name;
		}
		ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable
		{
			{ "C1", activityOrGroup },
			{
				"C2",
				createPrivateRoom ? 1 : 0
			},
			{
				"C3",
				GetMaxPlayers(activityOrGroup, roomSize) - playerCount
			},
			{ "C4", 0 },
			{ "C5", roomName },
			{
				"C6",
				PhotonNetwork.player.userId
			},
			{ "BUG", 1 }
		};
		string[] roomPropertyNames = new string[6] { "C1", "C2", "C3", "C4", "C5", "C6" };
		RoomOptions roomOptions = new RoomOptions
		{
			CustomRoomProperties = roomProperties,
			CustomRoomPropertiesForLobby = roomPropertyNames,
			MaxPlayers = (byte)roomSize,
			PublishUserId = true,
			PlayerTtl = 1000
		};
		roomJoinFailed = false;
		if (PhotonNetwork.CreateRoom(roomName, roomOptions, PhotonNetwork.lobby, expectedUsers))
		{
			yield return new WaitUntil(() => PhotonNetwork.connectionStateDetailed == ClientState.Joined || roomJoinFailed);
		}
	}

	private IEnumerator JoinRoomCoroutine(string roomName, string[] expectedUsers, string fallbackRoomName)
	{
		yield return LeaveRoomCoroutine();
		yield return InitializeNewConnection(false);
		roomJoinFailed = false;
		PhotonNetwork.JoinRoom(roomName, expectedUsers);
		yield return new WaitUntil(() => PhotonNetwork.connectionStateDetailed == ClientState.Joined || roomJoinFailed);
		if (PhotonNetwork.connectionStateDetailed != ClientState.Joined)
		{
			Debug.LogErrorFormat("Failed to join Photon room (error {0})", roomJoinErrorCode);
			AnalyticsHelper.NetworkRoomJoin(roomJoinErrorCode);
			if (fallbackRoomName != null)
			{
				JoinRoom(fallbackRoomName);
			}
			else
			{
				SwitchActivity("dormroom");
			}
		}
		else
		{
			AnalyticsHelper.NetworkRoomJoin(0);
			SendPartyActivitySwitchMessages(expectedUsers);
		}
	}

	private IEnumerator JoinPlayerCoroutine(ulong playerId, string[] expectedUsers)
	{
		string previousRoomName = ((PhotonNetwork.room == null) ? null : PhotonNetwork.room.name);
		yield return LeaveRoomCoroutine();
		yield return InitializeNewConnection(false);
		float startTime = Time.realtimeSinceStartup;
		roomJoinFailed = false;
		while (Time.realtimeSinceStartup - startTime < 20f)
		{
			friendListUpdated = false;
			PhotonNetwork.FindFriends(new string[1] { playerId.ToString() });
			yield return new WaitUntil(() => friendListUpdated);
			if (PhotonNetwork.Friends[0].IsInRoom)
			{
				roomJoinFailed = false;
				PhotonNetwork.JoinRoom(PhotonNetwork.Friends[0].Room, expectedUsers);
				yield return new WaitUntil(() => PhotonNetwork.connectionStateDetailed == ClientState.Joined || roomJoinFailed);
				break;
			}
		}
		if (PhotonNetwork.connectionStateDetailed == ClientState.Joined)
		{
			AnalyticsHelper.NetworkRoomJoin(0);
			SendPartyActivitySwitchMessages(expectedUsers);
			yield break;
		}
		int additionalPartySize = ((expectedUsers != null) ? expectedUsers.Length : 0);
		Messages.SendGameJoinFailed(playerId, additionalPartySize);
		int num = ((!roomJoinFailed) ? (-1) : roomJoinErrorCode);
		AnalyticsHelper.NetworkRoomJoin(num);
		Debug.LogErrorFormat("Failed to join player (error {0})", num);
		string format = ((num != 32765) ? genericJoinFailMessage : fullGameJoinFailMessage);
		Profile profileFromCache = Profiles.GetProfileFromCache(playerId);
		string arg = ((profileFromCache == null) ? "player" : profileFromCache.DisplayName);
		SingletonMonoBehaviour<SessionManager>.Instance.AlertTitle = joinTailTitle;
		SingletonMonoBehaviour<SessionManager>.Instance.AlertMessage = string.Format(format, arg, num);
		if (previousRoomName != null)
		{
			JoinRoom(previousRoomName);
		}
		else
		{
			SwitchActivity("dormroom");
		}
	}

	private void OnCreatedRoom()
	{
		string levelName = (string)PhotonNetwork.room.customProperties["C1"];
		PhotonNetwork.LoadLevel(levelName);
	}

	private void OnJoinedRoom()
	{
		if (!PhotonNetwork.offlineMode)
		{
			PhotonVoiceNetwork.Connect();
		}
		DelayedClearExpectedUsers();
		UpdateRichPresenceJoin();
		UpdatePresence();
		StartCoroutine(RunWaitForRecroomPlayerCreated(PhotonNetwork.player));
		StartCoroutine(ZombieRoomDetector());
	}

	private IEnumerator ZombieRoomDetector()
	{
		yield return new WaitForSeconds(10f);
		if (PhotonNetwork.player.ToPlayer() == null)
		{
			Debug.LogError("Zombie room encountered - assuming authority");
			AnalyticsHelper.NetworkZombieDetection(true);
			PhotonNetwork.SetMasterClient(PhotonNetwork.player);
		}
		else
		{
			AnalyticsHelper.NetworkZombieDetection(false);
		}
	}

	private void OnLeftRoom()
	{
		UpdateRichPresenceJoin();
		UpdatePresence();
	}

	private void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		UpdateRoomPlayerCount();
		DelayedClearExpectedUsers();
		StartCoroutine(RunWaitForRecroomPlayerCreated(newPlayer));
	}

	private IEnumerator RunWaitForRecroomPlayerCreated(PhotonPlayer newPlayer)
	{
		float timer = 30f;
		while (timer >= 0f)
		{
			timer -= Time.deltaTime;
			yield return null;
			Player player = newPlayer.ToPlayer();
			if (player != null && player.IsInitialized)
			{
				RecRoomPlayerCreated(player);
				break;
			}
		}
	}

	private void RecRoomPlayerCreated(Player player)
	{
		UpdateRoomPlayerIDs();
		if (this.OnRecRoomPlayerConnected != null)
		{
			this.OnRecRoomPlayerConnected(player);
		}
	}

	private void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		if (PhotonNetwork.isMasterClient)
		{
			UpdateRoomPlayerCount();
			UpdateRoomPlayerIDs();
			SynchronizedField.ClearAllFieldsForPlayer(player);
		}
		else if (PhotonNetwork.masterClient == null)
		{
			Debug.LogError("NetworkManager OnPlayerDisconnected with current master not set.");
		}
		if (this.PhotonPlayerDisconnected != null)
		{
			this.PhotonPlayerDisconnected(player);
		}
	}

	private void UpdateRoomPlayerIDs()
	{
		if (PhotonNetwork.isMasterClient && PhotonNetwork.inRoom)
		{
			string text = string.Empty;
			PhotonPlayer[] playerList = PhotonNetwork.playerList;
			foreach (PhotonPlayer photonPlayer in playerList)
			{
				text = text + photonPlayer.userId + ",";
			}
			PhotonNetwork.room.SetCustomProperties("C6", text);
		}
	}

	private void UpdatePresence()
	{
		PlayerPresence playerPresence = new PlayerPresence();
		playerPresence.PlayerId = Profiles.LocalProfile.Id;
		playerPresence.AppVersion = PhotonNetwork.gameVersion;
		PlayerPresence playerPresence2 = playerPresence;
		if (!PhotonNetwork.offlineMode && PhotonNetwork.inRoom)
		{
			playerPresence2.GameSessionId = PhotonNetwork.room.name;
			playerPresence2.Activity = (string)PhotonNetwork.room.customProperties["C1"];
			playerPresence2.Private = (int)PhotonNetwork.room.customProperties["C2"] == 1;
			playerPresence2.AvailableSpace = (int)PhotonNetwork.room.customProperties["C3"];
			playerPresence2.GameInProgress = (int)PhotonNetwork.room.customProperties["C4"] == 1;
		}
		PlayerPresenceManager.UpdatePlayerPresence(playerPresence2);
	}

	private void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
	{
		if (propertiesThatChanged.ContainsKey((byte)247))
		{
			UpdateRoomPlayerCount();
		}
		if (propertiesThatChanged.ContainsKey("C3"))
		{
			UpdateRichPresenceJoin();
		}
		if (PhotonNetwork.isMasterClient && (propertiesThatChanged.ContainsKey("C3") || propertiesThatChanged.ContainsKey("C4")))
		{
			UpdatePresence();
		}
	}

	private void OnMasterClientSwitched(PhotonPlayer newMasterClient)
	{
		if (RecRoomSceneManager.IsInitialized)
		{
			AnalyticsHelper.NetworkMasterClientChanged(RecRoomSceneManager.CurrentSceneName, PhotonNetwork.playerList.Length);
		}
		if (newMasterClient.isLocal)
		{
			UpdateRoomPlayerCount();
			UpdateRoomPlayerIDs();
			SynchronizedField.ClearAllFieldsForMissingPlayers();
		}
	}

	private void DelayedClearExpectedUsers()
	{
		CancelInvoke("ClearExpectedUsers");
		Invoke("ClearExpectedUsers", 20f);
	}

	private void ClearExpectedUsers()
	{
		if (PhotonNetwork.isMasterClient && PhotonNetwork.connectedAndReady && PhotonNetwork.room != null && !PhotonNetwork.offlineMode)
		{
			PhotonNetwork.room.ClearExpectedUsers();
		}
	}

	private void UpdateRoomPlayerCount()
	{
		if (PhotonNetwork.isMasterClient && PhotonNetwork.inRoom)
		{
			string activityOrGroup = (string)PhotonNetwork.room.customProperties["C1"];
			int maxPlayers = PhotonNetwork.room.maxPlayers;
			int maxPlayers2 = GetMaxPlayers(activityOrGroup, maxPlayers);
			int roomPlayerCount = GetRoomPlayerCount();
			PhotonNetwork.room.SetCustomProperties("C3", maxPlayers2 - roomPlayerCount);
		}
	}

	public int GetRoomPlayerCount()
	{
		int num = PhotonNetwork.playerList.Length;
		if (PhotonNetwork.room.expectedUsers != null)
		{
			string[] second = PhotonNetwork.playerList.Select((PhotonPlayer p) => p.userId).ToArray();
			num += PhotonNetwork.room.expectedUsers.Except(second).Count();
		}
		return num;
	}

	public bool CanInviteToCurrentGame()
	{
		return !PhotonNetwork.offlineMode && PhotonNetwork.inRoom && GetRoomPlayerCount() < PhotonNetwork.room.maxPlayers;
	}

	public bool IsJoinable()
	{
		return CanInviteToCurrentGame() && !IsActivityInviteOnly;
	}

	private void UpdateRichPresenceJoin()
	{
		if (PlatformManager.Instance.CurrentPlatform == PlatformManager.PlatformType.STEAM)
		{
			SteamFriends.SetRichPresence("connect", (!IsJoinable()) ? null : RichJoinCommand);
		}
	}

	public void ProcessRichJoinCommand(string connectCommand)
	{
		if (RecRoomSceneManager.Instance != null && !string.IsNullOrEmpty(connectCommand) && connectCommand.StartsWith("+join:"))
		{
			string s = connectCommand.Substring("+join:".Length);
			ulong result;
			if (ulong.TryParse(s, out result))
			{
				RecRoomSceneManager.Instance.JoinPlayer(result);
			}
		}
	}

	public ulong? CheckCommandLineArgsForRichPresenceJoin()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			ulong result;
			if (commandLineArgs[i].StartsWith("+join:") && ulong.TryParse(commandLineArgs[i].Substring("+join:".Length), out result))
			{
				return result;
			}
		}
		return null;
	}

	public PhotonPlayer DebugChangeMasterClient()
	{
		int num = 0;
		for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
		{
			if (PhotonNetwork.playerList[i].isMasterClient)
			{
				num = i;
				break;
			}
		}
		num = (num + 1) % PhotonNetwork.playerList.Length;
		PhotonNetwork.SetMasterClient(PhotonNetwork.playerList[num]);
		return PhotonNetwork.playerList[num];
	}
}
