using System;
using UnityEngine;
using UnityEngine.UI;

public class GameScoreboardFooterView : GameScoreboardElementView
{
	[Serializable]
	private struct ButtonColumnView
	{
		public GameObject ButtonColumn;

		public Button Button;

		public Text ButtonText;
	}

	[Header("Text")]
	[SerializeField]
	private GameObject configRow;

	[SerializeField]
	private Text configText;

	[Header("Buttons")]
	[SerializeField]
	private GameObject buttonRow;

	[SerializeField]
	private ButtonColumnView joinGameButtonColumn;

	[SerializeField]
	private ButtonColumnView startGameButtonColumn;

	[SerializeField]
	private ButtonColumnView switchTeamButtonColumn;

	[SerializeField]
	private ButtonColumnView switchModeButtonColumn;

	[SerializeField]
	private ButtonColumnView viewResultsButtonColumn;

	private const string JOIN_TEXT = "JOIN";

	private const string LEAVE_TEXT = "SPECTATE";

	private const string NEXT_TEXT = "NEXT";

	private const string RESULTS_TEXT = "RESULTS";

	public void SetGameModel(GameUIState state, GameUIDataModel model)
	{
		switch (state)
		{
		case GameUIState.WAITING_FOR_PLAYERS:
			configRow.SetActive(false);
			buttonRow.SetActive(true);
			viewResultsButtonColumn.ButtonColumn.SetActive(model.SupportsPreviousGameResults);
			viewResultsButtonColumn.Button.interactable = true;
			viewResultsButtonColumn.ButtonText.text = "RESULTS";
			joinGameButtonColumn.ButtonColumn.SetActive(false);
			startGameButtonColumn.ButtonColumn.SetActive(true);
			startGameButtonColumn.Button.interactable = false;
			switchTeamButtonColumn.ButtonColumn.SetActive(model.SupportsTeamSwitching);
			switchTeamButtonColumn.Button.interactable = model.LocalPlayer.CanSwitchTeams;
			switchModeButtonColumn.ButtonColumn.SetActive(model.SupportsModeSwitching);
			break;
		case GameUIState.PRE_GAME:
			configRow.SetActive(false);
			configText.text = model.ModeName;
			buttonRow.SetActive(true);
			viewResultsButtonColumn.ButtonColumn.SetActive(model.SupportsPreviousGameResults);
			viewResultsButtonColumn.Button.interactable = true;
			viewResultsButtonColumn.ButtonText.text = "RESULTS";
			if (model.LocalPlayer.IsSpectator)
			{
				startGameButtonColumn.ButtonColumn.SetActive(false);
				switchTeamButtonColumn.ButtonColumn.SetActive(false);
				switchModeButtonColumn.ButtonColumn.SetActive(false);
				joinGameButtonColumn.ButtonColumn.SetActive(true);
				joinGameButtonColumn.Button.interactable = model.LocalPlayer.CanJoinGame;
				joinGameButtonColumn.ButtonText.text = "JOIN";
			}
			else
			{
				buttonRow.SetActive(true);
				joinGameButtonColumn.ButtonColumn.SetActive(model.LocalPlayer.CanLeaveGame);
				joinGameButtonColumn.Button.interactable = true;
				joinGameButtonColumn.ButtonText.text = "SPECTATE";
				startGameButtonColumn.ButtonColumn.SetActive(true);
				startGameButtonColumn.Button.interactable = true;
				switchTeamButtonColumn.ButtonColumn.SetActive(model.SupportsTeamSwitching);
				switchTeamButtonColumn.Button.interactable = model.LocalPlayer.CanSwitchTeams;
				switchModeButtonColumn.ButtonColumn.SetActive(model.SupportsModeSwitching);
			}
			break;
		case GameUIState.GAME_STARTING:
			configRow.SetActive(false);
			buttonRow.SetActive(false);
			break;
		case GameUIState.GAME_ON:
			configRow.SetActive(false);
			buttonRow.SetActive(false);
			break;
		case GameUIState.GAME_RUNNING:
			if (model.LocalPlayer.IsSpectator)
			{
				configRow.SetActive(false);
				buttonRow.SetActive(true);
				startGameButtonColumn.ButtonColumn.SetActive(false);
				switchTeamButtonColumn.ButtonColumn.SetActive(false);
				switchModeButtonColumn.ButtonColumn.SetActive(false);
				joinGameButtonColumn.ButtonColumn.SetActive(true);
				joinGameButtonColumn.Button.interactable = model.LocalPlayer.CanJoinGame;
			}
			else
			{
				bool flag = string.IsNullOrEmpty(model.PresenceString);
				configRow.SetActive(!flag);
				if (!flag)
				{
					configText.text = model.PresenceString.ToUpper();
				}
				buttonRow.SetActive(false);
			}
			break;
		case GameUIState.GAME_OVER:
			configRow.SetActive(false);
			buttonRow.SetActive(false);
			break;
		case GameUIState.RESULTS_LOCKED:
		case GameUIState.RESULTS:
			configRow.SetActive(false);
			buttonRow.SetActive(true);
			joinGameButtonColumn.ButtonColumn.SetActive(false);
			startGameButtonColumn.ButtonColumn.SetActive(false);
			switchTeamButtonColumn.ButtonColumn.SetActive(false);
			switchModeButtonColumn.ButtonColumn.SetActive(false);
			viewResultsButtonColumn.ButtonColumn.SetActive(state != GameUIState.RESULTS_LOCKED);
			viewResultsButtonColumn.Button.interactable = true;
			viewResultsButtonColumn.ButtonText.text = "NEXT";
			break;
		}
		ApplyColors(false);
	}
}
