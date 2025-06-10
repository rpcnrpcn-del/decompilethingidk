using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class GameSpawnManager : IGameComponent
{
	private enum SpawnPointSelectionMethod
	{
		LEAST_RECENTLY_USED = 0,
		TEAM_PROXIMITY = 1,
		RANDOM = 2
	}

	private struct ScoredSpawnPoint
	{
		public GameSpawnPoint SpawnPoint;

		public float Score;
	}

	[Serializable]
	private struct SpawnPointTagMetadata
	{
		public SpawnPointTagType TagType;

		public string Name;
	}

	[Serializable]
	public struct ProximityScoreSettings
	{
		public float MinThreshold;

		public float MaxThreshold;

		public float MinScore;

		public float MaxScore;

		public bool UseCurve;

		public AnimationCurve DistanceScoreCurve;
	}

	[SerializeField]
	private SpawnPointSelectionMethod spawnPointSelectionMethod;

	[SerializeField]
	private SpawnPointTagMetadata[] spawnPointTags;

	[Header("Team Proximity Settings")]
	[SerializeField]
	[FormerlySerializedAs("enemyProximityScoreSettings")]
	public ProximityScoreSettings EnemyProximityScoreSettings;

	[SerializeField]
	[FormerlySerializedAs("teammateProximityScoreSettings")]
	public ProximityScoreSettings TeammateProximityScoreSettings;

	private const string SPAWN_TAG_KEY = "SPAWN_TAG";

	private GameManager gameManager;

	private List<GameSpawnPoint> prePostGameSpawnPoints = new List<GameSpawnPoint>();

	private List<GameSpawnPoint> playingGameSpawnPoints = new List<GameSpawnPoint>();

	private PlayerSpawnToolPool playerSpawnToolPool;

	private List<ScoredSpawnPoint> teammateProximitySpawnPointScores = new List<ScoredSpawnPoint>();

	private List<GameSpawnPoint> filteredSpawnPoints = new List<GameSpawnPoint>();

	private SpawnPointTagType localPlayerSpawnPointTag = SpawnPointTagType.DEFAULT;

	[NonSerialized]
	public bool SpawnToolRestrictedByTeam;

	[NonSerialized]
	public GameTeam SpawnToolSupportedTeam = GameTeam.INVALID;

	[NonSerialized]
	public bool SpawnToolRestrictedByTeamPlayerIndex;

	[NonSerialized]
	public GameTeamPlayerIndex SpawnToolSupportedTeamPlayerIndex = GameTeamPlayerIndex.ANY_INDEX;

	private bool IsGameRunning
	{
		get
		{
			return gameManager.CurrentState == GameStates.GAME_RUNNING;
		}
	}

	public PlayerSpawnToolPool SpawnToolPool
	{
		get
		{
			return playerSpawnToolPool;
		}
	}

	public void OnAwake(GameManager gameManager)
	{
		this.gameManager = gameManager;
	}

	public void OnStart()
	{
		GameSpawnPoint[] array = UnityEngine.Object.FindObjectsOfType<GameSpawnPoint>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].SupportsGameState(SupportedGameState.PRE_POST_GAME))
			{
				prePostGameSpawnPoints.Add(array[i]);
			}
			if (array[i].SupportsGameState(SupportedGameState.PLAYING_GAME))
			{
				playingGameSpawnPoints.Add(array[i]);
			}
		}
		playerSpawnToolPool = UnityEngine.Object.FindObjectOfType<PlayerSpawnToolPool>();
		if (playerSpawnToolPool == null)
		{
			Debug.Log("There is no Player Spawn Tool Pool in this scene.");
			return;
		}
		for (int j = 0; j < playerSpawnToolPool.Pool.Length; j++)
		{
			Tool tool = playerSpawnToolPool.Pool[j];
			tool.ResetEvent += OnPoolToolReset;
			tool.SpawnedFromPool = true;
			tool.AutoLock = true;
			ToolCleanup toolCleanup = null;
			toolCleanup = ((!(SingletonMonoBehaviour<ToolCleanupManager>.Instance != null)) ? tool.GetComponent<ToolCleanup>() : SingletonMonoBehaviour<ToolCleanupManager>.Instance.AddTool(tool));
			if (toolCleanup != null)
			{
				toolCleanup.MinDisplacementBeforeCleanup = -1f;
			}
		}
	}

	public void OnDestroy()
	{
	}

	public void OnUpdate()
	{
	}

	public void OnPlayerDisconnected(PhotonPlayer player)
	{
	}

	public void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
		SetLocalPlayerSpawnPointTag(SpawnPointTagType.DEFAULT);
	}

	public void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
	}

	public void SetLocalPlayerSpawnPointTag(SpawnPointTagType tag)
	{
		localPlayerSpawnPointTag = tag;
	}

	public string GetTagName(SpawnPointTagType tagType)
	{
		for (int i = 0; i < spawnPointTags.Length; i++)
		{
			if (spawnPointTags[i].TagType == tagType)
			{
				return spawnPointTags[i].Name;
			}
		}
		return null;
	}

	private SpawnPointTagType GetLocalPlayerSpawnPointTag()
	{
		return localPlayerSpawnPointTag;
	}

	public void LocalPlayerRequestRespawn(bool dropTools = true, bool requestGiftSpawn = false, bool forceRequestSpawnTool = false)
	{
		bool flag = forceRequestSpawnTool || ((Player.LocalPlayer.LeftHand == null || !Player.LocalPlayer.LeftHand.IsHoldingTool) && (Player.LocalPlayer.RightHand == null || !Player.LocalPlayer.RightHand.IsHoldingTool));
		gameManager.photonView.RPC("RpcMasterRequestPlayerRespawn", PhotonTargets.MasterClient, PhotonNetwork.player, (int)GetLocalPlayerSpawnPointTag(), dropTools, flag, requestGiftSpawn);
	}

	public void MasterRespawnPlayer(PhotonPlayer player, SpawnPointTagType tag, bool dropTools, bool requestSpawnTool, bool requestGiftSpawn)
	{
		if (!PhotonNetwork.isMasterClient)
		{
			return;
		}
		SpawnPoint spawnPoint = MasterGetSpawnPoint(player, tag);
		if (spawnPoint == null)
		{
			Debug.LogError("Spawn Point is null.  Cannot spawn player " + player.name);
			return;
		}
		spawnPoint.MasterUse();
		int num = -1;
		if (requestSpawnTool)
		{
			Tool tool = MasterGetPlayerSpawnTool(player, spawnPoint.transform.position, spawnPoint.transform.rotation);
			num = ((!(tool != null)) ? (-1) : tool.photonView.viewID);
		}
		gameManager.photonView.RPC("RpcRespawnPlayer", player, spawnPoint.transform.position, spawnPoint.transform.rotation, dropTools, num, requestGiftSpawn);
	}

	private Tool MasterGetPlayerSpawnTool(PhotonPlayer player, Vector3 spawnPosition, Quaternion spawnRotation)
	{
		Tool result = null;
		if (PhotonNetwork.isMasterClient && IsGameRunning && playerSpawnToolPool != null && (!SpawnToolRestrictedByTeam || gameManager.TeamManager.GetPlayerTeam(player) == SpawnToolSupportedTeam) && (!SpawnToolRestrictedByTeamPlayerIndex || gameManager.TeamManager.GetTeamPlayerIndex(player) == SpawnToolSupportedTeamPlayerIndex))
		{
			result = playerSpawnToolPool.Acquire(spawnPosition, spawnRotation);
		}
		return result;
	}

	private void OnPoolToolReset(Tool tool, Vector3 position, Quaternion rotation)
	{
		if (PhotonNetwork.isMasterClient)
		{
			playerSpawnToolPool.Release(tool);
		}
	}

	private SpawnPoint MasterGetSpawnPoint(PhotonPlayer player, SpawnPointTagType playerTag)
	{
		List<GameSpawnPoint> list = ((!IsGameRunning) ? prePostGameSpawnPoints : playingGameSpawnPoints);
		if (list.Count == 0)
		{
			Debug.LogError("Missing spawn points for " + gameManager.name + ((!IsGameRunning) ? " in pre/post game state" : " in playing game state"));
			return null;
		}
		GameTeam playerTeam = gameManager.TeamManager.GetPlayerTeam(player);
		GameTeamPlayerIndex teamPlayerIndex = gameManager.TeamManager.GetTeamPlayerIndex(player);
		FilterSpawnPoints(list, playerTeam, teamPlayerIndex, playerTag, filteredSpawnPoints);
		if (filteredSpawnPoints.Count == 0)
		{
			Debug.Log("Filtered out all possible spawn points.  Spawning anyway.");
			return list[0];
		}
		SpawnPoint spawnPoint = null;
		switch (IsGameRunning ? spawnPointSelectionMethod : SpawnPointSelectionMethod.LEAST_RECENTLY_USED)
		{
		case SpawnPointSelectionMethod.TEAM_PROXIMITY:
			return FindSpawnPointTeamProximity(playerTeam, playerTag, filteredSpawnPoints);
		case SpawnPointSelectionMethod.RANDOM:
			return FindSpawnPointRandom(playerTeam, playerTag, filteredSpawnPoints);
		default:
			return FindSpawnPointLeastRecentlyUsed(playerTeam, playerTag, filteredSpawnPoints);
		}
	}

	private void FilterSpawnPoints(List<GameSpawnPoint> allSpawnPoints, GameTeam requiredTeam, GameTeamPlayerIndex requiredPlayerIndex, SpawnPointTagType requiredTag, List<GameSpawnPoint> filteredSpawnPointsOut)
	{
		filteredSpawnPointsOut.Clear();
		for (int i = 0; i < allSpawnPoints.Count; i++)
		{
			if (allSpawnPoints[i].SpawnPointEnabled && (requiredTeam == GameTeam.INVALID || allSpawnPoints[i].GameTeam == GameTeam.INVALID || allSpawnPoints[i].GameTeam == requiredTeam) && (requiredPlayerIndex == GameTeamPlayerIndex.ANY_INDEX || allSpawnPoints[i].GameTeamPlayerIndex == GameTeamPlayerIndex.ANY_INDEX || allSpawnPoints[i].GameTeamPlayerIndex == requiredPlayerIndex) && (requiredTag == SpawnPointTagType.DEFAULT || allSpawnPoints[i].CustomTag == SpawnPointTagType.DEFAULT || allSpawnPoints[i].CustomTag == requiredTag) && !allSpawnPoints[i].IsOnCooldown && !allSpawnPoints[i].IsOverlappingPlayer)
			{
				filteredSpawnPointsOut.Add(allSpawnPoints[i]);
			}
		}
	}

	private SpawnPoint FindSpawnPointLeastRecentlyUsed(GameTeam spawningPlayerTeam, SpawnPointTagType spawningPlayerTag, List<GameSpawnPoint> spawnPoints)
	{
		spawnPoints.Sort((GameSpawnPoint x, GameSpawnPoint y) => x.LastUseTime.CompareTo(y.LastUseTime));
		return spawnPoints[0];
	}

	private SpawnPoint FindSpawnPointRandom(GameTeam spawningPlayerTeam, SpawnPointTagType spawningPlayerTag, List<GameSpawnPoint> spawnPoints)
	{
		return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
	}

	private SpawnPoint FindSpawnPointTeamProximity(GameTeam spawningPlayerTeam, SpawnPointTagType spawningPlayerTag, List<GameSpawnPoint> spawnPoints)
	{
		teammateProximitySpawnPointScores.Clear();
		for (int i = 0; i < spawnPoints.Count; i++)
		{
			float spawnPointTeamProximityScore = GetSpawnPointTeamProximityScore(spawningPlayerTeam, spawnPoints[i]);
			teammateProximitySpawnPointScores.Add(new ScoredSpawnPoint
			{
				SpawnPoint = spawnPoints[i],
				Score = spawnPointTeamProximityScore
			});
		}
		teammateProximitySpawnPointScores.Sort((ScoredSpawnPoint a, ScoredSpawnPoint b) => b.Score.CompareTo(a.Score));
		return teammateProximitySpawnPointScores[0].SpawnPoint;
	}

	private float GetSpawnPointTeamProximityScore(GameTeam playerTeam, GameSpawnPoint spawnPoint)
	{
		PhotonPlayer[] teamPlayers = gameManager.TeamManager.GetTeamPlayers(playerTeam);
		PhotonPlayer[] otherPlayers = gameManager.TeamManager.GetOtherPlayers(playerTeam);
		float spawnPointProximityScoreHelper = GetSpawnPointProximityScoreHelper(spawnPoint, GetPlayerPositions(teamPlayers), TeammateProximityScoreSettings);
		float spawnPointProximityScoreHelper2 = GetSpawnPointProximityScoreHelper(spawnPoint, GetPlayerPositions(otherPlayers), EnemyProximityScoreSettings);
		return spawnPointProximityScoreHelper + spawnPointProximityScoreHelper2;
	}

	private List<Vector3> GetPlayerPositions(PhotonPlayer[] players)
	{
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < players.Length; i++)
		{
			Player player = players[i].ToPlayer();
			if (player != null)
			{
				list.Add(player.Head.transform.position);
			}
		}
		return list;
	}

	public float GetSpawnPointProximityScoreHelper(GameSpawnPoint spawnPoint, List<Vector3> playerPositions, ProximityScoreSettings scoreSettings)
	{
		float num = 0f;
		for (int i = 0; i < playerPositions.Count; i++)
		{
			float spawnPointProximityScoreForPosition = GetSpawnPointProximityScoreForPosition(spawnPoint, playerPositions[i], scoreSettings);
			num += spawnPointProximityScoreForPosition;
		}
		return num;
	}

	public float GetSpawnPointProximityScoreForPosition(GameSpawnPoint spawnPoint, Vector3 playerPosition, ProximityScoreSettings scoreSettings)
	{
		float magnitude = Vector3.ProjectOnPlane(playerPosition - spawnPoint.transform.position, Vector3.up).magnitude;
		if (scoreSettings.UseCurve && scoreSettings.DistanceScoreCurve != null)
		{
			return scoreSettings.DistanceScoreCurve.Evaluate(magnitude);
		}
		float t = Mathf.InverseLerp(scoreSettings.MinThreshold, scoreSettings.MaxThreshold, magnitude);
		return Mathf.Lerp(scoreSettings.MinScore, scoreSettings.MaxScore, t);
	}
}
