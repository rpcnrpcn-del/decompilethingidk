using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameStatsManager : IGameComponent
{
	private enum ScoringMode
	{
		GREATEST_SCORE_WINS = 0,
		LOWEST_SCORE_WINS = 1,
		EVERYONE_WINS = 2
	}

	private class TeamData
	{
		private SynchronizedField<int> _score;

		public int Score
		{
			get
			{
				return _score.Get();
			}
			set
			{
				_score.ForceSet(value);
			}
		}

		public TeamData(GameTeam team, GameManager manager)
		{
			string key = string.Format("SCORE{0}", (int)team);
			_score = new SynchronizedField<int>(manager, key, 0, SetterPermissionMode.MASTER);
		}
	}

	[Serializable]
	private struct StatMetadata
	{
		public PlayerStatType Stat;

		public string Name;
	}

	[Header("Score")]
	[SerializeField]
	private bool supportsScore = true;

	[SerializeField]
	private ScoringMode scoringMode;

	[Header("Stats")]
	[SerializeField]
	private StatMetadata[] stats;

	private const string DEFAULT_STAT_NAME = "Invalid Stat";

	private GameManager gameManager;

	private Dictionary<GameTeam, TeamData> teamData = new Dictionary<GameTeam, TeamData>();

	private Dictionary<int, Dictionary<PlayerStatType, SynchronizedField<int>>> playerStats = new Dictionary<int, Dictionary<PlayerStatType, SynchronizedField<int>>>();

	public bool SupportsScore
	{
		get
		{
			return supportsScore;
		}
	}

	public event Action StatsChangeEvent;

	public void OnAwake(GameManager gameManager)
	{
		this.gameManager = gameManager;
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
		FireStatsChangeEvent();
	}

	public void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
		ResetAllLocalPlayerStats();
	}

	public void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
	}

	private TeamData GetTeamData(GameTeam team)
	{
		TeamData value;
		if (!teamData.TryGetValue(team, out value))
		{
			value = new TeamData(team, gameManager);
			teamData[team] = value;
		}
		return value;
	}

	public int GetScore(GameTeam team)
	{
		return supportsScore ? GetTeamData(team).Score : 0;
	}

	public int GetWinningScore(out GameTeam winningTeam)
	{
		int num = 0;
		winningTeam = GameTeam.INVALID;
		if (scoringMode != ScoringMode.EVERYONE_WINS)
		{
			GameTeam[] activeTeams = gameManager.TeamManager.GetActiveTeams();
			for (int i = 0; i < activeTeams.Length; i++)
			{
				int score = GetScore(activeTeams[i]);
				bool flag = (scoringMode == ScoringMode.GREATEST_SCORE_WINS && score > num) || (scoringMode == ScoringMode.LOWEST_SCORE_WINS && score < num);
				if (i == 0 || flag)
				{
					num = score;
					winningTeam = activeTeams[i];
				}
				else if (score == num)
				{
					winningTeam = GameTeam.INVALID;
				}
			}
		}
		else
		{
			winningTeam = gameManager.TeamManager.GetFirstActiveTeam();
		}
		return num;
	}

	public void AddScore(GameTeam team, int deltaScore)
	{
		gameManager.photonView.RPC("RpcMasterAddScore", PhotonTargets.MasterClient, (int)team, deltaScore);
	}

	public void MasterAddScore(GameTeam team, int deltaScore)
	{
		if (PhotonNetwork.isMasterClient && supportsScore)
		{
			int score = GetScore(team);
			MasterSetScore(team, score + deltaScore);
		}
	}

	public void MasterSetScore(GameTeam team, int score)
	{
		if (PhotonNetwork.isMasterClient && supportsScore)
		{
			GetTeamData(team).Score = score;
			BroadcastStatChange();
		}
	}

	public void MasterResetAllScores()
	{
		if (PhotonNetwork.isMasterClient && supportsScore)
		{
			GameTeam[] allTeams = gameManager.TeamManager.GetAllTeams();
			for (int i = 0; i < allTeams.Length; i++)
			{
				GetTeamData(allTeams[i]).Score = 0;
			}
			BroadcastStatChange();
		}
	}

	private SynchronizedField<int> GetPlayerStatField(PhotonPlayer player, PlayerStatType stat)
	{
		Dictionary<PlayerStatType, SynchronizedField<int>> value;
		if (!playerStats.TryGetValue(player.ID, out value))
		{
			value = new Dictionary<PlayerStatType, SynchronizedField<int>>();
			playerStats[player.ID] = value;
		}
		SynchronizedField<int> value2;
		if (!value.TryGetValue(stat, out value2))
		{
			string key = string.Format("STAT{0}", (int)stat);
			return new SynchronizedField<int>(player, key, 0, SetterPermissionMode.ANYONE);
		}
		return value2;
	}

	public int GetStatsCount()
	{
		return (stats != null) ? stats.Length : 0;
	}

	public int GetPlayerStat(PhotonPlayer player, PlayerStatType stat)
	{
		return GetPlayerStatField(player, stat).Get();
	}

	public void AddPlayerStat(PhotonPlayer player, PlayerStatType stat, int deltaValue)
	{
		gameManager.photonView.RPC("RpcMasterAddPlayerStat", PhotonTargets.MasterClient, player, (int)stat, deltaValue);
	}

	public void MasterAddPlayerStat(PhotonPlayer player, PlayerStatType stat, int deltaValue)
	{
		if (PhotonNetwork.isMasterClient)
		{
			int value = GetPlayerStatField(player, stat).Get() + deltaValue;
			MasterSetPlayerStat(player, stat, value);
		}
	}

	public void MasterSetPlayerStat(PhotonPlayer player, PlayerStatType stat, int value)
	{
		if (PhotonNetwork.isMasterClient)
		{
			GetPlayerStatField(player, stat).ForceSet(value);
			BroadcastStatChange();
		}
	}

	public void ResetAllLocalPlayerStats()
	{
		for (int i = 0; i < stats.Length; i++)
		{
			GetPlayerStatField(PhotonNetwork.player, stats[i].Stat).ForceSet(0);
		}
		BroadcastStatChange();
	}

	public string GetPlayerStatName(PlayerStatType stat)
	{
		for (int i = 0; i < stats.Length; i++)
		{
			if (stats[i].Stat == stat)
			{
				return stats[i].Name;
			}
		}
		return "Invalid Stat";
	}

	public void FireStatsChangeEvent()
	{
		if (this.StatsChangeEvent != null)
		{
			this.StatsChangeEvent();
		}
	}

	private void BroadcastStatChange()
	{
		gameManager.photonView.RPC("RpcBroadcastStatChange", PhotonTargets.All);
	}
}
