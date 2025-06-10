using System;
using System.Collections;
using System.Globalization;
using AmplitudeAnalytics;
using RecNet;
using UnityEngine;
using UnityEngine.VR;

public static class AnalyticsHelper
{
	private class CommonProperties
	{
		public static string OTHER_PLAYER = "otherPlayer";

		internal static string ACTIVITY_NAME = "activityName";

		internal static string ACTIVITY_NAME_FRIENDLY = "activityNameFriendly";
	}

	private static string AnalyticsUserId
	{
		get
		{
			return (Profiles.LocalProfile == null) ? "notYetSet" : Profiles.LocalProfile.Id.ToString();
		}
	}

	public static void ActivityLoad(string activityName, int playerCount)
	{
		//GoogleAnalytics.Client.SendEventHit(activityName, "load", "players:" + playerCount, playerCount);
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("activity_load").WithProperty(CommonProperties.ACTIVITY_NAME, activityName).WithProperty("playerCount", playerCount));
	}

	public static void ActivityUnload(string activityName, float duration)
	{
		//GoogleAnalytics.Client.SendEventHit(activityName, "unload", (duration / 60f).ToString("F2") + "mins", (int)duration);
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("activity_unload").WithProperty(CommonProperties.ACTIVITY_NAME, activityName).WithProperty("duration", duration)
		//	.WithProperty("duration_mins", (duration / 60f).ToString("F2") + "m"));
	}

	public static void ActivityJoinedEmpty(float duration, bool anotherPlayerJoined, bool appQuit)
	{
		//string currentSceneName = RecRoomSceneManager.CurrentSceneName;
		//GoogleAnalytics.Client.SendEventHit(currentSceneName, "empty" + (anotherPlayerJoined ? "_joined" : ((!appQuit) ? "_change" : "_quit")), (duration / 60f).ToString("F2") + "mins", (int)duration);
	}

	public static void DiscgolfHoleComplete(int holeIndex)
	{
		//int num = holeIndex + 1;
		//GoogleAnalytics.Client.SendEventHit(RecRoomSceneManager.CurrentSceneFriendlyName, "holeComplete", num.ToString(), num);
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("holeComplete").WithProperty("holeNo", num));
	}

	public static void ActivityFrameRate(float fps, string label)
	{
		//if (RecRoomSceneManager.IsCurrentSceneAValidActivity)
		//{
		//	string currentSceneName = RecRoomSceneManager.CurrentSceneName;
		//	GoogleAnalytics.Client.SendEventHit(currentSceneName, "FPS", label, (int)fps);
		//	AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("activity_fps").WithProperty(CommonProperties.ACTIVITY_NAME, currentSceneName).WithProperty("fps", fps));
		//}
	}

	public static void ActivityServerRoundTripTime(float rtt, string label)
	{
		//if (RecRoomSceneManager.IsCurrentSceneAValidActivity)
		//{
		//	string currentSceneName = RecRoomSceneManager.CurrentSceneName;
		//	GoogleAnalytics.Client.SendEventHit(currentSceneName, "PhotonPing", label, (int)rtt);
		//	AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("activity_rtt").WithProperty(CommonProperties.ACTIVITY_NAME, currentSceneName).WithProperty("rtt", rtt));
		//}
	}

	public static void SceneLoadDuration(float loadLevelDuration)
	{
		//if (RecRoomSceneManager.IsCurrentSceneAValidActivity)
		//{
		//	string currentSceneName = RecRoomSceneManager.CurrentSceneName;
		//	int num = (int)loadLevelDuration;
		//	GoogleAnalytics.Client.SendEventHit(currentSceneName, "SceneLoadDuration", num + "secs", num);
		//	AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("sceneLoadTime").WithProperty(CommonProperties.ACTIVITY_NAME, currentSceneName).WithProperty("duration", num));
		//}
	}

	public static void GameInviteMessageResponse(Message message, bool accepted)
	{
		//ulong fromPlayerId = message.FromPlayerId;
		//string event_type = ((!accepted) ? "declined" : "accepted") + "_game_invite";
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event(event_type).WithProperty("fromPlayerId", fromPlayerId));
	}

	public static void SentGameInvite(Player sender, ulong sentToPlayerId, string sentToPlayerName, string sentToPlayerStatus)
	{
		//AmplitudeAnalyticsEvent amplitudeAnalyticsEvent = AmplitudeAnalyticsClient.Event("sent_game_invite").WithProperty("sentToPlayerId", sentToPlayerId).WithProperty("sentToPlayerName", sentToPlayerName)
		//	.WithProperty("sentToPlayerStatus", sentToPlayerStatus);
		//if (sender != null)
		//{
		//	amplitudeAnalyticsEvent.WithProperty(CommonProperties.ACTIVITY_NAME, RecRoomSceneManager.CurrentSceneName).WithProperty(CommonProperties.ACTIVITY_NAME_FRIENDLY, RecRoomSceneManager.CurrentSceneFriendlyName).WithProperty("senderId", sender.PlayerId)
		//		.WithProperty("senderId", sender.PlayerName);
		//}
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(amplitudeAnalyticsEvent);
	}

	public static void TotalSessionCount(int count)
	{
		//GoogleAnalytics.Client.SendEventHit("user", "SessionCount", count.ToString(), count);
	}

	public static void UserRegistrationFlow(string state, string kioskState = "")
	{
		//GoogleAnalytics.Client.SendEventHit("registration", state, kioskState);
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("registration").WithProperty("state", state).WithProperty("kioskState", kioskState));
	}

	public static void UserSelfieUpdated()
	{
		//GoogleAnalytics.Client.SendEventHit("user", "selfieUpdated", string.Empty);
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("selfie_updated"));
	}

	public static void ModerationUserKicked(int duration)
	{
		//string currentSceneName = RecRoomSceneManager.CurrentSceneName;
		//GoogleAnalytics.Client.SendEventHit("user_kicked", currentSceneName, string.Empty, duration);
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("user_kicked").WithProperty(CommonProperties.ACTIVITY_NAME, currentSceneName).WithProperty("duration", duration));
	}

	public static void OnMessageReceived(Message message)
	{
		//switch (message.Type)
		//{
		//case Message.MessageType.GameInvite:
		//case Message.MessageType.FriendInvite:
		//	OnInviteReceived(message);
		//	break;
		//case Message.MessageType.GameJoinFailed:
		//	AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("game_join_failed").WithProperty("fromPlayerId", message.FromPlayerId));
		//	break;
		//case Message.MessageType.GameInviteDeclined:
		//case Message.MessageType.PartyActivitySwitch:
		//	break;
		//}
	}

	private static void OnInviteReceived(Message message)
	{
		//Profile profileFromCache = Profiles.GetProfileFromCache(message.FromPlayerId);
		//AmplitudeAnalyticsEvent amplitudeAnalyticsEvent = AmplitudeAnalyticsClient.Event("received_" + message.Type).WithProperty("fromPlayerId", message.FromPlayerId);
		//if (profileFromCache != null)
		//{
		//	amplitudeAnalyticsEvent.WithProperty("fromPlayerName", profileFromCache.Username);
		//}
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(amplitudeAnalyticsEvent);
	}

	public static void ModerationVoteKicked(Player player, bool firstTimeForTheActivity = false)
	{
		//string currentSceneName = RecRoomSceneManager.CurrentSceneName;
		//GoogleAnalytics.Client.SendEventHit("vote_kicked", currentSceneName, string.Empty + player.PlayerName);
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("vote_kicked").WithProperty(CommonProperties.ACTIVITY_NAME, currentSceneName).WithProperty("playerName", player.PlayerName));
		//if (firstTimeForTheActivity)
		//{
		//	GoogleAnalytics.Client.SendEventHit(currentSceneName, "voteKick_happened", string.Empty + player.PlayerName);
		//	AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("vote_kicked_happened").WithProperty(CommonProperties.ACTIVITY_NAME, currentSceneName).WithProperty("playerName", player.PlayerName));
		//}
	}

	internal static void InitialPlayerLogin(float currentFloorHeightFromHead)
	{
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("initial_player_login").WithUserProperty("playerHeight", currentFloorHeightFromHead));
	}

	public static void UserName(string name)
	{
		//GoogleAnalytics.Client.SendEventHit("user", "name", name);
	}

	public static void UserSettings(string settingName, string label, int value)
	{
		//GoogleAnalytics.Client.SendEventHit("settings", settingName, label, value);
	}

	public static void UserHandGesture(PlayerHand myHand, PlayerHand otherHand, PlayerHandGestures.Gesture gesture)
	{
		//GoogleAnalytics.Client.SendEventHit("handGesture", gesture.ToString(), string.Format("me_{0}Hand,{1}_{2}Hand", myHand.Type, otherHand.ThisPlayer.PlayerName, otherHand.Type));
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("handGesture").WithProperty("gesture", gesture.ToString()).WithProperty("myHandType", myHand.Type.ToString())
		//	.WithProperty("otherHandType", otherHand.Type.ToString())
		//	.WithProperty(CommonProperties.OTHER_PLAYER, otherHand.ThisPlayer.PlayerName));
	}

	public static void UserIgnorePlayer(Player otherPlayer, bool ignore)
	{
		//string text = ((!ignore) ? "unignore" : "ignore");
		//GoogleAnalytics.Client.SendEventHit("user", text, otherPlayer.PlayerName);
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event(string.Format("user_{0}_player", text)).WithProperty(CommonProperties.OTHER_PLAYER, otherPlayer.PlayerName));
	}

	public static void UserMutePlayer(Player otherPlayer, bool mute)
	{
		//if (!otherPlayer.isLocal)
		//{
		//	string text = ((!mute) ? "unmute" : "mute");
		//	GoogleAnalytics.Client.SendEventHit("user", text, otherPlayer.PlayerName);
		//	AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event(string.Format("user_{0}_player", text)).WithProperty(CommonProperties.OTHER_PLAYER, otherPlayer.PlayerName));
		//}
	}

	internal static void SettingsInitialized(RecRoomQualitySetting qualitySetting, int sessionCount)
	{
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("settings_initialized").WithUserProperty("qualitySetting", qualitySetting.ToString()).WithUserProperty("sessionCount", sessionCount));
	}

	public static void UserAFK(float inactiveDuration)
	{
		//GoogleAnalytics.Client.SendEventHit("user", "afk", string.Empty);
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("user_afk").WithProperty("inactiveDuration", inactiveDuration));
	}

	public static void PartyCreatedSelf(string partyId)
	{
		//GoogleAnalytics.Client.SendEventHit("user", "party_created", string.Empty);
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("user_party_created").WithProperty("partyId", partyId));
	}

	public static void PartyJoined(string partyId, int partySize)
	{
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("user_party_joined").WithUserProperty("partyId", partyId).WithProperty("partySize", partySize));
	}

	public static void PartyLeft(string previousPartyId, int partySize)
	{
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("user_party_left").WithProperty("previousPartyId", previousPartyId).WithProperty("partySize", partySize));
	}

	public static void PartyJoinedSelf(int partySize, string partyId)
	{
		//GoogleAnalytics.Client.SendEventHit("user", "party_joined", "size:" + partySize, partySize);
	}

	public static void PartyLeftSelf(string partyId)
	{
		//GoogleAnalytics.Client.SendEventHit("user", "party_left", string.Empty);
	}

	public static void PartyJoinedOther(Player otherPlayer, int partySize)
	{
	}

	public static void PartyLeftOther(Player otherPlayer, int partySize)
	{
	}

	internal static void ReportPlayer(ulong playerId, PlayerReporting.ReportCategory reportCategory)
	{
		/*AmplitudeAnalyticsEvent amplitudeAnalyticsEvent = AmplitudeAnalyticsClient.Event("reported_player").WithProperty(CommonProperties.ACTIVITY_NAME, RecRoomSceneManager.CurrentSceneName).WithProperty("reported_player_id", playerId)
			.WithProperty("report_category", reportCategory.ToString());
		Profile profileFromCache = Profiles.GetProfileFromCache(playerId);
		if (profileFromCache != null)
		{
			amplitudeAnalyticsEvent.WithProperty("reported_player_name", profileFromCache.DisplayName);
			amplitudeAnalyticsEvent.WithProperty("reported_player_rep", profileFromCache.Reputation);
			amplitudeAnalyticsEvent.WithProperty("reported_player_level", profileFromCache.Level);
		}
		AmplitudeAnalyticsClient.Instance.LogEventAsync(amplitudeAnalyticsEvent);*/
	}

	public static void NetworkMasterClientChanged(string activityName, int playerCount)
	{
		/*GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.EVENT);
		GoogleAnalytics.Client.SetEventCategory("network");
		GoogleAnalytics.Client.SetEventAction("MasterChanged");
		GoogleAnalytics.Client.SetEventLabel(activityName);
		GoogleAnalytics.Client.SetEventValue(playerCount);
		GoogleAnalytics.Client.Send();
		AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("network_masterChanged").WithProperty(CommonProperties.ACTIVITY_NAME, activityName).WithProperty("playerCount", playerCount));*/
	}

	public static void NetworkRoomJoin(int errorCode)
	{
		/*GoogleAnalytics.Client.SendEventHit("network", "join_room", errorCode.ToString());
		AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("network_join_room").WithProperty("errorCode", errorCode));*/
	}

	public static void NetworkZombieDetection(bool isZombieRoom)
	{
		/*GoogleAnalytics.Client.SendEventHit("network", "zombie_room_detection", isZombieRoom.ToString());
		if (!isZombieRoom)
		{
			return;
		}
		try
		{
			string serverAddress = PhotonNetwork.ServerAddress;
			string text = PhotonNetwork.room.IfNotNull((Room r) => r.name, "UNKNOWN");
			AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("network_zombie_room_" + text).WithProperty("serverAddress", serverAddress));
		}
		catch
		{
		}
		try
		{
			string text2 = PhotonNetwork.ServerAddress;
			if (PhotonNetwork.room != null)
			{
				text2 = text2 + " | " + PhotonNetwork.room.name;
			}
			GoogleAnalytics.Client.CreateHit(GoogleAnalyticsHitType.EXCEPTION);
			GoogleAnalytics.Client.SetExceptionDescription("Zombie Room");
			GoogleAnalytics.Client.SetScreenName(GoogleAnalytics.LoadedLevelName);
			GoogleAnalytics.Client.SetDocumentTitle(text2);
			GoogleAnalytics.Client.SetIsFatalException(false);
			GoogleAnalytics.Client.Send();
		}
		catch
		{
		}*/
	}

	public static void AvatarOutfitEquip(OutfitSelectionToBody selectionToBody, bool equip)
	{
		/*string name = selectionToBody.selection.outfitItem.name;
		GoogleAnalytics.Client.SendEventHit("avatar", (!equip) ? "unEquip" : "equip", name, (int)selectionToBody.bodyPart);
		AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("avatar_outfit").WithProperty("equipped", equip).WithProperty("outfitItemName", name)
			.WithProperty("bodyPart", selectionToBody.bodyPart.ToString()));*/
	}

	public static void AvatarHairColor(ColorVault.ColorToGuid colorToGuid)
	{
		/*GoogleAnalytics.Client.SendEventHit("avatar", "hColor", colorToGuid.guidString);
		AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("avatar_hColor").WithProperty("colorGuid", colorToGuid.guidString));*/
	}

	public static void AvatarSkinColor(ColorVault.ColorToGuid colorToGuid)
	{
		/*GoogleAnalytics.Client.SendEventHit("avatar", "sColor", colorToGuid.guidString);
		AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("avatar_sColor").WithProperty("colorGuid", colorToGuid.guidString));*/
	}

	public static void AvatarChangingRoom(bool enter)
	{
		//GoogleAnalytics.Client.SendEventHit("changeRoom", (!enter) ? "exit" : "enter", Player.LocalPlayer.PlayerOutfit.GetOutfitNameList());
	}

	public static void AvatarChangingRoomResetButton()
	{
		//GoogleAnalytics.Client.SendEventHit("changeRoom", "reset", string.Empty);
	}

	public static void EnteredKioskState(string kioskState)
	{
		//AmplitudeAnalyticsClient.Instance.LogEventAsync(AmplitudeAnalyticsClient.Event("kiosk_state_" + kioskState));
	}

	public static IEnumerator OnPlatformAndProfileInitialized()
	{
		/*if (SessionManager.IsDeveloper || !Core.REC_NET_HOST.Equals("recroom.azurewebsites.net"))
		{
			AGAmplitudeAnalyticsSettings component = AmplitudeAnalyticsClient.Instance.gameObject.GetComponent<AGAmplitudeAnalyticsSettings>();
			component.DoIfNotNull(delegate(AGAmplitudeAnalyticsSettings settingsComponent)
			{
				AmplitudeAnalyticsClient.Instance.ForceSettings(settingsComponent.EditorSettings);
			});
		}
		string timezone = string.Empty;
		try
		{
			timezone = TimeZone.CurrentTimeZone.StandardName;
		}
		catch
		{
		}
		AmplitudeAnalyticsEvent startEvent = AmplitudeAnalyticsClient.Instance.InitializeEvent(Profiles.LocalProfile.Id.ToString());
		startEvent.WithGeographicInfo(new AmplitudeAnalyticsEvent.GeographicInfo
		{
			region = timezone
		}).WithUserProperty("GraphicsDeviceVendor", SystemInfo.graphicsDeviceVendor).WithUserProperty("GraphicsDeviceName", SystemInfo.graphicsDeviceName)
			.WithUserProperty("RRDisplayName", Profiles.LocalProfile.DisplayName)
			.WithUserProperty("GamePlatform", PlatformManager.Instance.CurrentPlatform)
			.WithUserProperty("PlatformID", PlatformManager.Instance.PlatformProfileId)
			.WithUserProperty("PlatformName", PlatformManager.Instance.PlatformProfileName)
			.WithUserProperty("VRDevice", PlatformManager.Instance.CurrentHardwareType)
			.WithUserProperty("VRDeviceName", VRSettings.loadedDeviceName)
			.WithUserProperty("VRDeviceModel", VRDevice.model)
			.WithUserProperty("IsDeveloper", SessionManager.IsDeveloper);
		startEvent.language = CultureInfo.CurrentCulture.Name;
		yield return AmplitudeAnalyticsClient.Instance.Initialize(startEvent);*/
		yield break;
	}
}
