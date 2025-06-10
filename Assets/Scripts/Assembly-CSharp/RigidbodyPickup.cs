using System;
using System.Collections;
using Photon;
using UnityEngine;

[RequireComponent(typeof(CastCollisionRigidbody))]
public class RigidbodyPickup : Photon.MonoBehaviour
{
	[Tooltip("If true then this will accelerate towards the players hand over time.  Otherwise snap to player's hand.")]
	[SerializeField]
	protected bool interpolateToHand = true;

	protected const float interpolateToHandDuration = 0.06f;

	protected Coroutine attachToHandCoroutine;

	protected const float timeout = 0.5f;

	protected PUNNetworkTransform networkTransform;

	protected TrackedVelocity trackedVelocity;

	protected Rigidbody pickupHand;

	protected Vector3 localPickupPosition = Vector3.zero;

	protected Quaternion localPickupRotation = Quaternion.identity;

	public Rigidbody Rigidbody { get; protected set; }

	public bool IsPickedUp { get; protected set; }

	public Rigidbody Target
	{
		get
		{
			return pickupHand;
		}
	}

	public virtual Vector3 TargetRigidbodyPosition
	{
		get
		{
			if (pickupHand == null)
			{
				return base.transform.position;
			}
			return pickupHand.transform.TransformPoint(localPickupPosition);
		}
	}

	public virtual Quaternion TargetRigidbodyRotation
	{
		get
		{
			if (pickupHand == null)
			{
				return base.transform.rotation;
			}
			return pickupHand.transform.rotation * localPickupRotation;
		}
	}

	public event Action ReachedHand;

	protected override void Awake()
	{
		base.Awake();
		Rigidbody = GetComponent<Rigidbody>();
		trackedVelocity = GetComponent<TrackedVelocity>();
		networkTransform = GetComponent<PUNNetworkTransform>();
	}

	public virtual void Pickup(Rigidbody hand, Vector3 handSpacePickupPosition, Quaternion handSpacePickupRotation)
	{
		Release();
		Rigidbody.isKinematic = true;
		IsPickedUp = true;
		pickupHand = hand;
		localPickupPosition = handSpacePickupPosition;
		localPickupRotation = handSpacePickupRotation;
		if (networkTransform != null)
		{
			networkTransform.SynchronizationEnabled = base.hasAuthority;
		}
		StartAttachToHandCoroutine();
	}

	public virtual void Release()
	{
		Rigidbody.isKinematic = false;
		IsPickedUp = false;
		pickupHand = null;
		localPickupPosition = Vector3.zero;
		localPickupRotation = Quaternion.identity;
		if (networkTransform != null)
		{
			networkTransform.SynchronizationEnabled = true;
			if (!base.hasAuthority)
			{
				networkTransform.StartDynamicTimeExtrapolation();
			}
		}
		ClearAttachToHandCoroutine();
	}

	public void InstantaneousUpdate()
	{
		InterpolateRigidbodyTransform(TargetRigidbodyPosition, TargetRigidbodyRotation);
	}

	protected void StartAttachToHandCoroutine()
	{
		ClearAttachToHandCoroutine();
		attachToHandCoroutine = StartCoroutine(AttachToHandCoroutine());
	}

	private IEnumerator AttachToHandCoroutine()
	{
		float timer = 0f;
		while (!base.gameObject.activeSelf && timer <= 0.5f)
		{
			yield return null;
			timer += Time.deltaTime;
		}
		Vector3 velocity = Vector3.zero;
		Vector3 angularVelocity = Vector3.zero;
		Rigidbody.ClearVelocity();
		if (trackedVelocity != null)
		{
			trackedVelocity.Reset(Vector3.zero, Vector3.zero);
		}
		timer = 0f;
		while (interpolateToHand && timer < 0.06f)
		{
			Vector3 nextPosition = Vector3.SmoothDamp(Rigidbody.position, TargetRigidbodyPosition, ref velocity, 0.06f);
			Vector3 targetEulerAngles = TargetRigidbodyRotation.eulerAngles;
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			eulerAngles.x = Mathf.SmoothDampAngle(eulerAngles.x, targetEulerAngles.x, ref angularVelocity.x, 0.06f);
			eulerAngles.y = Mathf.SmoothDampAngle(eulerAngles.y, targetEulerAngles.y, ref angularVelocity.y, 0.06f);
			eulerAngles.z = Mathf.SmoothDampAngle(eulerAngles.z, targetEulerAngles.z, ref angularVelocity.z, 0.06f);
			Quaternion nextRotation = Quaternion.Euler(eulerAngles);
			InterpolateRigidbodyTransform(nextPosition, nextRotation);
			Rigidbody.ClearVelocity();
			yield return new WaitForFixedUpdate();
			timer += Time.fixedDeltaTime;
		}
		if (this.ReachedHand != null)
		{
			this.ReachedHand();
		}
		while (true)
		{
			yield return new WaitForFixedUpdate();
			InterpolateRigidbodyTransform(TargetRigidbodyPosition, TargetRigidbodyRotation);
		}
	}

	private void InterpolateRigidbodyTransform(Vector3 position, Quaternion rotation)
	{
		Vector3 vector = (position - Rigidbody.position) / Time.fixedDeltaTime;
		Vector3 vector2 = UnityExtensions.AngularVelocityFromTo(Rigidbody.rotation, rotation) / Time.fixedDeltaTime;
		Rigidbody.velocity = vector.ValueOrZeroIfBogus();
		Rigidbody.angularVelocity = vector2.ValueOrZeroIfBogus();
		Rigidbody.MovePosition(position.ValueOrZeroIfBogus());
		Rigidbody.MoveRotation(rotation.ValueOrIdentityIfBogus());
	}

	protected void ClearAttachToHandCoroutine()
	{
		if (attachToHandCoroutine != null)
		{
			StopCoroutine(attachToHandCoroutine);
			attachToHandCoroutine = null;
		}
	}
}
