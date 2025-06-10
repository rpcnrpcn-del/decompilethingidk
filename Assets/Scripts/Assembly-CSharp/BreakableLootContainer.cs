using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Tool))]
public class BreakableLootContainer : BreakableObject
{
	[Header("Loot")]
	[SerializeField]
	private LootDropper lootDrops;

	private List<PhotonPlayer> playerHitInstances = new List<PhotonPlayer>();

	public override void OnToolForceApplied(Player hittingPlayer, Vector3 position, Vector3 force)
	{
		base.OnToolForceApplied(hittingPlayer, position, force);
		playerHitInstances.Add(hittingPlayer.PhotonPlayer);
		if (base.hasAuthority)
		{
			AuthorityApplyHit(position, force, 1);
		}
	}

	protected override void Fracture()
	{
		base.Fracture();
		if (lootDrops != null && playerHitInstances.Count > 0)
		{
			lootDrops.DropLoot(playerHitInstances.ToArray());
		}
	}
}
