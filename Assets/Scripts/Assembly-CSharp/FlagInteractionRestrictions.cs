public class FlagInteractionRestrictions : PlayerInteractionRestriction
{
	private FlagTool thisFlag;

	protected override void Awake()
	{
		base.Awake();
		thisFlag = GetComponent<FlagTool>();
	}

	public override bool PlayerInteractionAllowed(PhotonPlayer player)
	{
		PaintballManager paintballManager = RecRoomSceneManager.Instance.GameManager as PaintballManager;
		if (thisFlag != null && paintballManager != null)
		{
			GameTeam playerTeam = paintballManager.TeamManager.GetPlayerTeam(player);
			if (playerTeam != GameTeam.INVALID && playerTeam == thisFlag.Team && thisFlag.AtHomeGoal)
			{
				return false;
			}
		}
		return base.PlayerInteractionAllowed(player);
	}
}
