using UnityEngine;

public class PaintballGrenade : Weapon
{
	[Header("PaintballGrenade")]
	[SerializeField]
	private Transform visualRoot;

	public PooledParticle ExplosionBluePrefab;

	public PooledParticle ExplosionRedPrefab;

	public ParticleSystem ArmedParticle;

	public float ExplosionTimeout = 5f;

	public float ExplosionRadius = 5f;

	[Header("Audio")]
	[SerializeField]
	private RecRoomAudioClip explodeAudio;

	[SerializeField]
	private RecRoomAudioClip activeAudio;

	[SerializeField]
	private RecRoomAudioClip countdownAudio;

	[SerializeField]
	private AnimationCurve explosionTimeLeftToCountdownAudioCurve;

	private SynchronizedField<int> _throwerId;

	private SynchronizedField<int> _explodeTime;

	private ToolCleanup _toolCleanup;

	private Player thrower;

	private const int MAX_RAYCAST_HIT = 256;

	private static Collider[] hitColliders = new Collider[256];

	private float lastTimeRemaining;

	private SFXAudioSource armedAudioSource;

	public int ThrowerId
	{
		get
		{
			return _throwerId.Get();
		}
		set
		{
			_throwerId.ForceSet(value);
		}
	}

	public int ExplodeTime
	{
		get
		{
			return _explodeTime.Get();
		}
		set
		{
			_explodeTime.ForceSet(value);
		}
	}

	private ToolCleanup toolCleanup
	{
		get
		{
			if (_toolCleanup == null)
			{
				_toolCleanup = GetComponent<ToolCleanup>();
			}
			return _toolCleanup;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_throwerId = new SynchronizedField<int>(this, "Grenade.Thrower", PhotonPlayer.Invalid, SetterPermissionMode.ANYONE, IsThrowerChanged);
		_explodeTime = new SynchronizedField<int>(this, "Grenade.ExplodeTime", 0, SetterPermissionMode.ANYONE);
	}

	protected override void Start()
	{
		base.Start();
		if (base.hasAuthority)
		{
			ThrowerId = PhotonPlayer.Invalid;
			ExplodeTime = 0;
		}
	}

	protected void Update()
	{
		if (thrower != null && ExplodeTime != 0 && visualRoot.gameObject.activeSelf)
		{
			float num = (float)(ExplodeTime - PhotonNetwork.ServerTimestamp) / 1000f;
			if (num > 0f)
			{
				Countdown(num);
			}
			else if (num <= 0f)
			{
				Explode();
			}
		}
	}

	private void Countdown(float timeRemaining)
	{
		float num = explosionTimeLeftToCountdownAudioCurve.Evaluate(timeRemaining / ExplosionTimeout);
		if (lastTimeRemaining - timeRemaining > num)
		{
			lastTimeRemaining = timeRemaining;
			AudioManager.Play3DSFX(countdownAudio, base.transform.position);
		}
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		StartCooking(player);
		base.Release(player, linearVelocity, angularVelocity);
	}

	protected override void OnReset(Vector3 position, Quaternion rotation, bool wasEnabled, Vector3 oldPosition, bool wasCleanedUp)
	{
		visualRoot.gameObject.SetActive(true);
		base.Rigidbody.useGravity = true;
		if (base.hasAuthority)
		{
			ThrowerId = PhotonPlayer.Invalid;
			ExplodeTime = 0;
		}
		base.OnReset(position, rotation, wasEnabled, oldPosition, wasCleanedUp);
	}

	private void IsThrowerChanged()
	{
		PhotonPlayer photonPlayer = PhotonPlayer.Find(ThrowerId);
		thrower = ((photonPlayer == null) ? null : photonPlayer.ToPlayer());
		bool armed = photonPlayer != null;
		lastTimeRemaining = ExplosionTimeout;
		SetArmed(armed);
	}

	private void SetArmed(bool isArmed)
	{
		if (ArmedParticle.gameObject.activeSelf != isArmed)
		{
			ArmedParticle.gameObject.SetActive(isArmed);
			ParticleSystem.MainModule main = ArmedParticle.main;
			main.startColor = base.ToolRenderer.AccentColor;
			if (isArmed)
			{
				armedAudioSource = AudioManager.Play3DSFX(activeAudio, base.transform);
			}
			else
			{
				armedAudioSource.Stop();
				armedAudioSource.Release();
				armedAudioSource = null;
			}
		}
		if (toolCleanup != null)
		{
			toolCleanup.enabled = !isArmed;
		}
	}

	public void StartCooking(Player player, bool explodeImmediate = false)
	{
		if (thrower == null && player.isLocal)
		{
			ThrowerId = player.PhotonPlayer.ID;
			base.ToolRenderer.UpdateTeamAccentColor();
			ExplodeTime = PhotonNetwork.ServerTimestamp + (int)(ExplosionTimeout * 1000f);
		}
		if (explodeImmediate && ExplodeTime > PhotonNetwork.ServerTimestamp)
		{
			ExplodeTime = PhotonNetwork.ServerTimestamp;
		}
	}

	private void Explode()
	{
		visualRoot.gameObject.SetActive(false);
		base.Rigidbody.useGravity = false;
		base.Rigidbody.velocity = Vector3.zero;
		if (base.IsHeld && base.Owner.isLocal)
		{
			base.Owner.ToolController.ReleaseTool(this);
		}
		if (thrower != null)
		{
			PooledParticle pooledParticle = ObjectPool.Instance.Acquire((thrower.Team != GameTeam.TEAM_1) ? ExplosionRedPrefab : ExplosionBluePrefab);
			if (pooledParticle != null)
			{
				pooledParticle.transform.position = base.transform.position;
				pooledParticle.transform.rotation = base.transform.rotation;
				pooledParticle.Play();
			}
			float num = 60f;
			int num2 = Physics.OverlapSphereNonAlloc(base.transform.position, ExplosionRadius, hitColliders, 227948032, QueryTriggerInteraction.Ignore);
			for (int i = 0; i < num2; i++)
			{
				Vector3 vector = hitColliders[i].ClosestPointOnBounds(base.transform.position);
				Vector3 normalized = (vector - base.transform.position).normalized;
				RaycastHit hitInfo;
				if (Physics.Raycast(vector, normalized, out hitInfo, 0.25f, 227948032, QueryTriggerInteraction.Ignore))
				{
					float num3 = ((!(hitInfo.rigidbody != null)) ? 1f : hitInfo.rigidbody.mass);
					Vector3 collisionForce = normalized * num * num3 * (1f + 1f / hitInfo.distance);
					OnRaycastCollisionEnter(collisionForce, hitInfo);
				}
			}
			RaycastHit hitInfo2;
			if (Physics.Raycast(base.transform.position, Vector3.down, out hitInfo2, 1f, 2048))
			{
				SpawnImpactDecal(hitInfo2.point, hitInfo2.normal, hitInfo2.collider.gameObject, new Vector2(6f, 7f));
			}
		}
		AudioManager.Play3DSFX(explodeAudio, base.transform.position);
		thrower = null;
		SetArmed(false);
	}

	private void OnRaycastCollisionEnter(Vector3 collisionForce, RaycastHit raycastCollision)
	{
		GameObject hitGameObject = ((!(raycastCollision.rigidbody != null)) ? raycastCollision.collider.gameObject : raycastCollision.rigidbody.gameObject);
		Player player = null;
		Tool tool = null;
		Player.BodyPart bodyPart;
		if ((player = raycastCollision.collider.GetColliderPlayer(out bodyPart)) != null)
		{
			OnPlayerImpact(thrower, player, bodyPart, hitGameObject, raycastCollision.point, Vector3.zero, raycastCollision.normal);
		}
		else if ((tool = raycastCollision.collider.GetColliderTool()) != null)
		{
			OnToolImpact(thrower, tool, collisionForce, hitGameObject, raycastCollision.point, Vector3.zero, raycastCollision.normal);
		}
		else
		{
			OnOtherImpact(thrower, hitGameObject, raycastCollision.point, Vector3.zero, raycastCollision.normal);
		}
	}
}
