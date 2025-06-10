using UnityEngine;

public class PlayerCollider : RecRoomCollider
{
	[SerializeField]
	protected Player.BodyPart bodyPart;

	public bool IsTrigger;

	[HideInInspector]
	public Player ThisPlayer;

	public Player.BodyPart BodyPart
	{
		get
		{
			return bodyPart;
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		Player.BodyPart otherPlayerBodyPart = Player.BodyPart.None;
		Player colliderPlayer = collider.GetColliderPlayer(out otherPlayerBodyPart);
		if (colliderPlayer != null)
		{
			ThisPlayer.PlayerEvents.PlayerHit(BodyPart, colliderPlayer, otherPlayerBodyPart);
		}
	}
}
