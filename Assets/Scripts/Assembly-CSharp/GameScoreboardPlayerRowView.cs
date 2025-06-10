using UnityEngine;
using UnityEngine.UI;

public class GameScoreboardPlayerRowView : GameScoreboardElementView
{
	[Header("Stat Columns")]
	[SerializeField]
	private GameScoreboardPlayerStatColumnView statColumnPrefab;

	[SerializeField]
	private int maxStatColumnCount = 4;

	[SerializeField]
	private Text nameText;

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

	public void SetPlayerModel(GameUIState state, GameUIPlayerDataModel playerModel, bool supportsScore)
	{
		bool flag = state == GameUIState.GAME_RUNNING || state == GameUIState.GAME_OVER || state == GameUIState.RESULTS || state == GameUIState.RESULTS_LOCKED;
		nameText.text = ((!playerModel.Active) ? string.Empty : playerModel.Name.ToUpper());
		int i = 0;
		if (flag)
		{
			for (; i < playerModel.Stats.Length && i < statColumns.Length; i++)
			{
				statColumns[i].gameObject.SetActive(true);
				statColumns[i].SetStatModel(playerModel.Stats[i], playerModel.Active, playerModel.IsLocal);
			}
		}
		for (; i < statColumns.Length; i++)
		{
			statColumns[i].gameObject.SetActive(false);
		}
		ApplyColors(playerModel.Active && playerModel.IsLocal);
	}

	protected override void Awake()
	{
		base.Awake();
		statColumns = new GameScoreboardPlayerStatColumnView[maxStatColumnCount];
		for (int i = 0; i < maxStatColumnCount; i++)
		{
			statColumns[i] = Object.Instantiate(statColumnPrefab);
			statColumns[i].transform.SetParent(base.transform, false);
			statColumns[i].gameObject.SetActive(false);
		}
	}
}
