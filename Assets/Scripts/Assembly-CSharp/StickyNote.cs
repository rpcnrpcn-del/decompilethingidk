using System.Collections;
using UnityEngine;

public class StickyNote : StickyTool
{
	[SerializeField]
	private float maxStickAngle = 45f;

	[SerializeField]
	private float castDistance = 0.4f;

	[SerializeField]
	private BoxCollider boxCollider;

	[Header("Sticky Note Audio")]
	[SerializeField]
	private RecRoomAudioClip onDetachAudio;

	[SerializeField]
	private RecRoomAudioClip onAttachAudio;

	private const int MAX_RAYCAST_HITS = 256;

	private static RaycastHit[] hits = new RaycastHit[256];

	private const float INTERPOLATE_TO_SURFACE_DURATION = 0.06f;

	protected override void Awake()
	{
		base.Awake();
		base.AttachEvent += OnAttach;
		base.DetachEvent += OnDetach;
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		base.Release(player, linearVelocity, angularVelocity);
		if (!base.hasAuthority)
		{
			return;
		}
		Vector3 center = base.transform.TransformPoint(boxCollider.center);
		Vector3 halfExtents = boxCollider.size.MultiplyComponents(boxCollider.transform.lossyScale) / 2f;
		int num = Physics.BoxCastNonAlloc(center, halfExtents, base.transform.forward, hits, base.transform.rotation, castDistance, 2048, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < num; i++)
		{
			StickyNoteBoard component = hits[i].collider.GetComponent<StickyNoteBoard>();
			if (component != null)
			{
				Vector3 targetPosition;
				Quaternion targetRotation;
				component.GetNoteAttachPosition(base.transform, out targetPosition, out targetRotation);
				if (Vector3.Angle(base.transform.forward, component.transform.forward) < maxStickAngle && component.WillFitOnBoard(targetPosition, targetRotation, halfExtents))
				{
					StartCoroutine(StickToBoardCoroutine(component, targetPosition, targetRotation));
					break;
				}
			}
		}
	}

	private IEnumerator StickToBoardCoroutine(StickyNoteBoard board, Vector3 targetPosition, Quaternion targetRotation)
	{
		base.Rigidbody.isKinematic = true;
		Vector3 velocity = base.TrackedVelocity.RecentLinearVelocity;
		Vector3 angularVelocity = base.TrackedVelocity.RecentAngularVelocity;
		for (float timer = 0f; timer < 0.06f; timer += Time.fixedDeltaTime)
		{
			Vector3 nextPosition = Vector3.SmoothDamp(base.Rigidbody.position, targetPosition, ref velocity, 0.06f);
			Vector3 targetEulerAngles = targetRotation.eulerAngles;
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			eulerAngles.x = Mathf.SmoothDampAngle(eulerAngles.x, targetEulerAngles.x, ref angularVelocity.x, 0.06f);
			eulerAngles.y = Mathf.SmoothDampAngle(eulerAngles.y, targetEulerAngles.y, ref angularVelocity.y, 0.06f);
			eulerAngles.z = Mathf.SmoothDampAngle(eulerAngles.z, targetEulerAngles.z, ref angularVelocity.z, 0.06f);
			Quaternion nextRotation = Quaternion.Euler(eulerAngles);
			InterpolateRigidbodyTransform(nextPosition, nextRotation);
			yield return new WaitForFixedUpdate();
		}
		base.transform.position = targetPosition;
		base.transform.rotation = targetRotation;
		base.Rigidbody.ClearVelocity();
		board.MoveNoteToTop(this);
		PhotonView connectedPhotonView = board.gameObject.GetComponentInParent<PhotonView>();
		AttachToObject(connectedPhotonView);
	}

	private void InterpolateRigidbodyTransform(Vector3 position, Quaternion rotation)
	{
		Vector3 vector = (position - base.Rigidbody.position) / Time.fixedDeltaTime;
		Vector3 vector2 = UnityExtensions.AngularVelocityFromTo(base.Rigidbody.rotation, rotation) / Time.fixedDeltaTime;
		base.Rigidbody.velocity = vector.ValueOrZeroIfBogus();
		base.Rigidbody.angularVelocity = vector2.ValueOrZeroIfBogus();
		base.Rigidbody.MovePosition(position.ValueOrZeroIfBogus());
		base.Rigidbody.MoveRotation(rotation.ValueOrIdentityIfBogus());
	}

	private void OnAttach(StickyTool tool, bool suppressAudio)
	{
		ToolCleanup component = GetComponent<ToolCleanup>();
		if (component != null)
		{
			component.ResetCleanupOriginalPosition(base.transform.position, base.transform.rotation);
		}
		if (!suppressAudio)
		{
			AudioManager.Play3DSFX(onAttachAudio, base.transform.position);
		}
	}

	private void OnDetach(StickyTool tool, bool suppressAudio)
	{
		if (!suppressAudio)
		{
			AudioManager.Play3DSFX(onDetachAudio, base.transform.position);
		}
	}
}
