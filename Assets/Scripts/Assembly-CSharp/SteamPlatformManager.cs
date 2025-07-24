using System;
using System.Collections;
using System.Text;
using Steamworks;
using UnityEngine;
using UnityEngine.VR;

internal class SteamPlatformManager : PlatformManager
{
	private HardwareType? currentHardwareType;

	public const uint RECROOM_APP_ID = 471710u;

	private bool initialized;

	private Callback<GameRichPresenceJoinRequested_t> gameRichPresenceJoinRequested;

	private SteamAPIWarningMessageHook_t warningMessageHook;

	public override PlatformType CurrentPlatform
	{
		get
		{
			return PlatformType.STEAM;
		}
	}

	public override bool IgnoreVRFocus
	{
		set
		{
		}
	}

	public override bool HasVRFocus
	{
		get
		{
			return true;
		}
	}

	public override HardwareType CurrentHardwareType
	{
		get
		{
			if (!currentHardwareType.HasValue)
			{
				currentHardwareType = ((VRDevice.model != null && VRDevice.model.StartsWith("oculus", StringComparison.InvariantCultureIgnoreCase)) ? HardwareType.OCULUS : HardwareType.VIVE);
			}
			return currentHardwareType.Value;
		}
	}

	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	private void OnDestroy()
	{
		/*if (initialized)
		{
			SteamAPI.Shutdown();
		}*/
	}

	private void Update()
	{
		/*if (initialized)
		{
			SteamAPI.RunCallbacks();
		}*/
	}

	private void OnRecRoomPlayerConnected(Player player)
	{
		/*if (!player.isLocal && player.Platform == PlatformType.STEAM)
		{
			SteamFriends.SetPlayedWith(new CSteamID(player.PlatformId));
		}*/
	}

	private bool ShouldRestart()
	{
		if (!Packsize.Test())
		{
			Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
			return true;
		}
		if (!DllCheck.Test())
		{
			Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
			return true;
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary(new AppId_t(471710u)))
			{
				return true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex, this);
			return true;
		}
		return false;
	}

	// funny
    ulong LongRandom()
    {
		/*long result = rand.Next((Int32)(min >> 32), (Int32)(max >> 32));
        result = (result << 32);
        result = result | (long)rand.Next((Int32)min, (Int32)max);*/

		string result = "";

		result += UnityEngine.Random.Range(0, int.MaxValue).ToString();
		result += UnityEngine.Random.Range(0, int.MaxValue).ToString();

		result.Replace(".", "");
		result.Replace("/", "");
		result.Replace("?", "");
		result.Replace("-", "");
		result.Replace(":", "");
		result.Replace("_", "");

        return ulong.Parse(result);
    }


    public override IEnumerator Initialize(InitializeCallback callback)
	{
		/*if (ShouldRestart())
		{
			Application.Quit();
			yield break;
		}
		if (!SteamAPI.Init())
		{
			callback("Failed to initialize Steam Platform");
			yield break;
		}*/
		initialized = true;
		/*warningMessageHook = SteamAPIDebugTextHook;
		SteamClient.SetWarningMessageHook(warningMessageHook);
		gameRichPresenceJoinRequested = Callback<GameRichPresenceJoinRequested_t>.Create(OnGameRichPresenceJoinRequested);*/
		PUNNetworkManager.Instance.OnRecRoomPlayerConnected += OnRecRoomPlayerConnected;

		/*CSteamID steamID = SteamUser.GetSteamID();
		base.PlatformProfileId = (ulong)steamID;
		base.PlatformProfileName = SteamFriends.GetPersonaName();*/

		// Steam ID workaround
		ulong expectedId = ulong.Parse(PlayerPrefs.GetString("AssignedPlayerId", "0"));
		if (!PlayerPrefs.HasKey("AssignedPlayerId"))
		{
			ulong newId = LongRandom();
			expectedId = newId;
			PlayerPrefs.SetString("AssignedPlayerId", newId.ToString());
		} else
		{
			expectedId = ulong.Parse(PlayerPrefs.GetString("AssignedPlayerId", "0"));
        }

        base.PlatformProfileId = expectedId;
        base.PlatformProfileName = "Guest " + expectedId.ToString();

        Player.SetPlatformPlayerId(CurrentPlatform, base.PlatformProfileId);
		callback(null);

		yield break;
	}


    public Texture2D LoadAvatarForPlayer(CSteamID steamID)
	{
		/*int mediumFriendAvatar = SteamFriends.GetMediumFriendAvatar(steamID);
		uint pnWidth;
		uint pnHeight;
		if (mediumFriendAvatar != 0 && SteamUtils.GetImageSize(mediumFriendAvatar, out pnWidth, out pnHeight) && pnWidth != 0 && pnHeight != 0)
		{
			byte[] array = new byte[pnWidth * pnHeight * 4];
			if (SteamUtils.GetImageRGBA(mediumFriendAvatar, array, (int)(pnWidth * pnHeight * 4)))
			{
				uint num = pnWidth * 4;
				byte[] array2 = new byte[num];
				for (uint num2 = 0u; num2 < pnHeight / 2; num2++)
				{
					uint num3 = num2 * num;
					uint num4 = (pnHeight - num2 - 1) * num;
					Array.Copy(array, num3, array2, 0L, num);
					Array.Copy(array, num4, array, num3, num);
					Array.Copy(array2, 0L, array, num4, num);
				}
				Texture2D texture2D = new Texture2D((int)pnWidth, (int)pnHeight, TextureFormat.RGBA32, false, true);
				texture2D.LoadRawTextureData(array);
				return texture2D;
			}
		}*/
		return null;
	}

	public override void SetRichPresenceStatus(string statusText)
	{
		//SteamFriends.SetRichPresence("status", statusText);
	}

	private void OnGameRichPresenceJoinRequested(GameRichPresenceJoinRequested_t param)
	{
		//PUNNetworkManager.Instance.ProcessRichJoinCommand(param.m_rgchConnect);
	}
}
