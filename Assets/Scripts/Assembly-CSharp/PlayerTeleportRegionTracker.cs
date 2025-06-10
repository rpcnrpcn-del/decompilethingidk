using UnityEngine;

public class PlayerTeleportRegionTracker : MonoBehaviour
{
	[SerializeField]
	private CollisionForwarder headTracker;

	[SerializeField]
	private CollisionForwarder torsoTracker;

	private Transform head;

	private Transform body;

	public Player ThisPlayer { get; private set; }

	private void Awake()
	{
		ThisPlayer = base.gameObject.GetComponentInParents<Player>();
		head = ThisPlayer.Head.transform;
		body = ThisPlayer.Body.transform;
	}

	private void Start()
	{
		headTracker.TriggerEnter += Tracker_TriggerEnter;
		torsoTracker.TriggerEnter += Tracker_TriggerEnter;
		headTracker.TriggerExit += Tracker_TriggerExit;
		torsoTracker.TriggerExit += Tracker_TriggerExit;
	}

	private void FixedUpdate()
	{
		if (head != null)
		{
			headTracker.Rigidbody.MovePosition(head.position);
			headTracker.Rigidbody.MoveRotation(head.rotation);
		}
		if (body != null)
		{
			torsoTracker.Rigidbody.MovePosition(body.position);
			torsoTracker.Rigidbody.MoveRotation(body.rotation);
		}
	}

	private bool CheckAvatarChangingRegion(Collider collider)
	{
		return collider.gameObject.layer == 5 && collider.gameObject.CompareTag("AvatarChangingRegion");
	}

	private void Tracker_TriggerEnter(Collider other)
	{
		if (CheckAvatarChangingRegion(other))
		{
			ThisPlayer.IsInChangingRoom = true;
		}
	}

	private void Tracker_TriggerExit(Collider other)
	{
		if (CheckAvatarChangingRegion(other))
		{
			ThisPlayer.IsInChangingRoom = false;
		}
	}
}
