using Photon;

public class SynchronizedStateMachine : AbstractStateMachine
{
	private SetterPermissionMode setterPermissionMode = SetterPermissionMode.MASTER;

	private readonly string STATE_ID_KEY = "STATE_ID";

	private SynchronizedField<int> _fullyQualifiedStateId;

	private uint SynchronizedFullyQualifiedStateId
	{
		get
		{
			return (uint)_fullyQualifiedStateId.Get();
		}
		set
		{
			_fullyQualifiedStateId.ForceSet((int)value);
		}
	}

	public SynchronizedStateMachine(string id, SetterPermissionMode setterPermissionMode)
	{
		STATE_ID_KEY = id + "_" + STATE_ID_KEY;
		this.setterPermissionMode = setterPermissionMode;
	}

	public void Initialize(MonoBehaviour component, ushort defaultStateId)
	{
		Initialize(component, defaultStateId, ushort.MaxValue);
	}

	public void Initialize(MonoBehaviour component, ushort defaultStateId, ushort defaultSubStateId)
	{
		_fullyQualifiedStateId = new SynchronizedField<int>(component, STATE_ID_KEY, 65535, setterPermissionMode, OnSynchronizedFullyQualifiedStateIdChange);
		if (PhotonNetwork.isMasterClient)
		{
			SynchronizedFullyQualifiedStateId = GetFullyQualifiedStateId(defaultStateId, defaultSubStateId);
		}
		else
		{
			base.EnterState(SynchronizedFullyQualifiedStateId);
		}
	}

	protected override void EnterState(uint nextFullyQualifiedSubStateId)
	{
		SynchronizedFullyQualifiedStateId = nextFullyQualifiedSubStateId;
	}

	private void OnSynchronizedFullyQualifiedStateIdChange()
	{
		base.EnterState(SynchronizedFullyQualifiedStateId);
	}
}
