using System;
using UnityEngine;

public class OccupiableSpawnPoint : MonoBehaviour
{
	[SerializeField]
	private bool automaticallyRelease = true;

	[SerializeField]
	private float sphereCheckRadius = 0.25f;

	[SerializeField]
	private Vector3 sphereCheckCenter = Vector3.zero;

	public bool Occupied { get; private set; }

	public event Action<OccupiableSpawnPoint> AutomaticallyReleasedEvent;

	private void Update()
	{
		if (!Occupied || !automaticallyRelease)
		{
			return;
		}
		Vector3 position = base.transform.TransformPoint(sphereCheckCenter);
		if (!Physics.CheckSphere(position, sphereCheckRadius, 226894848, QueryTriggerInteraction.Ignore))
		{
			if (this.AutomaticallyReleasedEvent != null)
			{
				this.AutomaticallyReleasedEvent(this);
			}
			Release();
		}
	}

	public void Occupy()
	{
		Occupied = true;
	}

	public void Release()
	{
		Occupied = false;
	}

	private void OnDrawGizmos()
	{
		if (automaticallyRelease)
		{
			Gizmos.color = ((!Occupied) ? Color.green : Color.red);
			Gizmos.DrawWireSphere(base.transform.TransformPoint(sphereCheckCenter), sphereCheckRadius);
		}
	}
}
