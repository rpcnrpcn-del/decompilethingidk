public class QuestRoomDoorTrigger : OverlapVolume
{
	private PulsingBeam attractor;

	private bool _active;

	public bool Active
	{
		get
		{
			return _active;
		}
		set
		{
			_active = value;
			OnActiveChange();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		attractor = GetComponentInChildren<PulsingBeam>();
		OnActiveChange();
	}

	private void OnActiveChange()
	{
		if (attractor != null)
		{
			attractor.gameObject.SetActive(Active);
		}
	}
}
