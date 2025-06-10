using System;
using Photon;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PhotonView))]
[AddComponentMenu("Photon Networking/PUN Network Transform")]
public class PUNNetworkTransform : Photon.MonoBehaviour, IPunObservable
{
	public enum ExtrapolationMode
	{
		None = 0,
		DynamicDistance = 1,
		DynamicTime = 2
	}

	[SerializeField]
	private float snapThreshold = 5f;

	[SerializeField]
	private float minSyncInterval = 5f;

	[NonSerialized]
	private bool synchronizationEnabled = true;

	[NonSerialized]
	public ExtrapolationMode CurrentExtrapolationMode;

	[NonSerialized]
	public bool SupportsDiscontinuitySmoothing;

	private CastCollisionRigidbody castCollisionRigidbody;

	private Rigidbody rigidbody;

	private TrackedVelocity trackedVelocity;

	private Tool tool;

	private PlayerInteractionRestriction interactionRestriction;

	private float lastSyncTime;

	private byte lastSyncContinuousMovementID;

	private bool syncRequested;

	private float networkLatency;

	private Vector3 lastExtrapolatedVelocity = Vector3.zero;

	private Vector3 lastExtrapolatedAngularVelocity = Vector3.zero;

	private double lastSnapTime;

	private byte syncContinuousMovementID;

	private bool syncHasData;

	private bool syncIsSleeping;

	private double syncNetworkTime;

	private Vector3 syncPosition;

	private Quaternion syncRotation;

	private Vector3 syncVelocity;

	private Vector3 syncAngularVelocity;

	private Vector3 smoothDiscontinuityVelocity;

	private float lastDiscontinuityStartTime;

	private float dynamicTimeExtrapolationStartTime;

	private const float k_MaxDynamicTimeExtrapolationDuration = 0.2f;

	private const float k_LocalMovementThreshold = 0.001f;

	private const float k_LocalRotationThreshold = 0.1f;

	private const float k_AccountForGravityThreshold = 0.05f;

	private const float k_InterpolationTime = 0.05f;

	private const float k_DiscontinuitySmoothTime = 0.25f;

	public bool SynchronizationEnabled
	{
		get
		{
			return synchronizationEnabled;
		}
		set
		{
			if (SupportsDiscontinuitySmoothing && value && !synchronizationEnabled)
			{
				OnIntentionalDiscontinuity();
			}
			synchronizationEnabled = value;
		}
	}

	private bool hasPhysicsDrivenRigidbody
	{
		get
		{
			return rigidbody != null && !rigidbody.isKinematic;
		}
	}

	private bool IsSmoothingDiscontinuity
	{
		get
		{
			return SupportsDiscontinuitySmoothing && Time.fixedTime - lastDiscontinuityStartTime < 0.25f;
		}
	}

	private void OnValidate()
	{
		if (snapThreshold < 0f)
		{
			snapThreshold = 0.01f;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		trackedVelocity = GetComponent<TrackedVelocity>();
		castCollisionRigidbody = GetComponent<CastCollisionRigidbody>();
		rigidbody = ((!(castCollisionRigidbody == null)) ? castCollisionRigidbody.Rigidbody : null);
		tool = GetComponent<Tool>();
		interactionRestriction = GetComponent<PlayerInteractionRestriction>();
	}

	private void Start()
	{
		syncHasData = false;
		syncContinuousMovementID = 0;
		syncNetworkTime = 0.0;
		syncPosition = base.transform.position;
		syncRotation = base.transform.rotation;
		syncVelocity = GetLinearVelocity();
		syncAngularVelocity = GetAngularVelocity();
		base.photonView.OwnerChangedEvent += OnPhotonOwnershipChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.photonView.OwnerChangedEvent -= OnPhotonOwnershipChanged;
	}

	private void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		if (base.hasAuthority)
		{
			syncRequested = true;
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isReading && info.sender != base.authority)
		{
			return;
		}
		if (stream.isWriting)
		{
			if (syncRequested || Time.fixedTime - lastSyncTime > minSyncInterval || syncContinuousMovementID != lastSyncContinuousMovementID)
			{
				syncHasData = true;
				syncRequested = false;
			}
			else if (hasPhysicsDrivenRigidbody)
			{
				syncHasData = !rigidbody.IsSleeping();
			}
			else
			{
				syncHasData = (base.transform.position - syncPosition).magnitude > 0.001f || Quaternion.Angle(base.transform.rotation, syncRotation) > 0.1f;
			}
		}
		stream.Serialize(ref syncHasData);
		if (!syncHasData)
		{
			if (stream.isReading && SynchronizationEnabled)
			{
				ResetPhysics(syncPosition, syncRotation);
				if (hasPhysicsDrivenRigidbody)
				{
					rigidbody.Sleep();
				}
			}
			return;
		}
		syncNetworkTime = PhotonNetwork.time;
		syncPosition = base.transform.position;
		syncRotation = base.transform.rotation;
		syncVelocity = GetLinearVelocity();
		syncAngularVelocity = GetAngularVelocity();
		syncIsSleeping = hasPhysicsDrivenRigidbody && rigidbody.IsSleeping();
		stream.Serialize(ref syncContinuousMovementID);
		stream.Serialize(ref syncNetworkTime);
		stream.Serialize(ref syncPosition);
		stream.Serialize(ref syncRotation);
		stream.Serialize(ref syncVelocity);
		stream.Serialize(ref syncAngularVelocity);
		stream.Serialize(ref syncIsSleeping);
		networkLatency = (float)(PhotonNetwork.time - syncNetworkTime);
		if (stream.isReading)
		{
			float magnitude = (base.transform.position - syncPosition).magnitude;
			if (SynchronizationEnabled && (syncIsSleeping || lastSyncTime == 0f || lastSyncContinuousMovementID != syncContinuousMovementID || (magnitude > snapThreshold && !IsSmoothingDiscontinuity)))
			{
				ResetPhysics(syncPosition, syncRotation);
			}
			if (syncIsSleeping && hasPhysicsDrivenRigidbody)
			{
				rigidbody.Sleep();
			}
		}
		lastSyncTime = Time.fixedTime;
		lastSyncContinuousMovementID = syncContinuousMovementID;
	}

	public void InsertPositionDiscontinuity()
	{
		if (base.hasAuthority)
		{
			ResetVelocity(Vector3.zero, Vector3.zero);
			syncContinuousMovementID = (byte)((syncContinuousMovementID < byte.MaxValue) ? ((byte)(syncContinuousMovementID + 1)) : 0);
		}
	}

	public void StartDynamicTimeExtrapolation()
	{
		dynamicTimeExtrapolationStartTime = Time.fixedTime;
	}

	public void OnIntentionalDiscontinuity()
	{
		lastDiscontinuityStartTime = Time.fixedTime;
		smoothDiscontinuityVelocity = GetLinearVelocity();
	}

	private void FixedUpdate()
	{
		if (!base.hasAuthority && SynchronizationEnabled && syncHasData)
		{
			InterpolateTowardLatestSync();
		}
		if (CurrentExtrapolationMode == ExtrapolationMode.DynamicDistance && (tool == null || !tool.IsHeld))
		{
			Player player = Player.FindClosest(base.transform.position, interactionRestriction);
			if (player != null && player.photonView.ownerId != base.photonView.ownerId && player == Player.LocalPlayer)
			{
				base.photonView.TransferOwnership(player.photonView.ownerId);
			}
		}
	}

	private void UpdatePhysics(Vector3 velocity, Vector3 angularVelocity)
	{
		velocity = velocity.ValueOrZeroIfBogus();
		angularVelocity = angularVelocity.ValueOrZeroIfBogus();
		if (rigidbody != null)
		{
			rigidbody.velocity = velocity;
			rigidbody.angularVelocity = angularVelocity;
		}
		if (!hasPhysicsDrivenRigidbody)
		{
			Vector3 vector = velocity * Time.fixedDeltaTime + base.transform.position;
			Quaternion quaternion = UnityExtensions.QuaternionFromAngularVelocity(angularVelocity * Time.fixedDeltaTime) * base.transform.rotation;
			vector = vector.ValueOrZeroIfBogus();
			quaternion = quaternion.ValueOrIdentityIfBogus();
			if (rigidbody != null)
			{
				rigidbody.MovePosition(vector);
				rigidbody.MoveRotation(quaternion);
			}
			else
			{
				base.transform.position = vector;
				base.transform.rotation = quaternion;
			}
		}
	}

	private Vector3 GetLinearVelocity()
	{
		if (hasPhysicsDrivenRigidbody)
		{
			return rigidbody.velocity;
		}
		if (trackedVelocity != null)
		{
			return trackedVelocity.RecentLinearVelocity;
		}
		return Vector3.zero;
	}

	private Vector3 GetAngularVelocity()
	{
		if (hasPhysicsDrivenRigidbody)
		{
			return rigidbody.angularVelocity;
		}
		if (trackedVelocity != null)
		{
			return trackedVelocity.RecentAngularVelocity;
		}
		return Vector3.zero;
	}

	private void ResetPhysics(Vector3 position, Quaternion rotation)
	{
		base.transform.position = position.ValueOrZeroIfBogus();
		base.transform.rotation = rotation.ValueOrIdentityIfBogus();
		ResetVelocity(Vector3.zero, Vector3.zero);
	}

	private void ResetVelocity(Vector3 newVelocity, Vector3 newAngularVelocity)
	{
		newVelocity = newVelocity.ValueOrZeroIfBogus();
		newAngularVelocity = newAngularVelocity.ValueOrZeroIfBogus();
		if (rigidbody != null)
		{
			rigidbody.velocity = newVelocity;
			rigidbody.angularVelocity = newAngularVelocity;
		}
		if (!hasPhysicsDrivenRigidbody && trackedVelocity != null)
		{
			trackedVelocity.Reset(newVelocity, newAngularVelocity);
		}
		lastExtrapolatedVelocity = newVelocity;
		lastExtrapolatedAngularVelocity = newAngularVelocity;
	}

	private void InterpolateTowardLatestSync()
	{
		float timeSinceLastSync = GetTimeSinceLastSync();
		Vector3 extrapolatedSyncPosition = GetExtrapolatedSyncPosition(timeSinceLastSync);
		Quaternion extrapolatedSyncRotation = GetExtrapolatedSyncRotation(timeSinceLastSync);
		Vector3 velocity;
		if (IsSmoothingDiscontinuity)
		{
			Vector3 vector = Vector3.SmoothDamp(base.transform.position, extrapolatedSyncPosition, ref smoothDiscontinuityVelocity, 0.125f, float.MaxValue, Time.fixedDeltaTime);
			velocity = (vector - base.transform.position) / Time.fixedDeltaTime;
		}
		else
		{
			velocity = (extrapolatedSyncPosition - base.transform.position) / 0.05f;
		}
		Vector3 angularVelocity = UnityExtensions.AngularVelocityFromTo(base.transform.rotation, extrapolatedSyncRotation) / 0.05f;
		UpdatePhysics(velocity, angularVelocity);
	}

	private Vector3 GetExtrapolatedSyncPosition(float timeSinceLastSync)
	{
		Vector3 vector = syncVelocity * timeSinceLastSync;
		lastExtrapolatedVelocity = syncVelocity;
		lastExtrapolatedAngularVelocity = syncAngularVelocity;
		bool flag = ShouldAccountForGravity();
		if (flag)
		{
			vector += Physics.gravity * timeSinceLastSync * timeSinceLastSync / 2f;
			lastExtrapolatedVelocity += Physics.gravity * timeSinceLastSync;
		}
		Vector3 vector2 = syncPosition + vector;
		Vector3 normalized = vector.normalized;
		float magnitude = vector.magnitude;
		CastColliderHit closestHit;
		if (magnitude > Mathf.Epsilon && castCollisionRigidbody != null && castCollisionRigidbody.Cast(syncPosition, base.transform.rotation, normalized, magnitude, 2048, out closestHit))
		{
			float num = timeSinceLastSync * (1f - closestHit.Distance / magnitude);
			if (flag)
			{
				vector2 -= Physics.gravity * num * num / 2f;
			}
			Vector3 vector3 = syncPosition + normalized * closestHit.Distance;
			Vector3 inDirection = vector2 - vector3;
			inDirection = Vector3.Reflect(inDirection, closestHit.Normal);
			Vector3 vector4 = Vector3.Project(inDirection, closestHit.Normal);
			Vector3 vector5 = inDirection - vector4;
			float num2 = ((!(closestHit.CastCollider.material == null)) ? closestHit.CastCollider.material.CombinedBounciness(closestHit.Collider.material) : 1f);
			Vector3 vector6 = vector5 + vector4 * num2;
			vector2 = vector3 + vector6;
			lastExtrapolatedVelocity = vector6 / inDirection.magnitude * syncVelocity.magnitude;
			if (flag)
			{
				vector2 += Physics.gravity * num * num / 2f;
				lastExtrapolatedVelocity += Physics.gravity * num;
			}
		}
		return vector2;
	}

	private Quaternion GetExtrapolatedSyncRotation(float timeSinceLastSync)
	{
		Vector3 angularVelocity = syncAngularVelocity * timeSinceLastSync;
		Quaternion quaternion = UnityExtensions.QuaternionFromAngularVelocity(angularVelocity);
		return quaternion * syncRotation;
	}

	private float GetTimeSinceLastSync()
	{
		float num = Time.fixedTime - lastSyncTime;
		if (CurrentExtrapolationMode != ExtrapolationMode.None)
		{
			num += GetExtrapolationAmount() * (networkLatency + 0.05f);
		}
		return num;
	}

	private float GetExtrapolationAmount()
	{
		float result = 1f;
		if (CurrentExtrapolationMode == ExtrapolationMode.DynamicDistance)
		{
			result = GetDynamicDistanceExtrapolation();
		}
		else if (CurrentExtrapolationMode == ExtrapolationMode.DynamicTime)
		{
			result = GetDynamicTimeExtrapolation();
		}
		return result;
	}

	private float GetDynamicDistanceExtrapolation()
	{
		float result = 1f;
		Player player = ((base.photonView.owner != null) ? (base.photonView.owner.TagObject as Player) : null);
		if (player != null && Player.LocalPlayer != null)
		{
			Vector3 position = player.Head.transform.position;
			Vector3 position2 = Player.LocalPlayer.Head.transform.position;
			Vector3 rhs = position2 - position;
			Vector3 lhs = base.transform.position - position;
			float num = Vector3.Dot(lhs, rhs);
			float sqrMagnitude = rhs.sqrMagnitude;
			float time = ((sqrMagnitude != 0f) ? (Mathf.Clamp01(num / sqrMagnitude) / 0.5f) : 0f);
			result = PUNNetworkManager.Instance.DynamicNetworkExtrapolationCurve.Evaluate(time);
		}
		return result;
	}

	private float GetDynamicTimeExtrapolation()
	{
		float time = Mathf.Clamp01((Time.fixedTime - dynamicTimeExtrapolationStartTime) / 0.2f);
		return PUNNetworkManager.Instance.DynamicNetworkExtrapolationCurve.Evaluate(time);
	}

	private bool ShouldAccountForGravity()
	{
		if (hasPhysicsDrivenRigidbody && rigidbody.useGravity)
		{
			float f = Vector3.Dot(syncVelocity, Physics.gravity.normalized);
			return Mathf.Abs(f) > 0.05f;
		}
		return false;
	}

	private void OnPhotonOwnershipChanged(int playerId)
	{
		syncHasData = false;
		if (base.hasAuthority)
		{
			syncRequested = true;
			ResetVelocity(lastExtrapolatedVelocity, lastExtrapolatedAngularVelocity);
		}
		else if (SupportsDiscontinuitySmoothing)
		{
			OnIntentionalDiscontinuity();
		}
	}
}
