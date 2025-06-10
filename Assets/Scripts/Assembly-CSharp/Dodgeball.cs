using UnityEngine;

public class Dodgeball : Ball
{
	public delegate void DodgeballCatch(Dodgeball thisDodgeball, Player catchPlayer, Player thrower, Vector3 catchPoint);

	public delegate void DodgeballHitPlayer(Dodgeball thisDodgeball, Player thrower, Player hitPlayer, Vector3 hitPoint);

	[SerializeField]
	private ParticleSystem fireTrailParticles;

	private SynchronizedField<bool> isAlive;

	private SynchronizedField<int> throwerId;

	private SynchronizedField<int> firstHitPlayerId;

	private SynchronizedField<Vector3> firstHitPlayerPoint;

	public DodgeballCatch DodgeballCatchEvent;

	public DodgeballHitPlayer DodgeballPlayerOutEvent;

	public DodgeballHitPlayer DodgeballPlayerHitEvent;

	private bool IsAlive
	{
		get
		{
			return isAlive.Get();
		}
		set
		{
			isAlive.ForceSet(value);
		}
	}

	private PhotonPlayer Thrower
	{
		get
		{
			return PhotonPlayer.Find(throwerId.Get());
		}
		set
		{
			throwerId.ForceSet((value == null) ? PhotonPlayer.Invalid : value.ID);
		}
	}

	private PhotonPlayer FirstHitPlayer
	{
		get
		{
			return PhotonPlayer.Find(firstHitPlayerId.Get());
		}
		set
		{
			firstHitPlayerId.ForceSet((value == null) ? PhotonPlayer.Invalid : value.ID);
		}
	}

	private Vector3 FirstHitPlayerPoint
	{
		get
		{
			return firstHitPlayerPoint.Get();
		}
		set
		{
			firstHitPlayerPoint.ForceSet(value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		isAlive = new SynchronizedField<bool>(this, "IS_ALIVE", true, SetterPermissionMode.AUTHORITY, UpdateAliveDeadState);
		throwerId = new SynchronizedField<int>(this, "THROWER_ID", PhotonPlayer.Invalid, SetterPermissionMode.AUTHORITY);
		firstHitPlayerId = new SynchronizedField<int>(this, "FIRST_HIT_ID", PhotonPlayer.Invalid, SetterPermissionMode.AUTHORITY);
		firstHitPlayerPoint = new SynchronizedField<Vector3>(this, "FIRST_HIT_POINT", Vector3.zero, SetterPermissionMode.AUTHORITY);
	}

	protected override void Start()
	{
		base.Start();
		UpdateAliveDeadState();
	}

	protected override void OnReset(Vector3 position, Quaternion rotation, bool wasEnabled, Vector3 oldPosition, bool wasCleanedUp)
	{
		base.OnReset(position, rotation, wasEnabled, oldPosition, wasCleanedUp);
		if (base.hasAuthority)
		{
			Thrower = null;
			FirstHitPlayer = null;
			IsAlive = false;
		}
	}

	public override void Pickup(Player player, Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		base.Pickup(player, target, targetSpacePickupPosition, targetSpacePickupRotation);
		if (base.hasAuthority)
		{
			if (IsAlive && Thrower != player.PhotonPlayer && DodgeballCatchEvent != null)
			{
				DodgeballCatchEvent(this, player, Thrower.ToPlayer(), base.transform.position);
			}
			Thrower = null;
			FirstHitPlayer = null;
			IsAlive = false;
		}
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		base.Release(player, linearVelocity, angularVelocity);
		if (base.hasAuthority)
		{
			Thrower = player.PhotonPlayer;
			FirstHitPlayer = null;
			IsAlive = true;
		}
	}

	protected override void OnPlayerCollisionEnter(Player hitPlayer, Player.BodyPart bodyPart, Vector3 point, Collision collision)
	{
		base.OnPlayerCollisionEnter(hitPlayer, bodyPart, point, collision);
		if (!base.hasAuthority || base.IsHeld || !IsAlive || Thrower == null)
		{
			return;
		}
		if (FirstHitPlayer == null)
		{
			FirstHitPlayer = hitPlayer.PhotonPlayer;
			FirstHitPlayerPoint = point;
			if (FirstHitPlayer != null && DodgeballPlayerHitEvent != null)
			{
				DodgeballPlayerHitEvent(this, Thrower.ToPlayer(), FirstHitPlayer.ToPlayer(), FirstHitPlayerPoint);
			}
		}
		else if (hitPlayer.PhotonPlayer != FirstHitPlayer)
		{
			if (DodgeballPlayerOutEvent != null)
			{
				DodgeballPlayerOutEvent(this, Thrower.ToPlayer(), FirstHitPlayer.ToPlayer(), FirstHitPlayerPoint);
			}
			IsAlive = false;
		}
	}

	protected override void OnToolCollisionEnter(Tool hitTool, Vector3 point, Collision collision)
	{
		base.OnToolCollisionEnter(hitTool, point, collision);
		if (base.hasAuthority)
		{
			if (FirstHitPlayer != null && DodgeballPlayerOutEvent != null)
			{
				DodgeballPlayerOutEvent(this, Thrower.ToPlayer(), FirstHitPlayer.ToPlayer(), FirstHitPlayerPoint);
			}
			IsAlive = false;
		}
	}

	protected override void OnOtherCollisionEnter(Layers hitLayer, Vector3 point, Collision collision)
	{
		base.OnOtherCollisionEnter(hitLayer, point, collision);
		if (base.hasAuthority)
		{
			if (FirstHitPlayer != null && DodgeballPlayerOutEvent != null)
			{
				DodgeballPlayerOutEvent(this, Thrower.ToPlayer(), FirstHitPlayer.ToPlayer(), FirstHitPlayerPoint);
			}
			IsAlive = false;
		}
	}

	private void UpdateAliveDeadState()
	{
		bool flag = IsAlive;
		SupportsAssistedCatching = !flag;
		if (flag)
		{
			fireTrailParticles.Play();
		}
		else
		{
			fireTrailParticles.Stop();
		}
	}
}
