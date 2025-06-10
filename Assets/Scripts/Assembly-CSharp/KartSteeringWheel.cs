using UnityEngine;
using UnityEngine.UI;

public class KartSteeringWheel : Tool
{
	[Header("Steering Wheel")]
	[SerializeField]
	private float maxSteeringWheelAngle = 135f;

	[SerializeField]
	private float minSteeringWheelAngle = 45f;

	[SerializeField]
	private Kart kart;

	[Header("Horn")]
	[SerializeField]
	private CollisionForwarder hornTriggerForwarder;

	[SerializeField]
	private RecRoomAudioClip hornAudio;

	[SerializeField]
	private float minTimeBetweenHorns = 0.5f;

	[Header("Speedometer")]
	[SerializeField]
	private Text speedometer;

	private bool inputInitialized;

	private Vector3 defaultUp = Vector3.up;

	private float lastHornTime;

	protected override void Awake()
	{
		base.Awake();
		defaultUp = kart.transform.InverseTransformDirection(base.transform.up);
		inputInitialized = false;
		if (hornTriggerForwarder != null)
		{
			hornTriggerForwarder.TriggerEnter += OnHornTriggerEnter;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (hornTriggerForwarder != null)
		{
			hornTriggerForwarder.TriggerEnter -= OnHornTriggerEnter;
		}
	}

	public override void OnInputDown()
	{
		base.OnInputDown();
		if (base.hasAuthority)
		{
			inputInitialized = true;
		}
	}

	public override void OnInputUp()
	{
		base.OnInputUp();
		inputInitialized = false;
	}

	public override bool OnPlayerTryingToRotateInPlace(float angle, float duration)
	{
		if (base.hasAuthority)
		{
			return kart.TryRotateInPlace(angle);
		}
		return base.OnPlayerTryingToRotateInPlace(angle, duration);
	}

	public override void Pickup(Player player, Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		base.Pickup(player, target, targetSpacePickupPosition, targetSpacePickupRotation);
		if (player.isLocal)
		{
			player.PlayerLocomotion.TeleportEvent += OnPlayerTeleport;
		}
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		base.Release(player, linearVelocity, angularVelocity);
		inputInitialized = false;
		if (player.isLocal)
		{
			player.PlayerLocomotion.TeleportEvent -= OnPlayerTeleport;
			kart.Accelerate(0f);
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.hasAuthority)
		{
			float f = kart.transform.TransformDirection(defaultUp).AngleSignedVector3(base.transform.up);
			float t = Mathf.InverseLerp(minSteeringWheelAngle, maxSteeringWheelAngle, Mathf.Abs(f));
			t = ((!(Mathf.Sign(f) < 0f)) ? Mathf.Lerp(1f, -1f, t) : Mathf.Lerp(-1f, 1f, t));
			kart.Steer(t);
			kart.Accelerate((!inputInitialized) ? 0f : InputAmount);
			speedometer.text = kart.Speed.ToString("F2");
		}
	}

	private void OnPlayerTeleport(Player player, Vector3 source, Vector3 dest, TeleportationPortal portal)
	{
		if (player.isLocal)
		{
			player.ReleaseToolsFromBothHands();
		}
	}

	private void OnHornTriggerEnter(Collider collider)
	{
		Player.BodyPart bodyPart;
		Player colliderPlayer = collider.GetColliderPlayer(out bodyPart);
		if (colliderPlayer != null && colliderPlayer.isLocal && bodyPart.IsHand() && Time.time - lastHornTime >= minTimeBetweenHorns && ((bodyPart == Player.BodyPart.LeftHand && colliderPlayer.LeftHand.Tool == null) || (bodyPart == Player.BodyPart.RightHand && colliderPlayer.RightHand.Tool == null)))
		{
			base.photonView.RPC("RpcOnHorn", PhotonTargets.All);
			lastHornTime = Time.time;
		}
	}

	[PunRPC]
	private void RpcOnHorn()
	{
		AudioManager.Play3DSFX(hornAudio, hornTriggerForwarder.transform);
	}
}
