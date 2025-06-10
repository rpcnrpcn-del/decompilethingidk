using UnityEngine;

public class DiscGolfPowerUp : Powerup
{
	public delegate void Pickup(DiscGolfPowerUp thisPowerup, DiscGolfDisc disc, Player discThrower);

	[Header("Hole")]
	[SerializeField]
	private DiscGolfHole hole = DiscGolfHole.INVALID;

	[SerializeField]
	private int scoreAdjust = -1;

	[SerializeField]
	private ParticleSystem hitParticles;

	[SerializeField]
	private RecRoomAudioClip onHit;

	private CollisionForwarder collisionForwarder;

	public Pickup PickupEvent;

	public DiscGolfHole Hole
	{
		get
		{
			return hole;
		}
	}

	public int ScoreAdjust
	{
		get
		{
			return scoreAdjust;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		collisionForwarder = GetComponentInChildren<CollisionForwarder>();
		if (collisionForwarder != null)
		{
			collisionForwarder.TriggerEnter += OnTriggerEnter;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (collisionForwarder != null)
		{
			collisionForwarder.TriggerEnter -= OnTriggerEnter;
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		Tool colliderTool = collider.GetColliderTool();
		DiscGolfDisc discGolfDisc = ((!(colliderTool != null)) ? null : colliderTool.GetComponent<DiscGolfDisc>());
		if (base.IsAlive && discGolfDisc != null && discGolfDisc.Thrower != null && discGolfDisc.Thrower.isLocal && PickupEvent != null)
		{
			PickupEvent(this, discGolfDisc, discGolfDisc.Thrower.ToPlayer());
		}
	}

	public void OnPickup()
	{
		if (hitParticles != null)
		{
			hitParticles.Play();
			AudioManager.Play3DSFX(onHit, base.transform.position);
		}
	}
}
