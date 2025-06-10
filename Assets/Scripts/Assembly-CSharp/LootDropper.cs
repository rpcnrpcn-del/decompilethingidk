using Photon;
using UnityEngine;

public class LootDropper : Photon.MonoBehaviour
{
	[Header("Drops")]
	[SerializeField]
	private int minGold;

	[SerializeField]
	private int maxGold;

	[Header("Visuals")]
	[SerializeField]
	private PooledParticle lootDropParticlePrefab;

	[SerializeField]
	private float dropLabelOffset = 0.5f;

	[SerializeField]
	private float dropParticleOffset = 0.15f;

	public void DropLoot(PhotonPlayer[] affectedPlayers)
	{
		int num = 0;
		if (base.hasAuthority)
		{
			QuestManager questManager = RecRoomSceneManager.Instance.GameManager as QuestManager;
			if (questManager != null)
			{
				num = Random.Range(minGold, maxGold) / affectedPlayers.Length;
				if (num > 0)
				{
					questManager.photonView.RPC("RpcMasterRequestPickupGold", PhotonTargets.MasterClient, affectedPlayers, num, base.transform.position + Vector3.up * dropLabelOffset);
				}
			}
		}
		PlayLootDropEffects();
	}

	private void PlayLootDropEffects()
	{
		if (lootDropParticlePrefab != null)
		{
			PooledParticle pooledParticle = ObjectPool.Instance.Acquire(lootDropParticlePrefab);
			if (pooledParticle != null)
			{
				pooledParticle.transform.position = base.transform.position + Vector3.up * dropParticleOffset;
				pooledParticle.Play();
			}
		}
	}
}
