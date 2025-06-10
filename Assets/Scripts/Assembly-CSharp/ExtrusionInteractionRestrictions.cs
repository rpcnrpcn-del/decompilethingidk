public class ExtrusionInteractionRestrictions : PlayerInteractionRestriction
{
	private Extrusion thisExtrusion;

	protected override void Awake()
	{
		base.Awake();
		thisExtrusion = GetComponent<Extrusion>();
	}

	public override bool PlayerInteractionAllowed(PhotonPlayer player)
	{
		if (thisExtrusion == null || thisExtrusion.IsExtrudingMesh)
		{
			return false;
		}
		return base.PlayerInteractionAllowed(player);
	}
}
