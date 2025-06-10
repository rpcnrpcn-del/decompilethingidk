using System;
using System.Collections;
using System.Collections.Generic;
using RecNet;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenu : MonoBehaviour
{
	[Flags]
	public enum NotificationEffectFlag
	{
		Sound = 1,
		Particle = 2,
		Haptic = 4,
		Text = 8
	}

	[Header("Watch Visual")]
	[SerializeField]
	private ClockFace watchVisual;

	[SerializeField]
	private PooledParticle notificationParticle;

	[Header("Sub Menus")]
	[SerializeField]
	private PlayerSubMenu[] subMenuPrefabs;

	[SerializeField]
	private int defaultSubMenuIndex = 5;

	[Header("Animations")]
	[SerializeField]
	private Transform uiVisualRoot;

	[Header("Audio")]
	[SerializeField]
	private RecRoomAudioClip menuVisibleAudio;

	[SerializeField]
	public RecRoomAudioClip buttonClickAudio;

	[SerializeField]
	private RecRoomAudioClip notificationAudio;

	[Header("Remote network")]
	[SerializeField]
	private Transform remoteVisual;

	private MenuController[] controllers;

	private List<PlayerSubMenu> subMenues;

	private PlayerSubMenuPanic panicSubMenu;

	private PlayerSubMenuMessages messagesSubMenu;

	private PlayerSubMenu peopleSubMenu;

	private AlertMessageMenuController alertMessageMenuController;

	private ConfirmationMenuController confirmationMenuController;

	private bool isDirty;

	private bool _visible;

	private bool _modalDialogVisible;

	private PlayerHand _menuAnchorHand;

	public const NotificationEffectFlag NotificationEffectsAll = (NotificationEffectFlag)(-1);

	public const NotificationEffectFlag NotificationEffectsAllButText = ~NotificationEffectFlag.Text;

	private Animator Animator { get; set; }

	public bool OverrideDefaultMenu { get; set; }

	public bool Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			if (value != _visible)
			{
				_visible = value;
				if (Animator != null)
				{
					Animator.SetTrigger((!_visible) ? "Hide" : "Show");
				}
				uiVisualRoot.gameObject.SetActive(_visible);
				if (ThisPlayer.isLocal)
				{
					ThisPlayer.PlayerUI.BroadcastPlayerMenuVisible(_visible, MenuAnchorHand);
					ThisPlayer.PlayerEvents.MenuVisible(_visible);
				}
				if (_visible)
				{
					AudioManager.Play3DSFX(menuVisibleAudio, base.transform.position);
				}
			}
		}
	}

	private bool modalDialogVisible
	{
		get
		{
			return _modalDialogVisible;
		}
		set
		{
			if (_modalDialogVisible != value)
			{
				_modalDialogVisible = value;
				SetHeaderButtonsInteractable(!value);
			}
		}
	}

	public Player ThisPlayer { get; private set; }

	public PlayerHand MenuAnchorHand
	{
		get
		{
			if (_menuAnchorHand != null && !_menuAnchorHand.IsHandTracking)
			{
				return ThisPlayer.GetOtherHand(_menuAnchorHand.Type);
			}
			return _menuAnchorHand;
		}
		set
		{
			_menuAnchorHand = value;
		}
	}

	public bool IsAlertMessageActive
	{
		get
		{
			return alertMessageMenuController != null && !alertMessageMenuController.Dismissable;
		}
	}

	public bool AttachedToWrist { get; set; }

	private void Awake()
	{
		Animator = base.gameObject.GetComponent<Animator>();
		ThisPlayer = base.gameObject.GetComponentInParents<Player>();
		watchVisual.gameObject.SetActive(false);
		uiVisualRoot.gameObject.SetActive(false);
		AttachedToWrist = true;
	}

	private void Start()
	{
		bool isLocal = ThisPlayer.isLocal;
		remoteVisual.gameObject.SetActive(!isLocal);
		MenuAnchorHand = ThisPlayer.GetOtherHand(ThisPlayer.DominantHand.Type);
		if (isLocal)
		{
			CreateLocalUserMenues();
			if (SingletonMonoBehaviour<SessionManager>.Instance != null && !string.IsNullOrEmpty(SingletonMonoBehaviour<SessionManager>.Instance.AlertMessage))
			{
				ShowAlertMessage(SingletonMonoBehaviour<SessionManager>.Instance.AlertTitle, SingletonMonoBehaviour<SessionManager>.Instance.AlertMessage);
				SingletonMonoBehaviour<SessionManager>.Instance.AlertTitle = null;
				SingletonMonoBehaviour<SessionManager>.Instance.AlertMessage = null;
			}
		}
	}

	private void Update()
	{
		if (MenuAnchorHand != null)
		{
			if (isDirty)
			{
				Refresh();
			}
			if (AttachedToWrist)
			{
				base.transform.position = MenuAnchorHand.WatchMenuOrigin.position;
				base.transform.rotation = MenuAnchorHand.WatchMenuOrigin.rotation;
			}
			watchVisual.gameObject.SetActive(MenuAnchorHand.IsVisible);
			watchVisual.transform.position = MenuAnchorHand.WatchAnchor.position;
			watchVisual.transform.rotation = MenuAnchorHand.WatchAnchor.rotation;
		}
		modalDialogVisible = (confirmationMenuController != null && confirmationMenuController.Visible) || (alertMessageMenuController != null && alertMessageMenuController.Visible);
	}

	private void CreateLocalUserMenues()
	{
		subMenues = new List<PlayerSubMenu>();
		PlayerSubMenu[] array = subMenuPrefabs;
		foreach (PlayerSubMenu original in array)
		{
			PlayerSubMenu playerSubMenu = UnityEngine.Object.Instantiate(original);
			playerSubMenu.transform.SetParent(uiVisualRoot, false);
			playerSubMenu.transform.localPosition = Vector3.zero;
			playerSubMenu.transform.localRotation = Quaternion.identity;
			playerSubMenu.PlayerMenu = this;
			subMenues.Add(playerSubMenu);
			if (!(playerSubMenu.Controller != null))
			{
				continue;
			}
			if (playerSubMenu.Controller is MessagesMenuController)
			{
				messagesSubMenu = playerSubMenu as PlayerSubMenuMessages;
				if (messagesSubMenu != null)
				{
					((MessagesMenuController)messagesSubMenu.Controller).MessageReceived += OnMessageReceived;
				}
			}
			else if (playerSubMenu.Controller is PartyMenuController)
			{
				peopleSubMenu = playerSubMenu;
			}
			else if (playerSubMenu.Controller is AlertMessageMenuController)
			{
				alertMessageMenuController = playerSubMenu.Controller as AlertMessageMenuController;
			}
			else if (playerSubMenu.Controller is PanicMenuController)
			{
				panicSubMenu = playerSubMenu as PlayerSubMenuPanic;
			}
			else if (playerSubMenu.Controller is ConfirmationMenuController)
			{
				confirmationMenuController = playerSubMenu.Controller as ConfirmationMenuController;
			}
		}
		controllers = uiVisualRoot.GetComponentsInChildren<MenuController>(true);
		if (controllers != null)
		{
			MenuController[] array2 = controllers;
			foreach (MenuController menuController in array2)
			{
				menuController.Initialize(this);
			}
		}
		RegisterWithViveInput();
		Button[] componentsInChildren = uiVisualRoot.GetComponentsInChildren<Button>(true);
		Button[] array3 = componentsInChildren;
		foreach (Button button in array3)
		{
			button.onClick.AddListener(Button_PlayVFX);
		}
		Toggle[] componentsInChildren2 = uiVisualRoot.GetComponentsInChildren<Toggle>(true);
		Toggle[] array4 = componentsInChildren2;
		foreach (Toggle toggle in array4)
		{
			toggle.onValueChanged.AddListener(Toggle_PlayVFX);
		}
	}

	private void RegisterWithViveInput()
	{
		Canvas[] componentsInChildren = GetComponentsInChildren<Canvas>(true);
		if (componentsInChildren != null && componentsInChildren.Length > 0 && ViveControllerInput.Instance != null)
		{
			Canvas[] array = componentsInChildren;
			foreach (Canvas canvas in array)
			{
				canvas.worldCamera = ViveControllerInput.Instance.ControllerCamera;
			}
		}
		else
		{
			Debug.LogError(string.Concat(base.gameObject.GetGameObjectHierarchy(), " failed to RegisterWithViveInput. ViveControllerInput.Instance : ", ViveControllerInput.Instance, " - Canvas count : ", (componentsInChildren != null) ? componentsInChildren.Length : 0));
		}
	}

	private void ShowMessagesSubMenu()
	{
		if (messagesSubMenu != null)
		{
			messagesSubMenu.Selected = true;
			((MessagesMenuController)messagesSubMenu.Controller).ShowLatestMessage();
			OverrideDefaultMenu = true;
		}
	}

	public void SetHeaderButtonsInteractable(bool interactable)
	{
		if (subMenues == null)
		{
			return;
		}
		foreach (PlayerSubMenu subMenue in subMenues)
		{
			subMenue.Interactable = interactable;
		}
	}

	public void ShowAlertMessage(string title, string message, bool persistent = false, NotificationEffectFlag notificationEffects = ~NotificationEffectFlag.Text, string notificationText = null)
	{
		if (!Visible)
		{
			PlayIncomingNotificationFeedback(notificationEffects, notificationText);
		}
		else
		{
			notificationEffects &= NotificationEffectFlag.Sound | NotificationEffectFlag.Haptic;
			PlayIncomingNotificationFeedback(notificationEffects);
		}
		alertMessageMenuController.ShowAlertMessage(title, message, persistent);
	}

	public void StopShowAlertMessage()
	{
		alertMessageMenuController.Dismissable = true;
	}

	public ConfirmationMenuController ShowConfirmation(string title, string body, string yesButton = "Ok", string noButton = "Cancel", NotificationEffectFlag notificationEffects = ~NotificationEffectFlag.Text, string notificationText = null)
	{
		if (!Visible)
		{
			PlayIncomingNotificationFeedback(notificationEffects, notificationText);
		}
		else
		{
			PlayIncomingNotificationFeedback(NotificationEffectFlag.Sound | NotificationEffectFlag.Haptic);
		}
		confirmationMenuController.ShowConfirmation(title, body, yesButton, noButton);
		MarkDirty();
		return confirmationMenuController;
	}

	public void ShowDefaultTab()
	{
		if (IsAlertMessageActive)
		{
			alertMessageMenuController.Visible = true;
		}
		else if (ThisPlayer.SituationPulse.Active)
		{
			panicSubMenu.Selected = true;
		}
		else if (defaultSubMenuIndex < subMenues.Count)
		{
			subMenues[defaultSubMenuIndex].Selected = true;
		}
	}

	public void ShowMenu(PlayerHand playerHand = null, bool show = true)
	{
		if (show)
		{
			if (!Visible && playerHand != null)
			{
				MenuAnchorHand = playerHand;
				if (!OverrideDefaultMenu)
				{
					ShowDefaultTab();
				}
				MarkDirty();
				Visible = true;
				AttachedToWrist = true;
				OverrideDefaultMenu = false;
			}
			return;
		}
		MenuController[] array = controllers;
		foreach (MenuController menuController in array)
		{
			if (menuController.Visible && menuController.ShowSubMenu(false))
			{
				menuController.Refresh();
				return;
			}
		}
		Visible = false;
	}

	public void ShowRemotePlayerMenu(Player player)
	{
		if (peopleSubMenu != null)
		{
			peopleSubMenu.Selected = true;
			(peopleSubMenu.Controller as PartyMenuController).ShowPlayerMenu(player);
		}
	}

	public void PlayIncomingNotificationFeedback(NotificationEffectFlag effectsToPlay = (NotificationEffectFlag)(-1), string textOverride = null, string textOverrideSubtitle = null)
	{
		if (MenuAnchorHand == null)
		{
			Debug.LogError("PlayerMenu notification feedback tried to play with a null AnchorHand.");
			return;
		}
		if ((effectsToPlay & NotificationEffectFlag.Sound) == NotificationEffectFlag.Sound)
		{
			AudioManager.Play3DSFX(notificationAudio, MenuAnchorHand.transform.position);
		}
		if ((effectsToPlay & NotificationEffectFlag.Particle) == NotificationEffectFlag.Particle)
		{
			PooledParticle pooledParticle = ObjectPool.Instance.Acquire(notificationParticle);
			if (pooledParticle != null)
			{
				pooledParticle.transform.position = MenuAnchorHand.transform.position;
				pooledParticle.transform.rotation = Quaternion.identity;
				pooledParticle.Play();
			}
		}
		if ((effectsToPlay & NotificationEffectFlag.Text) == NotificationEffectFlag.Text)
		{
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Medium, string.IsNullOrEmpty(textOverride) ? "New notification!" : textOverride, string.IsNullOrEmpty(textOverrideSubtitle) ? "Check your watch..." : textOverrideSubtitle, 3f);
		}
		if ((effectsToPlay & NotificationEffectFlag.Haptic) == NotificationEffectFlag.Haptic)
		{
			StartCoroutine(NotificationVibrationCoroutine(MenuAnchorHand));
		}
	}

	private IEnumerator NotificationVibrationCoroutine(PlayerHand handToVibrate)
	{
		yield return new WaitForSeconds(0.1f);
		handToVibrate.Vibrate(450, 1000);
		yield return new WaitForSeconds(0.1f);
		handToVibrate.Vibrate(450, 1000);
	}

	public void MarkDirty()
	{
		isDirty = true;
	}

	private void Refresh()
	{
		if (!isDirty)
		{
			return;
		}
		if (subMenues != null)
		{
			foreach (PlayerSubMenu subMenue in subMenues)
			{
				if (subMenue.Selected || (subMenue.Controller != null && subMenue.Controller.Visible))
				{
					subMenue.Refresh();
				}
			}
		}
		isDirty = false;
	}

	public void RunSwitchActivity(string activity, bool createPrivateRoom)
	{
		if (RecRoomSceneManager.Instance != null)
		{
			if (ThisPlayer.PlayerParty.PartySize > 1 && confirmationMenuController != null)
			{
				string empty = string.Empty;
				string empty2 = string.Empty;
				string empty3 = string.Empty;
				string empty4 = string.Empty;
				if (activity == "dormroom")
				{
					empty = "Go to your Dorm Room?";
					empty2 = "You will leave your current party!";
					empty3 = "OK";
					empty4 = "Cancel";
				}
				else
				{
					string activityFriendlyName = PUNNetworkManager.Instance.GetActivityFriendlyName(activity);
					empty = ((!string.IsNullOrEmpty(activityFriendlyName)) ? ("Go to " + activityFriendlyName) : "Go to new activity");
					empty2 = "Do you want to invite your party to go with you?";
					empty3 = "Yes";
					empty4 = "No";
				}
				ShowConfirmation(empty, empty2, empty3, empty4);
				confirmationMenuController.Data = new object[2] { activity, createPrivateRoom };
				confirmationMenuController.ConfirmAction += LoadSceneConfirmation;
			}
			else
			{
				RecRoomSceneManager.Instance.SwitchActivity(activity, createPrivateRoom);
				Visible = false;
			}
		}
		else
		{
			Debug.LogError("Button_LoadScene '" + activity + "', RecRoomSceneManager is null.");
		}
	}

	private void LoadSceneConfirmation(ConfirmationMenuController confirmMenu)
	{
		confirmationMenuController.ConfirmAction -= LoadSceneConfirmation;
		string text = confirmMenu.Data[0] as string;
		bool createPrivateRoom = (bool)confirmMenu.Data[1];
		if (!confirmMenu.Confirm.HasValue)
		{
			return;
		}
		if (text == "dormroom")
		{
			if (confirmMenu.Confirm.Value)
			{
				ThisPlayer.PlayerParty.LeaveCurrentParty();
				RecRoomSceneManager.Instance.SwitchActivity(text, createPrivateRoom);
				Visible = false;
			}
		}
		else
		{
			if (!confirmMenu.Confirm.Value)
			{
				ThisPlayer.PlayerParty.LeaveCurrentParty();
			}
			RecRoomSceneManager.Instance.SwitchActivity(text, createPrivateRoom);
			Visible = false;
		}
	}

	public void RunJoinPlayer(ulong playerId)
	{
		if (RecRoomSceneManager.Instance != null)
		{
			if (ThisPlayer.PlayerParty.PartySize > 1 && confirmationMenuController != null)
			{
				Profile profileFromCache = Profiles.GetProfileFromCache(playerId);
				string value = ((profileFromCache == null) ? null : profileFromCache.DisplayName);
				string title = (string.IsNullOrEmpty(value) ? "Join player" : string.Format("Join {0}", value.Truncate(16, "...")));
				string body = "Do you want to invite your party to go with you?";
				string yesButton = "Yes";
				string noButton = "No";
				ShowConfirmation(title, body, yesButton, noButton);
				confirmationMenuController.Data = new object[1] { playerId };
				confirmationMenuController.ConfirmAction += JoinPlayerConfirmation;
			}
			else
			{
				RecRoomSceneManager.Instance.JoinPlayer(playerId);
				Visible = false;
			}
		}
		else
		{
			Debug.LogError("Button_JoinPlayer '" + playerId + "', RecRoomSceneManager is null.");
		}
	}

	private void JoinPlayerConfirmation(ConfirmationMenuController confirmMenu)
	{
		confirmationMenuController.ConfirmAction -= JoinPlayerConfirmation;
		ulong playerId = (ulong)confirmMenu.Data[0];
		if (confirmMenu.Confirm.HasValue)
		{
			if (!confirmMenu.Confirm.Value)
			{
				ThisPlayer.PlayerParty.LeaveCurrentParty();
			}
			RecRoomSceneManager.Instance.JoinPlayer(playerId);
			Visible = false;
		}
	}

	private void Button_PlayVFX()
	{
		AudioManager.Play3DSFX(buttonClickAudio, base.transform.position);
	}

	private void Toggle_PlayVFX(bool value)
	{
		AudioManager.Play3DSFX(buttonClickAudio, base.transform.position);
	}

	private void Slider_PlayVFX(float value)
	{
		AudioManager.Play3DSFX(buttonClickAudio, base.transform.position);
	}

	private void OnMessageReceived(MessagesMenuController controller)
	{
		if (!Visible)
		{
			ShowMessagesSubMenu();
			PlayIncomingNotificationFeedback();
		}
		else
		{
			PlayIncomingNotificationFeedback(NotificationEffectFlag.Sound | NotificationEffectFlag.Haptic);
		}
	}
}
