using UnityEngine;

public class GameScoreboardBodyView : GameScoreboardElementView
{
	[Header("Team Rows")]
	[SerializeField]
	private GameScoreboardTeamRowView teamRowPrefab;

	[SerializeField]
	private int maxTeamRowCount = 8;

	private GameScoreboardTeamRowView[] teamRows;

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
			for (int i = 0; i < teamRows.Length; i++)
			{
				teamRows[i].Colors = _colors;
			}
		}
	}

	public void SetGameModel(GameUIState state, GameUIDataModel model)
	{
		int i;
		for (i = 0; i < model.TeamModels.Length && i < teamRows.Length; i++)
		{
			teamRows[i].gameObject.SetActive(true);
			ElementColors colors = Colors;
			colors.BackgroundColor = model.TeamModels[i].Color;
			teamRows[i].Colors = colors;
			teamRows[i].SetTeamModel(state, model.TeamModels[i], model.SupportsScore);
		}
		for (; i < teamRows.Length; i++)
		{
			teamRows[i].gameObject.SetActive(false);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		teamRows = new GameScoreboardTeamRowView[maxTeamRowCount];
		for (int i = 0; i < maxTeamRowCount; i++)
		{
			teamRows[i] = Object.Instantiate(teamRowPrefab);
			teamRows[i].transform.SetParent(base.transform, false);
			teamRows[i].gameObject.SetActive(false);
		}
	}
}
