using UnityEngine;
using UnityEngine.UI;

public class CharadesPodium : MonoBehaviour
{
	[Header("Configuration")]
	[SerializeField]
	private GameTeam team;

	[SerializeField]
	private GameTeamPlayerIndex teamPlayerIndex = GameTeamPlayerIndex.ANY_INDEX;

	[Header("UI References")]
	[SerializeField]
	private Image background;

	[SerializeField]
	private Text label;

	[Header("Tool Control")]
	[SerializeField]
	private AdjustablePodiumTool podiumTool;

	[SerializeField]
	private SinglePlayerInteractionRestriction podiumToolInteractionRestriction;

	public GameTeamPlayerIndex PlayerIndex
	{
		get
		{
			return teamPlayerIndex;
		}
	}

	public GameTeam Team
	{
		get
		{
			return team;
		}
	}

	private void Awake()
	{
		background.color = GameTeamSettings.GetTeamColor(team);
	}

	public void SetPlayer(PhotonPlayer player)
	{
		label.text = player.name.ToUpper();
		if (podiumToolInteractionRestriction != null)
		{
			podiumToolInteractionRestriction.SetPlayer(player);
		}
	}

	public void ClearPlayer()
	{
		label.text = string.Empty;
		if (podiumToolInteractionRestriction != null)
		{
			podiumToolInteractionRestriction.ClearPlayer();
		}
	}
}
