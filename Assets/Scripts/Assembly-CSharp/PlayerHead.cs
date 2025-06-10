using Photon;
using UnityEngine;

public class PlayerHead : Photon.MonoBehaviour
{
	[SerializeField]
	private CapsuleCollider worldOverlapCapsule;

	public Player Player { get; private set; }

	public Rigidbody Rigidbody { get; private set; }

	public PUNNetworkTransform NetworkTransform { get; private set; }

	public Vector3 HeightOffset
	{
		get
		{
			return Vector3.Project(base.transform.position - Player.CurrentFloorPosition, Vector3.up);
		}
	}

	public bool IsOutOfBounds
	{
		get
		{
			return !ActivityBounds.PointInBounds(base.transform.position) || Vector3.Dot(Vector3.up, HeightOffset) < 0f;
		}
	}

	public bool IsOverlappingWorld
	{
		get
		{
			return worldOverlapCapsule.CheckOverlap(2048, QueryTriggerInteraction.Ignore);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Player = GetComponentInParent<Player>();
		Rigidbody = GetComponent<Rigidbody>();
		NetworkTransform = GetComponent<PUNNetworkTransform>();
	}
}
