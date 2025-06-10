using UnityEngine;

public class GameScoreboardTeamHeaderView : GameScoreboardElementView
{
	[Header("Stat Columns")]
	[SerializeField]
	private GameScoreboardPlayerStatColumnView statColumnPrefab;

	[SerializeField]
	private int maxStatColumnCount = 4;

	[SerializeField]
	private Transform statColumnRoot;

	[Header("Score Column")]
	[SerializeField]
	private GameObject scoreColumn;

	private GameScoreboardPlayerStatColumnView[] statColumns;

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
			for (int i = 0; i < statColumns.Length; i++)
			{
				statColumns[i].Colors = _colors;
			}
		}
	}

	public void SetGameModel(GameUIState state, GameUIDataModel model)
	{
		bool flag = state == GameUIState.GAME_RUNNING || state == GameUIState.GAME_OVER || state == GameUIState.RESULTS || state == GameUIState.RESULTS_LOCKED;
		scoreColumn.SetActive(flag);
		int i = 0;
		if (flag)
		{
			for (; i < model.StatNames.Length && i < statColumns.Length; i++)
			{
				statColumns[i].gameObject.SetActive(true);
				statColumns[i].SetHeaderModel(model.StatNames[i]);
			}
		}
		for (; i < statColumns.Length; i++)
		{
			statColumns[i].gameObject.SetActive(false);
		}
		ApplyColors(false);
	}

	protected override void Awake()
	{
		base.Awake();
		statColumns = new GameScoreboardPlayerStatColumnView[maxStatColumnCount];
		for (int i = 0; i < maxStatColumnCount; i++)
		{
			statColumns[i] = Object.Instantiate(statColumnPrefab);
			statColumns[i].transform.SetParent(statColumnRoot, false);
			statColumns[i].gameObject.SetActive(false);
		}
	}
}
