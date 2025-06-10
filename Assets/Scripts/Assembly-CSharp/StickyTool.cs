using UnityEngine;

public class StickyTool : Tool
{
	public delegate void StickyToolAttachDelegate(StickyTool tool, bool suppressAudio);

	[Header("Sticky Tool")]
	[SerializeField]
	private PhotonView initiallyAttachedTo;

	private bool isAttached;

	private int attachViewID;

	private Vector3 attachRelativePosition;

	private Quaternion attachRelativeRotation;

	private PhotonView _attachPhotonViewField;

	private PhotonView attachPhotonView
	{
		get
		{
			return _attachPhotonViewField;
		}
		set
		{
			if (_attachPhotonViewField != value)
			{
				if (_attachPhotonViewField != null)
				{
					_attachPhotonViewField.OwnerChangedEvent -= AttachOwnerChanged;
				}
				_attachPhotonViewField = value;
				if (_attachPhotonViewField != null)
				{
					_attachPhotonViewField.OwnerChangedEvent += AttachOwnerChanged;
				}
			}
		}
	}

	public event StickyToolAttachDelegate AttachEvent;

	public event StickyToolAttachDelegate DetachEvent;

	protected override void Awake()
	{
		base.Awake();
		AttachToObject(initiallyAttachedTo, true, false);
	}

	protected override void Start()
	{
		base.Start();
		base.ResetEvent += OnReset;
	}

	protected override void OnDestroy()
	{
		Detach(true);
		base.OnDestroy();
	}

	private void OnReset(Tool tool, Vector3 position, Quaternion rotation)
	{
		AttachToObject(initiallyAttachedTo, true, false);
	}

	protected virtual void LateUpdate()
	{
		if (isAttached && attachPhotonView != null)
		{
			base.transform.position = attachPhotonView.transform.TransformPoint(attachRelativePosition);
			base.transform.rotation = attachPhotonView.transform.TransformRotation(attachRelativeRotation);
		}
	}

	public override void Pickup(Player player, Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		Detach(false);
		base.Pickup(player, target, targetSpacePickupPosition, targetSpacePickupRotation);
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		base.Release(player, linearVelocity, angularVelocity);
	}

	protected void AttachToObject(PhotonView connectedPhotonView, bool suppressAudio = false, bool broadcast = true)
	{
		if (connectedPhotonView != null && connectedPhotonView.ownerId != base.photonView.ownerId)
		{
			base.photonView.TransferOwnership(connectedPhotonView.ownerId);
		}
		int num = ((!(connectedPhotonView != null)) ? (-1) : connectedPhotonView.viewID);
		Vector3 vector = ((!(connectedPhotonView != null)) ? base.transform.position : connectedPhotonView.transform.InverseTransformPoint(base.transform.position));
		Quaternion quaternion = ((!(connectedPhotonView != null)) ? base.transform.rotation : connectedPhotonView.transform.InverseTransformRotation(base.transform.rotation));
		if (broadcast)
		{
			base.photonView.RPC("RpcAttachToObject", PhotonTargets.All, num, vector, quaternion, suppressAudio);
		}
		else
		{
			RpcAttachToObject(num, vector, quaternion, suppressAudio);
		}
	}

	protected void Detach(bool suppressAudio)
	{
		if (isAttached)
		{
			isAttached = false;
			base.Rigidbody.isKinematic = false;
			attachPhotonView = null;
			if (this.DetachEvent != null)
			{
				this.DetachEvent(this, suppressAudio);
			}
		}
	}

	[PunRPC]
	public void RpcAttachToObject(int viewId, Vector3 relativePosition, Quaternion relativeRotation, bool suppressAudio)
	{
		if (isAttached && this.DetachEvent != null)
		{
			this.DetachEvent(this, suppressAudio);
		}
		isAttached = true;
		attachViewID = viewId;
		attachRelativePosition = relativePosition.ValueOrZeroIfBogus();
		attachRelativeRotation = relativeRotation.ValueOrIdentityIfBogus();
		attachPhotonView = ((viewId == -1) ? null : PhotonView.Find(viewId));
		base.Rigidbody.isKinematic = true;
		base.transform.position = ((!(attachPhotonView != null)) ? attachRelativePosition : attachPhotonView.transform.TransformPoint(attachRelativePosition));
		base.transform.rotation = ((!(attachPhotonView != null)) ? attachRelativeRotation : attachPhotonView.transform.TransformRotation(attachRelativeRotation));
		base.Rigidbody.ClearVelocity();
		if (this.AttachEvent != null)
		{
			this.AttachEvent(this, suppressAudio);
		}
	}

	private void AttachOwnerChanged(int newOwnerId)
	{
		base.photonView.TransferOwnership(newOwnerId);
	}

	private void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		if (base.hasAuthority && isAttached)
		{
			base.photonView.RPC("RpcAttachToObject", newPlayer, attachViewID, attachRelativePosition, attachRelativeRotation, true);
		}
	}
}
