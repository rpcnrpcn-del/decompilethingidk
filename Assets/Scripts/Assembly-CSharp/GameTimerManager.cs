using System;
using UnityEngine;

[Serializable]
public class GameTimerManager : IGameComponent
{
	[SerializeField]
	private bool supportsGameTimer;

	[SerializeField]
	private float gameDuration;

	private GameManager gameManager;

	private SynchronizedTimer timer;

	public float GameDuration
	{
		get
		{
			return gameDuration;
		}
	}

	public bool SupportsGameTimer
	{
		get
		{
			return supportsGameTimer;
		}
	}

	public bool TimerOver
	{
		get
		{
			return supportsGameTimer && timer.TimeRemaining <= 0f;
		}
	}

	public float TimeElapsed
	{
		get
		{
			return timer.TimeElapsed;
		}
	}

	public float TimeRemaining
	{
		get
		{
			return (!supportsGameTimer) ? 0f : timer.TimeRemaining;
		}
	}

	public void OnAwake(GameManager gameManager)
	{
		this.gameManager = gameManager;
		timer = new SynchronizedTimer(gameManager, "GAME", SetterPermissionMode.MASTER);
		timer.StartTimerEvent += OnStartTimer;
	}

	public void OnStart()
	{
	}

	public void OnDestroy()
	{
	}

	public void OnPlayerDisconnected(PhotonPlayer player)
	{
	}

	public void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
	}

	public void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
	}

	public void StartTimer()
	{
		if (timer == null)
		{
			Debug.LogError("GameTimerManager has not been initialized.");
		}
		if (PhotonNetwork.isMasterClient)
		{
			if (supportsGameTimer)
			{
				timer.StartTimer(gameDuration);
			}
			else
			{
				timer.StartTimerWithoutMaxRunningTime();
			}
		}
	}

	public void Pause()
	{
		if (PhotonNetwork.isMasterClient)
		{
			timer.Pause();
		}
	}

	public void Resume()
	{
		if (PhotonNetwork.isMasterClient)
		{
			timer.Resume();
		}
	}

	public void OnUpdate()
	{
		if (timer != null)
		{
			timer.UpdateCallbacks();
		}
	}

	public void AddTimeElapsedCallback(float timeElapsed, Timer.Callback callback)
	{
		if (timer != null)
		{
			timer.AddTimeElapsedCallback(timeElapsed, callback);
		}
	}

	public void AddTimeRemainingCallback(float timeRemaining, Timer.Callback callback)
	{
		if (timer != null)
		{
			timer.AddTimeRemainingCallback(timeRemaining, callback);
		}
	}

	private void OnStartTimer()
	{
		timer.ClearCallbacks();
		if (TimeRemaining >= 60f)
		{
			timer.AddTimeRemainingCallback(60f, OnOneMinuteRemaining);
		}
		if (TimeRemaining >= 10f)
		{
			timer.AddTimeRemainingCallback(10f, OnTenSecondsRemaining);
			timer.AddTimeRemainingCallback(5f, OnFiveSecondsRemaining);
			timer.AddTimeRemainingCallback(4f, OnFourSecondsRemaining);
			timer.AddTimeRemainingCallback(3f, OnThreeSecondsRemaining);
			timer.AddTimeRemainingCallback(2f, OnTwoSecondsRemaining);
			timer.AddTimeRemainingCallback(1f, OnOneSecondRemaining);
		}
	}

	private void OnOneMinuteRemaining()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "1 Minute Remaining", 3f);
		gameManager.FxManager.PlaySFX(FxType.ONE_MINUTE_REMAINING);
	}

	private void OnTenSecondsRemaining()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "10 Seconds Remaining", 3f);
		gameManager.FxManager.PlaySFX(FxType.TEN_SECONDS_REMAINING);
	}

	private void OnFiveSecondsRemaining()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "5", 1f);
		gameManager.FxManager.PlaySFX(FxType.FIVE);
	}

	private void OnFourSecondsRemaining()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "4", 1f);
		gameManager.FxManager.PlaySFX(FxType.FOUR);
	}

	private void OnThreeSecondsRemaining()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "3", 1f);
		gameManager.FxManager.PlaySFX(FxType.THREE);
	}

	private void OnTwoSecondsRemaining()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "2", 1f);
		gameManager.FxManager.PlayFX(FxType.TWO);
	}

	private void OnOneSecondRemaining()
	{
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Minor, "1", 1f);
		gameManager.FxManager.PlayFX(FxType.ONE);
	}
}
