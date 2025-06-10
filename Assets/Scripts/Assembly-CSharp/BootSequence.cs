using System.Collections;
using System.IO;
using Photon;
using RecNet;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VR;

public class BootSequence : UnityEngine.MonoBehaviour
{
	private string error;

	private bool success
	{
		get
		{
			return string.IsNullOrEmpty(error);
		}
	}

	private void InitializeCallback(string error)
	{
		this.error = error;
	}

	private IEnumerator Start()
	{
		GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
		foreach (GameObject target in rootGameObjects)
		{
			Object.DontDestroyOnLoad(target);
		}
		if (!CheckForVRDevice() && !SessionManager.IsDeveloper && !Application.isEditor)
		{
			SceneManager.LoadScene("vr_device_required");
			yield break;
		}
		yield return LoadSplashScreen();
		PhotonNetwork.SendMonoMessageTargetType = typeof(Photon.MonoBehaviour);
		if (success)
		{
			// Test Connection
			yield return Core.TestConnection(InitializeCallback);
			// Init Platform Manager
			yield return PlatformManager.Instance.Initialize(InitializeCallback);
			LogPlatformInfo();
			// Init Profile Stuff
			yield return Profiles.DownloadLocalProfile(InitializeCallback);
			//yield return AnalyticsHelper.OnPlatformAndProfileInitialized(); kys rec room
			LogProfileInfo();
			// Config Stuff
			yield return Config.DownloadConfigSettings(InitializeCallback);
			// Notif Stuff
			yield return Core.InitializePushNotificationChannel(InitializeCallback);
			// Player Data :)
			Images.RefreshCachedProfileImage(Profiles.LocalProfile.Id);
			// Avatar :)
			yield return Avatars.DownloadLocalAvatar(InitializeCallback);
			// Player Configs :)
			yield return RecroomPrefs.DowloadLocalPlayerPreferences(InitializeCallback);
            // Avatar :)
            yield return OutfitManager.Instance.DownloadUnlockedAvatarItems(InitializeCallback);
			yield return Avatars.DowloadGiftPackages(InitializeCallback);
			// Photon :)
			yield return PUNNetworkManager.Instance.Initialize(InitializeCallback);
			yield return SingletonMonoBehaviour<SplashScreenManager>.Instance.WaitForMinimumSplashDuration();
			// Core Systems :)
			InitializeCoreSystems();
			yield return LoadInitialScene();
			Object.Destroy(base.gameObject);
		}
		else
		{
			Debug.LogError(error);
			SingletonMonoBehaviour<SplashScreenManager>.Instance.ShowErrorMessage(error);
		}
	}

	private void LogPlatformInfo()
	{
		string format = "Platform Info:\r\n    Platform: {0}\r\n    Hardware: {1}\r\n    Platform Profile Name: {2}\r\n    Platform Tracking Mode: {3}";
		string message = string.Format(format, PlatformManager.Instance.CurrentPlatform, PlatformManager.Instance.CurrentHardwareType, PlatformManager.Instance.PlatformProfileName, PlatformManager.Instance.CurrentTrackingMode);
		Debug.Log(message);
	}

	private void LogProfileInfo()
	{
		string format = "Profile Info:\r\n    Profile ID: {0}\r\n    Profile DisplayName: {1}";
		Profile localProfile = Profiles.LocalProfile;
		string message = string.Format(format, localProfile.Id, localProfile.DisplayName);
		Debug.Log(message);
	}

	private void InitializeCoreSystems()
	{
		SingletonMonoBehaviour<PlayerInputManager>.Instance.Initialize();
		SingletonMonoBehaviour<SettingsManager>.Instance.Initialize();
		SingletonMonoBehaviour<TutorialManager>.Instance.Initialize();
		SingletonMonoBehaviour<AudioManager>.Instance.Initialize();
		SingletonMonoBehaviour<GiftManager>.Instance.Initialize();
		if (SessionManager.IsDeveloper)
		{
			GoogleAnalytics.Client.UpdateHeaders();
		}
		SingletonMonoBehaviour<ProgressionManager>.Instance.Initialize();
		SingletonMonoBehaviour<ChildControlManager>.Instance.Initialize();
		AACCompilerCrashWorkAround();
	}

	private void AACCompilerCrashWorkAround()
	{
		string path = Path.Combine(Application.temporaryCachePath, "libfaac-win64.dll");
		bool flag = File.Exists(path);
		string path2 = Path.Combine(Application.dataPath, "FAAC_SelfBuild");
		bool flag2 = Directory.Exists(path2);
		if (!flag && flag2)
		{
			Debug.LogError("Detected failed previous FAAC build. Automatically disabling video cameras.");
			SingletonMonoBehaviour<SettingsManager>.Instance.H264Plugin = false;
			try
			{
				Directory.Delete(path2, true);
			}
			catch
			{
			}
		}
	}

	private bool CheckForVRDevice()
	{
		return VRDevice.isPresent;
	}

	private IEnumerator LoadSplashScreen()
	{
		SceneManager.LoadScene("splash_screen");
		yield return null;
		Vector3 headRot = SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.rotation.eulerAngles;
		SingletonMonoBehaviour<CameraRig>.Instance.transform.rotation = Quaternion.Euler(0f, 0f - headRot.y, 0f);
		Vector3 headPos = SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.position;
		SingletonMonoBehaviour<CameraRig>.Instance.transform.position = new Vector3(0f - headPos.x, 0f, 0f - headPos.z);
		yield return CameraFade.Instance.Fade(true, false, true, 1f);
	}

	private IEnumerator LoadInitialScene()
	{
		yield return CameraFade.Instance.Fade(false, true, true, 1f);
		PhotonNetwork.player.SetCustomProperties("prev_activity", "boot");
		ulong? playerToJoin = PUNNetworkManager.Instance.CheckCommandLineArgsForRichPresenceJoin();
		string initialScene = "dormroom";
		if (playerToJoin.HasValue)
		{
			PUNNetworkManager.Instance.JoinPlayer(playerToJoin.Value);
		}
		else
		{
			PUNNetworkManager.Instance.SwitchActivity(initialScene);
		}
	}
}
