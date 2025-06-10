using Photon;

public class ProximityOwnershipTransfer : MonoBehaviour
{
	private void Update()
	{
		if (base.hasAuthority)
		{
			Player player = Player.FindClosest(base.transform.position);
			if (player != null && player.photonView.ownerId != base.photonView.ownerId)
			{
				base.photonView.TransferOwnership(player.photonView.ownerId);
			}
		}
	}
}
