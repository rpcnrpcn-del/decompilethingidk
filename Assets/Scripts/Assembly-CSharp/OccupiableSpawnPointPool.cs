using System.Collections.Generic;
using UnityEngine;

public class OccupiableSpawnPointPool : MonoBehaviour
{
	[SerializeField]
	private OccupiableSpawnPoint[] pool;

	private List<OccupiableSpawnPoint> available = new List<OccupiableSpawnPoint>();

	public int Availability
	{
		get
		{
			return available.Count;
		}
	}

	private void Awake()
	{
		OccupiableSpawnPoint[] array = pool;
		foreach (OccupiableSpawnPoint occupiableSpawnPoint in array)
		{
			occupiableSpawnPoint.AutomaticallyReleasedEvent += Release;
		}
		Reset();
	}

	public OccupiableSpawnPoint AcquireNext()
	{
		if (Availability <= 0)
		{
			return null;
		}
		return AcquireAtIndex(0);
	}

	public OccupiableSpawnPoint AcquireRandom()
	{
		if (Availability <= 0)
		{
			return null;
		}
		return AcquireAtIndex(Random.Range(0, available.Count));
	}

	public void Release(OccupiableSpawnPoint tf)
	{
		if (!available.Contains(tf))
		{
			tf.Release();
			available.Add(tf);
		}
	}

	public void Reset()
	{
		available.Clear();
		OccupiableSpawnPoint[] array = pool;
		foreach (OccupiableSpawnPoint occupiableSpawnPoint in array)
		{
			available.Add(occupiableSpawnPoint);
			if (occupiableSpawnPoint.Occupied)
			{
				occupiableSpawnPoint.Release();
			}
		}
	}

	private OccupiableSpawnPoint AcquireAtIndex(int index)
	{
		if (index >= available.Count)
		{
			return null;
		}
		OccupiableSpawnPoint occupiableSpawnPoint = available[index];
		available.RemoveAt(index);
		occupiableSpawnPoint.Occupy();
		return occupiableSpawnPoint;
	}
}
