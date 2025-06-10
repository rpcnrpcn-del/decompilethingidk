using UnityEngine;

public class SoccerPowerup : Powerup
{
	public delegate void Pickup(SoccerPowerup thisPowerup, Player pickupPlayer);

	public Pickup PickupEvent;

	private void OnTriggerEnter(Collider collider)
	{
		Player.BodyPart bodyPart;
		Player colliderPlayer = collider.GetColliderPlayer(out bodyPart);
		if (base.IsAlive && colliderPlayer != null && colliderPlayer.isLocal && PickupEvent != null)
		{
			PickupEvent(this, colliderPlayer);
		}
	}
}
