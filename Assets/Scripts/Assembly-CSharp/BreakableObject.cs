using UnityEngine;

public class BreakableObject : Tool
{
	[Header("Meshes")]
	[SerializeField]
	private GameObject wholeObject;

	[SerializeField]
	private GameObject fracturedObject;

	[Header("Destruction")]
	[SerializeField]
	private int maxHitPoints = 1;

	[SerializeField]
	private float timeBeforeCleanup = 3f;

	private SynchronizedField<int> _hitPoints;

	private SynchronizedField<Vector3> _lastHitPoint;

	private SynchronizedField<Vector3> _lastHitForce;

	public int HitPoints
	{
		get
		{
			return _hitPoints.Get();
		}
		set
		{
			_hitPoints.ForceSet(value);
		}
	}

	public Vector3 LastHitPoint
	{
		get
		{
			return _lastHitPoint.Get();
		}
		set
		{
			_lastHitPoint.ForceSet(value);
		}
	}

	public Vector3 LastHitForce
	{
		get
		{
			return _lastHitForce.Get();
		}
		set
		{
			_lastHitForce.ForceSet(value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_hitPoints = new SynchronizedField<int>(this, "HP", maxHitPoints, SetterPermissionMode.AUTHORITY, OnHitPointsChanged);
		_lastHitPoint = new SynchronizedField<Vector3>(this, "LASTHIT_POINT", Vector3.zero, SetterPermissionMode.AUTHORITY);
		_lastHitForce = new SynchronizedField<Vector3>(this, "LASTHIT_FORCE", Vector3.zero, SetterPermissionMode.AUTHORITY);
	}

	protected void AuthorityApplyHit(Vector3 hitPoint, Vector3 hitForce, int damage)
	{
		if (base.hasAuthority)
		{
			LastHitPoint = hitPoint;
			LastHitForce = hitForce;
			HitPoints -= damage;
		}
	}

	protected void OnHitPointsChanged()
	{
		if (HitPoints <= 0)
		{
			Fracture();
		}
	}

	protected virtual void Fracture()
	{
		if (wholeObject != null)
		{
			wholeObject.SetActive(false);
		}
		if (!(fracturedObject != null))
		{
			return;
		}
		fracturedObject.SetActive(true);
		if (LastHitPoint != Vector3.zero)
		{
			base.Rigidbody.velocity = Vector3.zero;
			float num = Mathf.Min(10f, LastHitForce.magnitude);
			Rigidbody[] componentsInChildren = fracturedObject.GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody rigidbody in componentsInChildren)
			{
				rigidbody.AddForce((rigidbody.transform.position - LastHitPoint).normalized * num, ForceMode.VelocityChange);
			}
		}
	}

	protected override void OnReset(Vector3 position, Quaternion rotation, bool wasEnabled, Vector3 oldPosition, bool wasCleanedUp)
	{
		base.OnReset(position, rotation, wasEnabled, oldPosition, wasCleanedUp);
		if (base.Owner.isLocal)
		{
			PhotonNetwork.Destroy(base.photonView);
		}
	}
}
