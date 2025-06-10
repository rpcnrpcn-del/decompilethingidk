using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class SceneSpawnManager : IRecRoomSceneComponent
{
	private RecRoomSceneManager sceneManager;

	private SceneSpawnPoint[] spawnPoints;

	public const string PREVIOUSACTIVITY_PROPERTY = "prev_activity";

	public void OnStart(RecRoomSceneManager sceneManager)
	{
		this.sceneManager = sceneManager;
		spawnPoints = UnityEngine.Object.FindObjectsOfType<SceneSpawnPoint>();
	}

	public void OnDestroy()
	{
	}

	public void Update()
	{
	}

	public void SetLocalPlayerSettings()
	{
	}

	public void MasterSpawnNewPlayer(PhotonPlayer newPlayer)
	{
		if (PhotonNetwork.isMasterClient)
		{
			string customPropertyString = newPlayer.GetCustomPropertyString("prev_activity");
			SpawnPoint leastRecentlyUsedSpawnPoint = GetLeastRecentlyUsedSpawnPoint(customPropertyString);
			if (leastRecentlyUsedSpawnPoint == null)
			{
				Debug.LogError("Could not spawn player because a spawn point could not be acquired.");
				return;
			}
			leastRecentlyUsedSpawnPoint.MasterUse();
			sceneManager.photonView.RPC("RpcSpawnNewPlayer", newPlayer, leastRecentlyUsedSpawnPoint.transform.position, leastRecentlyUsedSpawnPoint.transform.rotation);
		}
	}

	public void SpawnNewPlayer(Vector3 position, Quaternion rotation)
	{
		Player.SpawnLocalPlayerAt(position, rotation);
		RunPostPlayerSpawnActivityLogic();
	}

	private void RunPostPlayerSpawnActivityLogic()
	{
		sceneManager.StartCoroutine(PostPlayerSpawnActivityLogicCoroutine());
	}

	private IEnumerator PostPlayerSpawnActivityLogicCoroutine()
	{
		while (CameraFade.Instance.IsFading)
		{
			yield return null;
		}
		yield return new WaitForSeconds(1.5f);
		if (!SingletonMonoBehaviour<TutorialManager>.Instance.SuppressActivityNameNotification)
		{
			sceneManager.FxManager.PlayFX(FxType.ACTIVITY_TITLE_JINGLE);
			ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, RecRoomSceneManager.CurrentSceneFriendlyName, 3f);
		}
	}

	private SpawnPoint GetLeastRecentlyUsedSpawnPoint(string previousActivityName)
	{
		if (spawnPoints.Length == 0)
		{
			Debug.LogError("Activity " + RecRoomSceneManager.CurrentSceneFriendlyName + " does not contain any spawn points.");
			return null;
		}
		SceneSpawnPoint sceneSpawnPoint = FindLRUSpawnPoint(spawnPoints, previousActivityName);
		if (sceneSpawnPoint == null)
		{
			sceneSpawnPoint = FindLRUSpawnPoint(spawnPoints, null);
		}
		if (sceneSpawnPoint == null)
		{
			Debug.Log("Could not find an activity spawn point configured for this player.  Spawned anyway.");
			return spawnPoints[0];
		}
		return sceneSpawnPoint;
	}

	private SceneSpawnPoint FindLRUSpawnPoint(SceneSpawnPoint[] spawnPoints, string requiredPreviousActivityName)
	{
		bool flag = string.IsNullOrEmpty(requiredPreviousActivityName);
		SceneSpawnPoint sceneSpawnPoint = null;
		for (int i = 0; i < spawnPoints.Length; i++)
		{
			if ((flag || !(spawnPoints[i].SupportedPreviousActivityName != requiredPreviousActivityName)) && !spawnPoints[i].IsOnCooldown && !spawnPoints[i].IsOverlappingPlayer && (sceneSpawnPoint == null || spawnPoints[i].LastUseTime < sceneSpawnPoint.LastUseTime))
			{
				sceneSpawnPoint = spawnPoints[i];
			}
		}
		return sceneSpawnPoint;
	}
}
