using System.Collections;
using System.Collections.Generic;
using Photon;
using UnityEngine;

[DisallowMultipleComponent]
public class RecRoomSceneManager : Photon.MonoBehaviour
{
	[SerializeField]
	private string friendlyName;

	[Header("Scene Settings")]
	[SerializeField]
	private SceneAFKSettings afkSettings;

	[SerializeField]
	private SceneFxManager audioSettings;

	[SerializeField]
	private SceneInteractionSettings interactionSettings;

	[SerializeField]
	private ScenePhysicsSettings physicsSettings;

	[SerializeField]
	private ScenePostEffectSettings postEffectSettings;

	[SerializeField]
	private SceneTeleportSettings sceneTeleportSettings;

	[SerializeField]
	private SceneSpawnManager spawnManager;

	[SerializeField]
	private ScenePresenceSettings presenceSettings;

	private List<IRecRoomSceneComponent> recRoomSceneComponents = new List<IRecRoomSceneComponent>();

	protected bool wasMasterClient;

	private bool isApplicationQuitting;

	private Coroutine changeActivityCoroutine;

	public GameManager GameManager { get; private set; }

	public SceneAFKSettings AFKSettings
	{
		get
		{
			return afkSettings;
		}
	}

	public SceneFxManager FxManager
	{
		get
		{
			return audioSettings;
		}
	}

	public SceneInteractionSettings InteractionSettings
	{
		get
		{
			return interactionSettings;
		}
	}

	public ScenePhysicsSettings PhysicsSettings
	{
		get
		{
			return physicsSettings;
		}
	}

	public ScenePostEffectSettings PostEffectSettings
	{
		get
		{
			return postEffectSettings;
		}
	}

	public SceneTeleportSettings SceneTeleportSettings
	{
		get
		{
			return sceneTeleportSettings;
		}
	}

	public SceneSpawnManager SpawnManager
	{
		get
		{
			return spawnManager;
		}
	}

	public ScenePresenceSettings PresenceSettings
	{
		get
		{
			return presenceSettings;
		}
	}

	public static string CurrentSceneFriendlyName
	{
		get
		{
			return (!(Instance != null) || string.IsNullOrEmpty(Instance.friendlyName)) ? CurrentSceneName : Instance.friendlyName;
		}
	}

	public static string CurrentSceneName
	{
		get
		{
			return SceneManagerHelper.ActiveSceneName;
		}
	}

	public static bool IsCurrentSceneAValidActivity
	{
		get
		{
			return Instance != null;
		}
	}

	public static RecRoomSceneManager Instance { get; private set; }

	public static bool IsInitialized
	{
		get
		{
			return Instance != null;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (Instance == null)
		{
			Instance = this;
		}
		GameManager = Object.FindObjectOfType<GameManager>();
		recRoomSceneComponents.Add(AFKSettings);
		recRoomSceneComponents.Add(FxManager);
		recRoomSceneComponents.Add(InteractionSettings);
		recRoomSceneComponents.Add(PhysicsSettings);
		recRoomSceneComponents.Add(PostEffectSettings);
		recRoomSceneComponents.Add(SceneTeleportSettings);
		recRoomSceneComponents.Add(PresenceSettings);
		recRoomSceneComponents.Add(SpawnManager);
	}

	protected virtual void Start()
	{
		for (int i = 0; i < recRoomSceneComponents.Count; i++)
		{
			recRoomSceneComponents[i].OnStart(this);
		}
		if (PhotonNetwork.isMasterClient)
		{
			wasMasterClient = true;
			SpawnManager.MasterSpawnNewPlayer(PhotonNetwork.player);
		}
	}

	private void OnApplicationQuit()
	{
		isApplicationQuitting = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		for (int i = 0; i < recRoomSceneComponents.Count; i++)
		{
			recRoomSceneComponents[i].OnDestroy();
		}
		if (!isApplicationQuitting)
		{
			if (SingletonMonoBehaviour<TutorialManager>.Instance != null && SingletonMonoBehaviour<TutorialManager>.Instance.IsOOBERunning)
			{
				PhotonNetwork.player.SetCustomProperties("prev_activity", "oobe");
			}
			else
			{
				PhotonNetwork.player.SetCustomProperties("prev_activity", CurrentSceneName);
			}
		}
		StopAllCoroutines();
		if (Instance == this)
		{
			Instance = null;
		}
		if (Player.LocalPlayer != null)
		{
			Player.LocalPlayer.OnRecRoomSceneManagerDestroyed();
		}
	}

	protected virtual void Update()
	{
		for (int i = 0; i < recRoomSceneComponents.Count; i++)
		{
			recRoomSceneComponents[i].Update();
		}
	}

	public void SwitchActivity(string activity, bool createPrivateRoom)
	{
		changeActivityCoroutine = StartCoroutine(SwitchActivityCoroutine(activity, createPrivateRoom));
	}

	private IEnumerator SwitchActivityCoroutine(string activity, bool createPrivateRoom)
	{
		string[] expectedUsers = null;
		if (Player.LocalPlayer != null && Player.LocalPlayer.PlayerParty.PartySize > 1)
		{
			expectedUsers = Player.LocalPlayer.PlayerParty.GetExpectedPhotonUsers();
		}
		if (PUNNetworkManager.Instance.IsInLockerRoom)
		{
			base.photonView.RPC("RpcPlayerSwitchedActivity", PhotonTargets.Others, Player.LocalPlayer.PlayerName, activity);
		}
		if (Player.LocalPlayer != null)
		{
			yield return Player.LocalPlayer.DespawnLocal(true);
		}
		PUNNetworkManager.Instance.SwitchActivity(activity, expectedUsers, createPrivateRoom);
		changeActivityCoroutine = null;
	}

	[PunRPC]
	public void RpcPlayerSwitchedActivity(string playerName, string targetActivity)
	{
		string activityFriendlyName = PUNNetworkManager.Instance.GetActivityFriendlyName(targetActivity);
		if (targetActivity != "lockerroom" && targetActivity != "dormroom" && !string.IsNullOrEmpty(activityFriendlyName))
		{
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, string.Format("{0} went to {1}", playerName.Truncate(12, "..."), activityFriendlyName), 3f);
		}
	}

	public void JoinPlayer(ulong playerId, bool bringParty = true)
	{
		changeActivityCoroutine = StartCoroutine(JoinPlayerCoroutine(playerId, bringParty));
	}

	private IEnumerator JoinPlayerCoroutine(ulong playerId, bool bringParty)
	{
		string playerIdStr = playerId.ToString();
		PhotonPlayer[] otherPlayers = PhotonNetwork.otherPlayers;
		foreach (PhotonPlayer photonPlayer in otherPlayers)
		{
			if (photonPlayer.userId == playerIdStr)
			{
				yield break;
			}
		}
		string[] expectedUsers = null;
		if (bringParty && Player.LocalPlayer != null && Player.LocalPlayer.PlayerParty.PartySize > 1)
		{
			expectedUsers = Player.LocalPlayer.PlayerParty.GetExpectedPhotonUsers();
		}
		yield return Player.LocalPlayer.DespawnLocal(true);
		PUNNetworkManager.Instance.JoinPlayer(playerId, expectedUsers);
		changeActivityCoroutine = null;
	}

	public void JoinRoom(string roomName)
	{
		changeActivityCoroutine = StartCoroutine(JoinRoomCoroutine(roomName));
	}

	private IEnumerator JoinRoomCoroutine(string roomName)
	{
		string[] expectedUsers = null;
		if (Player.LocalPlayer != null && Player.LocalPlayer.PlayerParty.PartySize > 1)
		{
			expectedUsers = Player.LocalPlayer.PlayerParty.GetExpectedPhotonUsers();
		}
		yield return Player.LocalPlayer.DespawnLocal(true);
		PUNNetworkManager.Instance.JoinRoom(roomName, expectedUsers);
		changeActivityCoroutine = null;
	}

	[PunRPC]
	protected void RpcSpawnNewPlayer(Vector3 spawnPosition, Quaternion spawnRotation)
	{
		SpawnManager.SpawnNewPlayer(spawnPosition, spawnRotation);
		for (int i = 0; i < recRoomSceneComponents.Count; i++)
		{
			recRoomSceneComponents[i].SetLocalPlayerSettings();
		}
	}

	public void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		if (PhotonNetwork.isMasterClient)
		{
			SpawnManager.MasterSpawnNewPlayer(newPlayer);
		}
	}

	public virtual void OnMasterClientSwitched(PhotonPlayer newMasterClient)
	{
		if (!PhotonNetwork.isMasterClient && wasMasterClient)
		{
			StopAllCoroutines();
			wasMasterClient = false;
		}
		else
		{
			if (!PhotonNetwork.isMasterClient)
			{
				return;
			}
			wasMasterClient = true;
			for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
			{
				if (PhotonNetwork.playerList[i].ToPlayer() == null)
				{
					SpawnManager.MasterSpawnNewPlayer(PhotonNetwork.playerList[i]);
				}
			}
		}
	}
}
