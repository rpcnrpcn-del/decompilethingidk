using UnityEngine;

[DisallowMultipleComponent]
public class GameTeleportRestriction : MonoBehaviour
{
	[SerializeField]
	private bool spectatorsAllowed = true;

	[SerializeField]
	private bool allTeamsAllowed = true;

	[SerializeField]
	private GameTeam[] allowedTeams;

	[SerializeField]
	private TeleportRegionTagType requiredTag = TeleportRegionTagType.DEFAULT;

	public bool LocalPlayerCanTeleport { get; private set; }

	private void Awake()
	{
		LocalPlayerCanTeleport = true;
	}

	public void UpdateRestrictions(bool isGameRunning, bool localPlayerIsSpectator, GameTeam localPlayerTeam, TeleportRegionTagType playerTag)
	{
		if (!isGameRunning)
		{
			LocalPlayerCanTeleport = true;
			return;
		}
		if (localPlayerIsSpectator)
		{
			LocalPlayerCanTeleport = spectatorsAllowed;
			return;
		}
		if (allTeamsAllowed)
		{
			LocalPlayerCanTeleport = true;
		}
		else
		{
			LocalPlayerCanTeleport = false;
			for (int i = 0; i < allowedTeams.Length; i++)
			{
				if (allowedTeams[i] == localPlayerTeam)
				{
					LocalPlayerCanTeleport = true;
					break;
				}
			}
		}
		LocalPlayerCanTeleport &= requiredTag == TeleportRegionTagType.DEFAULT || requiredTag == playerTag;
	}
}
