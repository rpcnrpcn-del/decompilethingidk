using System;
using UnityEngine;

public class PlayerInputManager : SingletonMonoBehaviour<PlayerInputManager>
{
	[Tooltip("RTT: The roundtrip time is the average of milliseconds until a message was acknowledged by the server.The variance value (behind the +/-) shows how stable the rtt is (a lower value being better).\n\nSim toggle: Enables and disables the simulation.A sudden, big change of network conditions might result in disconnects.\n\nLag slider: Adds a fixed delay to all outgoing and incoming messages.In milliseconds.\n\nJit slider: Adds a random delay of 'up to X milliseconds' per message.\n\nLoss slider: Drops the set percentage of messages.You can expect less than 2 % drop in the internet today.")]
	[SerializeField]
	private PhotonLagSimulationGui photonLagSimulationGui;

	public bool GUIEnabled = true;

	public bool PlayerControlsEnabled = true;

	public bool PerfCountersEnabled = true;

	private string analyticsClientID = string.Empty;

	private float debugMenuDuration = 15f;

	private float debugMenuStartTime;

	private string newPlayerName = string.Empty;

	public PerformanceCounters PerformanceCounters { get; private set; }

	private void Awake()
	{
		SingletonMonoBehaviour<PlayerInputManager>.Instance = this;
		if (photonLagSimulationGui == null)
		{
			photonLagSimulationGui = GetComponent<PhotonLagSimulationGui>();
		}
		PerformanceCounters = GetComponent<PerformanceCounters>();
		base.enabled = false;
	}

	public void Initialize()
	{
		analyticsClientID = PlayerPrefs.GetString("google_analytics_clientid_pref_key");
		base.enabled = SessionManager.IsDeveloper;
	}

	private void OnGUI()
	{
		if (!SessionManager.IsDeveloper || !GUIEnabled)
		{
			return;
		}
		if (!PhotonNetwork.insideLobby && PerfCountersEnabled)
		{
			GUI.Box(new Rect(0f, 110f, 210f, 80f), "Performance Counters");
			GUI.Label(new Rect(5f, 130f, 200f, 20f), string.Format("FramesPerSecond : {0}", PerformanceCounters.FPS.ToString("F2")));
			GUI.Label(new Rect(5f, 150f, 200f, 20f), string.Format("RountTrip (msec) : {0}", PerformanceCounters.RountTripTime.ToString("F2")));
		}
		if (PhotonNetwork.inRoom && PhotonNetwork.playerList.Length > 0)
		{
			int num = 200;
			int num2 = PhotonNetwork.playerList.Length;
			GUI.Box(new Rect(0f, num, 260f, num2 * 20 + 20), string.Empty);
			GUILayout.BeginVertical();
			GUILayout.Space(num);
			for (int i = 0; i < num2; i++)
			{
				PhotonPlayer photonPlayer = PhotonNetwork.playerList[i];
				GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("#{0} : {1}", photonPlayer.ID, photonPlayer.name), GUILayout.Width(200f));
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
		}
		if (!PlayerControlsEnabled || PhotonNetwork.player == null || !(Player.LocalPlayer != null))
		{
			return;
		}
		Event current = Event.current;
		bool flag = current.isKey && current.keyCode == KeyCode.Return && current.type == EventType.KeyDown;
		bool flag2 = current.isKey && current.keyCode == KeyCode.Tab && current.type == EventType.KeyDown;
		bool flag3 = current.isKey && current.keyCode == KeyCode.H && current.type == EventType.KeyDown;
		bool flag4 = current.isKey && current.keyCode == KeyCode.G && current.type == EventType.KeyDown;
		bool visible = false;
		bool flag5 = Time.time - debugMenuStartTime <= debugMenuDuration;
		GUI.Box(new Rect(0f, 0f, 850f, 70f), (!flag5) ? "Press 'Tab' to show Game Menu." : string.Empty);
		if (flag5)
		{
			GUI.Label(new Rect(5f, 30f, 300f, 20f), "Player Name : " + ((PhotonNetwork.player != null) ? PhotonNetwork.player.name : string.Empty));
			GUI.Label(new Rect(5f, 50f, 300f, 20f), "Build Version : " + BuildSettings.Version);
			GUI.Label(new Rect(305f, 30f, 200f, 20f), "Master : " + ((PhotonNetwork.masterClient != null) ? PhotonNetwork.masterClient.name : string.Empty));
			GUI.Label(new Rect(5f, 70f, 200f, 20f), "Analytics clientID : ");
			GUI.TextField(new Rect(210f, 70f, 300f, 20f), analyticsClientID);
			GUI.Label(new Rect(5f, 90f, 200f, 20f), "Photon Room Name : ");
			GUI.TextField(new Rect(210f, 90f, 300f, 20f), (!PhotonNetwork.inRoom) ? "<not in room>" : PhotonNetwork.room.name);
			newPlayerName = GUI.TextField(new Rect(5f, 10f, 200f, 20f), newPlayerName, 25);
			if ((GUI.Button(new Rect(205f, 10f, 120f, 20f), "Update Name") || flag) && !string.IsNullOrEmpty(newPlayerName))
			{
				PhotonNetwork.player.name = newPlayerName;
				GUI.FocusControl(null);
			}
			if (GUI.Button(new Rect(325f, 10f, 120f, 20f), "Change Master!"))
			{
				PUNNetworkManager.Instance.DebugChangeMasterClient();
			}
			if (GUI.Button(new Rect(445f, 10f, 120f, 20f), "Swap hands!"))
			{
				SingletonMonoBehaviour<CameraRig>.Instance.SwapHands();
			}
			if (GUI.Button(new Rect(565f, 10f, 120f, 20f), "Randomize Outfit"))
			{
				OutfitManager.Instance.RandomizePlayerOutfit();
			}
			if (GUI.Button(new Rect(685f, 10f, 120f, 20f), "Random Name"))
			{
				SingletonMonoBehaviour<DebugManager>.Instance.SetRandomName((DebugManager.RandomName)UnityEngine.Random.Range(0, Enum.GetNames(typeof(DebugManager.RandomName)).Length));
			}
			if (PhotonNetwork.inRoom && PhotonNetwork.room.expectedUsers != null && PhotonNetwork.room.expectedUsers.Length > 0 && GUI.Button(new Rect(685f, 10f, 120f, 20f), "Clear expected players"))
			{
				PhotonNetwork.room.ClearExpectedUsers();
			}
			if (PhotonNetwork.inRoom && (PhotonNetwork.room.open || (PhotonNetwork.otherPlayers != null && PhotonNetwork.otherPlayers.Length > 0)) && GUI.Button(new Rect(685f, 30f, 120f, 20f), "Kill the Room!"))
			{
				PhotonNetwork.room.open = false;
				Player.LocalPlayer.PlayerParty.photonView.RPC("RpcDebugLeaveRoom", PhotonTargets.Others);
			}
			if (GUI.Button(new Rect(565f, 30f, 120f, 20f), "Moderator Outfit"))
			{
				Player.LocalPlayer.PlayerOutfit.DebugEquipModeratorOutfits();
			}
			if (flag3)
			{
				Transform parent = Player.LocalPlayer.transform.parent;
				OutfitTrigger[] componentsInChildren = parent.GetComponentsInChildren<OutfitTrigger>(true);
				OutfitTrigger outfitTrigger = null;
				OutfitTrigger[] array = componentsInChildren;
				foreach (OutfitTrigger outfitTrigger2 in array)
				{
					if (outfitTrigger2.OutfitItem.Type == OutfitManager.OutfitType.Hat)
					{
						outfitTrigger = outfitTrigger2;
						break;
					}
				}
				if (outfitTrigger != null)
				{
					Player.LocalPlayer.LeftHand.TriggeredOutfit = outfitTrigger;
					Player.LocalPlayer.LeftHand.TryRemoveTriggeredOutfit(true);
				}
			}
			if (flag4)
			{
				OutfitSelection outfitSelection = OutfitManager.Instance.CreateRandomOutfitSelection(20);
				if (outfitSelection != null && OutfitManager.Instance.AddAvatarItemToUnlockedList(outfitSelection.ToString()))
				{
					ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, "Got new item : " + outfitSelection.outfitItem.name, 2f);
				}
			}
			bool flag6 = current.isKey || (current.isMouse && current.type == EventType.MouseDown) || current.type == EventType.MouseUp;
			if (flag2)
			{
				debugMenuStartTime = 0f;
			}
			else if (flag6)
			{
				debugMenuStartTime = Time.time;
			}
			visible = true;
		}
		else if (flag || flag2)
		{
			debugMenuStartTime = Time.time;
			GUI.FocusControl(null);
		}
		if (photonLagSimulationGui != null)
		{
			photonLagSimulationGui.Visible = visible;
		}
	}
}
