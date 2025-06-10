using System.Collections.Generic;

public abstract class AbstractStateMachine
{
	public delegate void OnStateChange(ushort currentStateId, ushort previousStateId, ushort currentSubStateId, ushort previousSubStateId);

	public delegate void StateTransition(ushort otherStateId, ushort otherSubStateId);

	public delegate void StateUpdate();

	protected class State
	{
		public StateTransition OnEnter;

		public StateTransition OnExit;

		public StateUpdate OnUpdate;
	}

	public const ushort INVALID_STATE_ID = ushort.MaxValue;

	private Dictionary<uint, State> states = new Dictionary<uint, State>();

	public ushort CurrentStateId { get; private set; }

	public ushort CurrentSubStateId { get; private set; }

	public event OnStateChange StateChangeEvent;

	public AbstractStateMachine()
	{
		CurrentStateId = ushort.MaxValue;
		CurrentSubStateId = ushort.MaxValue;
	}

	private State GetSuperState(ushort superStateId)
	{
		State value = null;
		if (superStateId != ushort.MaxValue)
		{
			states.TryGetValue(GetFullyQualifiedStateId(superStateId, ushort.MaxValue), out value);
		}
		return value;
	}

	private State GetSubState(ushort superStateId, ushort subStateId)
	{
		State value = null;
		if (superStateId != ushort.MaxValue && subStateId != ushort.MaxValue)
		{
			states.TryGetValue(GetFullyQualifiedStateId(superStateId, subStateId), out value);
		}
		return value;
	}

	public void EnterState(ushort nextStateId)
	{
		EnterState(nextStateId, ushort.MaxValue);
	}

	public void EnterState(ushort nextStateId, ushort nextSubStateId)
	{
		EnterState(GetFullyQualifiedStateId(nextStateId, nextSubStateId));
	}

	protected virtual void EnterState(uint nextFullyQualifiedStateId)
	{
		ushort superStateIdFromFullyQualifiedId = GetSuperStateIdFromFullyQualifiedId(nextFullyQualifiedStateId);
		ushort subStateIdFromFullyQualifiedId = GetSubStateIdFromFullyQualifiedId(nextFullyQualifiedStateId);
		bool flag = superStateIdFromFullyQualifiedId != CurrentStateId;
		bool flag2 = subStateIdFromFullyQualifiedId != CurrentSubStateId;
		State subState = GetSubState(CurrentStateId, CurrentSubStateId);
		if ((flag || flag2) && subState != null && subState.OnExit != null)
		{
			subState.OnExit(superStateIdFromFullyQualifiedId, subStateIdFromFullyQualifiedId);
		}
		State superState = GetSuperState(CurrentStateId);
		if (flag && superState != null && superState.OnExit != null)
		{
			superState.OnExit(superStateIdFromFullyQualifiedId, subStateIdFromFullyQualifiedId);
		}
		ushort currentStateId = CurrentStateId;
		ushort currentSubStateId = CurrentSubStateId;
		CurrentStateId = superStateIdFromFullyQualifiedId;
		CurrentSubStateId = subStateIdFromFullyQualifiedId;
		superState = GetSuperState(CurrentStateId);
		if (flag && superState != null && superState.OnEnter != null)
		{
			superState.OnEnter(currentStateId, currentSubStateId);
		}
		subState = GetSubState(CurrentStateId, CurrentSubStateId);
		if ((flag || flag2) && subState != null && subState.OnEnter != null)
		{
			subState.OnEnter(currentStateId, currentSubStateId);
		}
		if (this.StateChangeEvent != null)
		{
			this.StateChangeEvent(superStateIdFromFullyQualifiedId, currentStateId, subStateIdFromFullyQualifiedId, currentSubStateId);
		}
	}

	public void AddState(ushort stateId, StateTransition onEnter, StateTransition onExit, StateUpdate onUpdate)
	{
		AddState(stateId, ushort.MaxValue, onEnter, onExit, onUpdate);
	}

	public void AddState(ushort superStateId, ushort subStateId, StateTransition onEnter, StateTransition onExit, StateUpdate onUpdate)
	{
		AddFullyQualifiedState(GetFullyQualifiedStateId(superStateId, subStateId), onEnter, onExit, onUpdate);
	}

	protected void AddFullyQualifiedState(uint fullyQualifiedStateId, StateTransition onEnter, StateTransition onExit, StateUpdate onUpdate)
	{
		if (!states.ContainsKey(fullyQualifiedStateId))
		{
			states.Add(fullyQualifiedStateId, new State
			{
				OnEnter = onEnter,
				OnExit = onExit,
				OnUpdate = onUpdate
			});
		}
	}

	public void Update()
	{
		State superState = GetSuperState(CurrentStateId);
		if (superState != null && superState.OnUpdate != null)
		{
			superState.OnUpdate();
		}
		State subState = GetSubState(CurrentStateId, CurrentSubStateId);
		if (subState != null && subState.OnUpdate != null)
		{
			subState.OnUpdate();
		}
	}

	protected uint GetFullyQualifiedStateId(ushort superStateId, ushort subStateId)
	{
		return (uint)((superStateId << 16) + subStateId);
	}

	protected ushort GetSuperStateIdFromFullyQualifiedId(uint fullyQualifiedId)
	{
		return (ushort)(fullyQualifiedId >> 16);
	}

	protected ushort GetSubStateIdFromFullyQualifiedId(uint fullyQualifiedId)
	{
		return (ushort)fullyQualifiedId;
	}
}
