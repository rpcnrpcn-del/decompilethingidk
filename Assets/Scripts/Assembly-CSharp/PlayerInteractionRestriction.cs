using Photon;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerInteractionRestriction : Photon.MonoBehaviour
{
	[Header("Pre Game State")]
	public bool AllowedInPreGame = true;

	[Header("Playing Game State")]
	public bool AllowSpectators = true;

	public bool AllowAllActiveTeams = true;

	public GameTeam[] AllowedTeams;

	protected GameManager gameManager;

	public bool ForceRestricted { get; set; }

	protected virtual void Start()
	{
		gameManager = RecRoomSceneManager.Instance.GameManager;
	}

	public virtual bool PlayerInteractionAllowed(PhotonPlayer player)
	{
		if (ForceRestricted)
		{
			return false;
		}
		if (gameManager == null)
		{
			return true;
		}
		if (gameManager.CurrentState != GameStates.GAME_RUNNING && AllowedInPreGame)
		{
			return true;
		}
		bool flag = gameManager.TeamManager.IsPlayerSpectator(player);
		if (flag && AllowSpectators)
		{
			return true;
		}
		if (!flag && AllowAllActiveTeams)
		{
			return true;
		}
		if (AllowedTeams != null && AllowedTeams.Length > 0)
		{
			GameTeam playerTeam = gameManager.TeamManager.GetPlayerTeam(player);
			if (playerTeam != GameTeam.INVALID && AllowedTeams.Contains(playerTeam))
			{
				return true;
			}
		}
		return false;
	}
}
