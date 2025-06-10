using UnityEngine;

public abstract class GameUIView : MonoBehaviour
{
	public delegate void ButtonPress();

	public event ButtonPress StartGameButtonPressEvent;

	public event ButtonPress JoinLeaveGameButtonPressEvent;

	public event ButtonPress SwitchTeamButtonPressEvent;

	public event ButtonPress SwitchModeButtonPressEvent;

	public event ButtonPress ViewResultsButtonPressEvent;

	public abstract void SetGameDataModel(GameUIState state, GameUIDataModel dataModel);

	public abstract void SetTimerDataModel(GameUIState state, GameUITimerDataModel timerDataModel);

	protected void FireStartGameButtonEvent()
	{
		if (this.StartGameButtonPressEvent != null)
		{
			this.StartGameButtonPressEvent();
		}
	}

	protected void FireJoinLeaveGameEvent()
	{
		if (this.JoinLeaveGameButtonPressEvent != null)
		{
			this.JoinLeaveGameButtonPressEvent();
		}
	}

	protected void FireSwitchTeamEvent()
	{
		if (this.SwitchTeamButtonPressEvent != null)
		{
			this.SwitchTeamButtonPressEvent();
		}
	}

	protected void FireSwitchModeEvent()
	{
		if (this.SwitchModeButtonPressEvent != null)
		{
			this.SwitchModeButtonPressEvent();
		}
	}

	protected void FireViewResultsEvent()
	{
		if (this.ViewResultsButtonPressEvent != null)
		{
			this.ViewResultsButtonPressEvent();
		}
	}
}
