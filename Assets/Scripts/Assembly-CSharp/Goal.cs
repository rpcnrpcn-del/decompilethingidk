using Photon;
using UnityEngine;

public class Goal : Photon.MonoBehaviour
{
	[SerializeField]
	private int ownerPlayerSlot = -1;

	[SerializeField]
	private int ownerTeamSlot = -1;

	public NetworkCollider NetworkCollider { get; private set; }

	public int OwnerPlayerSlot
	{
		get
		{
			return ownerPlayerSlot;
		}
	}

	public int OwnerTeamSlot
	{
		get
		{
			return ownerTeamSlot;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		NetworkCollider = GetComponent<NetworkCollider>();
	}
}
