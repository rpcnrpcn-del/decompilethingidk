public class StateMachine : AbstractStateMachine
{
	public void Initialize(ushort initialStateId)
	{
		Initialize(initialStateId, ushort.MaxValue);
	}

	public void Initialize(ushort initialStateId, ushort initialSubStateId)
	{
		EnterState(initialStateId, initialSubStateId);
	}
}
