using System;
using UnityEngine;

public class TearStripTab : Tool
{
	[Header("Tear Strip")]
	[SerializeField]
	private RecRoomAudioClip poppingSound;

	[SerializeField]
	private SkinnedMeshRenderer meshRenderer;

	[SerializeField]
	private Transform transformWhenTearing;

	private bool torn;

	private bool tearing;

	private ToolCleanup toolCleanup;

	private int linksPopped;

	private Transform[] linkBones;

	private BezierPath stripPath;

	private Vector3 unbrokenStripEnd;

	private float unbrokenStripLength;

	private float maxStripLength;

	private Vector3 unbrokenStripTangent;

	private float linkLength;

	public AxisConstrainedRigidbodyPickup ConstrainedRigidbodyPickup { get; private set; }

	public override bool OwnershipTransferAllowed
	{
		get
		{
			return false;
		}
	}

	public event Action<TearStripTab> TornOff;

	protected override void Awake()
	{
		base.Awake();
		ConstrainedRigidbodyPickup = GetComponent<AxisConstrainedRigidbodyPickup>();
		base.OnlyOwnerCanPickup = true;
		if (meshRenderer != null)
		{
			linkBones = meshRenderer.bones;
		}
		stripPath = new BezierPath();
		if (linkBones.Length > 0)
		{
			unbrokenStripTangent = linkBones[0].up;
			unbrokenStripEnd = linkBones.LastItem().position;
			maxStripLength = (linkBones[0].position - unbrokenStripEnd).magnitude;
			linkLength = maxStripLength / (float)(linkBones.Length - 1);
			unbrokenStripLength = maxStripLength - linkLength;
			ConstrainedRigidbodyPickup.sphericalRestrictionCenter = unbrokenStripEnd;
			ConstrainedRigidbodyPickup.sphericalRestrictionRadius = linkLength;
			ConstrainedRigidbodyPickup.constrainToSphere = true;
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!tearing || torn || !(base.HolderHand != null))
		{
			return;
		}
		Vector3 lhs = base.HolderHand.transform.position - unbrokenStripEnd;
		float num = maxStripLength - unbrokenStripLength;
		if (linksPopped < linkBones.Length - 1 && Vector3.Dot(lhs, base.transform.parent.right) > linkLength && lhs.sqrMagnitude > num * num)
		{
			unbrokenStripLength = Mathf.Max(unbrokenStripLength - linkLength, 0f);
			PopLink();
			linksPopped++;
			if (linksPopped >= linkBones.Length - 1)
			{
				TearOff();
			}
		}
		stripPath.Clear();
		float magnitude = (base.transform.position - unbrokenStripEnd).magnitude;
		unbrokenStripEnd = linkBones[0].position + unbrokenStripTangent * unbrokenStripLength;
		Vector3 controlPoint = unbrokenStripEnd + unbrokenStripTangent * magnitude / 5f;
		Vector3 controlPoint2 = base.transform.position - lhs.normalized * magnitude / 3f;
		stripPath.AddCurve(unbrokenStripEnd, controlPoint, controlPoint2, base.transform.position);
		Transform transform = null;
		int num2 = linkBones.Length - linksPopped - 1;
		for (int i = num2; i < linkBones.Length - 1; i++)
		{
			transform = linkBones[i];
			transform.position = stripPath.EvaluatePath((float)(i - num2) / (float)linksPopped);
			Vector3 zero = Vector3.zero;
			Vector3 normalized = Vector3.Cross(rhs: (i != linkBones.Length - 2) ? (linkBones[i + 1].position - transform.position) : (base.transform.position - transform.position), lhs: base.transform.up).normalized;
			transform.LookAt(transform.position + normalized);
		}
		ConstrainedRigidbodyPickup.sphericalRestrictionCenter = unbrokenStripEnd;
		ConstrainedRigidbodyPickup.sphericalRestrictionRadius = num;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void Pickup(Player player, Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		base.Pickup(player, target, targetSpacePickupPosition, targetSpacePickupRotation);
		tearing = true;
		if (transformWhenTearing != null)
		{
			base.transform.SetParent(transformWhenTearing);
		}
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		base.Release(player, Vector3.zero, Vector3.zero);
		if (torn)
		{
			base.Rigidbody.useGravity = true;
		}
		else
		{
			base.Rigidbody.isKinematic = true;
		}
	}

	protected override void OnReset(Vector3 position, Quaternion rotation, bool wasEnabled, Vector3 oldPosition, bool wasCleanedUp)
	{
		base.OnReset(position, rotation, wasEnabled, oldPosition, wasCleanedUp);
		PhotonNetwork.Destroy(base.gameObject);
	}

	private void TearOff()
	{
		ConstrainedRigidbodyPickup.SetAllConstraints(false);
		meshRenderer.enabled = false;
		torn = true;
		if (this.TornOff != null)
		{
			this.TornOff(this);
		}
	}

	private void PopLink()
	{
		AudioManager.Play3DSFX(poppingSound, base.transform.position);
		if (base.HolderHand != null && base.HolderHand.isLocal)
		{
			base.HolderHand.Vibrate(50, 1000);
		}
	}
}
