using UnityEngine;
using UnityEngine.UI;

public class GameScoreboardHeaderView : GameScoreboardElementView
{
	[Header("Title")]
	[SerializeField]
	private Text titleText;

	[Header("Timer")]
	[SerializeField]
	private GameObject timerColumn;

	[SerializeField]
	private Text timerText;

	[Header("Subtitle")]
	[SerializeField]
	private GameObject subtitleRow;

	[SerializeField]
	private GameObject subtitleColumn;

	[SerializeField]
	private Text subtitleText;

	[Header("Subtitle Score")]
	[SerializeField]
	private GameObject subtitleScoreColumn;

	[SerializeField]
	private Text subtitleScoreText;

	[Header("Team Header")]
	[SerializeField]
	private GameScoreboardTeamHeaderView teamHeader;

	private const string PRE_GAME_TEXT = "PRE-GAME";

	private const string MATCHMAKING_TEXT = "LOOKING FOR PLAYERS";

	private const string GAME_STARTING_TEXT = "STARTING IN";

	private const string GAME_ON_TEXT = "GAME ON";

	private const string GAME_OVER_TEXT = "GAME OVER";

	private const string GAME_RUNNING_TEXT = "GAME IN PROGRESS";

	private const string POST_GAME_TEXT = "NEXT GAME IN";

	private const string RESULTS_TEXT = "PREVIOUS RESULTS";

	private ElementColors _colors;

	public override ElementColors Colors
	{
		get
		{
			return _colors;
		}
		set
		{
			_colors = value;
			teamHeader.Colors = _colors;
		}
	}

	public void SetTimerDataModel(GameUIState state, GameUITimerDataModel timerDataModel)
	{
		switch (state)
		{
		case GameUIState.PRE_GAME:
		case GameUIState.WAITING_FOR_PLAYERS:
			timerText.text = timerDataModel.GameDuration.ToTimeString();
			break;
		case GameUIState.GAME_ON:
		case GameUIState.GAME_RUNNING:
		case GameUIState.GAME_OVER:
		case GameUIState.RESULTS:
			timerText.text = timerDataModel.GameTimeRemaining.ToTimeString();
			break;
		case GameUIState.GAME_STARTING:
		case GameUIState.RESULTS_LOCKED:
			timerText.text = ((int)timerDataModel.GameStateTransitionTimeRemaining/*cast due to .constrained prefix*/).ToString();
			break;
		}
	}

	public void SetGameModel(GameUIState state, GameUIDataModel model)
	{
		teamHeader.SetGameModel(state, model);
		switch (state)
		{
		case GameUIState.WAITING_FOR_PLAYERS:
			titleText.text = "LOOKING FOR PLAYERS";
			subtitleRow.SetActive(true);
			subtitleColumn.SetActive(true);
			subtitleScoreColumn.SetActive(true);
			subtitleText.text = model.ModeName.ToUpper();
			subtitleScoreText.text = model.WinCondition;
			teamHeader.gameObject.SetActive(false);
			break;
		case GameUIState.PRE_GAME:
			titleText.text = "PRE-GAME";
			timerColumn.SetActive(true);
			subtitleRow.SetActive(true);
			subtitleColumn.SetActive(true);
			subtitleScoreColumn.SetActive(true);
			subtitleText.text = model.ModeName.ToUpper();
			subtitleScoreText.text = model.WinCondition;
			teamHeader.gameObject.SetActive(false);
			break;
		case GameUIState.GAME_STARTING:
			titleText.text = "STARTING IN";
			timerColumn.SetActive(true);
			subtitleRow.SetActive(false);
			teamHeader.gameObject.SetActive(false);
			break;
		case GameUIState.GAME_ON:
			titleText.text = "GAME ON";
			timerColumn.SetActive(false);
			subtitleRow.SetActive(false);
			teamHeader.gameObject.SetActive(false);
			break;
		case GameUIState.GAME_RUNNING:
			titleText.text = "GAME IN PROGRESS";
			timerColumn.SetActive(true);
			subtitleRow.SetActive(false);
			teamHeader.gameObject.SetActive(true);
			break;
		case GameUIState.GAME_OVER:
			titleText.text = "GAME OVER";
			timerColumn.SetActive(false);
			subtitleRow.SetActive(false);
			teamHeader.gameObject.SetActive(true);
			break;
		case GameUIState.RESULTS_LOCKED:
			titleText.text = "NEXT GAME IN";
			timerColumn.SetActive(true);
			subtitleRow.SetActive(false);
			teamHeader.gameObject.SetActive(true);
			break;
		case GameUIState.RESULTS:
			titleText.text = "PREVIOUS RESULTS";
			timerColumn.SetActive(true);
			subtitleRow.SetActive(false);
			teamHeader.gameObject.SetActive(true);
			break;
		}
		ApplyColors(false);
	}
}
