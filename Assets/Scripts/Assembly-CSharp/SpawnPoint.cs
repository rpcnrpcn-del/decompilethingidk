using System;
using Photon;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public abstract class SpawnPoint : Photon.MonoBehaviour
{
	[SerializeField]
	private bool testsForOverlap = true;

	private const float COOLDOWN = 2f;

	private const float PLAYER_OVERLAP_MAX_HEIGHT = 2f;

	public const float PLAYER_OVERLAP_RADIUS = 0.5f;

	[NonSerialized]
	public bool SpawnPointEnabled = true;

	private SynchronizedField<float> _lastUseTime;

	public float LastUseTime
	{
		get
		{
			return _lastUseTime.Get();
		}
		private set
		{
			_lastUseTime.ForceSet(value);
		}
	}

	public float TimeSinceLastUse
	{
		get
		{
			return (float)PhotonNetwork.time - LastUseTime;
		}
	}

	public bool IsOnCooldown
	{
		get
		{
			return testsForOverlap && TimeSinceLastUse <= 2f;
		}
	}

	public bool IsOverlappingPlayer
	{
		get
		{
			return testsForOverlap && Physics.CheckCapsule(base.transform.position, base.transform.position + Vector3.up * 2f, 0.5f, 1053184);
		}
	}

	protected abstract Color GizmoColor { get; }

	protected override void Awake()
	{
		base.Awake();
		_lastUseTime = new SynchronizedField<float>(this, "LAST_USE_TIME", 0f, SetterPermissionMode.MASTER);
	}

	public void MasterUse()
	{
		if (PhotonNetwork.isMasterClient)
		{
			LastUseTime = (float)PhotonNetwork.time;
		}
	}

	protected virtual void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.forward * 1.5f);
		Gizmos.color = GizmoColor;
		Gizmos.DrawSphere(base.transform.position, 0.5f);
	}
}
