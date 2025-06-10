using System;
using UnityEngine;

public class TrackedVelocity : MonoBehaviour
{
	private static TrackedVelocity loggedInstance;

	private const float RECENT_TIME = 0.05f;

	[SerializeField]
	private bool useUnscaledTime = true;

	private float previousUpdateTime;

	private Vector3 previousPosition = Vector3.zero;

	private Quaternion previousRotation = Quaternion.identity;

	private TimestampedRollingBufferVector3 linearVelocityHistory;

	private TimestampedRollingBufferVector3 angularVelocityHistory;

	public Action<TrackedVelocity> UpdateEvent;

	private float now
	{
		get
		{
			return (!useUnscaledTime) ? Time.time : Time.unscaledTime;
		}
	}

	public Vector3 LatestVelocity { get; private set; }

	public Vector3 LatestAngularVelocity { get; private set; }

	public Vector3 LatestDisplacement { get; private set; }

	public Vector3 RecentLinearVelocity
	{
		get
		{
			return GetAverageLinearVelocity(0.05f);
		}
	}

	public Vector3 RecentAngularVelocity
	{
		get
		{
			return GetAverageAngularVelocity(0.05f);
		}
	}

	public static event Action<TrackedVelocity> LoggedInstanceUpdateEvent;

	public static void StartLogging(TrackedVelocity instance)
	{
		loggedInstance = instance;
	}

	public static void StopLogging(TrackedVelocity instance)
	{
		if (loggedInstance == instance)
		{
			loggedInstance = null;
		}
	}

	public Vector3 GetAverageLinearVelocity(float timeWindow)
	{
		Vector3 value;
		if (linearVelocityHistory.TryGetAverageValueOverTime(now - timeWindow, now, out value))
		{
			return value;
		}
		return LatestVelocity;
	}

	public Vector3 GetAverageAngularVelocity(float timeWindow)
	{
		Vector3 value;
		if (angularVelocityHistory.TryGetAverageValueOverTime(now - timeWindow, now, out value))
		{
			return value;
		}
		return LatestAngularVelocity;
	}

	public void Reset(Vector3 newVelocity, Vector3 newAngularVelocity)
	{
		LatestVelocity = newVelocity;
		LatestAngularVelocity = newAngularVelocity;
		if (linearVelocityHistory == null)
		{
			linearVelocityHistory = new TimestampedRollingBufferVector3();
		}
		if (angularVelocityHistory == null)
		{
			angularVelocityHistory = new TimestampedRollingBufferVector3();
		}
		linearVelocityHistory.Clear();
		angularVelocityHistory.Clear();
		linearVelocityHistory.Add(now, LatestVelocity);
		angularVelocityHistory.Add(now, LatestAngularVelocity);
		previousUpdateTime = now;
		previousPosition = base.transform.position;
		previousRotation = base.transform.rotation;
	}

	public void ClearDisplacement()
	{
		LatestDisplacement = Vector3.zero;
	}

	private void OnEnable()
	{
		Reset(Vector3.zero, Vector3.zero);
	}

	private void LateUpdate()
	{
		float num = now - previousUpdateTime;
		if (num > 0f)
		{
			LatestVelocity = (base.transform.position - previousPosition) / num;
			LatestDisplacement += LatestVelocity * num;
			LatestAngularVelocity = UnityExtensions.AngularVelocityFromTo(previousRotation, base.transform.rotation) / num;
			linearVelocityHistory.Add(now, LatestVelocity);
			angularVelocityHistory.Add(now, LatestAngularVelocity);
			if (UpdateEvent != null)
			{
				UpdateEvent(this);
			}
			if (TrackedVelocity.LoggedInstanceUpdateEvent != null && loggedInstance == this)
			{
				TrackedVelocity.LoggedInstanceUpdateEvent(this);
			}
		}
		previousUpdateTime = now;
		previousPosition = base.transform.position;
		previousRotation = base.transform.rotation;
	}
}
