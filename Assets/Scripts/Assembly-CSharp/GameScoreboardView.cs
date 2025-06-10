using UnityEngine;
using UnityEngine.UI;

public class GameScoreboardView : GameUIView
{
	[Header("Colors")]
	[SerializeField]
	private Color primaryColor = new Color(1f, 0.952f, 0.411f, 1f);

	[SerializeField]
	private Color textColor = new Color(0f, 0f, 0f, 1f);

	[Header("Alpha")]
	[SerializeField]
	private float highlightBackgroundAlpha = 1f;

	[SerializeField]
	private float defaultBackgroundAlpha = 0.68f;

	private Canvas thisCanvas;

	private GraphicRaycaster thisRaycaster;

	private GameScoreboardHeaderView header;

	private GameScoreboardBodyView body;

	private GameScoreboardFooterView footer;

	private GameUIState state;

	private GameUIDataModel dataModel;

	private bool isDirty;

	private bool initialized;

	private void Awake()
	{
		thisCanvas = GetComponent<Canvas>();
		thisRaycaster = GetComponent<GraphicRaycaster>();
		header = GetComponentInChildren<GameScoreboardHeaderView>();
		body = GetComponentInChildren<GameScoreboardBodyView>();
		footer = GetComponentInChildren<GameScoreboardFooterView>();
	}

	private void Start()
	{
		if (thisCanvas != null && ViveControllerInput.Instance != null)
		{
			thisCanvas.worldCamera = ViveControllerInput.Instance.ControllerCamera;
		}
		GameScoreboardElementView.ElementColors colors = new GameScoreboardElementView.ElementColors
		{
			TextColor = textColor,
			BackgroundColor = primaryColor,
			DefaultBackgroundAlpha = defaultBackgroundAlpha,
			HighlightBackgroundAlpha = highlightBackgroundAlpha
		};
		header.Colors = colors;
		body.Colors = colors;
		footer.Colors = colors;
		initialized = true;
	}

	private void Update()
	{
		UpdateView();
	}

	public void OnStartGameButtonPress()
	{
		FireStartGameButtonEvent();
	}

	public void OnJoinGameButtonPress()
	{
		FireJoinLeaveGameEvent();
	}

	public void OnSwitchTeamButtonPress()
	{
		FireSwitchTeamEvent();
	}

	public void OnSwitchModeButtonPress()
	{
		FireSwitchModeEvent();
	}

	public void OnViewResultsButtonPress()
	{
		FireViewResultsEvent();
	}

	public override void SetGameDataModel(GameUIState state, GameUIDataModel dataModel)
	{
		this.state = state;
		this.dataModel = dataModel;
		isDirty = true;
	}

	public override void SetTimerDataModel(GameUIState state, GameUITimerDataModel timerDataModel)
	{
		header.SetTimerDataModel(state, timerDataModel);
	}

	private void UpdateView()
	{
		if (isDirty && initialized)
		{
			thisRaycaster.enabled = state == GameUIState.PRE_GAME || state == GameUIState.WAITING_FOR_PLAYERS || state == GameUIState.RESULTS || (state == GameUIState.GAME_RUNNING && dataModel.LocalPlayer.IsSpectator);
			header.SetGameModel(state, dataModel);
			footer.SetGameModel(state, dataModel);
			body.SetGameModel(state, dataModel);
			isDirty = false;
		}
	}
}
