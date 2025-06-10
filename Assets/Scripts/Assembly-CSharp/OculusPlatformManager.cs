using System;
using System.Collections;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;

public class OculusPlatformManager : PlatformManager
{
	private const float HMD_REFRESH_RATE = 90f;

	public override PlatformType CurrentPlatform
	{
		get
		{
			return PlatformType.OCULUS;
		}
	}

	public override HardwareType CurrentHardwareType
	{
		get
		{
			return HardwareType.OCULUS;
		}
	}

	public override bool IgnoreVRFocus
	{
		set
		{
			OVRPlugin.ignoreVrFocus = value;
		}
	}

	public override bool HasVRFocus
	{
		get
		{
			return OVRManager.hasVrFocus;
		}
	}

	public override IEnumerator Initialize(InitializeCallback callback)
	{
		try
		{
			Core.Initialize();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			callback("Failed to initialize Oculus Platform");
			yield break;
		}
		OculusApiCoroutine checkEntitlement = Entitlements.IsUserEntitledToApplication().AsCoroutine();
		yield return checkEntitlement;
		if (checkEntitlement.Result.IsError)
		{
			callback("You do not have permission to play this game");
			yield break;
		}
		OculusApiCoroutine<User> getLoggedInUser = Users.GetLoggedInUser().AsCoroutine();
		yield return getLoggedInUser;
		if (getLoggedInUser.Result.IsError)
		{
			callback("Unable to load Oculus user profile");
			yield break;
		}
		User user = getLoggedInUser.Result.Data;
		base.PlatformProfileId = user.ID;
		base.PlatformProfileName = user.OculusID;
		Player.SetPlatformPlayerId(CurrentPlatform, base.PlatformProfileId);
		UnityEngine.Application.runInBackground = true;
		OVRPlugin.ignoreVrFocus = false;
		Time.fixedDeltaTime = Time.timeScale / 90f;
		callback(null);
	}

	public override void SetRichPresenceStatus(string statusText)
	{
	}
}
