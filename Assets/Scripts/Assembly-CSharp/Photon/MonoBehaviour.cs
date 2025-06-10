using UnityEngine;

namespace Photon
{
	public class MonoBehaviour : UnityEngine.MonoBehaviour
	{
		private PhotonView pvCache;

		public PhotonView photonView
		{
			get
			{
				if (pvCache == null)
				{
					pvCache = PhotonView.Get(this);
				}
				return pvCache;
			}
		}

		public bool isLocal
		{
			get
			{
				return !(photonView != null) || photonView.isLocal;
			}
		}

		public bool hasAuthority
		{
			get
			{
				return !(photonView != null) || photonView.isMine;
			}
		}

		public bool hasOwnership
		{
			get
			{
				return photonView != null && photonView.isMineNoMaster;
			}
		}

		public PhotonPlayer authority
		{
			get
			{
				return photonView.owner ?? PhotonNetwork.masterClient;
			}
		}

		public PhotonPlayer owner
		{
			get
			{
				return photonView.owner;
			}
		}

		protected virtual void Awake()
		{
			PhotonNetwork.AddMonoMessageTarget(base.gameObject);
		}

		protected virtual void OnDestroy()
		{
			PhotonNetwork.RemoveMonoMessageTarget(base.gameObject);
		}
	}
}
