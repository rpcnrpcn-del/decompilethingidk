using Photon;
using UnityEngine;

public class Powerup : Photon.MonoBehaviour
{
	[SerializeField]
	private Transform visualRoot;

	[SerializeField]
	private float value = 1f;

	[Header("Reset")]
	[SerializeField]
	protected bool automaticallyResets;

	[SerializeField]
	protected float resetDuration = 10f;

	private SynchronizedField<bool> _isEnabled;

	private SynchronizedField<bool> _isAlive;

	protected SynchronizedTimer resetTimer;

	public float PowerupValue
	{
		get
		{
			return value;
		}
	}

	public bool IsEnabled
	{
		get
		{
			return _isEnabled.Get();
		}
		protected set
		{
			_isEnabled.ForceSet(value);
		}
	}

	public bool IsAlive
	{
		get
		{
			return _isAlive.Get();
		}
		protected set
		{
			_isAlive.ForceSet(value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_isEnabled = new SynchronizedField<bool>(this, "IS_ENABLED", true, SetterPermissionMode.MASTER, OnIsEnabledChange);
		_isAlive = new SynchronizedField<bool>(this, "IS_ALIVE", true, SetterPermissionMode.MASTER, OnIsAliveChange);
		resetTimer = new SynchronizedTimer(this, "POWERUP", SetterPermissionMode.MASTER);
	}

	protected virtual void Start()
	{
		OnIsEnabledChange();
		OnIsAliveChange();
	}

	protected virtual void Update()
	{
		if (PhotonNetwork.isMasterClient && automaticallyResets && !IsAlive && resetTimer.TimerOver)
		{
			IsAlive = true;
		}
	}

	private void OnIsEnabledChange()
	{
		base.gameObject.SetActive(IsEnabled);
	}

	private void OnIsAliveChange()
	{
		visualRoot.gameObject.SetActive(IsAlive);
	}

	public void MasterDisable()
	{
		if (PhotonNetwork.isMasterClient)
		{
			IsEnabled = false;
			IsAlive = false;
		}
	}

	public void MasterEnable()
	{
		if (PhotonNetwork.isMasterClient)
		{
			IsEnabled = true;
			IsAlive = true;
		}
	}

	public void MasterOnPickup()
	{
		if (PhotonNetwork.isMasterClient)
		{
			IsAlive = false;
			if (automaticallyResets)
			{
				resetTimer.StartTimer(resetDuration);
			}
		}
	}
}
