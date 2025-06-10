using System.Collections.Generic;
using UnityEngine;

public class Timer
{
	public delegate void Callback();

	private struct TimerCallback
	{
		public Callback Callback;

		public float TimeRemaining;

		public float TimeElapsed;

		public bool IsRecurring;

		public float RecurringDelay;
	}

	private List<TimerCallback> timerCallbacks = new List<TimerCallback>();

	private List<int> callbackRemoveIndices = new List<int>();

	public virtual float StartTime { get; protected set; }

	public virtual float EndTime { get; protected set; }

	public virtual float PausedTimeElapsed { get; protected set; }

	public virtual float PausedTimeRemaining { get; protected set; }

	public virtual bool IsPaused { get; protected set; }

	public float TimeElapsed
	{
		get
		{
			return (!IsPaused) ? Mathf.Clamp((float)PhotonNetwork.time - StartTime, 0f, float.MaxValue) : PausedTimeElapsed;
		}
	}

	public float TimeRemaining
	{
		get
		{
			return (!IsPaused) ? Mathf.Clamp(EndTime - (float)PhotonNetwork.time, 0f, float.MaxValue) : PausedTimeRemaining;
		}
	}

	public bool TimerOver
	{
		get
		{
			return TimeRemaining <= 0f;
		}
	}

	public void StartTimer(float maxRunningTime)
	{
		ClearCallbacks();
		float num = (StartTime = (float)PhotonNetwork.time);
		EndTime = num + maxRunningTime;
		IsPaused = false;
	}

	public void StartTimerWithoutMaxRunningTime()
	{
		float maxRunningTime = float.MaxValue - (float)PhotonNetwork.time;
		StartTimer(maxRunningTime);
	}

	public void Pause()
	{
		if (!IsPaused)
		{
			PausedTimeElapsed = TimeElapsed;
			PausedTimeRemaining = TimeRemaining;
			IsPaused = true;
		}
	}

	public void Resume()
	{
		if (IsPaused)
		{
			float num = (StartTime = (float)PhotonNetwork.time);
			EndTime = num + PausedTimeRemaining;
			IsPaused = false;
		}
	}

	public void UpdateCallbacks()
	{
		callbackRemoveIndices.Clear();
		float timeElapsed = TimeElapsed;
		float num = timeElapsed + 1f;
		float timeRemaining = TimeRemaining;
		float num2 = timeRemaining + 1f;
		for (int i = 0; i < timerCallbacks.Count; i++)
		{
			if ((timeElapsed <= timerCallbacks[i].TimeElapsed && num > timerCallbacks[i].TimeElapsed) || (timeRemaining <= timerCallbacks[i].TimeRemaining && num2 > timerCallbacks[i].TimeRemaining))
			{
				timerCallbacks[i].Callback();
				if (timerCallbacks[i].IsRecurring)
				{
					TimerCallback value = timerCallbacks[i];
					value.TimeRemaining -= value.RecurringDelay;
					value.TimeElapsed += value.RecurringDelay;
					timerCallbacks[i] = value;
				}
				else
				{
					callbackRemoveIndices.Add(i);
				}
			}
		}
		for (int num3 = callbackRemoveIndices.Count - 1; num3 >= 0; num3--)
		{
			timerCallbacks.RemoveAt(callbackRemoveIndices[num3]);
		}
	}

	public void ClearCallbacks()
	{
		timerCallbacks.Clear();
	}

	public void AddTimeElapsedCallback(float timeElapsed, Callback callback)
	{
		timerCallbacks.Add(new TimerCallback
		{
			Callback = callback,
			TimeRemaining = 2.1474836E+09f,
			TimeElapsed = timeElapsed,
			IsRecurring = false,
			RecurringDelay = 0f
		});
	}

	public void AddTimeElapsedCallback(float timeElapsed, float recurringDelay, Callback callback)
	{
		timerCallbacks.Add(new TimerCallback
		{
			Callback = callback,
			TimeRemaining = 2.1474836E+09f,
			TimeElapsed = timeElapsed,
			IsRecurring = true,
			RecurringDelay = recurringDelay
		});
	}

	public void AddTimeRemainingCallback(float timeRemaining, Callback callback)
	{
		timerCallbacks.Add(new TimerCallback
		{
			Callback = callback,
			TimeRemaining = timeRemaining,
			TimeElapsed = 2.1474836E+09f,
			IsRecurring = false,
			RecurringDelay = 0f
		});
	}

	public void AddTimeRemainingCallback(float timeRemaining, float recurringDelay, Callback callback)
	{
		timerCallbacks.Add(new TimerCallback
		{
			Callback = callback,
			TimeRemaining = timeRemaining,
			TimeElapsed = 2.1474836E+09f,
			IsRecurring = true,
			RecurringDelay = recurringDelay
		});
	}
}
