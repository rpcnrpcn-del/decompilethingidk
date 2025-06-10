using UnityEngine;

public class MeshExtruderTool : Tool
{
	private const float TRIGGER_RELEASE_SPEED = 5f;

	[Header("Extrusion")]
	[SerializeField]
	private Transform extrusionPoint;

	[Tooltip("Amount of time between updates to the mesh")]
	[SerializeField]
	private float addControlPointInterval = 0.25f;

	private float lastControlPointAddTime;

	private Animator animator;

	private static readonly int triggerId = Animator.StringToHash("Trigger");

	private float _triggerAmount;

	private SynchronizedField<int> _extrusionId;

	public Transform ExtrusionPoint
	{
		get
		{
			return extrusionPoint;
		}
	}

	private float TriggerAmount
	{
		get
		{
			return _triggerAmount;
		}
		set
		{
			_triggerAmount = value;
			if (animator != null && animator.isActiveAndEnabled)
			{
				animator.SetFloat(triggerId, _triggerAmount);
			}
		}
	}

	private int extrusionId
	{
		get
		{
			return _extrusionId.Get();
		}
		set
		{
			_extrusionId.ForceSet(value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_extrusionId = new SynchronizedField<int>(this, "EXTRUSION_ID", -1, SetterPermissionMode.ANYONE);
		animator = GetComponentInChildren<Animator>();
	}

	private void Update()
	{
		if (base.hasAuthority)
		{
			TriggerAmount = InputAmount;
			animator.SetFloat(triggerId, TriggerAmount);
			UpdateExtrusion();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		TriggerAmount = 0f;
	}

	public override void OnInputDown()
	{
		base.OnInputDown();
		if (base.hasAuthority)
		{
			StartExtrudingMesh();
		}
	}

	public override void OnInputUp()
	{
		base.OnInputUp();
		if (base.hasAuthority)
		{
			StopExtrudingMesh();
		}
	}

	protected override void OnUnlock()
	{
		base.OnUnlock();
		if (base.hasAuthority)
		{
			FinishExtrusion();
		}
	}

	public override void OnPlayerTeleport(float duration)
	{
		base.OnPlayerTeleport(duration);
		if (base.hasAuthority)
		{
			StopExtrudingMesh();
		}
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		if (base.hasAuthority)
		{
			FinishExtrusion();
		}
		base.Release(player, linearVelocity, angularVelocity);
	}

	protected override void OnPhotonPlayerDisconnected(PhotonPlayer oldPlayer)
	{
		if (base.hasAuthority && base.Owner.PhotonPlayer == oldPlayer)
		{
			FinishExtrusion();
		}
		base.OnPhotonPlayerDisconnected(oldPlayer);
	}

	private void UpdateExtrusion()
	{
		Extrusion extrusion = GetExtrusion();
		if (extrusion != null && extrusion.IsExtrudingMesh)
		{
			if (!base.IsHeld)
			{
				StopExtrudingMesh();
			}
			else if (Time.time - lastControlPointAddTime > addControlPointInterval)
			{
				lastControlPointAddTime = Time.time;
				extrusion.MasterAddControlPoint();
			}
		}
	}

	private void StartExtrudingMesh()
	{
		Extrusion extrusion = GetExtrusion();
		if (extrusion == null)
		{
			extrusion = Extrusion.CreateNewExtrusion(this);
			extrusion.ExtrusionFinishedEvent += OnExtrusionFinished;
			SetExtrusion(extrusion);
		}
		if (!extrusion.IsExtrudingMesh)
		{
			lastControlPointAddTime = Time.time;
			extrusion.MasterStartExtrudingNewMesh();
		}
	}

	private void StopExtrudingMesh()
	{
		Extrusion extrusion = GetExtrusion();
		if (extrusion != null && extrusion.IsExtrudingMesh)
		{
			extrusion.MasterFinishExtrudingCurrentMesh();
		}
	}

	private void FinishExtrusion()
	{
		Extrusion extrusion = GetExtrusion();
		if (extrusion != null)
		{
			if (extrusion.IsExtrudingMesh)
			{
				extrusion.MasterFinishExtrudingCurrentMesh();
			}
			extrusion.MasterFinishExtrusion();
		}
	}

	private void OnExtrusionFinished()
	{
		Extrusion extrusion = GetExtrusion();
		if (extrusion != null)
		{
			extrusion.ExtrusionFinishedEvent -= OnExtrusionFinished;
			SetExtrusion(null);
		}
	}

	private Extrusion GetExtrusion()
	{
		if (extrusionId < 0)
		{
			return null;
		}
		PhotonView photonView = PhotonView.Find(extrusionId);
		if (photonView == null)
		{
			return null;
		}
		return photonView.GetComponent<Extrusion>();
	}

	private void SetExtrusion(Extrusion extrusion)
	{
		extrusionId = ((!(extrusion != null)) ? (-1) : extrusion.photonView.viewID);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isReading)
		{
			TriggerAmount = (float)stream.ReceiveNext();
		}
		else
		{
			stream.SendNext(TriggerAmount);
		}
	}
}
