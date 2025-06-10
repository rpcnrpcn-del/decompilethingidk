using System;
using System.Collections;
using System.Collections.Generic;
using GAMiniJSON;
using Photon;
using UnityEngine;

[RequireComponent(typeof(PlayerUI))]
public class TournamentMonitor : Photon.MonoBehaviour
{
	private class TournamentMatchInfo
	{
		public long MatchId;

		public string RoomName;

		public string YourTeam;

		public string OtherTeam;

		public bool BringParty;

		public static TournamentMatchInfo Parse(string json)
		{
			try
			{
				Dictionary<string, object> dictionary = Json.Deserialize(json) as Dictionary<string, object>;
				TournamentMatchInfo tournamentMatchInfo = new TournamentMatchInfo();
				tournamentMatchInfo.MatchId = (long)dictionary["MatchId"];
				tournamentMatchInfo.RoomName = (string)dictionary["RoomName"];
				tournamentMatchInfo.YourTeam = (string)dictionary["YourTeam"];
				tournamentMatchInfo.OtherTeam = (string)dictionary["OtherTeam"];
				tournamentMatchInfo.BringParty = (bool)dictionary["BringParty"];
				return tournamentMatchInfo;
			}
			catch
			{
				return null;
			}
		}
	}

	[Header("Tournament Settings")]
	[SerializeField]
	private string messageTitle = "VR World Championships";

	[SerializeField]
	private string matchReadyYesButton = "Go Play!";

	[SerializeField]
	private string matchReadyNoButton = "Forfeit";

	[SerializeField]
	[Multiline]
	private string matchReadyMessage = "It's game time!\nYour match is about to start.\n{0} vs {1}";

	[SerializeField]
	[Multiline]
	private string confirmForfeitMessage = "Are you sure you want to forfeit? Your team is counting on you!";

	[SerializeField]
	[Multiline]
	private string missedMatchMessage = "Unfortunately the match you were scheduled to play in has already started without you.";

	[SerializeField]
	private float checkServerInterval = 60f;

	private static TournamentMatchInfo matchInfo;

	private static string forfeitedMatchName;

	private static bool IsMatchAvailable()
	{
		return matchInfo != null && matchInfo.RoomName != forfeitedMatchName && (PhotonNetwork.room == null || PhotonNetwork.room.name != matchInfo.RoomName);
	}

	private void Start()
	{
		if (base.isLocal)
		{
			StartCoroutine(CheckForTournamentMatches());
			StartCoroutine(DisplayTournamentDialogs());
		}
	}

	private IEnumerator CheckForTournamentMatches()
	{
		while (true)
		{
			WWW www = new WWW("http://recroom.azurewebsites.net/api/tournament?player=" + Uri.EscapeDataString(PhotonNetwork.player.name));
			yield return www;
			matchInfo = TournamentMatchInfo.Parse(www.text);
			yield return new WaitForSeconds(checkServerInterval);
		}
	}

	private IEnumerator DisplayTournamentDialogs()
	{
		PlayerMenu playerMenu = GetComponent<PlayerUI>().Menu;
		while (true)
		{
			yield return new WaitUntil(IsMatchAvailable);
			if (matchInfo.BringParty)
			{
				yield return new WaitWhile(() => !playerMenu.Visible && IsMatchAvailable());
			}
			bool? joinGameConfirmed = null;
			if (IsMatchAvailable())
			{
				ConfirmationMenuController confirmationMenu = playerMenu.ShowConfirmation(messageTitle, string.Format(matchReadyMessage, matchInfo.YourTeam, matchInfo.OtherTeam), matchReadyYesButton, matchReadyNoButton);
				yield return new WaitWhile(() => !playerMenu.Visible);
				yield return new WaitWhile(() => playerMenu.Visible && confirmationMenu.Visible && IsMatchAvailable());
				if (confirmationMenu.Confirm == false)
				{
					confirmationMenu = playerMenu.ShowConfirmation(messageTitle, confirmForfeitMessage, matchReadyYesButton, matchReadyNoButton);
					yield return new WaitWhile(() => !playerMenu.Visible);
					yield return new WaitWhile(() => playerMenu.Visible && confirmationMenu.Visible && IsMatchAvailable());
				}
				joinGameConfirmed = confirmationMenu.Confirm;
				confirmationMenu.Visible = false;
			}
			if (!IsMatchAvailable())
			{
				playerMenu.ShowAlertMessage(messageTitle, missedMatchMessage);
				continue;
			}
			playerMenu.Visible = false;
			if (joinGameConfirmed == true)
			{
				if (!matchInfo.BringParty)
				{
					GetComponent<PlayerParty>().LeaveCurrentParty();
				}
				RecRoomSceneManager.Instance.JoinRoom(matchInfo.RoomName);
			}
			else if (joinGameConfirmed == false)
			{
				forfeitedMatchName = matchInfo.RoomName;
				yield return new WWW("http://recroom.azurewebsites.net/api/tournament/forfeit?match=" + matchInfo.MatchId + "&player=" + Uri.EscapeDataString(PhotonNetwork.player.name));
			}
		}
	}
}
