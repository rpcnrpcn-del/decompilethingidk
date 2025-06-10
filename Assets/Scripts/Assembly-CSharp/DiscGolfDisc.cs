using System;
using UnityEngine;

public class DiscGolfDisc : Tool
{
	public delegate void DiscPickup(DiscGolfDisc disc, Player pickupPlayer);

	public delegate void DiscThrow(DiscGolfDisc disc, Player throwPlayer);

	public delegate void DiscScore(DiscGolfDisc disc, DiscGolfGoal goal, Player thrower);

	public delegate void DiscHazard(DiscGolfDisc disc, Killzone hazard, Player thrower);

	public delegate void DiscStopMoving(DiscGolfDisc disc, Player throwPlayer);

	[Header("Disc Properties")]
	[Tooltip("Any faster than this will not lift the disc any more")]
	public static readonly float MaxAngularVelocity = 20f;

	[Tooltip("Any faster than this will not lift the disc any more")]
	public static readonly float MaxVelocity = 20f;

	[Tooltip("We will help you with the throw speed based on the percent of your current throw to the MaxVelocity. Higher speeds should get less assist.")]
	public AnimationCurve ThrowAssistCurve;

	[Tooltip("We will help you with the throw speed based on the percent of your current throw to the MaxVelocity. Higher speeds should get less assist.")]
	public float ThrowAssistMultiplier = 1f;

	[Header("Aero Dynamics")]
	[Tooltip("How much airlift can be applied to lift the object. 1 means as much as the gravity itself.")]
	[Range(0f, 1f)]
	[SerializeField]
	private float airLiftModifier = 0.85f;

	[Header("Angular Stability (Anti-Wobble)")]
	[Tooltip("How much angular torque to apply overall to stabilize this tool.")]
	[Range(0f, 1f)]
	[SerializeField]
	private float angularStabilityModifier = 0.9f;

	[Tooltip("How stable the rotation will get overtime.")]
	[Range(0f, 1f)]
	[SerializeField]
	private float angularStabilityPercent = 0.3f;

	[Tooltip("How fast the disc is going to get to the percent stability.")]
	[SerializeField]
	private float angularStabilitySpeed = 2f;

	[Header("Curve")]
	[Tooltip("How much curve can be put on the disc (default is 0.5)")]
	[SerializeField]
	private float curveScaleFactor = 0.5f;

	[Header("Beam")]
	[SerializeField]
	private Renderer beamRenderer;

	[SerializeField]
	private Vector3 beamRotationOffset = new Vector3(90f, 0f, 0f);

	[Header("Team")]
	[SerializeField]
	private GameTeam team = GameTeam.INVALID;

	private bool wasShowBeam;

	private float showBeamTime;

	private float differenceZ;

	[NonSerialized]
	public bool BeamEnabled;

	public DiscPickup DiscPickupEvent;

	public DiscThrow DiscThrowEvent;

	public DiscScore DiscScoreEvent;

	public DiscHazard DiscHazardEvent;

	private SynchronizedField<int> throwerId;

	public GameTeam Team
	{
		get
		{
			return team;
		}
	}

	public PhotonPlayer Thrower
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

	public event DiscStopMoving DiscStopMovingEvent;

	protected override void Awake()
	{
		base.Awake();
		throwerId = new SynchronizedField<int>(this, "THROWER_ID", PhotonPlayer.Invalid, SetterPermissionMode.AUTHORITY);
		if (Team != GameTeam.INVALID)
		{
			base.PlayerInteractionRestriction.AllowAllActiveTeams = false;
			base.PlayerInteractionRestriction.AllowedTeams = new GameTeam[1] { Team };
		}
		else
		{
			base.PlayerInteractionRestriction.AllowAllActiveTeams = true;
		}
		BeamEnabled = false;
	}

	protected override void Start()
	{
		base.Start();
		if (Team != GameTeam.INVALID)
		{
			Color teamColor = GameTeamSettings.GetTeamColor(Team);
			base.ToolRenderer.AccentColor = teamColor;
			beamRenderer.material.SetColor("_Color", teamColor);
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		bool flag = false;
		if (!base.IsHeld)
		{
			float magnitude = base.Rigidbody.angularVelocity.magnitude;
			float a = Mathf.Clamp01(magnitude / MaxAngularVelocity);
			float magnitude2 = base.Rigidbody.velocity.magnitude;
			float b = Mathf.Clamp01(magnitude2 / MaxVelocity);
			float num = Vector3.Dot(Vector3.up, base.transform.up);
			float num2 = num * Mathf.Max(a, b) * base.Rigidbody.mass;
			if (num2 > 0.01f)
			{
				base.Rigidbody.AddForce(-Physics.gravity * num2 * airLiftModifier, ForceMode.Force);
				base.Rigidbody.AddForce(Vector3.Cross(base.Rigidbody.velocity, Vector3.up) * differenceZ, ForceMode.Force);
				float num3 = num2 * angularStabilityModifier;
				Vector3 lhs = Quaternion.AngleAxis(num3 * magnitude * 57.29578f * angularStabilityPercent / angularStabilitySpeed, base.Rigidbody.angularVelocity) * base.transform.up;
				float z = Mathf.Clamp(differenceZ / (curveScaleFactor / 1000f) / 5f, -30f, 30f);
				Vector3 rhs = Quaternion.LookRotation(Vector3.ProjectOnPlane(base.Rigidbody.velocity, Vector3.up)) * Quaternion.Euler(0f, 0f, z) * Vector3.up;
				Vector3 vector = Vector3.Cross(lhs, rhs);
				base.Rigidbody.AddTorque(vector * angularStabilitySpeed * angularStabilitySpeed * num3);
			}
			flag = magnitude < 0.1f && magnitude2 < 0.01f;
		}
		if (!(beamRenderer != null))
		{
			return;
		}
		if (BeamEnabled)
		{
			if (!wasShowBeam && flag)
			{
				showBeamTime = Time.time;
			}
			if (flag && Time.time - showBeamTime > 0.5f)
			{
				beamRenderer.gameObject.SetActive(true);
				beamRenderer.transform.position = base.transform.position;
				beamRenderer.transform.rotation = Quaternion.LookRotation(Vector3.forward) * Quaternion.Euler(beamRotationOffset);
			}
			else
			{
				beamRenderer.gameObject.SetActive(false);
			}
			wasShowBeam = flag;
		}
		else
		{
			beamRenderer.gameObject.SetActive(false);
			wasShowBeam = false;
		}
	}

	public override void Pickup(Player player, Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		base.Pickup(player, target, targetSpacePickupPosition, targetSpacePickupRotation);
		if (base.hasAuthority)
		{
			Thrower = null;
			if (DiscPickupEvent != null)
			{
				DiscPickupEvent(this, player);
			}
		}
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		float num = Mathf.Clamp01(linearVelocity.magnitude / MaxVelocity);
		float num2 = 1f + ThrowAssistCurve.Evaluate(1f - num) * ThrowAssistMultiplier;
		Vector3 normalized = linearVelocity.normalized;
		Vector3 up = base.transform.up;
		Vector3 lhs = Vector3.Cross(normalized, up);
		up = Vector3.Cross(lhs, normalized);
		Vector3 vector = -Physics.gravity;
		vector = Vector3.ProjectOnPlane(vector, normalized);
		float num3 = up.AngleSignedVector3(vector, normalized);
		for (differenceZ = 0f - num3; differenceZ < -180f; differenceZ += 360f)
		{
		}
		while (differenceZ > 180f)
		{
			differenceZ -= 360f;
		}
		float num4 = 5f;
		if (Mathf.Abs(differenceZ) < num4)
		{
			differenceZ = 0f;
		}
		else
		{
			differenceZ *= curveScaleFactor / 1000f;
		}
		base.Release(player, linearVelocity * num2, angularVelocity * num2);
		if (base.hasAuthority)
		{
			Thrower = player.PhotonPlayer;
			if (DiscThrowEvent != null)
			{
				DiscThrowEvent(this, player);
			}
		}
	}

	protected override void OnCollisionEnter(Collision collision)
	{
		base.OnCollisionEnter(collision);
		PhotonView colliderPhotonView = collision.GetColliderPhotonView();
		DiscGolfGoal discGolfGoal = ((!(colliderPhotonView != null)) ? null : colliderPhotonView.GetComponent<DiscGolfGoal>());
		if (Thrower != null && Thrower.isLocal && discGolfGoal != null && DiscScoreEvent != null)
		{
			DiscScoreEvent(this, discGolfGoal, Thrower.ToPlayer());
		}
	}

	protected override void ResetOnKillzone(Killzone killzone)
	{
		base.ResetOnKillzone(killzone);
		if (Thrower != null && Thrower.isLocal && DiscHazardEvent != null)
		{
			DiscHazardEvent(this, killzone, Thrower.ToPlayer());
		}
	}

	protected override void OnStopMoving()
	{
		base.OnStopMoving();
		if (Thrower != null && Thrower.isLocal && this.DiscStopMovingEvent != null)
		{
			this.DiscStopMovingEvent(this, Thrower.ToPlayer());
		}
	}
}
