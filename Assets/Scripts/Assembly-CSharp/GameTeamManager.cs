using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameTeamManager : IGameComponent
{
	public enum TeamSelectionMethod
	{
		RANDOM = 0,
		TEAM_INDEX_ORDER = 1
	}

	public delegate void OnTeamChange();

	private class PlayerData
	{
		private SynchronizedField<int> _teamId;

		private SynchronizedField<int> _teamPlayerIndex;

		public GameTeam TeamId
		{
			get
			{
				return (GameTeam)_teamId.Get();
			}
			set
			{
				_teamId.ForceSet((int)value);
			}
		}

		public GameTeamPlayerIndex TeamPlayerIndex
		{
			get
			{
				return (GameTeamPlayerIndex)_teamPlayerIndex.Get();
			}
			set
			{
				_teamPlayerIndex.ForceSet((int)value);
			}
		}

		public event Action TeamChangeEvent;

		public PlayerData(PhotonPlayer player)
		{
			_teamId = new SynchronizedField<int>(player, "TEAM_ID", -1, SetterPermissionMode.ANYONE, TeamChangeCallback);
			_teamPlayerIndex = new SynchronizedField<int>(player, "TEAM_PLAYER_INDEX", -1, SetterPermissionMode.ANYONE);
		}

		private void TeamChangeCallback()
		{
			if (this.TeamChangeEvent != null)
			{
				this.TeamChangeEvent();
			}
		}
	}

	[Serializable]
	private struct TeamDescriptor
	{
		public GameTeam Team;

		public int MaxTeamSize;
	}

	[Serializable]
	private struct CustomTeamMetadata
	{
		public GameTeam Team;

		public string Name;
	}

	[Header("Game Start Requirements")]
	[SerializeField]
	private GameStartTeamRequirement GameStartTeamRequirement;

	[Header("Teams")]
	[SerializeField]
	private TeamDescriptor[] teamConfigs;

	[SerializeField]
	private TeamSelectionMethod teamSelectionMethod;

	[SerializeField]
	private bool supportsTeamOutfits;

	[SerializeField]
	private bool useTeamNamesInFeedback = true;

	[SerializeField]
	private CustomTeamMetadata[] customTeamNames;

	private GameManager gameManager;

	private GameTeam localPlayerForbiddenTeam = GameTeam.INVALID;

	private Dictionary<int, PlayerData> playerData = new Dictionary<int, PlayerData>();

	public bool SupportsTeamOutfits
	{
		get
		{
			return supportsTeamOutfits;
		}
	}

	public bool GameStartTeamRequirementsMet
	{
		get
		{
			GameTeam[] activeTeams = GetActiveTeams();
			int currentTeamCount = ((activeTeams != null) ? activeTeams.Length : 0);
			int currentMinTeamSize = ((activeTeams != null) ? GetMinTeamSize(activeTeams) : 0);
			return GetTeamRequirementsMet(currentTeamCount, currentMinTeamSize);
		}
	}

	public event OnTeamChange TeamChangeEvent;

	public event Action LocalPlayerTeamChangeEvent;

	public void OnAwake(GameManager gameManager)
	{
		this.gameManager = gameManager;
		PlayerData playerData = GetPlayerData(PhotonNetwork.player);
		if (playerData == null)
		{
			Debug.LogError("Could not retrieve player data for local player.");
		}
		else
		{
			playerData.TeamChangeEvent += OnLocalPlayerTeamChange;
		}
	}

	public void OnStart()
	{
	}

	public void OnDestroy()
	{
	}

	public void OnUpdate()
	{
	}

	public void OnPlayerDisconnected(PhotonPlayer player)
	{
		FireTeamChangeEvent();
	}

	private bool GameTeamRequirementsMetWithoutPlayer(PhotonPlayer player)
	{
		GameTeam playerTeam = GetPlayerTeam(player);
		if (playerTeam == GameTeam.INVALID)
		{
			return GameStartTeamRequirementsMet;
		}
		GameTeam[] activeTeams = GetActiveTeams();
		int num = ((activeTeams != null) ? activeTeams.Length : 0);
		int num2 = ((activeTeams != null) ? GetMinTeamSize(activeTeams) : 0);
		int num3 = GetTeamPlayerCount(playerTeam) - 1;
		if (num3 == 0)
		{
			num--;
		}
		else if (num3 < num2)
		{
			num2 = num3;
		}
		return GetTeamRequirementsMet(num, num2);
	}

	private bool GetTeamRequirementsMet(int currentTeamCount, int currentMinTeamSize)
	{
		int num = int.MaxValue;
		int num2 = int.MaxValue;
		switch (GameStartTeamRequirement)
		{
		case GameStartTeamRequirement.TWO_TEAMS_ONE_PLAYER:
			num = 2;
			num2 = 1;
			break;
		case GameStartTeamRequirement.TWO_TEAMS_TWO_PLAYERS:
			num = 2;
			num2 = 2;
			break;
		case GameStartTeamRequirement.TWO_TEAMS_THREE_PLAYERS:
			num = 2;
			num2 = 3;
			break;
		case GameStartTeamRequirement.TWO_TEAMS_FOUR_PLAYERS:
			num = 2;
			num2 = 4;
			break;
		case GameStartTeamRequirement.THREE_TEAMS_ONE_PLAYER:
			num = 3;
			num2 = 1;
			break;
		case GameStartTeamRequirement.THREE_TEAMS_TWO_PLAYERS:
			num = 3;
			num2 = 2;
			break;
		case GameStartTeamRequirement.FOUR_TEAMS_ONE_PLAYER:
			num = 4;
			num2 = 1;
			break;
		case GameStartTeamRequirement.FOUR_TEAMS_TWO_PLAYERS:
			num = 4;
			num2 = 2;
			break;
		case GameStartTeamRequirement.ONE_TEAM_ANY_PLAYERS:
			num = 1;
			num2 = 1;
			break;
		case GameStartTeamRequirement.ONE_TEAM_TWO_PLAYERS:
			num = 1;
			num2 = 2;
			break;
		}
		return currentTeamCount >= num && currentMinTeamSize >= num2;
	}

	private int GetMinTeamSize(GameTeam[] teams)
	{
		int num = int.MaxValue;
		for (int i = 0; i < teams.Length; i++)
		{
			int teamPlayerCount = GetTeamPlayerCount(teams[i]);
			if (teamPlayerCount < num)
			{
				num = teamPlayerCount;
			}
		}
		return num;
	}

	public void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
		localPlayerForbiddenTeam = GameTeam.INVALID;
	}

	public void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
	}

	public void RequestNextTeam(PhotonPlayer player, bool tryToGroupPartyMembers = true)
	{
		GameTeam gameTeam = ((!tryToGroupPartyMembers) ? GameTeam.INVALID : GetPartyMemberTeam(player));
		gameManager.photonView.RPC("RpcMasterRequestTeam", PhotonTargets.MasterClient, player, (int)gameTeam, (int)localPlayerForbiddenTeam);
	}

	public void RequestTeamChange(PhotonPlayer player)
	{
		gameManager.photonView.RPC("RpcMasterRequestTeam", PhotonTargets.MasterClient, player, -1, (int)GetPlayerTeam(player));
	}

	public void RequestTeam(PhotonPlayer player, GameTeam desiredTeam)
	{
		gameManager.photonView.RPC("RpcMasterRequestTeam", PhotonTargets.MasterClient, player, (int)desiredTeam, -1);
	}

	public void LeaveTeam(PhotonPlayer player)
	{
		if ((player.isLocal || PhotonNetwork.isMasterClient) && PlayerCanLeaveGame(player))
		{
			SetPlayerTeam(player, GameTeam.INVALID);
		}
	}

	public void MasterClearTeams()
	{
		if (PhotonNetwork.isMasterClient)
		{
			for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
			{
				SetPlayerTeam(PhotonNetwork.playerList[i], GameTeam.INVALID, false);
			}
			BroadcastTeamChangeEvent();
		}
	}

	public void SetLocalPlayerForbiddenTeam(GameTeam team)
	{
		localPlayerForbiddenTeam = team;
	}

	private PlayerData GetPlayerData(PhotonPlayer player)
	{
		PlayerData value;
		if (!playerData.TryGetValue(player.ID, out value))
		{
			value = new PlayerData(player);
			playerData[player.ID] = value;
		}
		return value;
	}

	private void OnLocalPlayerTeamChange()
	{
		if (this.LocalPlayerTeamChangeEvent != null)
		{
			this.LocalPlayerTeamChangeEvent();
		}
	}

	public bool IsPlayerSpectator(PhotonPlayer player)
	{
		return GetPlayerTeam(player) == GameTeam.INVALID;
	}

	public GameTeam GetPlayerTeam(PhotonPlayer player)
	{
		return (player == null) ? GameTeam.INVALID : GetPlayerData(player).TeamId;
	}

	public GameTeamPlayerIndex GetTeamPlayerIndex(PhotonPlayer player)
	{
		return (player == null) ? GameTeamPlayerIndex.ANY_INDEX : GetPlayerData(player).TeamPlayerIndex;
	}

	public PhotonPlayer GetPlayerAtTeamPlayerIndex(GameTeam team, GameTeamPlayerIndex teamPlayerIndex)
	{
		PhotonPlayer[] teamPlayers = GetTeamPlayers(team);
		for (int i = 0; i < teamPlayers.Length; i++)
		{
			if (GetTeamPlayerIndex(teamPlayers[i]) == teamPlayerIndex)
			{
				return teamPlayers[i];
			}
		}
		return null;
	}

	public int GetTeamPlayerCount(GameTeam team)
	{
		return GetTeamPlayers(team).Length;
	}

	public int GetTeamMaxPlayerCount(GameTeam team)
	{
		int result = 0;
		for (int i = 0; i < teamConfigs.Length; i++)
		{
			if (teamConfigs[i].Team == team)
			{
				result = teamConfigs[i].MaxTeamSize;
				break;
			}
		}
		return result;
	}

	public GameTeam GetAnotherTeam(GameTeam team)
	{
		GameTeam result = GameTeam.INVALID;
		GameTeam[] allTeams = GetAllTeams();
		for (int i = 0; i < allTeams.Length; i++)
		{
			if (allTeams[i] != team)
			{
				result = allTeams[i];
				break;
			}
		}
		return result;
	}

	public int GetTeamVacancy(GameTeam team)
	{
		return GetTeamMaxPlayerCount(team) - GetTeamPlayerCount(team);
	}

	public int GetGameVacancy()
	{
		int num = 0;
		GameTeam[] allTeams = GetAllTeams();
		for (int i = 0; i < allTeams.Length; i++)
		{
			num += GetTeamVacancy(allTeams[i]);
		}
		return num;
	}

	public PhotonPlayer[] GetTeamPlayers(GameTeam team)
	{
		List<PhotonPlayer> list = new List<PhotonPlayer>();
		if (team != GameTeam.INVALID)
		{
			for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
			{
				if (GetPlayerTeam(PhotonNetwork.playerList[i]) == team)
				{
					list.Add(PhotonNetwork.playerList[i]);
				}
			}
		}
		return list.ToArray();
	}

	public PhotonPlayer[] GetTeamPlayersSortedByIndex(GameTeam team)
	{
		PhotonPlayer[] teamPlayers = GetTeamPlayers(team);
		Array.Sort(teamPlayers, (PhotonPlayer a, PhotonPlayer b) => GetTeamPlayerIndex(a).CompareTo(GetTeamPlayerIndex(b)));
		return teamPlayers;
	}

	public PhotonPlayer[] GetOtherPlayers(GameTeam team)
	{
		List<PhotonPlayer> list = new List<PhotonPlayer>();
		if (team != GameTeam.INVALID)
		{
			for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
			{
				GameTeam playerTeam = GetPlayerTeam(PhotonNetwork.playerList[i]);
				if (playerTeam != team && playerTeam != GameTeam.INVALID)
				{
					list.Add(PhotonNetwork.playerList[i]);
				}
			}
		}
		return list.ToArray();
	}

	public GameTeam[] GetAllTeams()
	{
		List<GameTeam> list = new List<GameTeam>();
		for (int i = 0; i < teamConfigs.Length; i++)
		{
			list.Add(teamConfigs[i].Team);
		}
		return list.ToArray();
	}

	public GameTeam[] GetActiveTeams()
	{
		List<GameTeam> list = new List<GameTeam>();
		for (int i = 0; i < teamConfigs.Length; i++)
		{
			if (GetTeamPlayerCount(teamConfigs[i].Team) > 0)
			{
				list.Add(teamConfigs[i].Team);
			}
		}
		return list.ToArray();
	}

	public GameTeam GetFirstActiveTeam()
	{
		for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
		{
			GameTeam playerTeam = GetPlayerTeam(PhotonNetwork.playerList[i]);
			if (playerTeam != GameTeam.INVALID)
			{
				return playerTeam;
			}
		}
		return GameTeam.INVALID;
	}

	public int GetActiveTeamCount()
	{
		return GetActiveTeams().Length;
	}

	public PhotonPlayer[] GetActivePlayers()
	{
		List<PhotonPlayer> list = new List<PhotonPlayer>();
		for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
		{
			if (GetPlayerTeam(PhotonNetwork.playerList[i]) != GameTeam.INVALID)
			{
				list.Add(PhotonNetwork.playerList[i]);
			}
		}
		return list.ToArray();
	}

	public int GetActivePlayerCount()
	{
		return GetActivePlayers().Length;
	}

	public int GetActivePlayersMaxCount()
	{
		int num = 0;
		for (int i = 0; i < teamConfigs.Length; i++)
		{
			num += teamConfigs[i].MaxTeamSize;
		}
		return num;
	}

	public int GetActivePlayerVacancy()
	{
		return GetActivePlayersMaxCount() - GetActivePlayerCount();
	}

	public bool PlayerCanSwitchTeam(PhotonPlayer player)
	{
		bool result = false;
		GameTeam playerTeam = GetPlayerTeam(player);
		for (int i = 0; i < teamConfigs.Length; i++)
		{
			if (teamConfigs[i].Team != playerTeam && GetTeamVacancy(teamConfigs[i].Team) > 0)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public bool PlayersAreTeammates(PhotonPlayer playerA, PhotonPlayer playerB)
	{
		GameTeam playerTeam = GetPlayerTeam(playerA);
		GameTeam playerTeam2 = GetPlayerTeam(playerB);
		return playerTeam != GameTeam.INVALID && playerTeam == playerTeam2;
	}

	public bool PlayerCanLeaveGame(PhotonPlayer player)
	{
		return !IsPlayerSpectator(player) && GameTeamRequirementsMetWithoutPlayer(player);
	}

	private PlayerData[] GetTeamPlayerDatas(GameTeam team)
	{
		List<PlayerData> list = new List<PlayerData>();
		if (team != GameTeam.INVALID)
		{
			for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
			{
				PlayerData playerData = GetPlayerData(PhotonNetwork.playerList[i]);
				if (playerData.TeamId == team)
				{
					list.Add(playerData);
				}
			}
		}
		return list.ToArray();
	}

	public string GetTeamName(GameTeam team)
	{
		if (!useTeamNamesInFeedback)
		{
			PhotonPlayer[] teamPlayers = GetTeamPlayers(team);
			if (teamPlayers.Length > 0)
			{
				return teamPlayers[0].name;
			}
		}
		string teamName = null;
		if (!TryGetCustomTeamName(team, out teamName))
		{
			teamName = GameTeamSettings.GetTeamName(team) + " Team";
		}
		return teamName;
	}

	private bool TryGetCustomTeamName(GameTeam team, out string teamName)
	{
		for (int i = 0; i < customTeamNames.Length; i++)
		{
			if (customTeamNames[i].Team == team)
			{
				teamName = customTeamNames[i].Name;
				return true;
			}
		}
		teamName = null;
		return false;
	}

	public void MasterAssignTeam(PhotonPlayer player, GameTeam desiredTeam, GameTeam forbiddenTeam)
	{
		if (PhotonNetwork.isMasterClient)
		{
			GameTeam gameTeam = GameTeam.INVALID;
			gameTeam = ((desiredTeam == GameTeam.INVALID || GetTeamVacancy(desiredTeam) <= 0) ? GetLeastOccupiedTeam(forbiddenTeam) : desiredTeam);
			if (gameTeam == GameTeam.INVALID)
			{
				Debug.LogWarning(("Unable to assign a team to " + player == null) ? "null player" : player.name);
			}
			else
			{
				SetPlayerTeam(player, gameTeam);
			}
		}
	}

	private GameTeam GetPartyMemberTeam(PhotonPlayer photonPlayer)
	{
		Player player = photonPlayer.ToPlayer();
		if (player != null && player.PlayerParty.IsInParty && player.PlayerParty.PartySize > 1)
		{
			foreach (Player partyPlayer in player.PlayerParty.PartyPlayers)
			{
				if (partyPlayer != player)
				{
					GameTeam playerTeam = GetPlayerTeam(partyPlayer.PhotonPlayer);
					if (playerTeam != GameTeam.INVALID)
					{
						return playerTeam;
					}
				}
			}
		}
		return GameTeam.INVALID;
	}

	private GameTeam GetLeastOccupiedTeam(GameTeam forbiddenTeam = GameTeam.INVALID)
	{
		if (GetActivePlayerVacancy() <= 0)
		{
			return GameTeam.INVALID;
		}
		int num = -1;
		List<GameTeam> list = new List<GameTeam>();
		for (int i = 0; i < teamConfigs.Length; i++)
		{
			if (GetTeamVacancy(teamConfigs[i].Team) == 0)
			{
				continue;
			}
			int teamPlayerCount = GetTeamPlayerCount(teamConfigs[i].Team);
			if ((forbiddenTeam == GameTeam.INVALID || forbiddenTeam != teamConfigs[i].Team) && (num < 0 || teamPlayerCount <= num))
			{
				if (num < 0 || teamPlayerCount < num)
				{
					list.Clear();
					num = teamPlayerCount;
				}
				list.Add(teamConfigs[i].Team);
			}
		}
		if (list.Count == 0)
		{
			return GameTeam.INVALID;
		}
		if (teamSelectionMethod == TeamSelectionMethod.TEAM_INDEX_ORDER)
		{
			list.Sort(delegate(GameTeam a, GameTeam b)
			{
				int num3 = (int)a;
				return num3.CompareTo((int)b);
			});
			if (forbiddenTeam != GameTeam.INVALID)
			{
				for (int num2 = 0; num2 < list.Count; num2++)
				{
					if (list[num2] > forbiddenTeam)
					{
						return list[num2];
					}
				}
			}
			return list[0];
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	private void SetPlayerTeam(PhotonPlayer player, GameTeam team, bool broadcastChange = true)
	{
		PlayerData playerData = GetPlayerData(player);
		playerData.TeamId = team;
		if (team == GameTeam.INVALID)
		{
			playerData.TeamPlayerIndex = GameTeamPlayerIndex.ANY_INDEX;
		}
		else
		{
			playerData.TeamPlayerIndex = GetNextTeamPlayerIndex(team);
		}
		if (broadcastChange)
		{
			BroadcastTeamChangeEvent();
		}
	}

	private GameTeamPlayerIndex GetNextTeamPlayerIndex(GameTeam team)
	{
		PlayerData[] teamPlayerDatas = GetTeamPlayerDatas(team);
		Array.Sort(teamPlayerDatas, (PlayerData a, PlayerData b) => a.TeamPlayerIndex.CompareTo(b.TeamPlayerIndex));
		GameTeamPlayerIndex gameTeamPlayerIndex = GameTeamPlayerIndex.INDEX_0;
		for (int num = 0; num < teamPlayerDatas.Length; num++)
		{
			if (teamPlayerDatas[num].TeamPlayerIndex > gameTeamPlayerIndex)
			{
				return gameTeamPlayerIndex;
			}
			if (teamPlayerDatas[num].TeamPlayerIndex == gameTeamPlayerIndex)
			{
				gameTeamPlayerIndex++;
			}
		}
		return gameTeamPlayerIndex;
	}

	public void FireTeamChangeEvent()
	{
		if (this.TeamChangeEvent != null)
		{
			this.TeamChangeEvent();
		}
	}

	private void BroadcastTeamChangeEvent()
	{
		gameManager.photonView.RPC("RpcOnTeamChange", PhotonTargets.All);
	}
}
