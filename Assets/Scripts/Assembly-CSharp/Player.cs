using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using Photon;
using Steamworks;
using UnityEngine;
using Valve.VR;

[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkToolController))]
[RequireComponent(typeof(PlayerOutfit))]
[RequireComponent(typeof(PlayerUI))]
[RequireComponent(typeof(PlayerData))]
[RequireComponent(typeof(PlayerLocomotion))]
public class Player : PunBehaviour
{
	public enum BodyPart
	{
		None = -1,
		Head = 0,
		Torso = 1,
		LeftHand = 2,
		RightHand = 3,
		Mouth = 4
	}

	public enum GhostingType
	{
		Panic = 0,
		Permanent = 1
	}

	public static List<Player> All = new List<Player>();

	public static Player LocalPlayer = null;

	private float distanceToLocalPlayer;

	private bool playerIndexDirty = true;

	private int playerIndex = -1;

	[SerializeField]
	private Transform trackingSpaceOrigin;

	[SerializeField]
	private PlayerHead head;

	[SerializeField]
	private PlayerBody body;

	[SerializeField]
	private PlayerHand leftHand;

	[SerializeField]
	private PlayerHand rightHand;

	[SerializeField]
	private GameObject[] hideInFirstPerson;

	[SerializeField]
	private Transform voiceObject;

	[SerializeField]
	private Transform floorPositionTransform;

	[SerializeField]
	private Transform ignoredPlayerVisual;

	[SerializeField]
	private GhostVisual ghostedPlayerVisual;

	[SerializeField]
	private PlayerChaperone chaperone;

	[SerializeField]
	private Texture2D defaultProfileImage;

	[Header("AFK settings")]
	[SerializeField]
	private string afkWarningMessageTitle = "Looks like you are inactive!";

	private string afkWarningMessageSubTitle = "Returning to lobby in ";

	[SerializeField]
	private string afkIntroMessage = "You have been inactive for a long time.\nPlease enter the game again if you wish to continue playing.";

	[SerializeField]
	private Transform afkPlayerVisual;

	[SerializeField]
	private Transform playerIdentity;

	[SerializeField]
	private Vector3 localIdentityOffset = new Vector3(0f, -0.1f, 0.5f);

	private PhotonVoiceRecorder voiceRecorder;

	private PhotonVoiceSpeaker voiceSpeaker;

	private GameManager gameManager;

	private List<PlayerHand> _bothHands;

	private PlayerHand _dominantHand;

	private bool canInteractWithTools = true;

	private bool wasMutePreAFK;

	private bool _mute;

	private bool _remoteMute;

	private const string PLATFORM_TYPE_PROP = "Platform";

	private const string PLATFORM_ID_PROP = "PlatformId";

	private const string VOICE_FILTER_PROP = "PlayerVoiceFilter";

	private Stack<string> localPlayerStatusStack = new Stack<string>();

	private Stack<string> richPresenceStack = new Stack<string>();

	private bool wasGribButtonPressed;

	public bool SupportsOutOfBounds;

	private Timer outOfBoundsTimer = new Timer();

	private Stack<float> afkKickToIntroTimeoutStack = new Stack<float>();

	private Stack<float> afkWarningStartTimeoutStack = new Stack<float>();

	public const string PLAYER_REGISTERING = "PlayerRegistering";

	private SynchronizedField<bool> _isRegistering;

	public const string PLAYER_AFK_VISIBLE = "PlayerAfkVisible";

	private int currentAfkCoundown = -1;

	private float lastHeadMovedTime;

	private bool isSpawned = true;

	private bool wasVisible = true;

	private bool _isInPersonalBubble;

	private bool _panicGhosted;

	private bool _isInChangingRoom;

	private bool _ghostModeEnabled = true;

	private bool _permaGhosted;

	private bool _remotePermaGhosted;

	private bool _remotePanicGhosted;

	public int PlayerIndex
	{
		get
		{
			if (playerIndexDirty)
			{
				for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
				{
					if (PhotonNetwork.playerList[i] == PhotonPlayer)
					{
						playerIndex = i;
						playerIndexDirty = false;
						break;
					}
				}
			}
			return playerIndex;
		}
	}

	public static float RespawnDuration
	{
		get
		{
			return DespawnDuration + SpawnDuration;
		}
	}

	public static float DespawnDuration
	{
		get
		{
			return CameraFade.Instance.TotalFadeOutDuration;
		}
	}

	public static float SpawnDuration
	{
		get
		{
			return CameraFade.Instance.TotalFadeInDuration;
		}
	}

	public NetworkToolController ToolController { get; private set; }

	public PlayerAudio PlayerAudio { get; private set; }

	public PlayerOutfit PlayerOutfit { get; private set; }

	public PlayerUI PlayerUI { get; private set; }

	public PlayerParty PlayerParty { get; private set; }

	public PlayerData PlayerData { get; private set; }

	public PlayerLocomotion PlayerLocomotion { get; private set; }

	public PlayerModeration PlayerModeration { get; private set; }

	public PlayerChaperone PlayerChaperone
	{
		get
		{
			return chaperone;
		}
	}

	public PlayerObjectiveTracker ObjectiveTracker { get; private set; }

	public PlayerProgression PlayerProgression { get; private set; }

	public PlayerFacialAnimator FacialAnimator { get; private set; }

	public PlayerEvents PlayerEvents { get; private set; }

	public AnimateInOut AnimateInOut { get; private set; }

	public PlayerHead Head
	{
		get
		{
			return head;
		}
	}

	public PlayerBody Body
	{
		get
		{
			return body;
		}
	}

	public PlayerHand LeftHand
	{
		get
		{
			return leftHand;
		}
	}

	public PlayerHand RightHand
	{
		get
		{
			return rightHand;
		}
	}

	public GhostingPulse SituationPulse { get; private set; }

	public Texture2D DefaultProfileImage
	{
		get
		{
			return defaultProfileImage;
		}
	}

	public List<PlayerHand> BothHands
	{
		get
		{
			if (_bothHands == null)
			{
				_bothHands = new List<PlayerHand>();
				_bothHands.Add(LeftHand);
				_bothHands.Add(RightHand);
			}
			return _bothHands;
		}
	}

	public PlayerHand DominantHand
	{
		get
		{
			return _dominantHand;
		}
		private set
		{
			if (value != null && !value.IsHandTracking)
			{
				_dominantHand = GetOtherHand(value.Type);
			}
			else
			{
				_dominantHand = value;
			}
		}
	}

	public bool CanInteractWithTools
	{
		get
		{
			return isSpawned && canInteractWithTools;
		}
		set
		{
			if (CanInteractWithTools && !value)
			{
				ReleaseToolsFromBothHands();
			}
			canInteractWithTools = value;
		}
	}

	public Vector3 CurrentFloorPosition
	{
		get
		{
			Vector3 position = Head.transform.position;
			position.y = trackingSpaceOrigin.position.y;
			return position;
		}
	}

	public Transform FloorPositionTransform
	{
		get
		{
			return floorPositionTransform;
		}
	}

	public bool IsOutOfBounds { get; private set; }

	public bool IsOverlappingWorld
	{
		get
		{
			return Head.IsOverlappingWorld;
		}
	}

	public float CurrentFloorHeightFromHead
	{
		get
		{
			return Head.transform.position.y - trackingSpaceOrigin.position.y;
		}
	}

	public GameObject Root { get; private set; }

	public bool Mute
	{
		get
		{
			return _mute;
		}
		set
		{
			if (_mute == value)
			{
				return;
			}
			_mute = value;
			if (base.isLocal)
			{
				voiceRecorder.Transmit = !_mute;
			}
			else
			{
				voiceSpeaker.enabled = !RemoteMute && !Mute;
				if (!voiceSpeaker.enabled)
				{
					voiceSpeaker.Pause();
				}
			}
			bool flag = false;
			if (SingletonMonoBehaviour<SessionManager>.Instance != null)
			{
				if (_mute && !SingletonMonoBehaviour<SessionManager>.Instance.PlayerMuteList.Contains(PhotonPlayer.userId))
				{
					SingletonMonoBehaviour<SessionManager>.Instance.PlayerMuteList.Add(PhotonPlayer.userId);
					flag = true;
				}
				else if (!_mute && SingletonMonoBehaviour<SessionManager>.Instance.PlayerMuteList.Contains(PhotonPlayer.userId))
				{
					SingletonMonoBehaviour<SessionManager>.Instance.PlayerMuteList.Remove(PhotonPlayer.userId);
					flag = true;
				}
			}
			if (flag)
			{
				AnalyticsHelper.UserMutePlayer(this, _mute);
			}
			PlayerEvents.MicrophoneMuted(_mute);
		}
	}

	public bool RemoteMute
	{
		get
		{
			return _remoteMute && (gameManager == null || gameManager.GhostingEnabled);
		}
		private set
		{
			if (_remoteMute == value)
			{
				return;
			}
			_remoteMute = value;
			if (base.isLocal)
			{
				Debug.LogError("You should never call Block voice on yourself.");
				return;
			}
			voiceSpeaker.enabled = !RemoteMute && !Mute;
			if (!voiceSpeaker.enabled)
			{
				voiceSpeaker.Pause();
			}
		}
	}

	public float AudioMaxForFrame
	{
		get
		{
			if (voiceSpeaker != null)
			{
				return voiceSpeaker.MaxAudioSample;
			}
			return 0f;
		}
	}

	public bool IsTalking
	{
		get
		{
			bool flag = false;
			flag = ((!base.isLocal) ? (voiceSpeaker != null && voiceSpeaker.IsVoiceDetected && PhotonVoiceNetwork.ClientState == ExitGames.Client.Photon.LoadBalancing.ClientState.Joined) : (voiceRecorder != null && voiceRecorder.VoiceDetector != null && voiceRecorder.VoiceDetector.Detected));
			return !Mute && flag;
		}
	}

	public GameTeam Team
	{
		get
		{
			if (gameManager == null)
			{
				return GameTeam.INVALID;
			}
			return gameManager.TeamManager.GetPlayerTeam(PhotonPlayer);
		}
	}

	public string PlayerName
	{
		get
		{
			return (PhotonPlayer == null) ? "UNKNOWN" : PhotonPlayer.name;
		}
	}

	public PhotonPlayer PhotonPlayer
	{
		get
		{
			return (!(base.photonView != null)) ? null : base.photonView.owner;
		}
	}

	public ulong PlayerId { get; private set; }

	public PlatformManager.PlatformType Platform { get; private set; }

	public ulong PlatformId { get; private set; }

	/*public CSteamID SteamID
	{
		get
		{
			return (Platform != PlatformManager.PlatformType.STEAM) ? CSteamID.Nil : new CSteamID(PlatformId);
		}
	}*/

	public ulong PatchedSteamID
	{
		get
		{
			return PlatformId;
		}
	}

	public AudioSource VoiceAudioSource { get; private set; }

	public bool IsInitialized { get; private set; }

	public AudioManager.VOIPFilter VOIPFilter
	{
		get
		{
			return (AudioManager.VOIPFilter)PlayerData.GetData("PlayerVoiceFilter", 2);
		}
		set
		{
			PlayerData.SetData("PlayerVoiceFilter", (int)value);
		}
	}

	public string RichPresence
	{
		get
		{
			return richPresenceStack.Peek();
		}
	}

	public string LocalPlayerStatus
	{
		get
		{
			return (localPlayerStatusStack.Count <= 0) ? string.Empty : localPlayerStatusStack.Peek();
		}
	}

	public float AfkKickToIntroTimeout
	{
		get
		{
			return afkKickToIntroTimeoutStack.Peek();
		}
	}

	public float AfkWarningStartTimeout
	{
		get
		{
			return afkWarningStartTimeoutStack.Peek();
		}
	}

	public bool IsRegistering
	{
		get
		{
			return _isRegistering.Get();
		}
		set
		{
			if (IsRegistering != value)
			{
				_isRegistering.ForceSet(value);
			}
		}
	}

	public bool AFKVisible
	{
		get
		{
			return PlayerData.GetData("PlayerAfkVisible", true);
		}
		set
		{
			if (AFKVisible != value)
			{
				PlayerData.SetData("PlayerAfkVisible", value);
				if (value)
				{
					Mute = wasMutePreAFK;
					return;
				}
				wasMutePreAFK = Mute;
				Mute = true;
			}
		}
	}

	public bool HeadActivityDetected { get; private set; }

	public Vector3 LastSpawnPosition { get; private set; }

	public Quaternion LastSpawnRotation { get; private set; }

	public bool IsSpawning
	{
		get
		{
			return !isSpawned;
		}
	}

	public bool IsVisible
	{
		get
		{
			return base.isLocal || (isSpawned && !PermaGhost && !RemotePermaGhosted && !PhotonNetwork.offlineMode && AFKVisible && !InPersonalBubble && !IsGhosted && !IsRegistering);
		}
	}

	public bool InPersonalBubble
	{
		get
		{
			return _isInPersonalBubble;
		}
		set
		{
			_isInPersonalBubble = value;
		}
	}

	public bool IsGhosted
	{
		get
		{
			return (_panicGhosted || PermaGhost || RemotePanicGhosted) && !RemotePermaGhosted;
		}
	}

	public bool IsGhostedOrPermaghosted
	{
		get
		{
			return _panicGhosted || PermaGhost || RemotePermaGhosted;
		}
	}

	public bool IsInChangingRoom
	{
		get
		{
			return _isInChangingRoom;
		}
		set
		{
			_isInChangingRoom = value;
			if (base.isLocal)
			{
				AnalyticsHelper.AvatarChangingRoom(_isInChangingRoom);
			}
			if (this.IsInChangingRoomUpdated != null)
			{
				this.IsInChangingRoomUpdated(_isInChangingRoom);
			}
		}
	}

	public bool GhostModeEnabled
	{
		get
		{
			return _ghostModeEnabled;
		}
		set
		{
			_ghostModeEnabled = value;
			SituationPulse.PulseEnabled = value;
		}
	}

	public bool PermaGhost
	{
		get
		{
			return _permaGhosted && (gameManager == null || gameManager.GhostingEnabled);
		}
		set
		{
			if (base.isLocal)
			{
				return;
			}
			_permaGhosted = value;
			base.photonView.RPC("RpcGhostPlayer", base.owner, PhotonNetwork.player, _permaGhosted, GhostingType.Permanent);
			bool flag = false;
			if (SingletonMonoBehaviour<SessionManager>.Instance != null)
			{
				if (_permaGhosted && !SingletonMonoBehaviour<SessionManager>.Instance.PlayerIgnoreList.Contains(PhotonPlayer.userId))
				{
					SingletonMonoBehaviour<SessionManager>.Instance.PlayerIgnoreList.Add(PhotonPlayer.userId);
					flag = true;
				}
				else if (!_permaGhosted && SingletonMonoBehaviour<SessionManager>.Instance.PlayerIgnoreList.Contains(PhotonPlayer.userId))
				{
					SingletonMonoBehaviour<SessionManager>.Instance.PlayerIgnoreList.Remove(PhotonPlayer.userId);
					flag = true;
				}
			}
			if (flag)
			{
				AnalyticsHelper.UserIgnorePlayer(this, _permaGhosted);
			}
			Mute = _permaGhosted;
			base.photonView.RPC("RpcBlockMyPlayersVoice", PhotonPlayer, PhotonNetwork.player, _permaGhosted);
		}
	}

	public bool RemotePermaGhosted
	{
		get
		{
			return _remotePermaGhosted && (gameManager == null || gameManager.GhostingEnabled);
		}
		private set
		{
			if (!base.isLocal)
			{
				_remotePermaGhosted = value;
			}
		}
	}

	public bool RemotePanicGhosted
	{
		get
		{
			return _remotePanicGhosted && (gameManager == null || gameManager.GhostingEnabled);
		}
		private set
		{
			if (!base.isLocal)
			{
				_remotePanicGhosted = value;
			}
		}
	}

	public event Action OutOfBoundsEvent;

	public event Action<bool> VisibilityChanged;

	public event Action<bool> IsInChangingRoomUpdated;

	public static void SetPlatformPlayerId(PlatformManager.PlatformType platform, ulong platformPlayerId)
	{
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		hashtable.Add("Platform", (int)platform);
		hashtable.Add("PlatformId", (long)platformPlayerId);
		ExitGames.Client.Photon.Hashtable propertiesToSet = hashtable;
		PhotonNetwork.player.SetCustomProperties(propertiesToSet);
	}

	protected override void Awake()
	{
		base.Awake();
		Root = new GameObject("PlayerRoot");
		UnityEngine.Object.DontDestroyOnLoad(Root);
		SetParentPlayerRoot(base.transform);
		SetParentPlayerRoot(voiceObject);
		PhotonPlayer.TagObject = this;
		PlayerId = ulong.Parse(PhotonPlayer.userId);
		if (base.isLocal)
		{
			LocalPlayer = this;
		}
		if (chaperone != null)
		{
			chaperone.gameObject.SetActive(base.isLocal);
		}
		PlayerEvents = GetComponent<PlayerEvents>();
		FacialAnimator = Head.GetComponent<PlayerFacialAnimator>();
		FacialAnimator.ThisPlayer = this;
		ToolController = GetComponent<NetworkToolController>();
		PlayerAudio = GetComponent<PlayerAudio>();
		PlayerOutfit = GetComponent<PlayerOutfit>();
		PlayerOutfit.ThisPlayer = this;
		PlayerUI = GetComponent<PlayerUI>();
		PlayerParty = GetComponent<PlayerParty>();
		PlayerData = GetComponent<PlayerData>();
		PlayerLocomotion = GetComponent<PlayerLocomotion>();
		ObjectiveTracker = GetComponent<PlayerObjectiveTracker>();
		PlayerModeration = GetComponent<PlayerModeration>();
		voiceRecorder = voiceObject.GetComponent<PhotonVoiceRecorder>();
		voiceSpeaker = voiceObject.GetComponent<PhotonVoiceSpeaker>();
		VoiceAudioSource = voiceObject.GetComponent<AudioSource>();
		AnimateInOut = GetComponent<AnimateInOut>();
		PlayerProgression = GetComponent<PlayerProgression>();
		SituationPulse = GetComponentInChildren<GhostingPulse>();
		PlayerCollider[] componentsInChildren = GetComponentsInChildren<PlayerCollider>(true);
		PlayerCollider[] array = componentsInChildren;
		foreach (PlayerCollider playerCollider in array)
		{
			playerCollider.ThisPlayer = this;
		}
		if (base.isLocal)
		{
			base.gameObject.ReplaceLayerRecursively(Layers.RemotePlayerPhysics, Layers.LocalPlayerPhysics);
		}
		LeftHand.ThisPlayer = this;
		LeftHand.PickupToolEvent += OnHandPickupTool;
		RightHand.ThisPlayer = this;
		RightHand.PickupToolEvent += OnHandPickupTool;
		DominantHand = RightHand;
		object[] instantiationData = base.photonView.instantiationData;
		Vector3 position = Head.transform.InverseTransformPoint(Body.transform.position);
		Quaternion rotation = Head.transform.InverseTransformRotation(Body.transform.rotation);
		Head.transform.position = base.transform.TransformPoint(((Vector3)instantiationData[0]).ValueOrZeroIfBogus());
		Head.transform.rotation = base.transform.TransformRotation(((Quaternion)instantiationData[1]).ValueOrIdentityIfBogus());
		LeftHand.transform.position = base.transform.TransformPoint(((Vector3)instantiationData[2]).ValueOrZeroIfBogus());
		LeftHand.transform.rotation = base.transform.TransformRotation(((Quaternion)instantiationData[3]).ValueOrIdentityIfBogus());
		RightHand.transform.position = base.transform.TransformPoint(((Vector3)instantiationData[4]).ValueOrZeroIfBogus());
		RightHand.transform.rotation = base.transform.TransformRotation(((Quaternion)instantiationData[5]).ValueOrIdentityIfBogus());
		LastSpawnPosition = ((Vector3)instantiationData[6]).ValueOrZeroIfBogus();
		LastSpawnRotation = ((Quaternion)instantiationData[7]).ValueOrIdentityIfBogus();
		Body.transform.position = Head.transform.TransformPoint(position);
		Body.transform.rotation = Head.transform.TransformRotation(rotation);
		All.Add(this);
		PushAfkKickToIntroTimeout(0f);
		PushAfkWarningStartTimeout(0f);
		GhostModeEnabled = true;
		_isRegistering = new SynchronizedField<bool>(this, "PlayerRegistering", false, SetterPermissionMode.AUTHORITY, OnPlayerOutOfHMDVisibleChanged);
		VisibilityChanged += OnVisibilityChanged;
	}

	private void Start()
	{
		gameManager = RecRoomSceneManager.Instance.GameManager;
		Platform = (PlatformManager.PlatformType)PlayerData.GetData("Platform", 0);
		PlatformId = (ulong)PlayerData.GetData("PlatformId", 0L);
		if (base.isLocal)
		{
			GameObject[] array = hideInFirstPerson;
			foreach (GameObject gameObject in array)
			{
				if (gameObject.GetComponentInChildren<Collider>() != null)
				{
					Debug.LogError("Hidable object has a collider.  Its layer will be changed to hidden in first person.");
				}
				gameObject.SetLayerRecursively(Layers.HiddenInFirstPerson);
			}
			base.name += "_local";
			SetupControllerIOForHands();
			SingletonMonoBehaviour<CameraRig>.Instance.transform.position = base.transform.position;
			SingletonMonoBehaviour<CameraRig>.Instance.transform.rotation = base.transform.rotation;
			AnimateInOut.SuppressAnimation = true;
			SpawnLocal();
			Mute = SingletonMonoBehaviour<SettingsManager>.Instance.VoiceChat != VoiceChat.AlwaysOn;
		}
		else if (SingletonMonoBehaviour<SessionManager>.Instance != null && !string.IsNullOrEmpty(PhotonPlayer.userId) && SingletonMonoBehaviour<SessionManager>.Instance.PlayerIgnoreList.Contains(PhotonPlayer.userId))
		{
			PermaGhost = true;
		}
		if (!base.isLocal)
		{
			if (PhotonPlayer != null && !string.IsNullOrEmpty(PhotonPlayer.userId) && SingletonMonoBehaviour<SessionManager>.Instance.PlayerMuteList.Contains(PhotonPlayer.userId))
			{
				Mute = true;
			}
			PlayerData.RegisterCallback("PlayerVoiceFilter", OnPlayerVoiceChangerSet);
			OnPlayerVoiceChangerSet(PlayerData, "PlayerVoiceFilter");
		}
		GameObject root = Root;
		root.name = root.name + "_" + PhotonPlayer.name;
		if (gameManager != null)
		{
			gameManager.StateChangeEvent += OnGameStateChange;
		}
		IsInitialized = true;
	}

	private void OnDisable()
	{
		if (PhotonNetwork.inRoom)
		{
			ReleaseToolsFromBothHands();
		}
	}

	private void OnPlayerVoiceChangerSet(PlayerData sender, string key)
	{
		PlayerAudio.SetFilterType(VOIPFilter);
	}

	public void SetParentPlayerRoot(Transform newTransform)
	{
		newTransform.SetParent(Root.transform, true);
	}

	public void SetupControllerIOForHands()
	{
		leftHand.ControllerIO = SingletonMonoBehaviour<CameraRig>.Instance.LeftHandControllerIO;
		rightHand.ControllerIO = SingletonMonoBehaviour<CameraRig>.Instance.RightHandControllerIO;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (base.isLocal)
		{
			LocalPlayer = null;
		}
		if (gameManager != null)
		{
			gameManager.StateChangeEvent -= OnGameStateChange;
		}
		if (Root != null)
		{
			UnityEngine.Object.Destroy(Root);
		}
		VisibilityChanged -= OnVisibilityChanged;
		All.Remove(this);
	}

	protected void OnEnable()
	{
		lastHeadMovedTime = Time.unscaledTime;
	}

	private void Update()
	{
		FacialAnimator.Talking = IsTalking;
		voiceObject.transform.position = head.transform.position;
		voiceObject.transform.rotation = head.transform.rotation;
		if (!base.isLocal && (ignoredPlayerVisual.gameObject.activeSelf || afkPlayerVisual.gameObject.activeSelf))
		{
			Vector3 position = Head.transform.position;
			position.y -= CurrentFloorHeightFromHead;
			ignoredPlayerVisual.transform.position = position;
			afkPlayerVisual.transform.position = position;
		}
		UpdateVisibility();
		UpdatePushToTalk();
	}

	private void FixedUpdate()
	{
		if (base.isLocal)
		{
			UpdateAFKStatus();
			head.Rigidbody.MovePosition(SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.position);
			head.Rigidbody.MoveRotation(SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.rotation);
			floorPositionTransform.position = CurrentFloorPosition;
			UpdateOutOfBounds();
		}
		else
		{
			UpdateGhostStatus();
			UpdateGhostPosition();
		}
		UpdateIdentityPosition();
	}

	private void UpdateGhostPosition()
	{
		if (IsGhosted)
		{
			ghostedPlayerVisual.transform.position = Head.transform.position;
			Vector3 eulerAngles = Head.transform.rotation.eulerAngles;
			eulerAngles.x = (eulerAngles.z = 0f);
			ghostedPlayerVisual.transform.rotation = Quaternion.Euler(eulerAngles);
			if (LocalPlayer != null)
			{
				distanceToLocalPlayer = (LocalPlayer.Head.transform.position - Head.transform.position).magnitude;
			}
			float alpha = SituationPulse.AlphaForDistance(distanceToLocalPlayer);
			ghostedPlayerVisual.SetAlpha(alpha);
		}
	}

	private void UpdateIdentityPosition()
	{
		if (!base.isLocal)
		{
			playerIdentity.transform.position = head.transform.position;
			return;
		}
		Vector3 up = Vector3.up;
		Vector3 normalized = Vector3.ProjectOnPlane(SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.forward, up).normalized;
		Vector3 normalized2 = Vector3.ProjectOnPlane(SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.right, up).normalized;
		Vector3 vector = normalized2 * localIdentityOffset.x + up * localIdentityOffset.y + normalized * localIdentityOffset.z;
		playerIdentity.transform.position = head.transform.position + vector;
	}

	public void PushRichPresence(string newRichPresenceString)
	{
		richPresenceStack.Push(newRichPresenceString);
		if (PlatformManager.Instance != null)
		{
			PlatformManager.Instance.SetRichPresenceStatus(RichPresence);
		}
	}

	public void PopRichPresence()
	{
		richPresenceStack.Pop();
		if (PlatformManager.Instance != null)
		{
			PlatformManager.Instance.SetRichPresenceStatus(RichPresence);
		}
	}

	public void PushLocalPlayerStatus(string newLocalPlayerStatus)
	{
		localPlayerStatusStack.Push(newLocalPlayerStatus);
	}

	public void PopLocalPlayerStatus()
	{
		localPlayerStatusStack.Pop();
	}

	private void UpdatePushToTalk()
	{
		if (!base.isLocal)
		{
			return;
		}
		if (SingletonMonoBehaviour<SettingsManager>.Instance.VoiceChat == VoiceChat.PushToTalk && AFKVisible)
		{
			bool flag = (LeftHand != null && LeftHand.ControllerIO != null && LeftHand.IsHandTracking && LeftHand.ControllerIO.PushToTalkButtonPressed) || (RightHand != null && RightHand.ControllerIO != null && RightHand.IsHandTracking && RightHand.ControllerIO.PushToTalkButtonPressed);
			if (flag != wasGribButtonPressed)
			{
				Mute = !flag;
				wasGribButtonPressed = flag;
				PlayerAudio.OnPushToTalk(flag);
			}
		}
		else
		{
			wasGribButtonPressed = false;
		}
	}

	private void UpdateOutOfBounds()
	{
		if (!SupportsOutOfBounds)
		{
			return;
		}
		bool isOutOfBounds = IsOutOfBounds;
		IsOutOfBounds = Head.IsOutOfBounds;
		if (IsOutOfBounds)
		{
			if (!isOutOfBounds)
			{
				ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "Out of bounds!", "Get back to the field!", 2f);
				outOfBoundsTimer.StartTimer(5f);
				outOfBoundsTimer.AddTimeRemainingCallback(3f, OnOutOfBoundsThreeSecondsLeft);
				outOfBoundsTimer.AddTimeRemainingCallback(2f, OnOutOfBoundsTwoSecondsLeft);
				outOfBoundsTimer.AddTimeRemainingCallback(1f, OnOutOfBoundsOneSecondsLeft);
			}
			outOfBoundsTimer.UpdateCallbacks();
			if (outOfBoundsTimer.TimerOver && this.OutOfBoundsEvent != null)
			{
				this.OutOfBoundsEvent();
			}
		}
		else if (isOutOfBounds)
		{
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "You're back!", 1.5f);
		}
	}

	private void OnOutOfBoundsThreeSecondsLeft()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "3", 1f);
	}

	private void OnOutOfBoundsTwoSecondsLeft()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "2", 1f);
	}

	private void OnOutOfBoundsOneSecondsLeft()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "1", 1f);
	}

	public void PushAfkKickToIntroTimeout(float timeout)
	{
		afkKickToIntroTimeoutStack.Push(timeout);
	}

	public void PopAfkKickToIntroTimeout()
	{
		afkKickToIntroTimeoutStack.Pop();
	}

	public void PushAfkWarningStartTimeout(float timeout)
	{
		afkWarningStartTimeoutStack.Push(timeout);
	}

	public void PopAfkWarningStartTimeout()
	{
		afkWarningStartTimeoutStack.Pop();
	}

	private void OnPlayerOutOfHMDVisibleChanged()
	{
		bool isVisible = IsVisible;
		if (!base.isLocal && wasVisible != isVisible)
		{
			AnimateInOut.PlayEffect(isVisible);
		}
	}

	private void UpdateAFKStatus()
	{
		bool flag = !PhotonNetwork.offlineMode && PhotonNetwork.inRoom && RecRoomSceneManager.Instance != null && !PUNNetworkManager.Instance.IsInDormRoom && !IsRegistering && !PUNNetworkManager.Instance.IsActivityInviteOnly;
		HeadActivityDetected = true;
		if (OpenVR.System != null)
		{
			EDeviceActivityLevel trackedDeviceActivityLevel = OpenVR.System.GetTrackedDeviceActivityLevel(0u);
			HeadActivityDetected = trackedDeviceActivityLevel == EDeviceActivityLevel.k_EDeviceActivityLevel_UserInteraction;
		}
		if (flag)
		{
			float num = Time.unscaledTime - lastHeadMovedTime;
			AFKVisible = HeadActivityDetected;
			if (HeadActivityDetected)
			{
				lastHeadMovedTime = Time.unscaledTime;
				currentAfkCoundown = -1;
			}
			else if (num > AfkKickToIntroTimeout)
			{
				ReturnToDormRoom(afkIntroMessage);
				AnalyticsHelper.UserAFK(num);
				lastHeadMovedTime = Time.unscaledTime;
			}
			else if (num > AfkWarningStartTimeout)
			{
				int num2 = Mathf.CeilToInt(AfkKickToIntroTimeout - num);
				if (currentAfkCoundown != num2)
				{
					MenuNotification.PlayNext(afkWarningMessageTitle, 1f, afkWarningMessageSubTitle + num2, true, null, 0.3f);
					currentAfkCoundown = num2;
				}
			}
		}
		else
		{
			lastHeadMovedTime = Time.unscaledTime;
			currentAfkCoundown = -1;
		}
	}

	private void UpdateGhostStatus()
	{
		bool panicGhosted = _panicGhosted;
		if (LocalPlayer != null && LocalPlayer.SituationPulse.Active && (gameManager == null || gameManager.GhostingEnabled))
		{
			if (!IsGhosted)
			{
				distanceToLocalPlayer = (LocalPlayer.Head.transform.position - Head.transform.position).magnitude;
			}
			bool panicGhosted2 = distanceToLocalPlayer <= LocalPlayer.SituationPulse.Radius;
			_panicGhosted = panicGhosted2;
		}
		else
		{
			_panicGhosted = false;
		}
		if (_panicGhosted != panicGhosted)
		{
			base.photonView.RPC("RpcGhostPlayer", base.owner, PhotonNetwork.player, _panicGhosted, GhostingType.Panic);
		}
	}

	private void OnGameStateChange(ushort currentStateId, ushort previousStateId, ushort currentSubStateId, ushort previousSubStateId)
	{
		RemoteMute = RemoteMute;
	}

	public void ReturnToDormRoom(string message)
	{
		SingletonMonoBehaviour<SessionManager>.Instance.AlertMessage = message;
		SingletonMonoBehaviour<SessionManager>.Instance.AlertTitle = null;
		PlayerParty.LeaveCurrentParty();
		RecRoomSceneManager.Instance.SwitchActivity("dormroom", true);
	}

	public void OnRecRoomSceneManagerDestroyed()
	{
		ReleaseToolsFromBothHands();
	}

	public void ReleaseToolsFromBothHands(bool releaseDueToRespawn = false)
	{
		if (LeftHand.Tool != null && (LeftHand.Tool.IsReleasedOnPlayerRespawn || !releaseDueToRespawn))
		{
			ToolController.ReleaseTool(LeftHand.Tool);
		}
		if (RightHand.Tool != null && (RightHand.Tool.IsReleasedOnPlayerRespawn || !releaseDueToRespawn))
		{
			ToolController.ReleaseTool(RightHand.Tool);
		}
	}

	public PlayerHand GetHand(BodyPart bodyPart)
	{
		PlayerHand result = null;
		switch (bodyPart)
		{
		case BodyPart.LeftHand:
			result = LeftHand;
			break;
		case BodyPart.RightHand:
			result = RightHand;
			break;
		}
		return result;
	}

	public PlayerHand GetHand(PlayerHand.HandType id)
	{
		PlayerHand result = null;
		switch (id)
		{
		case PlayerHand.HandType.Left:
			result = LeftHand;
			break;
		case PlayerHand.HandType.Right:
			result = RightHand;
			break;
		}
		return result;
	}

	public PlayerHand GetHand(int id)
	{
		return GetHand((PlayerHand.HandType)id);
	}

	public PlayerHand GetOtherHand(PlayerHand.HandType handType)
	{
		PlayerHand playerHand = null;
		switch (handType)
		{
		case PlayerHand.HandType.Left:
			playerHand = RightHand;
			break;
		case PlayerHand.HandType.Right:
			playerHand = LeftHand;
			break;
		}
		if (!playerHand.IsHandTracking)
		{
			playerHand = GetHand(handType);
		}
		return playerHand;
	}

	private void OnHandPickupTool(PlayerHand hand, Tool tool)
	{
		PlayerHand.HandType type = hand.Type;
		if (!(GetOtherHand(type).Tool != null) && hand != DominantHand)
		{
			DominantHand = hand;
		}
	}

	public static void SpawnLocalPlayerAt(Vector3 position, Quaternion rotation)
	{
		if (LocalPlayer == null)
		{
			Vector3 vector = SingletonMonoBehaviour<CameraRig>.Instance.transform.InverseTransformPoint(SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.position);
			Vector3 vector2 = SingletonMonoBehaviour<CameraRig>.Instance.transform.InverseTransformPoint(SingletonMonoBehaviour<CameraRig>.Instance.LeftHandControllerIO.transform.position);
			Vector3 vector3 = SingletonMonoBehaviour<CameraRig>.Instance.transform.InverseTransformPoint(SingletonMonoBehaviour<CameraRig>.Instance.RightHandControllerIO.transform.position);
			Quaternion quaternion = SingletonMonoBehaviour<CameraRig>.Instance.transform.InverseTransformRotation(SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.rotation);
			Quaternion quaternion2 = SingletonMonoBehaviour<CameraRig>.Instance.transform.InverseTransformRotation(SingletonMonoBehaviour<CameraRig>.Instance.LeftHandControllerIO.transform.rotation);
			Quaternion quaternion3 = SingletonMonoBehaviour<CameraRig>.Instance.transform.InverseTransformRotation(SingletonMonoBehaviour<CameraRig>.Instance.RightHandControllerIO.transform.rotation);
			Quaternion rotation2 = Quaternion.identity;
			if (PlatformManager.Instance.CurrentTrackingMode == PlatformManager.TrackingMode.THREE_SIXTY_DEGREE)
			{
				rotation2 = Quaternion.Euler(0f, quaternion.eulerAngles.y, 0f);
			}
			object[] data = new object[8] { vector, quaternion, vector2, quaternion2, vector3, quaternion3, position, rotation };
			rotation *= Quaternion.Inverse(rotation2);
			position -= rotation * SingletonMonoBehaviour<CameraRig>.Instance.CameraFloorOffsetLocalSpace;
			PhotonNetwork.Instantiate("[Player]", position, rotation, 0, data);
		}
	}

	public void Respawn(Vector3 position, Quaternion rotation, bool dropTools = true, int spawnToolPhotonViewId = -1, bool requestGiftSpawn = false)
	{
		base.photonView.RPC("RpcRespawnPlayer", PhotonPlayer, position, rotation, dropTools, spawnToolPhotonViewId, requestGiftSpawn);
	}

	public void Despawn()
	{
		base.photonView.RPC("RpcDespawnPlayer", PhotonPlayer);
	}

	public Coroutine SpawnLocal()
	{
		if (!base.isLocal)
		{
			Debug.LogError("Player.SpawnLocal() can only be called on the LocalPlayer!");
		}
		isSpawned = true;
		base.photonView.RPC("RpcBroadcastSpawnPlayer", PhotonTargets.Others);
		PhotonNetwork.SendOutgoingCommands();
		return CameraFade.Instance.Fade(true);
	}

	public Coroutine DespawnLocal(bool prepForLevelSwitch, bool dropTools = true)
	{
		if (!base.isLocal)
		{
			Debug.LogError("Player.DespawnLocal() can only be called on the LocalPlayer!");
		}
		if (!isSpawned)
		{
			return StartCoroutine(WaitForCameraFadeCoroutine());
		}
		PlayerLocomotion.OnPlayerDespawn();
		isSpawned = false;
		if (dropTools)
		{
			ReleaseToolsFromBothHands(true);
		}
		base.photonView.RPC("RpcBroadcastDespawnPlayer", PhotonTargets.Others);
		PhotonNetwork.SendOutgoingCommands();
		return CameraFade.Instance.Fade(false, prepForLevelSwitch);
	}

	private IEnumerator WaitForCameraFadeCoroutine()
	{
		float startTime = Time.time;
		yield return new WaitWhile(() => CameraFade.Instance.IsFading && Time.time - startTime <= 3f);
	}

	[PunRPC]
	public IEnumerator RpcRespawnPlayer(Vector3 position, Quaternion rotation, bool dropTools, int spawnToolPhotonViewId, bool requestGiftSpawn)
	{
		yield return DespawnLocal(dropTools, false);
		PlayerLocomotion.CoordinateSpace spawnRotationCoordinateSpace = PlayerLocomotion.CoordinateSpace.PLAYER_SPACE;
		if (PlatformManager.Instance.CurrentTrackingMode == PlatformManager.TrackingMode.ONE_EIGHTY_DEGREE)
		{
			spawnRotationCoordinateSpace = PlayerLocomotion.CoordinateSpace.TRACKING_SPACE;
		}
		PlayerLocomotion.MoveCameraRig(position, rotation * Vector3.forward, true, spawnRotationCoordinateSpace);
		LastSpawnPosition = position;
		LastSpawnRotation = rotation;
		yield return SpawnLocal();
		Tool spawnTool = Tool.Find(spawnToolPhotonViewId);
		if (spawnTool != null)
		{
			ToolController.TryPickupToolWithDominantHand(spawnTool);
		}
		if (requestGiftSpawn)
		{
			SingletonMonoBehaviour<GiftManager>.Instance.StartReceiveGiftsCoroutine();
		}
	}

	[PunRPC]
	public IEnumerator RpcDespawnPlayer()
	{
		yield return DespawnLocal(false, false);
	}

	[PunRPC]
	public void RpcBroadcastSpawnPlayer()
	{
		isSpawned = true;
		if (IsVisible)
		{
			AnimateInOut.PlayEffect(true);
		}
	}

	[PunRPC]
	public void RpcBroadcastDespawnPlayer()
	{
		if (IsVisible)
		{
			AnimateInOut.PlayEffect(false);
		}
		PlayerLocomotion.OnPlayerDespawn();
		isSpawned = false;
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		playerIndexDirty = true;
	}

	public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
	{
		playerIndexDirty = true;
	}

	private void UpdateVisibility()
	{
		bool isVisible = IsVisible;
		if (wasVisible != isVisible)
		{
			if (this.VisibilityChanged != null)
			{
				this.VisibilityChanged(isVisible);
			}
			wasVisible = isVisible;
		}
	}

	private void OnVisibilityChanged(bool newVisibility)
	{
		for (int i = 0; i < head.transform.childCount; i++)
		{
			head.transform.GetChild(i).gameObject.SetActive(newVisibility);
		}
		Body.Visible = newVisibility;
		for (int j = 0; j < leftHand.transform.childCount; j++)
		{
			leftHand.transform.GetChild(j).gameObject.SetActive(newVisibility);
		}
		for (int k = 0; k < rightHand.transform.childCount; k++)
		{
			rightHand.transform.GetChild(k).gameObject.SetActive(newVisibility);
		}
		afkPlayerVisual.gameObject.SetActive(!base.isLocal && (!AFKVisible || InPersonalBubble) && !IsGhosted);
		playerIdentity.gameObject.SetActive(newVisibility || IsGhosted || afkPlayerVisual.gameObject.activeSelf);
		for (int l = 0; l < ghostedPlayerVisual.transform.childCount; l++)
		{
			ghostedPlayerVisual.transform.GetChild(l).gameObject.SetActive(IsGhosted);
		}
		if (!base.isLocal)
		{
			ignoredPlayerVisual.gameObject.SetActive(RemotePermaGhosted || IsRegistering);
		}
		AnimateInOut.SuppressAnimation = base.isLocal || !newVisibility;
	}

	[PunRPC]
	public void RpcBlockMyPlayersVoice(PhotonPlayer photonPlayer, bool blockRemotePlayersVoice)
	{
		Player player = photonPlayer.ToPlayer();
		if (player != null)
		{
			player.RemoteMute = blockRemotePlayersVoice;
		}
	}

	[PunRPC]
	public void RpcGhostPlayer(PhotonPlayer photonPlayer, bool ghostPlayer, GhostingType ghostingType)
	{
		Player player = photonPlayer.ToPlayer();
		if (player != null)
		{
			switch (ghostingType)
			{
			case GhostingType.Permanent:
				player.RemotePermaGhosted = ghostPlayer;
				break;
			case GhostingType.Panic:
				player.RemotePanicGhosted = ghostPlayer;
				break;
			}
		}
	}

	public static Player FindClosest(Vector3 position, PlayerInteractionRestriction restrictions = null)
	{
		Player result = null;
		float num = float.MaxValue;
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		foreach (PhotonPlayer photonPlayer in playerList)
		{
			if (photonPlayer == null || (restrictions != null && !restrictions.PlayerInteractionAllowed(photonPlayer)))
			{
				continue;
			}
			Player player = photonPlayer.ToPlayer();
			if (!(player == null))
			{
				float sqrMagnitude = (player.Head.transform.position - position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = player;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public static Player Find(int photonPlayerId)
	{
		PhotonPlayer photonPlayer = PhotonPlayer.Find(photonPlayerId);
		if (photonPlayer != null)
		{
			return photonPlayer.TagObject as Player;
		}
		return null;
	}

	public static Player Find(ulong playerId)
	{
		foreach (Player item in All)
		{
			if (item.PlayerId == playerId)
			{
				return item;
			}
		}
		return null;
	}

	public static Player GetFromViewId(int photonViewId)
	{
		PhotonView photonView = PhotonView.Find(photonViewId);
		return (!(photonView == null)) ? photonView.GetComponent<Player>() : null;
	}

	public static Player GetRecRoomPlayer(int playerIndex)
	{
		Player result = null;
		if (playerIndex < PhotonNetwork.playerList.Length)
		{
			result = PhotonNetwork.playerList[playerIndex].TagObject as Player;
		}
		return result;
	}

	public static bool IsCloseToAnyPlayer(Vector3 position, float distance = 2f)
	{
		if (distance <= Mathf.Epsilon)
		{
			return false;
		}
		float num = distance * distance;
		foreach (Player item in All)
		{
			if (item != null && item.IsVisible && (position - item.CurrentFloorPosition).sqrMagnitude <= num)
			{
				return true;
			}
		}
		return false;
	}

	public static float DistanceSqrBetweenPlayers(Player lhs, Player rhs)
	{
		if (lhs == null || rhs == null)
		{
			return float.MaxValue;
		}
		return Vector3.ProjectOnPlane(lhs.Head.transform.position - rhs.Head.transform.position, Vector3.up).sqrMagnitude;
	}
}
