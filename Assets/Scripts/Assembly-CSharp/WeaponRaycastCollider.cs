using System;
using UnityEngine;

public class WeaponRaycastCollider
{
	private RaycastHit[] hits = new RaycastHit[20];

	private RaycastHitComparer raycastHitComparer = new RaycastHitComparer();

	public bool RunCollision(Vector3 previousPosition, Vector3 currentPosition, float radius, Rigidbody ignoredRigidbody, out RaycastHit hit)
	{
		hit = default(RaycastHit);
		int num = UpdateCollisionResults(previousPosition, currentPosition, radius, ignoredRigidbody);
		for (int i = 0; i < num; i++)
		{
			if (ignoredRigidbody == null || hits[i].rigidbody != ignoredRigidbody)
			{
				hit = hits[i];
				return true;
			}
		}
		return false;
	}

	public int RunAllCollisions(Vector3 previousPosition, Vector3 currentPosition, float radius, Rigidbody ignoredRigidbody, ref RaycastHit[] outHits)
	{
		int num = 0;
		int num2 = UpdateCollisionResults(previousPosition, currentPosition, radius, ignoredRigidbody);
		for (int i = 0; i < num2; i++)
		{
			if (num >= outHits.Length)
			{
				break;
			}
			if ((ignoredRigidbody == null || hits[i].rigidbody != ignoredRigidbody) && !HitsArrayContainsRigidbody(outHits, num, hits[i].rigidbody))
			{
				outHits[num++] = hits[i];
			}
		}
		return num;
	}

	private int UpdateCollisionResults(Vector3 previousPosition, Vector3 currentPosition, float radius, Rigidbody ignoredRigidbody)
	{
		Vector3 vector = currentPosition - previousPosition;
		if (vector.sqrMagnitude <= Mathf.Epsilon || !Physics.CheckCapsule(previousPosition, currentPosition, radius, 227950080, QueryTriggerInteraction.Ignore))
		{
			return 0;
		}
		float magnitude = vector.magnitude;
		Vector3 direction = vector / magnitude;
		int num = Physics.SphereCastNonAlloc(previousPosition, radius, direction, hits, magnitude, 227950080, QueryTriggerInteraction.Ignore);
		if (hits == null || num == 0)
		{
			return 0;
		}
		Array.Sort(hits, 0, num, raycastHitComparer);
		return num;
	}

	private bool HitsArrayContainsRigidbody(RaycastHit[] hitsArray, int arraySize, Rigidbody rigidbody)
	{
		for (int i = 0; i < arraySize; i++)
		{
			if (hitsArray[i].rigidbody == rigidbody)
			{
				return true;
			}
		}
		return false;
	}
}
