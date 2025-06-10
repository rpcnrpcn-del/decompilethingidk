using System.Collections.Generic;
using RecNet;
using UnityEngine;

public class SessionManager : SingletonMonoBehaviour<SessionManager>
{
	public ColorVault GroupColorVault;

	[HideInInspector]
	public int NumberOfTimesUserGotKicked;

	public static bool IsDeveloper
	{
		get
		{
			return Profiles.LocalProfile != null && Profiles.LocalProfile.Developer;
		}
	}

	public string PlayerPartyId { get; set; }

	public string PlayerPartyColorId { get; set; }

	public List<string> PlayerIgnoreList { get; private set; }

	public List<string> PlayerMuteList { get; private set; }

	public string AlertTitle { get; set; }

	public string AlertMessage { get; set; }

	public List<ulong> Highfives { get; private set; }

	public List<ulong> Fistbumps { get; private set; }

	public string BlockedRoomName { get; set; }

	private void Awake()
	{
		SingletonMonoBehaviour<SessionManager>.Instance = this;
		PlayerIgnoreList = new List<string>();
		PlayerMuteList = new List<string>();
		Highfives = new List<ulong>();
		Fistbumps = new List<ulong>();
	}
}
