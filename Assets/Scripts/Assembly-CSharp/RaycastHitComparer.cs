using System.Collections.Generic;
using UnityEngine;

public class RaycastHitComparer : IComparer<RaycastHit>
{
	public int Compare(RaycastHit a, RaycastHit b)
	{
		return a.distance.CompareTo(b.distance);
	}
}
