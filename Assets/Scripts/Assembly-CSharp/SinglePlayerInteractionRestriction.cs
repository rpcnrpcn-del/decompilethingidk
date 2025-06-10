public class SinglePlayerInteractionRestriction : PlayerInteractionRestriction
{
	private int allowedPhotonPlayerID = PhotonPlayer.Invalid;

	public override bool PlayerInteractionAllowed(PhotonPlayer player)
	{
		return base.PlayerInteractionAllowed(player) && (allowedPhotonPlayerID == PhotonPlayer.Invalid || allowedPhotonPlayerID == player.ID);
	}

	public void SetPlayer(PhotonPlayer player)
	{
		if (player != null)
		{
			allowedPhotonPlayerID = player.ID;
		}
		else
		{
			ClearPlayer();
		}
	}

	public void ClearPlayer()
	{
		allowedPhotonPlayerID = PhotonPlayer.Invalid;
	}
}
