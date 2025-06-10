using System.Collections;
using UnityEngine;

public class NetworkToolSpawner : Tool
{
	public Tool ToolPrefab;

	private Coroutine pickupSpawnCoroutine;

	protected override void OnDestroy()
	{
		PhotonNetwork.RemoveRPCs(base.photonView);
		base.OnDestroy();
	}

	public void InstantiateTool(Vector3 position, Quaternion rotation)
	{
		int num = ToolPrefab.GetComponentsInChildren<PhotonView>(true).Length;
		int[] array = new int[num];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = PhotonNetwork.AllocateViewID();
		}
		base.photonView.RPC("SpawnToolOnNetwork", PhotonTargets.AllBufferedViaServer, position, rotation, array);
		PhotonNetwork.SendOutgoingCommands();
	}

	[PunRPC]
	public void SpawnToolOnNetwork(Vector3 pos, Quaternion rot, int[] ids)
	{
		Tool tool = Object.Instantiate(ToolPrefab, pos, rot);
		PhotonView[] componentsInChildren = tool.GetComponentsInChildren<PhotonView>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].viewID = ids[i];
		}
		if (tool != null && base.Owner.isLocal)
		{
			int handTypeId = 0;
			bool flag = false;
			if (base.Owner.LeftHand.Tool == this)
			{
				flag = true;
				handTypeId = 0;
			}
			else if (base.Owner.RightHand.Tool == this)
			{
				flag = true;
				handTypeId = 1;
			}
			if (flag && pickupSpawnCoroutine == null)
			{
				pickupSpawnCoroutine = StartCoroutine(RunPickupSpawn(tool, base.Owner, handTypeId));
			}
		}
	}

	public IEnumerator RunPickupSpawn(Tool tool, Player player, int handTypeId)
	{
		while (tool != null && tool.ToolRenderer == null)
		{
			yield return null;
		}
		if (player != null && tool != null && !tool.IsHeld)
		{
			player.ToolController.ReleaseTool(this);
			player.ToolController.TryPickupTool(tool, handTypeId);
		}
		yield return null;
		pickupSpawnCoroutine = null;
	}

	public override void Pickup(Player player, Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		InstantiateTool(target.transform.position, target.transform.rotation);
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
	}
}
