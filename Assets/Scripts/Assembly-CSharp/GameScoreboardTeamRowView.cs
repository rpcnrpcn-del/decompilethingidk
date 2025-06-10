using UnityEngine;
using UnityEngine.UI;

public class GameScoreboardTeamRowView : GameScoreboardElementView
{
	[Header("Player Rows")]
	[SerializeField]
	private GameScoreboardPlayerRowView playerRowPrefab;

	[SerializeField]
	private Transform playerRowRoot;

	[SerializeField]
	private int maxPlayerRowCount = 8;

	[Header("Score")]
	[SerializeField]
	private Text scoreText;

	[SerializeField]
	private GameObject scoreColumn;

	private GameScoreboardPlayerRowView[] playerRows;

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
			for (int i = 0; i < playerRows.Length; i++)
			{
				playerRows[i].Colors = _colors;
			}
		}
	}

	public GameScoreboardPlayerRowView GetPlayerRow(int index)
	{
		return (index >= playerRows.Length) ? null : playerRows[index];
	}

	public void SetTeamModel(GameUIState state, GameUITeamDataModel teamModel, bool supportsScore)
	{
		bool active = state == GameUIState.GAME_RUNNING || state == GameUIState.GAME_OVER || state == GameUIState.RESULTS || state == GameUIState.RESULTS_LOCKED;
		scoreColumn.SetActive(active);
		scoreText.text = ((!teamModel.Active) ? "-" : teamModel.Score.ToString());
		int i;
		for (i = 0; i < teamModel.PlayerModels.Length && i < playerRows.Length; i++)
		{
			playerRows[i].gameObject.SetActive(true);
			playerRows[i].SetPlayerModel(state, teamModel.PlayerModels[i], supportsScore);
		}
		for (; i < playerRows.Length; i++)
		{
			playerRows[i].gameObject.SetActive(false);
		}
		ApplyColors(teamModel.IsLocal);
	}

	protected override void Awake()
	{
		base.Awake();
		playerRows = new GameScoreboardPlayerRowView[maxPlayerRowCount];
		for (int i = 0; i < maxPlayerRowCount; i++)
		{
			playerRows[i] = Object.Instantiate(playerRowPrefab);
			playerRows[i].transform.SetParent(playerRowRoot, false);
		}
	}
}
