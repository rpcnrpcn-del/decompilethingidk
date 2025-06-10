using ExitGames.Client.Photon.LoadBalancing;
using ExitGames.Client.Photon.Voice;
using UnityEngine;

public class PhotonVoiceNetwork : MonoBehaviour
{
	private static PhotonVoiceNetwork _instance;

	private static GameObject _singleton;

	private static object instanceLock = new object();

	private static bool destroyed = false;

	internal UnityVoiceFrontend client;

	private static string microphoneDevice = null;

	internal static PhotonVoiceNetwork instance
	{
		get
		{
			return getInstance();
		}
	}

	public static UnityVoiceFrontend Client
	{
		get
		{
			return instance.client;
		}
	}

	public static ExitGames.Client.Photon.LoadBalancing.ClientState ClientState
	{
		get
		{
			return instance.client.State;
		}
	}

	public static string CurrentRoomName
	{
		get
		{
			return (instance.client.CurrentRoom != null) ? instance.client.CurrentRoom.Name : string.Empty;
		}
	}

	public static string MicrophoneDevice
	{
		get
		{
			return microphoneDevice;
		}
		set
		{
			if (value != null && !Microphone.devices.Contains(value))
			{
				Debug.LogError("PUNVoice: " + value + " is not a valid microphone device");
				return;
			}
			microphoneDevice = value;
			if (PhotonVoiceSettings.Instance.DebugInfo)
			{
				Debug.LogFormat("PUNVoice: Setting global microphone device to {0}", microphoneDevice);
			}
			PhotonVoiceRecorder[] array = Object.FindObjectsOfType<PhotonVoiceRecorder>();
			foreach (PhotonVoiceRecorder photonVoiceRecorder in array)
			{
				if (photonVoiceRecorder.photonView.isMine && photonVoiceRecorder.MicrophoneDevice == null)
				{
					photonVoiceRecorder.MicrophoneDevice = null;
				}
			}
		}
	}

	private PhotonVoiceNetwork()
	{
		client = new UnityVoiceFrontend(this);
	}

	internal static PhotonVoiceNetwork getInstance()
	{
		lock (instanceLock)
		{
			if (destroyed)
			{
				return null;
			}
			if (_instance == null)
			{
				_singleton = new GameObject();
				_instance = _singleton.AddComponent<PhotonVoiceNetwork>();
				_singleton.name = "PhotonVoiceNetworkSingleton";
				Object.DontDestroyOnLoad(_singleton);
			}
			return _instance;
		}
	}

	private void OnDestroy()
	{
		if (!(this != _instance))
		{
			destroyed = true;
		}
	}

	[RuntimeInitializeOnLoadMethod]
	public static void RuntimeInitializeOnLoad()
	{
		getInstance();
	}

	public void Awake()
	{
		if (Microphone.devices.Length < 1)
		{
			Debug.LogError("PUNVoice: No microphone device found");
		}
	}

	public static bool Connect()
	{
		instance.client.AppId = PhotonNetwork.PhotonServerSettings.VoiceAppID;
		instance.client.AppVersion = "1.0";
		if (PhotonNetwork.PhotonServerSettings.HostType == ServerSettings.HostingOption.SelfHosted)
		{
			string masterServerAddress = string.Format("{0}:{1}", PhotonNetwork.PhotonServerSettings.ServerAddress, PhotonNetwork.PhotonServerSettings.VoiceServerPort);
			return instance.client.Connect(masterServerAddress, null, null, null, null);
		}
		return instance.client.ConnectToRegionMaster(PhotonNetwork.networkingPeer.CloudRegion.ToString());
	}

	public static void Disconnect()
	{
		instance.client.Disconnect();
	}

	protected void OnEnable()
	{
		if (!(this != _instance))
		{
			Application.RequestUserAuthorization(UserAuthorization.Microphone);
		}
	}

	protected void OnApplicationQuit()
	{
		if (!(this != _instance))
		{
			client.Disconnect();
		}
	}

	protected void Update()
	{
		if (!(this != _instance))
		{
			client.DebugLostPercent = PhotonVoiceSettings.Instance.DebugLostPercent;
			client.Service();
		}
	}

	public static LocalVoice CreateLocalVoice(IAudioStream audioClip, VoiceInfo voiceInfo)
	{
		return instance.client.CreateLocalVoice(audioClip, voiceInfo);
	}

	public static void RemoveLocalVoice(LocalVoice voice)
	{
		if (!destroyed)
		{
			instance.client.RemoveLocalVoice(voice);
		}
	}

	private void OnJoinedRoom()
	{
		if (this != _instance)
		{
			return;
		}
		ExitGames.Client.Photon.LoadBalancing.ClientState state = client.State;
		if (state == ExitGames.Client.Photon.LoadBalancing.ClientState.Joined)
		{
			if (PhotonVoiceSettings.Instance.AutoConnect)
			{
				client.OpLeaveRoom();
			}
		}
		else if (PhotonVoiceSettings.Instance.AutoConnect)
		{
			client.Reconnect();
		}
	}

	private void OnLeftRoom()
	{
		if (!(this != _instance) && PhotonVoiceSettings.Instance.AutoDisconnect)
		{
			client.Disconnect();
		}
	}

	private void OnDisconnectedFromPhoton()
	{
		if (!(this != _instance) && PhotonVoiceSettings.Instance.AutoDisconnect)
		{
			client.Disconnect();
		}
	}

	internal static void LinkSpeakerToRemoteVoice(PhotonVoiceSpeaker speaker)
	{
		instance.client.LinkSpeakerToRemoteVoice(speaker);
	}

	internal static void UnlinkSpeakerFromRemoteVoice(PhotonVoiceSpeaker speaker)
	{
		if (!destroyed)
		{
			instance.client.UnlinkSpeakerFromRemoteVoice(speaker);
		}
	}
}
