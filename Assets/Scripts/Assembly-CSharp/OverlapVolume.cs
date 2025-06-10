using System;
using UnityEngine;

public class OverlapVolume : MonoBehaviour
{
	[SerializeField]
	private Vector3 localSize;

	[SerializeField]
	private Vector3 center;

	private Vector3 localExtents;

	private bool localPlayerInTrigger;

	private Vector3 GlobalSize
	{
		get
		{
			return Vector3.Scale(base.transform.lossyScale, localSize);
		}
	}

	public event Action LocalPlayerTriggerEnterEvent;

	protected virtual void Awake()
	{
		localExtents = localSize / 2f;
	}

	public bool ContainsPoint(Vector3 point)
	{
		Vector3 vector = base.transform.InverseTransformPoint(point) - center;
		return vector.x < localExtents.x && vector.x > 0f - localExtents.x && vector.y < localExtents.y && vector.y > 0f - localExtents.y && vector.z < localExtents.z && vector.z > 0f - localExtents.z;
	}

	private void FixedUpdate()
	{
		bool flag = localPlayerInTrigger;
		localPlayerInTrigger = Player.LocalPlayer != null && ContainsPoint(Player.LocalPlayer.Head.transform.position);
		if (!flag && localPlayerInTrigger && this.LocalPlayerTriggerEnterEvent != null)
		{
			this.LocalPlayerTriggerEnterEvent();
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.matrix = Matrix4x4.TRS(base.transform.TransformPoint(center), base.transform.rotation, GlobalSize);
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}
}
