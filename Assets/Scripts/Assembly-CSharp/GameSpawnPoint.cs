using UnityEngine;

public class GameSpawnPoint : SpawnPoint
{
	[SerializeField]
	private SupportedGameState supportedState;

	[SerializeField]
	private GameTeam supportedTeam = GameTeam.INVALID;

	[SerializeField]
	private GameTeamPlayerIndex supportedTeamPlayerIndex = GameTeamPlayerIndex.ANY_INDEX;

	[SerializeField]
	private SpawnPointTagType customTag = SpawnPointTagType.DEFAULT;

	public SupportedGameState SupportedGameState
	{
		get
		{
			return supportedState;
		}
	}

	public GameTeam GameTeam
	{
		get
		{
			return supportedTeam;
		}
	}

	public GameTeamPlayerIndex GameTeamPlayerIndex
	{
		get
		{
			return supportedTeamPlayerIndex;
		}
	}

	public SpawnPointTagType CustomTag
	{
		get
		{
			return customTag;
		}
	}

	protected override Color GizmoColor
	{
		get
		{
			return Color.red;
		}
	}

	public bool SupportsGameState(SupportedGameState state)
	{
		return supportedState == SupportedGameState.ALL_STATES || supportedState == state;
	}
}
