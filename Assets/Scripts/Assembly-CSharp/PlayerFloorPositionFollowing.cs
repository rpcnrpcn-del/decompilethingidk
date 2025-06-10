using System;
using UnityEngine;

public class PlayerFloorPositionFollowing : MonoBehaviour
{
	[NonSerialized]
	public Player TrackedPlayer;

	[SerializeField]
	private Vector3 eulerAngleParentRotation = new Vector3(-90f, -90f, 0f);

	private void Update()
	{
		if (TrackedPlayer != null)
		{
			base.transform.position = TrackedPlayer.CurrentFloorPosition;
			Vector3 forward = Vector3.ProjectOnPlane(TrackedPlayer.Head.transform.forward, Vector3.up);
			base.transform.rotation = Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Euler(eulerAngleParentRotation);
		}
	}
}
