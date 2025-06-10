public class PlayerSubMenuPanic : PlayerSubMenu
{
	private ThrobbingUIElement buttonThrobber;

	protected override void Awake()
	{
		base.Awake();
		buttonThrobber = GetComponentInChildren<ThrobbingUIElement>();
	}

	protected virtual void Start()
	{
		base.PlayerMenu.ThisPlayer.SituationPulse.PanicToggled += OnPanicToggled;
	}

	public override void Refresh()
	{
		base.Refresh();
		if (buttonThrobber != null)
		{
			buttonThrobber.Active = base.PlayerMenu.ThisPlayer.SituationPulse.Active;
		}
	}

	public void Button_Panic()
	{
		base.PlayerMenu.ThisPlayer.SituationPulse.TogglePanic();
	}

	private void OnPanicToggled(object sender)
	{
		if (buttonThrobber != null)
		{
			buttonThrobber.Active = base.PlayerMenu.ThisPlayer.SituationPulse.Active;
		}
		if (base.PlayerMenu.ThisPlayer.SituationPulse.Active)
		{
			base.Selected = true;
			if (!base.PlayerMenu.Visible)
			{
				base.PlayerMenu.PlayIncomingNotificationFeedback((PlayerMenu.NotificationEffectFlag)(-1), "You entered ghost mode");
			}
		}
		else if (base.Selected)
		{
			base.PlayerMenu.ShowDefaultTab();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.PlayerMenu.ThisPlayer.SituationPulse.PanicToggled -= OnPanicToggled;
	}
}
