using System;
using Photon;
using UnityEngine;

[RequireComponent(typeof(PlayerHand))]
public class PlayerHandGestures : Photon.MonoBehaviour
{
	public enum Gesture
	{
		None = -1,
		Highfive = 0,
		Fistbump = 1,
		HandshakeStart = 2,
		HandshakeCancel = 3,
		HandshakeSuccess = 4
	}

	private enum HandshakeState
	{
		None = 0,
		Ready = 1,
		Waiting = 2,
		Running = 3,
		Finished = 4,
		Broken = 5
	}

	private enum CollisionGestureState
	{
		None = 0,
		Colliding = 1,
		Finished = 2
	}

	public enum ThumbsUpState
	{
		None = -1,
		Started = 0,
		Playing = 1
	}

	private enum ShakeState
	{
		None = -1,
		Tracking = 0,
		Playing = 1
	}

	private float lastGestureTime;

	private float gestureCooldown = 1f;

	[Header("Handshake")]
	[SerializeField]
	private float handshakeCooldown = 1f;

	[SerializeField]
	private float sqrHandshakeDistance = 0.1f;

	[SerializeField]
	private float maxHandshakeDistanceFromOriginSqr = 0.0625f;

	private SynchronizedField<int> _gestureConnectedHandId;

	private SynchronizedField<int> _handshakeStateId;

	[SerializeField]
	private float requiredHandshakeHoldDuration = 2f;

	private float handshakeStartTime;

	private float handshakeBreakTime;

	private Vector3 handshakeStartPosition = Vector3.zero;

	[Header("Collision Gestures")]
	[SerializeField]
	private float minFistBumpSpeed = 1f;

	[SerializeField]
	private float fistbumpDuration = 1f;

	[SerializeField]
	private float fistbumpDistanceThreshold = 0.4f;

	[SerializeField]
	private float fistbumpPullbackDistance = 0.01f;

	[SerializeField]
	private float minHighFiveSpeed = 1f;

	[SerializeField]
	private float highFiveDuration = 1f;

	[SerializeField]
	private float highFiveDistanceThreshold = 0.5f;

	[SerializeField]
	private float highFivePullbackDistance = 0.01f;

	private float collisionGestureDuration = 1f;

	private float collisionGestureDistanceThreshold = 0.4f;

	private float collisionGesturePullBackDistance = 0.01f;

	private float collisionGestureStartTime;

	private Vector3 collisionGestureStartPosition = Vector3.zero;

	private Vector3 collisionGestureDirection = Vector3.zero;

	private Gesture currentCollisionGesture = Gesture.None;

	private SynchronizedField<int> _collisionGestureStateId;

	[Header("Thumbs up")]
	[SerializeField]
	private float thumbsUpDelay = 1f;

	private float thumbsUpDelayStart;

	private float thumbsUpAngleThreshold = 0.85f;

	private SynchronizedField<int> _currentThumbsUpState;

	[SerializeField]
	private float minShakeStartSpeed = 1f;

	[SerializeField]
	private float shakeTravelDistance = 0.1f;

	[SerializeField]
	private float shakeTime = 0.7f;

	[SerializeField]
	private float shakeMaxDisplacement = 0.25f;

	private float shakeStartTime;

	private float distanceTraveled;

	private Vector3 shakeStartPosition = Vector3.zero;

	private Vector3 shakePreviousPosition = Vector3.zero;

	private ShakeState currentShakeState = ShakeState.None;

	private static readonly int rpcGestureHandleTimeoutMiliseconds = 600;

	public PlayerHand Hand { get; private set; }

	private PlayerHand gestureConnectedHand
	{
		get
		{
			PhotonView photonView = PhotonView.Find(_gestureConnectedHandId.Get());
			return (!(photonView != null)) ? null : photonView.GetComponent<PlayerHand>();
		}
		set
		{
			_gestureConnectedHandId.ForceSet(value.photonView.viewID);
		}
	}

	private HandshakeState currentHandshakeState
	{
		get
		{
			return (HandshakeState)_handshakeStateId.Get();
		}
		set
		{
			_handshakeStateId.ForceSet((int)value);
		}
	}

	private CollisionGestureState currentCollisionGestureState
	{
		get
		{
			return (CollisionGestureState)_collisionGestureStateId.Get();
		}
		set
		{
			_collisionGestureStateId.ForceSet((int)value);
		}
	}

	private ThumbsUpState CurrentThumbsUpState
	{
		get
		{
			return (ThumbsUpState)_currentThumbsUpState.Get();
		}
		set
		{
			_currentThumbsUpState.ForceSet((int)value);
		}
	}

	public int LastGestureServerTimestamp { get; private set; }

	public Gesture LastGestureType { get; private set; }

	public event Action<PlayerHand, PlayerHand, Gesture> GestureDetected;

	protected override void Awake()
	{
		base.Awake();
		Hand = GetComponent<PlayerHand>();
		_handshakeStateId = new SynchronizedField<int>(this, "HANDSHAKE_STATE", 0, SetterPermissionMode.AUTHORITY, OnHandshakeStateChange);
		_collisionGestureStateId = new SynchronizedField<int>(this, "COL_GESTURE_STATE", 0, SetterPermissionMode.AUTHORITY, OnCollisionGestureStateChange);
		_currentThumbsUpState = new SynchronizedField<int>(this, "THUMBS_UP_STATE", -1, SetterPermissionMode.AUTHORITY, OnThumbsUpStateChange);
		_gestureConnectedHandId = new SynchronizedField<int>(this, "GESTURE_HAND_ID", -1, SetterPermissionMode.AUTHORITY);
	}

	private void Start()
	{
		OnHandshakeStateChange();
	}

	private void FixedUpdate()
	{
		if (base.isLocal)
		{
			UpdateHandshakeState();
			UpdateCollisionGestureState();
			UpdateThumbsUpState();
			UpdateShakeState();
		}
	}

	private void TryEvaluateRemoteHandGesture(PlayerHand otherHand)
	{
		if (Time.time - lastGestureTime < gestureCooldown || !Hand.IsVisible || !otherHand.IsVisible || Hand.ThisPlayer.IsGhostedOrPermaghosted || otherHand.ThisPlayer.IsGhostedOrPermaghosted || currentHandshakeState != HandshakeState.None)
		{
			return;
		}
		Gesture gesture = Gesture.None;
		Vector3 rhs = Hand.TrackedVelocity.RecentLinearVelocity - otherHand.TrackedVelocity.RecentLinearVelocity;
		if (Hand.OpenClosedAxis >= 0.99f && otherHand.OpenClosedAxis >= 0.99f)
		{
			float num = Vector3.Dot(Hand.transform.forward, otherHand.transform.forward);
			float num2 = Mathf.Abs(Vector3.Dot(Hand.FingerDirection, rhs));
			float num3 = Vector3.Dot(Hand.TrackedVelocity.RecentLinearVelocity, Hand.FingerDirection);
			float num4 = Vector3.Dot(otherHand.TrackedVelocity.RecentLinearVelocity, otherHand.FingerDirection);
			if (num < -0.8f && num2 >= minFistBumpSpeed && num3 >= -0.001f && num4 >= -0.001f)
			{
				StartCollisionGesture(Gesture.Fistbump, otherHand);
			}
		}
		else if (Hand.OpenClosedAxis <= 0.01f && otherHand.OpenClosedAxis <= 0.01f)
		{
			float num5 = Vector3.Dot(Hand.PalmDirection, otherHand.PalmDirection);
			if (num5 < -0.8f)
			{
				float num6 = Mathf.Abs(Vector3.Dot(Hand.PalmDirection, rhs));
				float num7 = Vector3.Dot(Hand.TrackedVelocity.RecentLinearVelocity, Hand.PalmDirection);
				float num8 = Vector3.Dot(otherHand.TrackedVelocity.RecentLinearVelocity, otherHand.PalmDirection);
				if (num6 >= minHighFiveSpeed && num7 >= -0.001f && num8 >= -0.001f)
				{
					StartCollisionGesture(Gesture.Highfive, otherHand);
				}
				else
				{
					float num9 = Vector3.Dot(Hand.FingerDirection, otherHand.FingerDirection);
					if (num9 < -0.6f && Hand.Type == otherHand.Type && (otherHand.Gestures.currentHandshakeState == HandshakeState.None || otherHand.Gestures.currentHandshakeState == HandshakeState.Ready))
					{
						StartHandshake(otherHand);
					}
				}
			}
		}
		if (gesture != Gesture.None)
		{
			lastGestureTime = Time.time;
			base.photonView.RPC("RpcHandGestureDetected", PhotonTargets.All, otherHand.photonView.viewID, (int)gesture, PhotonNetwork.ServerTimestamp);
		}
	}

	private void StartHandshake(PlayerHand otherHand)
	{
		gestureConnectedHand = otherHand;
		currentHandshakeState = HandshakeState.Ready;
	}

	private void RequestHandshakeCompletion()
	{
		base.photonView.RPC("RpcAuthorityRequestHandshakeCompletion", base.authority);
	}

	private void OnHandshakeStateChange()
	{
		if (base.isLocal)
		{
			OnEnterHandshakeState();
		}
		UpdateHandshakeFeedback();
	}

	private void UpdateHandshakeState()
	{
		switch (currentHandshakeState)
		{
		case HandshakeState.Ready:
			UpdateHandshakeReadyState();
			break;
		case HandshakeState.Waiting:
			UpdateHandshakeWaitingState();
			break;
		case HandshakeState.Running:
			UpdateHandshakeRunningState();
			break;
		case HandshakeState.Finished:
			UpdateHandshakeFinishedState();
			break;
		case HandshakeState.Broken:
			UpdateHandshakeBrokenState();
			break;
		case HandshakeState.None:
			break;
		}
	}

	private void OnEnterHandshakeState()
	{
		switch (currentHandshakeState)
		{
		case HandshakeState.Running:
			handshakeStartTime = Time.time;
			handshakeStartPosition = Hand.transform.position;
			if (gestureConnectedHand != null)
			{
				base.photonView.RPC("RpcHandGestureDetected", PhotonTargets.All, gestureConnectedHand.photonView.viewID, 2, PhotonNetwork.ServerTimestamp);
			}
			break;
		case HandshakeState.Broken:
			handshakeBreakTime = Time.time;
			if (gestureConnectedHand != null)
			{
				base.photonView.RPC("RpcHandGestureDetected", PhotonTargets.All, gestureConnectedHand.photonView.viewID, 3, PhotonNetwork.ServerTimestamp);
			}
			break;
		}
	}

	private void UpdateHandshakeFeedback()
	{
		if (currentHandshakeState == HandshakeState.Running || currentHandshakeState == HandshakeState.Finished)
		{
			PlayerHand.ConnectHandshake(Hand, gestureConnectedHand);
			return;
		}
		PlayerHand.BreakHandshake(Hand, gestureConnectedHand);
		Hand.HandshakeAnimationEnabled = currentHandshakeState == HandshakeState.Waiting;
	}

	private void UpdateHandshakeReadyState()
	{
		if (!HandsAreWithinHandshakeRange())
		{
			currentHandshakeState = HandshakeState.None;
		}
		else if (LocalHandIsGrippedForHandshake())
		{
			currentHandshakeState = HandshakeState.Waiting;
		}
	}

	private void UpdateHandshakeWaitingState()
	{
		if (!HandsAreWithinHandshakeRange() || !LocalHandIsGrippedForHandshake())
		{
			currentHandshakeState = HandshakeState.None;
		}
		else if (gestureConnectedHand != null && (RemoteHandIsGrippedForHandshake() || gestureConnectedHand.Gestures.currentHandshakeState == HandshakeState.Running))
		{
			currentHandshakeState = HandshakeState.Running;
		}
	}

	private void UpdateHandshakeRunningState()
	{
		if (gestureConnectedHand != null && (gestureConnectedHand.Gestures.currentHandshakeState == HandshakeState.Finished || Time.time - handshakeStartTime >= requiredHandshakeHoldDuration))
		{
			gestureConnectedHand.Gestures.RequestHandshakeCompletion();
			currentHandshakeState = HandshakeState.Finished;
		}
		else if (HandshakeIsBroken() || gestureConnectedHand.Gestures.currentHandshakeState == HandshakeState.Broken)
		{
			currentHandshakeState = HandshakeState.Broken;
		}
	}

	private void UpdateHandshakeFinishedState()
	{
		if (gestureConnectedHand != null && gestureConnectedHand.Gestures.currentHandshakeState == HandshakeState.Broken)
		{
			currentHandshakeState = HandshakeState.Broken;
		}
		else if (HandshakeIsBroken())
		{
			currentHandshakeState = HandshakeState.None;
		}
	}

	private void UpdateHandshakeBrokenState()
	{
		if (Time.time - handshakeBreakTime >= handshakeCooldown)
		{
			currentHandshakeState = HandshakeState.None;
		}
	}

	private bool HandshakeIsBroken()
	{
		return gestureConnectedHand == null || !LocalHandIsGrippedForHandshake() || !HandNearHandshakeOrigin();
	}

	private bool HandsAreWithinHandshakeRange()
	{
		bool result = false;
		if (gestureConnectedHand != null)
		{
			float num = Vector3.Dot(Hand.PalmDirection, gestureConnectedHand.PalmDirection);
			float num2 = Vector3.Dot(Hand.FingerDirection, gestureConnectedHand.FingerDirection);
			float sqrMagnitude = (Hand.transform.position - gestureConnectedHand.transform.position).sqrMagnitude;
			result = (sqrMagnitude <= sqrHandshakeDistance && num <= -0.8f) || num2 <= -0.4f;
		}
		return result;
	}

	private bool HandNearHandshakeOrigin()
	{
		return (Hand.transform.position - handshakeStartPosition).sqrMagnitude <= maxHandshakeDistanceFromOriginSqr;
	}

	private bool LocalHandIsGrippedForHandshake()
	{
		return Hand != null && Hand.OpenClosedAxis > 0.01f;
	}

	private bool RemoteHandIsGrippedForHandshake()
	{
		return gestureConnectedHand != null && gestureConnectedHand.OpenClosedAxis > 0.01f;
	}

	[PunRPC]
	private void RpcAuthorityRequestHandshakeCompletion()
	{
		if (gestureConnectedHand != null && currentHandshakeState != HandshakeState.Broken)
		{
			lastGestureTime = Time.time;
			base.photonView.RPC("RpcHandGestureDetected", PhotonTargets.All, gestureConnectedHand.photonView.viewID, 4, PhotonNetwork.ServerTimestamp);
		}
	}

	private void StartCollisionGesture(Gesture gestureType, PlayerHand otherHand)
	{
		if (currentCollisionGesture == Gesture.None)
		{
			currentCollisionGesture = gestureType;
			gestureConnectedHand = otherHand;
			currentCollisionGestureState = CollisionGestureState.Colliding;
		}
	}

	private void OnCollisionGestureStateChange()
	{
		if (base.isLocal)
		{
			OnEnterCollisionGestureState();
		}
		UpdateCollisionGestureFeedback();
	}

	private void UpdateCollisionGestureState()
	{
		CollisionGestureState collisionGestureState = currentCollisionGestureState;
		if (collisionGestureState == CollisionGestureState.Colliding && (Time.time - collisionGestureStartTime > collisionGestureDuration || !HandNearCollisionGestureOrigin() || !HandInCorrectStateForCollisionGesture()))
		{
			currentCollisionGestureState = CollisionGestureState.Finished;
		}
	}

	private void OnEnterCollisionGestureState()
	{
		switch (currentCollisionGestureState)
		{
		case CollisionGestureState.Colliding:
			collisionGestureStartTime = Time.time;
			collisionGestureStartPosition = Hand.transform.position;
			if (currentCollisionGesture == Gesture.Fistbump)
			{
				collisionGestureDirection = Hand.FingerDirection;
				collisionGestureDuration = fistbumpDuration;
				collisionGestureDistanceThreshold = fistbumpDistanceThreshold;
				collisionGesturePullBackDistance = fistbumpPullbackDistance;
			}
			else
			{
				collisionGestureDirection = Hand.PalmDirection;
				collisionGestureDuration = highFiveDuration;
				collisionGestureDistanceThreshold = highFiveDistanceThreshold;
				collisionGesturePullBackDistance = highFivePullbackDistance;
			}
			if (gestureConnectedHand != null)
			{
				lastGestureTime = Time.time;
				base.photonView.RPC("RpcHandGestureDetected", PhotonTargets.All, gestureConnectedHand.photonView.viewID, (int)currentCollisionGesture, PhotonNetwork.ServerTimestamp);
			}
			break;
		case CollisionGestureState.Finished:
			currentCollisionGesture = Gesture.None;
			currentCollisionGestureState = CollisionGestureState.None;
			break;
		}
	}

	private void UpdateCollisionGestureFeedback()
	{
		if (currentCollisionGestureState == CollisionGestureState.Colliding)
		{
			PlayerHand.ConnectCollisionGesture(currentCollisionGesture, Hand, gestureConnectedHand);
		}
		else
		{
			PlayerHand.BreakCollisionGesture(currentCollisionGesture, Hand, gestureConnectedHand);
		}
	}

	private bool HandNearCollisionGestureOrigin()
	{
		bool flag = (Hand.transform.position - collisionGestureStartPosition).sqrMagnitude <= collisionGestureDistanceThreshold * collisionGestureDistanceThreshold;
		bool flag2 = Vector3.Dot(Hand.transform.position - collisionGestureStartPosition, -collisionGestureDirection) <= collisionGesturePullBackDistance;
		return flag && flag2;
	}

	private bool HandInCorrectStateForCollisionGesture()
	{
		bool result = false;
		switch (currentCollisionGesture)
		{
		case Gesture.Fistbump:
			result = Hand.OpenClosedAxis >= 0.99f;
			break;
		case Gesture.Highfive:
			result = Hand.OpenClosedAxis <= 0.01f;
			break;
		}
		return result;
	}

	private void UpdateThumbsUpState()
	{
		if (!base.isLocal)
		{
			return;
		}
		switch (CurrentThumbsUpState)
		{
		case ThumbsUpState.None:
			if (ThumbsUpStartConditions())
			{
				thumbsUpDelayStart = Time.time;
				CurrentThumbsUpState = ThumbsUpState.Started;
			}
			break;
		case ThumbsUpState.Started:
			if (ThumbsUpStartConditions())
			{
				if (Time.time - thumbsUpDelayStart >= thumbsUpDelay)
				{
					CurrentThumbsUpState = ThumbsUpState.Playing;
				}
			}
			else
			{
				CurrentThumbsUpState = ThumbsUpState.None;
			}
			break;
		case ThumbsUpState.Playing:
			if (ThumbsUpEndConditions())
			{
				thumbsUpDelayStart = 0f;
				CurrentThumbsUpState = ThumbsUpState.None;
			}
			break;
		}
	}

	private void OnThumbsUpStateChange()
	{
		switch (CurrentThumbsUpState)
		{
		case ThumbsUpState.None:
			Hand.ThumbsUpAnimationEnabled = false;
			break;
		case ThumbsUpState.Playing:
			Hand.ThumbsUpAnimationEnabled = true;
			break;
		}
	}

	private bool ThumbsUpStartConditions()
	{
		return currentHandshakeState == HandshakeState.None && currentCollisionGesture == Gesture.None && Hand.OpenClosedAxis >= 0.99f && Mathf.Abs(Vector3.Dot(Hand.ThumbDirection, Vector3.up)) >= thumbsUpAngleThreshold;
	}

	private bool ThumbsUpEndConditions()
	{
		return currentHandshakeState != HandshakeState.None || currentCollisionGesture != Gesture.None || Hand.OpenClosedAxis < 0.99f;
	}

	private void UpdateShakeState()
	{
		switch (currentShakeState)
		{
		case ShakeState.None:
			if (Hand.TrackedVelocity.RecentLinearVelocity.sqrMagnitude >= minShakeStartSpeed * minShakeStartSpeed)
			{
				currentShakeState = ShakeState.Tracking;
				distanceTraveled = 0f;
				shakeStartPosition = Hand.transform.position;
				shakePreviousPosition = shakeStartPosition;
				shakeStartTime = Time.time;
			}
			break;
		case ShakeState.Tracking:
			if (Time.time - shakeStartTime < shakeTime)
			{
				if ((Hand.transform.position - shakeStartPosition).sqrMagnitude < shakeMaxDisplacement * shakeMaxDisplacement)
				{
					distanceTraveled += (Hand.transform.position - shakePreviousPosition).sqrMagnitude;
					shakePreviousPosition = Hand.transform.position;
				}
				else
				{
					currentShakeState = ShakeState.None;
				}
			}
			else if (distanceTraveled >= shakeTravelDistance * shakeTravelDistance)
			{
				currentShakeState = ShakeState.Playing;
			}
			else
			{
				currentShakeState = ShakeState.None;
			}
			break;
		case ShakeState.Playing:
			if (Hand.Tool != null)
			{
				Hand.Tool.OnShake();
			}
			currentShakeState = ShakeState.None;
			break;
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		TriggerAction(col, true);
	}

	private void OnTriggerExit(Collider col)
	{
		TriggerAction(col, false);
	}

	private void TriggerAction(Collider col, bool enter)
	{
		if (!enter || !base.isLocal)
		{
			return;
		}
		Player.BodyPart bodyPart;
		Player colliderPlayer = col.GetColliderPlayer(out bodyPart);
		if (colliderPlayer != null && !colliderPlayer.isLocal && bodyPart.IsHand())
		{
			PlayerHand hand = colliderPlayer.GetHand(bodyPart);
			if (hand != null)
			{
				TryEvaluateRemoteHandGesture(hand);
			}
		}
	}

	[PunRPC]
	public void RpcHandGestureDetected(int otherHandViewId, int gesture, int serverTimeMiliseconds)
	{
		PlayerHand playerHand = PlayerHand.Find(otherHandViewId);
		if (playerHand != null)
		{
			if (playerHand.photonView.owner.isLocal)
			{
				playerHand.Gestures.HandleHandGestureDetected(Hand, (Gesture)gesture, serverTimeMiliseconds);
			}
			else
			{
				HandleHandGestureDetected(playerHand, (Gesture)gesture, serverTimeMiliseconds);
			}
		}
	}

	public void HandleHandGestureDetected(PlayerHand otherHand, Gesture gesture, int serverTimeMiliseconds)
	{
		if ((otherHand.Gestures.LastGestureServerTimestamp == 0 || otherHand.Gestures.LastGestureType != gesture || Math.Abs(otherHand.Gestures.LastGestureServerTimestamp - serverTimeMiliseconds) >= rpcGestureHandleTimeoutMiliseconds) && (LastGestureServerTimestamp == 0 || LastGestureType != gesture || Math.Abs(LastGestureServerTimestamp - serverTimeMiliseconds) >= rpcGestureHandleTimeoutMiliseconds))
		{
			LastGestureServerTimestamp = serverTimeMiliseconds;
			LastGestureType = gesture;
			if (this.GestureDetected != null)
			{
				this.GestureDetected(Hand, otherHand, gesture);
			}
		}
	}
}
