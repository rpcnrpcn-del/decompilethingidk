using System;
using Photon;

public class SynchronizedTimer : Timer
{
	protected SynchronizedField<float> SynchronizedStartTime;

	protected SynchronizedField<float> SynchronizedEndTime;

	protected SynchronizedField<float> SynchronizedPausedTimeElapsed;

	protected SynchronizedField<float> SynchronizedPausedTimeRemaining;

	protected SynchronizedField<bool> SynchronizedIsPaused;

	public override float StartTime
	{
		get
		{
			return SynchronizedStartTime.Get();
		}
		protected set
		{
			SynchronizedStartTime.ForceSet(value);
		}
	}

	public override float EndTime
	{
		get
		{
			return SynchronizedEndTime.Get();
		}
		protected set
		{
			SynchronizedEndTime.ForceSet(value);
		}
	}

	public override float PausedTimeElapsed
	{
		get
		{
			return SynchronizedPausedTimeElapsed.Get();
		}
		protected set
		{
			SynchronizedPausedTimeElapsed.ForceSet(value);
		}
	}

	public override float PausedTimeRemaining
	{
		get
		{
			return SynchronizedPausedTimeRemaining.Get();
		}
		protected set
		{
			SynchronizedPausedTimeRemaining.ForceSet(value);
		}
	}

	public override bool IsPaused
	{
		get
		{
			return SynchronizedIsPaused.Get();
		}
		protected set
		{
			SynchronizedIsPaused.ForceSet(value);
		}
	}

	public event Action StartTimerEvent;

	public SynchronizedTimer(MonoBehaviour component, string id, SetterPermissionMode permissionMode)
	{
		SynchronizedStartTime = new SynchronizedField<float>(component, GetKey(id, "TIMER_START_TIME"), 0f, permissionMode, OnStartTimeChange);
		SynchronizedEndTime = new SynchronizedField<float>(component, GetKey(id, "TIMER_END_TIME"), 0f, permissionMode);
		SynchronizedPausedTimeElapsed = new SynchronizedField<float>(component, GetKey(id, "TIMER_PAUSED_TIME_ELAPSED"), 0f, permissionMode);
		SynchronizedPausedTimeRemaining = new SynchronizedField<float>(component, GetKey(id, "TIMER_PAUSED_TIME_REMAINING"), 0f, permissionMode);
		SynchronizedIsPaused = new SynchronizedField<bool>(component, GetKey(id, "TIMER_IS_PAUSED"), false, permissionMode);
	}

	private string GetKey(string id, string subkey)
	{
		return id + "_" + subkey;
	}

	private void OnStartTimeChange()
	{
		if (this.StartTimerEvent != null)
		{
			this.StartTimerEvent();
		}
	}
}
