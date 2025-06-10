using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class CastCollisionRigidbody : MonoBehaviour
{
	[SerializeField]
	private float castDuration = 0.05f;

	private Rigidbody rigidbody;

	private List<CastCollider> castColliders = new List<CastCollider>();

	private List<CastColliderHit> latestHits = new List<CastColliderHit>();

	private const int MAX_RAYCAST_HITS = 256;

	private static CastColliderHit[] hits = new CastColliderHit[256];

	public Rigidbody Rigidbody
	{
		get
		{
			return rigidbody;
		}
	}

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
		BoxCollider[] componentsInChildren = GetComponentsInChildren<BoxCollider>();
		foreach (BoxCollider boxCollider in componentsInChildren)
		{
			castColliders.Add(new BoxCastCollider(rigidbody, boxCollider, boxCollider.center));
		}
		SphereCollider[] componentsInChildren2 = GetComponentsInChildren<SphereCollider>();
		foreach (SphereCollider sphereCollider in componentsInChildren2)
		{
			castColliders.Add(new SphereCastCollider(rigidbody, sphereCollider, sphereCollider.center));
		}
	}

	public bool Cast(Vector3 initialPosition, Quaternion initialRotation, Vector3 direction, float distance, int layerMask, out CastColliderHit closestHit)
	{
		RunCast(initialPosition, initialRotation, direction, distance, layerMask);
		bool flag = false;
		int index = -1;
		float num = float.MaxValue;
		for (int i = 0; i < latestHits.Count; i++)
		{
			if (latestHits[i].Distance < num)
			{
				flag = true;
				index = i;
				num = latestHits[i].Distance;
			}
		}
		closestHit = ((!flag) ? default(CastColliderHit) : latestHits[index]);
		return flag;
	}

	public void CastToCollide(Vector3 initialPosition, Vector3 targetPosition, Quaternion targetRotation, Vector3 velocity, int layerMask)
	{
		if (!rigidbody.gameObject.activeSelf)
		{
			return;
		}
		Vector3 normalized = velocity.normalized;
		float num = 2f * castDuration * velocity.magnitude;
		initialPosition -= normalized * num / 2f;
		RunCast(initialPosition, targetRotation, normalized, num, layerMask);
		foreach (CastColliderHit latestHit in latestHits)
		{
			latestHit.Rigidbody.AddForceAtPosition(Vector3.Project(velocity, latestHit.CastColliderNormal), latestHit.Point, ForceMode.VelocityChange);
		}
	}

	private void RunCast(Vector3 initialPosition, Quaternion initialRotation, Vector3 direction, float distance, int layerMask)
	{
		latestHits.Clear();
		foreach (CastCollider castCollider in castColliders)
		{
			int num = castCollider.Cast(initialPosition, initialRotation, direction, distance, layerMask, hits);
			int i;
			for (i = 0; i < num; i++)
			{
				int num2 = -1;
				num2 = ((!(hits[i].Rigidbody != null)) ? latestHits.FindIndex((CastColliderHit x) => x.Collider == hits[i].Collider) : latestHits.FindIndex((CastColliderHit x) => x.Rigidbody == hits[i].Rigidbody));
				if (num2 < 0)
				{
					latestHits.Add(hits[i]);
				}
				else if (latestHits[num2].Distance < hits[i].Distance)
				{
					latestHits[num2] = hits[i];
				}
			}
		}
	}
}
