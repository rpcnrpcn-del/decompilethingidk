using System.Collections.Generic;
using UnityEngine;

public class ActivityBounds : MonoBehaviour
{
	private static List<ActivityBounds> allBounds = new List<ActivityBounds>();

	private OverlapVolume overlapVolume;

	private void Awake()
	{
		overlapVolume = GetComponent<OverlapVolume>();
		allBounds.Add(this);
	}

	private void OnDestroy()
	{
		allBounds.Remove(this);
	}

	private bool ContainsPoint(Vector3 point)
	{
		return overlapVolume.ContainsPoint(point);
	}

	public static bool PointInBounds(Vector3 point)
	{
		if (allBounds.Count == 0)
		{
			return true;
		}
		for (int i = 0; i < allBounds.Count; i++)
		{
			if (allBounds[i].ContainsPoint(point))
			{
				return true;
			}
		}
		return false;
	}
}
