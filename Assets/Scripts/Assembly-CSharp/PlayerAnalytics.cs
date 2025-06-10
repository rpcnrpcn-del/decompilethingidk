using Photon;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAnalytics : Photon.MonoBehaviour
{
	private string activityName = "lockerroom";

	private Player thisPlayer;

	private bool joinedEmptyActivityTracking;

	private float joinedEmptyActivityTime;

	private bool isLogActivityEnded;

	private float startTime;

	private void Start()
	{
		thisPlayer = GetComponent<Player>();
		if (base.isLocal)
		{
			int roomPlayerCount = PUNNetworkManager.Instance.GetRoomPlayerCount();
			if (PhotonNetwork.inRoom && roomPlayerCount == 1 && PUNNetworkManager.Instance.AvailableSpaceInRoom > 0)
			{
				joinedEmptyActivityTracking = true;
				joinedEmptyActivityTime = Time.unscaledTime;
				RecnetAnalyticsHelper.CricketStart();
			}
			activityName = RecRoomSceneManager.CurrentSceneName;
			AnalyticsHelper.ActivityLoad(activityName, roomPlayerCount);
			Debug.Log(string.Format("Player joined activity \"{0}\" with playerCount {1}", RecRoomSceneManager.CurrentSceneFriendlyName, roomPlayerCount));
			startTime = Time.unscaledTime;
			thisPlayer.LeftHand.Gestures.GestureDetected += PlayerHandGestures_GestureDetected;
			thisPlayer.RightHand.Gestures.GestureDetected += PlayerHandGestures_GestureDetected;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (base.isLocal)
		{
			ProcessJoinedEmptyActivityEnded(false, false);
			LogActivityEnded();
		}
	}

	private void OnApplicationQuit()
	{
		if (base.isLocal)
		{
			ProcessJoinedEmptyActivityEnded(false, true);
			LogActivityEnded();
		}
	}

	public void OnPhotonPlayerConnected(PhotonPlayer otherPlayer)
	{
		if (base.isLocal)
		{
			ProcessJoinedEmptyActivityEnded(true, false);
		}
	}

	private void PlayerHandGestures_GestureDetected(PlayerHand myHand, PlayerHand otherHand, PlayerHandGestures.Gesture gesture)
	{
		if (base.isLocal)
		{
			AnalyticsHelper.UserHandGesture(myHand, otherHand, gesture);
		}
	}

	private void ProcessJoinedEmptyActivityEnded(bool anotherPlayerJoined, bool appQuit)
	{
		if (joinedEmptyActivityTracking)
		{
			float num = Time.unscaledTime - joinedEmptyActivityTime;
			AnalyticsHelper.ActivityJoinedEmpty(num, anotherPlayerJoined, appQuit);
			if (!appQuit)
			{
				RecnetAnalyticsHelper.CricketEnd(anotherPlayerJoined, num);
			}
			joinedEmptyActivityTracking = false;
		}
	}

	private void LogActivityEnded()
	{
		if (!isLogActivityEnded)
		{
			AnalyticsHelper.ActivityUnload(activityName, Time.unscaledTime - startTime);
			isLogActivityEnded = true;
		}
	}
}
