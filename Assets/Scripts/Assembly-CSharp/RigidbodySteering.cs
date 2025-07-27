using Photon;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodySteering : Photon.MonoBehaviour
{
	[SerializeField]
	private float maxLinearSpeed = 5f;

	[SerializeField]
	private float maxSteeringForce = 999f;

	[Header("Avoidance")]
	[SerializeField]
	private Transform avoidanceSearchOrigin;

	[SerializeField]
	private float avoidanceRadius = 0.35f;

	[SerializeField]
	private float maxAvoidanceForce = 50f;

	private Rigidbody rigidbody;

	private int avoidanceCastHitCount;

	private RaycastHit[] avoidanceCastHits = new RaycastHit[5];

	private bool isSteering;

	private Vector3 seekTargetPosition = Vector3.zero;

	private float seekTargetSlowdownRadius = 1f;

	private bool isLooking;

	private Vector3 lookTargetDirection = Vector3.zero;

	private Vector3 velocity = Vector3.zero;

	private const float AVOIDANCE_SEARCH_DISTANCE = 1.5f;

	private bool isAvoidingTarget;

	private Vector3 avoidTargetPoint = Vector3.zero;

	private Vector3 avoidTargetNormal = Vector3.zero;

	public float MaxLinearSpeed { get; set; }

    public Vector3 CurrentVelocity
    {
        get
        {
            return this.velocity;
        }
    }

    protected override void Awake()
	{
		base.Awake();
		MaxLinearSpeed = maxLinearSpeed;
		rigidbody = GetComponent<Rigidbody>();
	}

	public void Seek(Vector3 target, float slowdownDistance = 1f)
	{
		seekTargetPosition = target;
		seekTargetSlowdownRadius = slowdownDistance;
		isSteering = true;
	}

	public void Look(Vector3 target)
	{
		lookTargetDirection = target;
		isLooking = true;
	}

	public void StopSeeking()
	{
		isSteering = false;
		velocity = Vector3.zero;
	}

	public void StopLooking()
	{
		isLooking = false;
	}

	public void StopAll()
	{
		StopSeeking();
		StopLooking();
	}

	private void FixedUpdate()
	{
		if (base.hasAuthority)
		{
			if (isSteering)
			{
				UpdateSteering();
			}
			if (isLooking)
			{
				UpdateLook();
			}
		}
	}

	private void UpdateSteering()
	{
		Vector3 zero = Vector3.zero;
		zero += Seek();
		zero += AvoidCollision();
		zero = Vector3.ClampMagnitude(zero, maxSteeringForce);
		Vector3 vector = zero / rigidbody.mass;
		velocity += vector * Time.fixedDeltaTime;
		velocity = Vector3.ClampMagnitude(velocity, MaxLinearSpeed);
		rigidbody.MovePosition(rigidbody.position + velocity * Time.fixedDeltaTime);
		if (velocity.sqrMagnitude >= 0.001f)
		{
			Vector3 normalized = Vector3.ProjectOnPlane(velocity, Vector3.up).normalized;
			rigidbody.MoveRotation(Quaternion.LookRotation(normalized, Vector3.up));
		}
	}

	private Vector3 Seek()
	{
		Vector3 vector = seekTargetPosition - rigidbody.position;
		float magnitude = vector.magnitude;
		float num = MaxLinearSpeed;
		if (magnitude < seekTargetSlowdownRadius && seekTargetSlowdownRadius >= Mathf.Epsilon)
		{
			num = MaxLinearSpeed * Mathf.Clamp01(magnitude / seekTargetSlowdownRadius);
		}
		Vector3 normalized = vector.normalized;
		vector = normalized * num;
		Vector3 normalized2 = velocity.normalized;
		float num2 = 1f - Mathf.Clamp01(Vector3.Dot(normalized, normalized2));
		Vector3 vector2 = -velocity * num2;
		return vector - velocity + vector2;
	}

	private Vector3 AvoidCollision()
	{
		Vector3 direction = base.transform.forward;
		if (velocity.sqrMagnitude >= Mathf.Epsilon)
		{
			direction = velocity.normalized;
		}
		Vector3 result = Vector3.zero;
		RaycastHit hit;
		if (TryGetAvoidanceTarget(direction, 1.5f, out hit))
		{
			result = hit.normal * maxAvoidanceForce;
			isAvoidingTarget = true;
			avoidTargetPoint = hit.point;
			avoidTargetNormal = hit.normal;
		}
		else
		{
			isAvoidingTarget = false;
		}
		return result;
	}

	private bool TryGetAvoidanceTarget(Vector3 direction, float maxDistance, out RaycastHit hit)
	{
		bool result = false;
		hit = default(RaycastHit);
		if (avoidanceSearchOrigin != null)
		{
			avoidanceCastHitCount = Physics.SphereCastNonAlloc(avoidanceSearchOrigin.position, avoidanceRadius, direction, avoidanceCastHits, maxDistance, 227948032);
			for (int i = 0; i < avoidanceCastHitCount; i++)
			{
				if (avoidanceCastHits[i].point.sqrMagnitude > 0.001f && avoidanceCastHits[i].rigidbody != rigidbody)
				{
					hit = avoidanceCastHits[i];
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private void UpdateLook()
	{
		rigidbody.MoveRotation(Quaternion.LookRotation(lookTargetDirection, Vector3.up));
	}

	private void OnDrawGizmos()
	{
		if (avoidanceSearchOrigin != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(avoidanceSearchOrigin.position, avoidanceRadius);
		}
		if (isSteering)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(base.transform.position, seekTargetPosition);
			Gizmos.DrawWireSphere(seekTargetPosition, 0.25f);
		}
	}
}
