using System;
using System.Collections;
using Photon;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerUI : Photon.MonoBehaviour
{
	[SerializeField]
	private Transform positionGlow;

	[SerializeField]
	private float positionGlowVisibleDuration = 0.5f;

	[SerializeField]
	private float positionGlowHeightOffset = 1f;

	[SerializeField]
	private Text nameLabelText;

	[SerializeField]
	private Text guestLabelText;

	[SerializeField]
	private Text levelText;

	[SerializeField]
	private Image levelTextBackground;

	[SerializeField]
	private float nameLabelTextFadeSpeed = 15f;

	[SerializeField]
	private float nameHighlightAlphaMin = 0.15f;

	[SerializeField]
	private float nameHighlightAlphaMax = 0.95f;

	[SerializeField]
	private float nameHighlightOnTeamAlphaMin = 0.85f;

	[SerializeField]
	private float nameHighlightOnTeamAlphaMax = 1f;

	[SerializeField]
	private PlayerMenu menu;

	[SerializeField]
	private ParticleSystem highFiveParticles;

	[SerializeField]
	private ParticleSystem fistBumpParticles;

	[Header("Handshake")]
	[SerializeField]
	private ParticleSystem handshakeChargeParticles;

	[SerializeField]
	private ParticleSystem handshakeSuccessParticles;

	[SerializeField]
	private RecRoomAudioClip handshakeChargeAudio;

	[SerializeField]
	private RecRoomAudioClip handshakeSuccessAudio;

	[SerializeField]
	private UIPlayerCamera uIPlayerCameraPrefab;

	[Range(0.1f, 1f)]
	[SerializeField]
	private float lookAtWristSensitivity = 0.9f;

	[Range(0.1f, 1f)]
	[SerializeField]
	private float menuDismissSensitivity = 0.25f;

	[SerializeField]
	private float requiredGhostButtonPressTime = 1f;

	private Player thisPlayer;

	private GameManager gameManager;

	private SFXAudioSource runningHandshakeAudio;

	private ParticleSystem runningHandshakeParticles;

	private Coroutine handshakeLoopingFeedbackCoroutine;

	private PlayerHand ghostButtonHeldHand;

	private float ghostButtonPressedStartTime;

	private bool _nameVisible;

	private float nameHighlightTargetAlpha = 1f;

	private bool _nameHighlighted;

	private float lastglowVisibleTime;

	private bool _glowVisible;

	public UIPlayerCamera UIPlayerCamera { get; private set; }

	public PlayerMenu Menu
	{
		get
		{
			return menu;
		}
	}

	public Transform LabelTransform
	{
		get
		{
			return nameLabelText.rectTransform.parent;
		}
	}

	public bool NameVisible
	{
		get
		{
			return _nameVisible;
		}
		set
		{
			_nameVisible = value;
			nameLabelText.transform.parent.gameObject.SetActive(_nameVisible);
		}
	}

	private bool NameHighlighted
	{
		get
		{
			return _nameHighlighted;
		}
		set
		{
			_nameHighlighted = value;
			if (PlayerTeam == GameTeam.INVALID)
			{
				nameHighlightTargetAlpha = ((!value) ? nameHighlightAlphaMin : nameHighlightAlphaMax);
			}
			else
			{
				nameHighlightTargetAlpha = ((!value) ? nameHighlightOnTeamAlphaMin : nameHighlightOnTeamAlphaMax);
			}
		}
	}

	public bool GlowVisible
	{
		get
		{
			return _glowVisible;
		}
		set
		{
			if (value != _glowVisible)
			{
				bool flag = gameManager == null || gameManager.CurrentState != GameStates.GAME_RUNNING || gameManager.TeamManager.PlayersAreTeammates(PhotonNetwork.player, thisPlayer.PhotonPlayer);
				_glowVisible = value && flag;
				positionGlow.gameObject.SetActive(_glowVisible);
			}
			if (_glowVisible)
			{
				lastglowVisibleTime = Time.time;
			}
		}
	}

	private GameTeam PlayerTeam
	{
		get
		{
			if (thisPlayer == null || gameManager == null || gameManager.CurrentState != GameStates.GAME_RUNNING)
			{
				return GameTeam.INVALID;
			}
			return gameManager.TeamManager.GetPlayerTeam(thisPlayer.PhotonPlayer);
		}
	}

	public bool MenuInteractionAllowed { get; set; }

	private bool ShouldDismissMenu
	{
		get
		{
			bool flag = Menu.AttachedToWrist && (!Menu.MenuAnchorHand.IsVisible || StoppedLookingAtWrist());
			bool flag2 = !Menu.AttachedToWrist && (thisPlayer.LeftHand.ControllerIO.TeleportButtonUp || thisPlayer.RightHand.ControllerIO.TeleportButtonUp || Menu.MenuAnchorHand.ControllerIO.MenuButtonUp);
			return flag || flag2;
		}
	}

	private bool IsGhostButtonDown(PlayerHand hand)
	{
		return SingletonMonoBehaviour<CameraRig>.IsInitialized && hand.ThisPlayer.isLocal && hand.ControllerIO != null && hand.ControllerIO.MenuButtonDown;
	}

	private bool IsGhostButtonPressed(PlayerHand hand)
	{
		return SingletonMonoBehaviour<CameraRig>.IsInitialized && hand.ThisPlayer.isLocal && hand.ControllerIO != null && hand.ControllerIO.MenuButtonPressed;
	}

	private PlayerHand IsLookingAtWrist()
	{
		PlayerHand playerHand = null;
		if (IsLookingAtWrist(thisPlayer.LeftHand))
		{
			playerHand = thisPlayer.LeftHand;
		}
		if (playerHand == null && IsLookingAtWrist(thisPlayer.RightHand))
		{
			playerHand = thisPlayer.RightHand;
		}
		return playerHand;
	}

	private bool IsLookingAtWrist(PlayerHand wrist)
	{
		bool result = false;
		if (wrist != null)
		{
			Vector3 forward = wrist.WatchMenuOrigin.forward;
			Vector3 up = wrist.WatchMenuOrigin.up;
			Vector3 forward2 = thisPlayer.Head.transform.forward;
			Vector3 up2 = thisPlayer.Head.transform.up;
			Vector3 normalized = (wrist.WatchMenuOrigin.position - thisPlayer.Head.transform.position).normalized;
			result = Vector3.Dot(forward2, normalized) > lookAtWristSensitivity && Vector3.Dot(forward, normalized) > lookAtWristSensitivity && Vector3.Dot(up2, up) > lookAtWristSensitivity;
		}
		return result;
	}

	private bool StoppedLookingAtWrist()
	{
		bool result = false;
		if (Menu.MenuAnchorHand != null)
		{
			Vector3 forward = Menu.MenuAnchorHand.WatchMenuOrigin.forward;
			Vector3 forward2 = thisPlayer.Head.transform.forward;
			Vector3 normalized = (Menu.MenuAnchorHand.WatchMenuOrigin.position - thisPlayer.Head.transform.position).normalized;
			result = Vector3.Dot(forward2, normalized) < menuDismissSensitivity || Vector3.Dot(forward, normalized) < menuDismissSensitivity;
		}
		return result;
	}

	[Obsolete("Use the PlayerMenu system")]
	public void HideMenu()
	{
		throw new NotImplementedException();
	}

	protected override void Awake()
	{
		base.Awake();
		thisPlayer = GetComponent<Player>();
		thisPlayer.VisibilityChanged += ThisPlayer_VisibilityChanged;
		NameVisible = false;
		MenuInteractionAllowed = true;
	}

	private void Start()
	{
		if (base.isLocal)
		{
			thisPlayer.PlayerParty.PartyUpdateReceived += PlayerParty_PartyUpdateReceived;
			SingletonMonoBehaviour<CameraRig>.Instance.HeadGestureDetect.GestureDetected += HeadGestureDetect_GestureDetected;
			NameVisible = false;
			UIPlayerCamera = UnityEngine.Object.Instantiate(uIPlayerCameraPrefab);
			thisPlayer.SetParentPlayerRoot(UIPlayerCamera.transform);
		}
		else if (Player.LocalPlayer != null && Player.LocalPlayer.PlayerUI.Menu != null && Player.LocalPlayer.PlayerUI.Menu.Visible)
		{
			Player.LocalPlayer.PlayerUI.Menu.MarkDirty();
		}
		gameManager = RecRoomSceneManager.Instance.GameManager;
		thisPlayer.LeftHand.Gestures.GestureDetected += PlayerHandGestures_GestureDetected;
		thisPlayer.RightHand.Gestures.GestureDetected += PlayerHandGestures_GestureDetected;
	}

	private void HeadGestureDetect_GestureDetected(HeadGestureDetect.Gesture gesture)
	{
	}

	private void PlayerHandGestures_GestureDetected(PlayerHand myHand, PlayerHand otherHand, PlayerHandGestures.Gesture gesture)
	{
		switch (gesture)
		{
		case PlayerHandGestures.Gesture.Highfive:
		{
			Vector3 position2 = (myHand.transform.position + otherHand.transform.position) / 2f;
			thisPlayer.PlayerAudio.OnHighFive(position2);
			if (highFiveParticles != null)
			{
				ParticleSystem particleSystem2 = UnityEngine.Object.Instantiate(highFiveParticles);
				particleSystem2.transform.position = position2;
				particleSystem2.Play();
				UnityEngine.Object.Destroy(particleSystem2.gameObject, particleSystem2.main.duration);
			}
			break;
		}
		case PlayerHandGestures.Gesture.Fistbump:
		{
			Vector3 position = (myHand.transform.position + otherHand.transform.position) / 2f;
			thisPlayer.PlayerAudio.OnFistBump(position);
			if (fistBumpParticles != null)
			{
				ParticleSystem particleSystem = UnityEngine.Object.Instantiate(fistBumpParticles);
				particleSystem.transform.position = position;
				particleSystem.Play();
				UnityEngine.Object.Destroy(particleSystem.gameObject, particleSystem.main.duration);
			}
			break;
		}
		case PlayerHandGestures.Gesture.HandshakeStart:
			StartHandshakeFeedback(myHand, otherHand);
			break;
		case PlayerHandGestures.Gesture.HandshakeCancel:
			StopHandshakeFeedback();
			break;
		case PlayerHandGestures.Gesture.HandshakeSuccess:
			StopHandshakeFeedback();
			PlayHandshakeSuccessParticles(myHand, otherHand);
			break;
		}
		if (base.isLocal)
		{
			myHand.Vibrate(100, 1000);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (thisPlayer != null)
		{
			thisPlayer.VisibilityChanged -= ThisPlayer_VisibilityChanged;
			if (base.isLocal)
			{
				thisPlayer.PlayerParty.PartyUpdateReceived -= PlayerParty_PartyUpdateReceived;
			}
		}
	}

	private void Update()
	{
		if (base.isLocal && MenuInteractionAllowed)
		{
			UpdateGhostButton();
			if (!Menu.Visible)
			{
				PlayerHand playerHand = IsLookingAtWrist();
				if (playerHand != null && playerHand.IsVisible)
				{
					Menu.ShowMenu(playerHand);
				}
				if (ControllerIO.CurrentInputMode == ControllerIO.InputMode.Debug)
				{
					PlayerHand playerHand2 = null;
					if (thisPlayer.LeftHand.Tool == null && thisPlayer.LeftHand.ControllerIO != null && thisPlayer.LeftHand.ControllerIO.MenuButtonUp)
					{
						playerHand2 = thisPlayer.LeftHand;
					}
					else if (thisPlayer.RightHand.Tool == null && thisPlayer.RightHand.ControllerIO != null && thisPlayer.RightHand.ControllerIO.MenuButtonUp)
					{
						playerHand2 = thisPlayer.RightHand;
					}
					if (playerHand2 != null)
					{
						Menu.ShowMenu(playerHand2);
						Menu.AttachedToWrist = false;
						Menu.transform.position = SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.TransformPoint(0f, -0.1f, 0.7f);
						Menu.transform.rotation = SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.rotation;
					}
				}
			}
			else if (Menu.MenuAnchorHand != null)
			{
				if (ShouldDismissMenu)
				{
					Menu.ShowMenu(null, false);
				}
				else if (Menu.AttachedToWrist && Menu.MenuAnchorHand.ControllerIO.MenuButtonUp)
				{
					Menu.AttachedToWrist = false;
				}
			}
		}
		if (!base.isLocal)
		{
			NameHighlighted = thisPlayer.IsTalking;
			NameVisible = SingletonMonoBehaviour<SettingsManager>.Instance.ShowNames;
			guestLabelText.gameObject.SetActive(!thisPlayer.PlayerProgression.IsMember);
		}
		GameTeam gameTeam = ((!(thisPlayer != null)) ? GameTeam.INVALID : thisPlayer.Team);
		if (NameVisible && base.owner != null)
		{
			if (nameLabelText.text != base.owner.name)
			{
				nameLabelText.text = base.owner.name;
			}
			levelText.text = thisPlayer.PlayerProgression.Level.ToString();
			Color color = Color.white;
			if (gameTeam != GameTeam.INVALID)
			{
				color = GameTeamSettings.GetTeamColor(gameTeam);
			}
			else if (thisPlayer.PlayerParty.PartyColor.HasValue)
			{
				color = thisPlayer.PlayerParty.PartyColor.Value;
			}
			color.a = nameLabelText.color.a;
			if (color.a != nameHighlightTargetAlpha)
			{
				color.a = Mathf.SmoothStep(color.a, nameHighlightTargetAlpha, Time.deltaTime * nameLabelTextFadeSpeed);
			}
			nameLabelText.color = color;
			if (guestLabelText.gameObject.activeSelf)
			{
				guestLabelText.color = color;
			}
			levelText.color = color;
			levelTextBackground.color = color;
		}
		if (gameTeam != GameTeam.INVALID)
		{
			thisPlayer.PlayerOutfit.TeamIndicator = GameTeamSettings.GetTeamColor(gameTeam);
		}
		else
		{
			thisPlayer.PlayerOutfit.TeamIndicator = null;
		}
		if (GlowVisible)
		{
			if (Time.time - lastglowVisibleTime > positionGlowVisibleDuration)
			{
				GlowVisible = false;
				return;
			}
			positionGlow.transform.rotation = Quaternion.LookRotation(Vector3.forward);
			positionGlow.transform.position = thisPlayer.Head.transform.position + Vector3.up * positionGlowHeightOffset;
		}
	}

	private void UpdateGhostButton()
	{
		if (ghostButtonHeldHand == null)
		{
			if (IsGhostButtonDown(thisPlayer.LeftHand))
			{
				ghostButtonHeldHand = thisPlayer.LeftHand;
				ghostButtonPressedStartTime = Time.time;
			}
			else if (IsGhostButtonDown(thisPlayer.RightHand))
			{
				ghostButtonHeldHand = thisPlayer.RightHand;
				ghostButtonPressedStartTime = Time.time;
			}
			return;
		}
		bool flag = Time.time - ghostButtonPressedStartTime >= requiredGhostButtonPressTime;
		if (flag || !IsGhostButtonPressed(ghostButtonHeldHand))
		{
			ghostButtonHeldHand = null;
			if (flag && thisPlayer.GhostModeEnabled)
			{
				thisPlayer.SituationPulse.TogglePanic();
			}
		}
	}

	private void ThisPlayer_VisibilityChanged(bool visible)
	{
		if (Menu != null && Menu.Visible)
		{
			Menu.Visible = false;
		}
	}

	private void PlayerParty_PartyUpdateReceived()
	{
		if (Menu != null && Menu.Visible)
		{
			Menu.MarkDirty();
		}
	}

	private void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		if (base.isLocal && Menu != null && Menu.Visible)
		{
			Menu.MarkDirty();
		}
	}

	private void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
	{
		if (base.isLocal && Menu != null && Menu.Visible)
		{
			Menu.MarkDirty();
		}
	}

	public void BroadcastPlayerMenuVisible(bool visible, PlayerHand anchorHand)
	{
		if (base.isLocal && thisPlayer.IsVisible)
		{
			base.photonView.RPC("RpcPlayerMenuVisible", PhotonTargets.Others, visible, (int)((anchorHand != null) ? anchorHand.Type : PlayerHand.HandType.Left));
		}
	}

	private void StartHandshakeFeedback(PlayerHand thisHand, PlayerHand otherHand)
	{
		StopHandshakeFeedback();
		handshakeLoopingFeedbackCoroutine = StartCoroutine(HandshakeFeedbackCoroutine(thisHand, otherHand));
	}

	private void StopHandshakeFeedback()
	{
		if (runningHandshakeAudio != null)
		{
			runningHandshakeAudio.Stop();
			runningHandshakeAudio = null;
		}
		if (runningHandshakeParticles != null)
		{
			UnityEngine.Object.Destroy(runningHandshakeParticles.gameObject);
		}
		if (handshakeLoopingFeedbackCoroutine != null)
		{
			StopCoroutine(handshakeLoopingFeedbackCoroutine);
		}
	}

	private IEnumerator HandshakeFeedbackCoroutine(PlayerHand thisHand, PlayerHand otherHand)
	{
		runningHandshakeAudio = AudioManager.Play3DSFX(handshakeChargeAudio, Vector3.Lerp(thisHand.transform.position, otherHand.transform.position, 0.5f));
		runningHandshakeParticles = UnityEngine.Object.Instantiate(handshakeChargeParticles);
		runningHandshakeParticles.Play();
		float timer = 0f;
		while (timer < handshakeChargeParticles.main.duration)
		{
			runningHandshakeParticles.transform.position = Vector3.Lerp(thisHand.transform.position, otherHand.transform.position, 0.5f);
			timer += Time.deltaTime;
			yield return null;
		}
		UnityEngine.Object.Destroy(runningHandshakeParticles.gameObject);
		runningHandshakeParticles = null;
	}

	private void PlayHandshakeSuccessParticles(PlayerHand thisHand, PlayerHand otherHand)
	{
		Vector3 vector = Vector3.Lerp(thisHand.transform.position, otherHand.transform.position, 0.5f);
		AudioManager.Play3DSFX(handshakeSuccessAudio, vector);
		if (handshakeSuccessParticles != null)
		{
			ParticleSystem particleSystem = UnityEngine.Object.Instantiate(handshakeSuccessParticles);
			particleSystem.transform.position = vector;
			particleSystem.Play();
			UnityEngine.Object.Destroy(particleSystem.gameObject, particleSystem.main.duration);
		}
	}

	[PunRPC]
	public void RpcPlayerMenuVisible(bool visible, int anchorHandId)
	{
		if (Menu != null)
		{
			Menu.MenuAnchorHand = thisPlayer.GetHand(anchorHandId);
			Menu.Visible = visible && thisPlayer.IsVisible;
		}
	}
}
