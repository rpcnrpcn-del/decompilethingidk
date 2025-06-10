using System;
using Photon;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class ToolPool : Photon.MonoBehaviour
{
	[SerializeField]
	protected Tool[] pool;

	[SerializeField]
	private bool forceDropIfNecessary;

	[SerializeField]
	private bool toolsDefaultDisabled = true;

	private SynchronizedField<float>[] lastAcquiredTimestamps;

	public Tool[] Pool
	{
		get
		{
			return pool;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < pool.Length; i++)
		{
			pool[i].SpawnedFromPool = true;
		}
		lastAcquiredTimestamps = new SynchronizedField<float>[pool.Length];
		for (int j = 0; j < pool.Length; j++)
		{
			lastAcquiredTimestamps[j] = new SynchronizedField<float>(pool[j], "LastAcquiredTime", 0f, SetterPermissionMode.MASTER);
		}
	}

	protected virtual void Start()
	{
		if (PhotonNetwork.isMasterClient && toolsDefaultDisabled)
		{
			for (int i = 0; i < pool.Length; i++)
			{
				pool[i].IsEnabled = false;
			}
		}
	}

	public Tool Acquire(Vector3 position, Quaternion rotation)
	{
		if (PhotonNetwork.isMasterClient)
		{
			int oldestToolIndex = GetOldestToolIndex((Tool tool2) => !tool2.IsEnabled);
			if (oldestToolIndex == -1)
			{
				oldestToolIndex = GetOldestToolIndex((Tool tool2) => !tool2.IsHeld);
				if (oldestToolIndex != -1)
				{
					Release(pool[oldestToolIndex]);
				}
				else if (forceDropIfNecessary)
				{
					oldestToolIndex = GetOldestToolIndex((Tool tool2) => true);
					pool[oldestToolIndex].HolderId = PhotonPlayer.Invalid;
					pool[oldestToolIndex].IsLocked = false;
					Release(pool[oldestToolIndex]);
				}
			}
			if (oldestToolIndex != -1)
			{
				lastAcquiredTimestamps[oldestToolIndex].ForceSet((float)PhotonNetwork.time);
				Tool tool = pool[oldestToolIndex];
				tool.photonView.TransferOwnership(PhotonNetwork.masterClient);
				tool.transform.position = position;
				tool.transform.rotation = rotation;
				PUNNetworkTransform[] componentsInChildren = tool.GetComponentsInChildren<PUNNetworkTransform>(true);
				foreach (PUNNetworkTransform pUNNetworkTransform in componentsInChildren)
				{
					pUNNetworkTransform.InsertPositionDiscontinuity();
				}
				tool.IsEnabled = true;
				return tool;
			}
		}
		return null;
	}

	public void Release(Tool tool)
	{
		if (PhotonNetwork.isMasterClient && pool.Contains(tool) && tool.IsEnabled)
		{
			tool.photonView.TransferOwnership(PhotonNetwork.masterClient);
			tool.IsEnabled = false;
			tool.transform.position = Vector3.zero;
			tool.transform.rotation = Quaternion.identity;
			PUNNetworkTransform[] componentsInChildren = tool.GetComponentsInChildren<PUNNetworkTransform>(true);
			foreach (PUNNetworkTransform pUNNetworkTransform in componentsInChildren)
			{
				pUNNetworkTransform.InsertPositionDiscontinuity();
			}
		}
	}

	public void MasterDisableAll()
	{
		if (PhotonNetwork.isMasterClient)
		{
			Tool[] array = pool;
			foreach (Tool tool in array)
			{
				Release(tool);
			}
		}
	}

	private int GetOldestToolIndex(Func<Tool, bool> predicate)
	{
		int result = -1;
		float num = float.MaxValue;
		for (int i = 0; i < pool.Length; i++)
		{
			float num2 = lastAcquiredTimestamps[i].Get();
			if (num2 < num && predicate(pool[i]))
			{
				result = i;
				num = num2;
			}
		}
		return result;
	}
}
