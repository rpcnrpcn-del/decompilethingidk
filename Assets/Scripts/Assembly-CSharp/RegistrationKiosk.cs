using System.Collections;
using System.Collections.Generic;
using Photon;
using RecNet;
using UnityEngine;
using UnityEngine.UI;

public class RegistrationKiosk : Photon.MonoBehaviour
{
	private enum KioskState
	{
		None = 0,
		DownloadingProfile = 1,
		SignupFlowStart = 2,
		TakingSelfie = 3,
		VerifyingSelfie = 4,
		SelfieUpdated = 5,
		PendingRegistration = 6,
		PendingProfileVerification = 7,
		RegistrationComplete = 8
	}

	private enum KioskUIState
	{
		None = 0,
		EnterEmail = 1,
		SendingEmail = 2,
		EmailSent = 3,
		Error = 4
	}

	public static List<RegistrationKiosk> All = new List<RegistrationKiosk>();

	[Header("Settings")]
	[SerializeField]
	[Tooltip("How long before the kiosk timeouts and resets.")]
	private float kioskTimeoutSeconds = 45f;

	[SerializeField]
	[Tooltip("How far does user need to be from the kiosk for the reset to happen.")]
	private float kioskTimeoutPlayerDistance = 4f;

	[Header("Kiosk Canvas")]
	[SerializeField]
	private Canvas kioskCanvas;

	[SerializeField]
	private Button submitButton;

	[SerializeField]
	private Text submitButtonText;

	[SerializeField]
	private Button cancelButton;

	[SerializeField]
	private Text cancelButtonText;

	[SerializeField]
	private Text inUseText;

	[SerializeField]
	private Transform notificationRoot;

	[Header("Screen")]
	[SerializeField]
	private Texture2D signupTexture;

	[SerializeField]
	private Texture2D signupFlowTexture;

	[SerializeField]
	private Texture2D updateSelfieTexture;

	[SerializeField]
	private Texture2D takingSelfieRemoteTexture;

	[SerializeField]
	private Texture2D checkPCTexture;

	[SerializeField]
	private Texture2D checkEmailTexture;

	[SerializeField]
	private Texture2D remoteCheckPCTexture;

	[SerializeField]
	private Texture2D selfieUpdatedTexture;

	[SerializeField]
	private Texture2D registrationCompleteTexture;

	[Header("Selfie-Camera")]
	[SerializeField]
	private Renderer selfieCamRenderer;

	[SerializeField]
	private Transform cameraLensTransform;

	[SerializeField]
	private UIPlayerCamera selfieCameraPrefab;

	private UIPlayerCamera selfieCam;

	[SerializeField]
	private int numDownsamples = 4;

	[SerializeField]
	private Vector2 cameraResolution = new Vector2(1080f, 1080f);

	[SerializeField]
	private Vector2 imageSize = new Vector2(256f, 256f);

	[Header("Camera Flash")]
	[SerializeField]
	private Animator cameraFlashAnimator;

	[Header("Audio")]
	[SerializeField]
	private RecRoomAudioClip selfieCountdownBeep;

	[SerializeField]
	private RecRoomAudioClip selfieCamShutter;

	[SerializeField]
	private RecRoomAudioClip buttonPressNext;

	[SerializeField]
	private RecRoomAudioClip buttonPressCancel;

	[SerializeField]
	private RecRoomAudioClip cardDispense;

	[SerializeField]
	private RecRoomAudioClip cardDispenseWarmup;

	[SerializeField]
	private RecRoomAudioClip takeYourCard;

	[SerializeField]
	private RecRoomAudioClip modemSound;

	[SerializeField]
	private RecRoomAudioClip powerupAmbience;

	[SerializeField]
	private RecRoomAudioClip stateChangeClip;

	[SerializeField]
	private AudioSource powerupAmbienceLoopSource;

	[Header("2D Canvas")]
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private InputField emailInput;

	[SerializeField]
	private Button emailSubmitButton;

	[SerializeField]
	private Button tryAgainButton;

	[SerializeField]
	private Button uiOkButton;

	[SerializeField]
	private Button uiCancelButton;

	[SerializeField]
	private Text informationText;

	[SerializeField]
	private RawImage returnToVRImage;

	private Coroutine mainFlowCoroutine;

	private bool submitButtonPressed;

	private float lastTimeKioskStateChanged;

	private SynchronizedField<int> _kioskState;

	private string submittedEmail;

	private string errorMessage = string.Empty;

	private KioskUIState _kioskUIState;

	private SynchronizedField<int> _kioskHolderId;

	private KioskState kioskState
	{
		get
		{
			return (KioskState)_kioskState.Get();
		}
		set
		{
			if (value != (KioskState)_kioskState.Get())
			{
				_kioskState.CompareAndSwapSet((int)value);
			}
		}
	}

	private KioskUIState kioskUIState
	{
		get
		{
			return _kioskUIState;
		}
		set
		{
			if (_kioskUIState != value && value == KioskUIState.SendingEmail)
			{
				AudioManager.Play3DSFX(modemSound, notificationRoot.position);
			}
			_kioskUIState = value;
			UpdateKioskUIState();
		}
	}

	private int KioskHolderId
	{
		get
		{
			return _kioskHolderId.Get();
		}
		set
		{
			_kioskHolderId.CompareAndSwapSet(value);
		}
	}

	public bool IsInUse
	{
		get
		{
			return KioskHolderId != PhotonPlayer.Invalid;
		}
	}

	public bool IsHolderLocal
	{
		get
		{
			return KioskHolderId == PhotonNetwork.player.ID;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		selfieCam = Object.Instantiate(selfieCameraPrefab);
		selfieCam.transform.SetParent(base.transform.parent);
		selfieCam.Resolution = cameraResolution;
		_kioskHolderId = new SynchronizedField<int>(this, "KIOSK_HOLDER", PhotonPlayer.Invalid, SetterPermissionMode.ANYONE, OnKioskHolderChanged);
		_kioskState = new SynchronizedField<int>(this, "KIOSK_STATE", 0, SetterPermissionMode.ANYONE, OnKioskStateChanged);
		All.Add(this);
	}

	private void Start()
	{
		kioskCanvas.worldCamera = ViveControllerInput.Instance.ControllerCamera;
		canvas.transform.SetParent(base.transform.parent, true);
		ResetFlow();
		UpdateKioskVisual();
	}

	private void Update()
	{
		if (IsHolderLocal && Player.LocalPlayer != null && !Player.LocalPlayer.IsRegistering && Time.time - lastTimeKioskStateChanged > kioskTimeoutSeconds && (Player.LocalPlayer.CurrentFloorPosition - base.transform.position).magnitude > kioskTimeoutPlayerDistance)
		{
			ResetFlow();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (kioskState != KioskState.None)
		{
			ResetFlow();
		}
		if (ViveControllerInput.Instance != null)
		{
			ViveControllerInput.Instance.StandAloneInput = false;
		}
		if (PlatformManager.Instance != null)
		{
			PlatformManager.Instance.IgnoreVRFocus = false;
		}
		All.Remove(this);
	}

	public void KioskMenu_SubmitButton()
	{
		AudioManager.Play3DSFX(buttonPressNext, notificationRoot.position);
		if (!IsInUse)
		{
			KioskHolderId = PhotonNetwork.player.ID;
			foreach (RegistrationKiosk item in All)
			{
				if (item != null && item != this && item.IsHolderLocal)
				{
					item.ResetFlow();
				}
			}
		}
		submitButtonPressed = true;
	}

	public void KioskMenu_CancelButton()
	{
		AudioManager.Play3DSFX(buttonPressCancel, notificationRoot.position);
		if (IsHolderLocal)
		{
			ResetFlow();
		}
	}

	private void ResetFlow()
	{
		StopAllCoroutines();
		mainFlowCoroutine = null;
		if (Player.LocalPlayer != null && Player.LocalPlayer.PlayerUI != null && Player.LocalPlayer.PlayerUI.UIPlayerCamera != null)
		{
			selfieCam.Setup(null);
		}
		if (IsHolderLocal && Player.LocalPlayer != null)
		{
			Player.LocalPlayer.IsRegistering = false;
			if (!Profiles.LocalProfile.Verified && kioskState != KioskState.None)
			{
				AnalyticsHelper.UserRegistrationFlow("canceled", kioskState.ToString());
			}
		}
		ReleaseKioskHolder();
		cameraFlashAnimator.gameObject.SetActive(false);
	}

	private IEnumerator RunMainFlow()
	{
		if (!Profiles.LocalProfile.Verified)
		{
			kioskState = KioskState.DownloadingProfile;
			yield return Profiles.DownloadLocalProfile(delegate(string e)
			{
				if (!string.IsNullOrEmpty(e) && IsHolderLocal && kioskState == KioskState.DownloadingProfile)
				{
					ShowNotification("Failed to sync profile for membership!");
					ResetFlow();
				}
			});
		}
		if (!Profiles.LocalProfile.Verified)
		{
			kioskState = KioskState.SignupFlowStart;
			AnalyticsHelper.UserRegistrationFlow("flowStart", string.Empty);
			yield return new WaitForSeconds(0.5f);
			submitButtonPressed = false;
			while (!submitButtonPressed)
			{
				yield return null;
			}
		}
		yield return RunTakeSelfie();
		if (Profiles.LocalProfile.Verified)
		{
			kioskState = KioskState.SelfieUpdated;
			AnalyticsHelper.UserSelfieUpdated();
			yield return new WaitForSeconds(3f);
		}
		else
		{
			kioskState = KioskState.PendingRegistration;
			ShowNotification("Remove headset and check PC!", 10f, ScreenSpaceNotificationManager.NotificationType.Major);
			if (IsHolderLocal)
			{
				Player.LocalPlayer.IsRegistering = true;
			}
			kioskUIState = KioskUIState.EnterEmail;
			while (kioskUIState == KioskUIState.EnterEmail || kioskUIState == KioskUIState.Error)
			{
				yield return null;
				if (kioskUIState != KioskUIState.SendingEmail || string.IsNullOrEmpty(submittedEmail))
				{
					continue;
				}
				if (Profiles.LocalProfile.Verified)
				{
					errorMessage = "You are already a member!";
					kioskUIState = KioskUIState.Error;
					continue;
				}
				yield return Profiles.SendRegistrationEmail(submittedEmail, delegate(string error, string text)
				{
					if (string.IsNullOrEmpty(error))
					{
						kioskUIState = KioskUIState.EmailSent;
					}
					else
					{
						errorMessage = text;
						kioskUIState = KioskUIState.Error;
						AnalyticsHelper.UserRegistrationFlow("failedToSendEmail", string.Empty);
					}
				});
			}
			AnalyticsHelper.UserRegistrationFlow("emailSent", string.Empty);
			kioskState = KioskState.PendingProfileVerification;
			yield return WaitForProfileVerification();
			kioskUIState = KioskUIState.None;
			AnalyticsHelper.UserRegistrationFlow("complete", string.Empty);
			kioskState = KioskState.RegistrationComplete;
			submitButtonPressed = false;
			while (!submitButtonPressed)
			{
				yield return null;
			}
			if (IsHolderLocal)
			{
				Player.LocalPlayer.IsRegistering = false;
			}
			Player.LocalPlayer.PlayerProgression.PlayLevelUpFeedback();
			yield return new WaitForSeconds(2f);
		}
		ResetFlow();
	}

	private IEnumerator WaitForProfileVerification()
	{
		while (!Profiles.LocalProfile.Verified)
		{
			float timer = 15f;
			while (timer > 0f)
			{
				timer -= Time.unscaledDeltaTime;
				yield return null;
			}
			yield return Profiles.DownloadLocalProfile(null);
		}
	}

	private IEnumerator RunTakeSelfie()
	{
		kioskState = KioskState.TakingSelfie;
		selfieCam.Setup(Player.LocalPlayer, selfieCamRenderer);
		selfieCam.FixedPoint = cameraLensTransform;
		selfieCam.transform.position = cameraLensTransform.position;
		selfieCam.gameObject.SetActive(true);
		yield return new WaitForSeconds(1f);
		ShowNotification("3");
		AudioManager.Play3DSFX(selfieCountdownBeep, submitButton.transform.position);
		yield return new WaitForSeconds(1.5f);
		ShowNotification("2");
		AudioManager.Play3DSFX(selfieCountdownBeep, submitButton.transform.position);
		yield return new WaitForSeconds(1.5f);
		ShowNotification("1");
		AudioManager.Play3DSFX(selfieCountdownBeep, submitButton.transform.position);
		yield return new WaitForSeconds(1.5f);
		selfieCam.gameObject.SetActive(false);
		AudioManager.Play3DSFX(selfieCamShutter, submitButton.transform.position);
		StartCoroutine(ShowCameraFlash());
		kioskState = KioskState.VerifyingSelfie;
		submitButtonPressed = false;
		while (!submitButtonPressed)
		{
			yield return null;
		}
		UpdateKioskVisual();
		yield return RunUploadNewPicture();
	}

	private IEnumerator ShowCameraFlash()
	{
		cameraFlashAnimator.gameObject.SetActive(true);
		yield return null;
		float clipDuration = cameraFlashAnimator.GetCurrentAnimatorStateInfo(0).length;
		float t = 0f;
		while (t < clipDuration)
		{
			t += Time.deltaTime;
			yield return null;
		}
		cameraFlashAnimator.gameObject.SetActive(false);
	}

	private IEnumerator RunUploadNewPicture()
	{
		RenderTexture oldActive = RenderTexture.active;
		RenderTexture.active = selfieCam.RenderTexture;
		int width = selfieCam.RenderTexture.width;
		int height = selfieCam.RenderTexture.height;
		Texture2D virtualPhoto = new Texture2D(width, height, TextureFormat.RGB24, false);
		virtualPhoto.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
		RenderTexture.active = oldActive;
		Texture2D scaledVirtualPhoto = Object.Instantiate(virtualPhoto);
		int xDownsizePerSample = (int)((float)width - imageSize.x) / numDownsamples;
		int yDownsizePerSample = (int)((float)height - imageSize.y) / numDownsamples;
		for (int i = 1; i < numDownsamples - 1; i++)
		{
			TextureScale.Bilinear(scaledVirtualPhoto, width - i * xDownsizePerSample, height - i * yDownsizePerSample);
			yield return null;
		}
		TextureScale.Bilinear(scaledVirtualPhoto, (int)imageSize.x, (int)imageSize.y);
		byte[] imageData = scaledVirtualPhoto.EncodeToPNG();
		yield return Images.SetLocalProfileImage(imageData, delegate(string error)
		{
			if (!string.IsNullOrEmpty(error))
			{
				ShowNotification("Failed to update the profile picture!", 3f, ScreenSpaceNotificationManager.NotificationType.Medium);
				Debug.LogError("Failed to update the profile picture. " + error);
				ResetFlow();
			}
		});
	}

	private void ShowNotification(string message, float duration = 1f, ScreenSpaceNotificationManager.NotificationType type = ScreenSpaceNotificationManager.NotificationType.Minor)
	{
		ScreenSpaceNotificationManager.Instance.Play(type, message, duration);
	}

	private void UpdateKioskVisual()
	{
		submitButtonPressed = false;
		bool flag = true;
		bool flag2 = true;
		string text = "Thanks!";
		Texture texture = selfieCamRenderer.material.mainTexture;
		switch (kioskState)
		{
		case KioskState.None:
		{
			bool verified = Profiles.LocalProfile.Verified;
			text = ((!verified) ? "START" : "Selfie");
			texture = ((!verified) ? signupTexture : updateSelfieTexture);
			flag2 = false;
			break;
		}
		case KioskState.TakingSelfie:
			text = "Smile :)";
			flag = false;
			flag2 = false;
			if (!IsHolderLocal)
			{
				texture = takingSelfieRemoteTexture;
			}
			break;
		case KioskState.VerifyingSelfie:
			text = "Accept?";
			if (!IsHolderLocal)
			{
				texture = takingSelfieRemoteTexture;
			}
			break;
		case KioskState.SelfieUpdated:
			flag = false;
			flag2 = false;
			texture = selfieUpdatedTexture;
			break;
		case KioskState.DownloadingProfile:
			text = "Syncing...";
			texture = signupFlowTexture;
			flag = false;
			break;
		case KioskState.SignupFlowStart:
			text = "Selfie";
			texture = signupFlowTexture;
			flag2 = false;
			break;
		case KioskState.PendingRegistration:
			flag = false;
			flag2 = false;
			text = "Check PC";
			texture = ((!IsHolderLocal) ? remoteCheckPCTexture : checkPCTexture);
			break;
		case KioskState.PendingProfileVerification:
			flag = false;
			flag2 = false;
			text = "Check Email";
			texture = ((!IsHolderLocal) ? remoteCheckPCTexture : checkEmailTexture);
			break;
		case KioskState.RegistrationComplete:
			flag2 = false;
			text = "Done!";
			texture = registrationCompleteTexture;
			break;
		}
		if (selfieCamRenderer.material.mainTexture != texture)
		{
			selfieCamRenderer.material.mainTexture = texture;
		}
		inUseText.gameObject.SetActive(IsInUse && !IsHolderLocal);
		submitButton.gameObject.SetActive(IsHolderLocal || !IsInUse);
		cancelButton.gameObject.SetActive(IsHolderLocal || !IsInUse);
		submitButton.interactable = flag;
		cancelButton.interactable = flag2;
		submitButton.OnDeselect(null);
		cancelButton.OnDeselect(null);
		submitButtonText.color = submitButtonText.color.ChangeAlpha((!flag) ? 0.3f : 1f);
		cancelButtonText.color = submitButtonText.color.ChangeAlpha((!flag2) ? 0.3f : 1f);
		submitButtonText.text = text;
		if (IsHolderLocal)
		{
			UpdateKioskUIState();
		}
		if (IsInUse)
		{
			powerupAmbienceLoopSource.Play();
		}
		else
		{
			powerupAmbienceLoopSource.Stop();
		}
	}

	private void UpdateKioskUIState()
	{
		bool flag = kioskUIState == KioskUIState.EnterEmail || kioskUIState == KioskUIState.Error || kioskUIState == KioskUIState.EmailSent;
		if (ViveControllerInput.Instance != null)
		{
			ViveControllerInput.Instance.StandAloneInput = flag;
		}
		if (PlatformManager.Instance != null)
		{
			PlatformManager.Instance.IgnoreVRFocus = flag;
		}
		canvas.gameObject.SetActive(kioskUIState != KioskUIState.None);
		UpdateEmailSubmitInteractive();
		emailSubmitButton.gameObject.SetActive(kioskUIState == KioskUIState.EnterEmail);
		emailInput.gameObject.SetActive(kioskUIState == KioskUIState.EnterEmail);
		tryAgainButton.gameObject.SetActive(kioskUIState == KioskUIState.Error);
		uiOkButton.gameObject.SetActive(kioskUIState == KioskUIState.EmailSent);
		returnToVRImage.gameObject.SetActive(kioskUIState == KioskUIState.EmailSent);
		uiCancelButton.gameObject.SetActive(kioskUIState != KioskUIState.SendingEmail && kioskUIState != KioskUIState.EmailSent);
		string text = "Please enter the email you'd like to use for membership:";
		switch (kioskUIState)
		{
		case KioskUIState.SendingEmail:
			text = "Sending email, please wait...";
			break;
		case KioskUIState.EmailSent:
			text = string.Format("Please check your email: {0}", submittedEmail);
			break;
		case KioskUIState.Error:
			text = errorMessage;
			break;
		}
		informationText.text = text;
	}

	private void UpdateEmailSubmitInteractive()
	{
		bool interactable = UnityExtensions.IsValidEmail(emailInput.text);
		emailSubmitButton.interactable = interactable;
	}

	public void Email_InputChanged(string text)
	{
		UpdateEmailSubmitInteractive();
	}

	public void Button_Cancel()
	{
		ResetFlow();
	}

	public void Button_SubmitEmail(string email)
	{
		if (UnityExtensions.IsValidEmail(email))
		{
			submittedEmail = email;
			kioskUIState = KioskUIState.SendingEmail;
		}
	}

	public void Button_TryAgain()
	{
		kioskUIState = KioskUIState.EnterEmail;
	}

	public void Button_HideUI()
	{
		kioskUIState = KioskUIState.None;
	}

	private void OnKioskHolderChanged()
	{
		if (mainFlowCoroutine == null && kioskState == KioskState.None)
		{
			if (IsHolderLocal)
			{
				mainFlowCoroutine = StartCoroutine(RunMainFlow());
			}
			if (IsInUse)
			{
				AudioManager.Play3DSFX(powerupAmbience, notificationRoot.position);
			}
		}
		UpdateKioskVisual();
	}

	private void OnKioskStateChanged()
	{
		lastTimeKioskStateChanged = Time.time;
		AudioManager.Play3DSFX(stateChangeClip, notificationRoot.position);
		UpdateKioskVisual();
	}

	public void ReleaseKioskHolder()
	{
		if (IsHolderLocal || (PhotonNetwork.isMasterClient && PhotonPlayer.Find(KioskHolderId) == null))
		{
			KioskHolderId = PhotonPlayer.Invalid;
			kioskUIState = KioskUIState.None;
			kioskState = KioskState.None;
		}
	}

	protected virtual void OnPhotonPlayerDisconnected(PhotonPlayer oldPlayer)
	{
		if (PhotonNetwork.isMasterClient && KioskHolderId == oldPlayer.ID)
		{
			ResetFlow();
		}
	}
}
