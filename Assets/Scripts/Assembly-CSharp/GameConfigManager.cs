using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameConfigManager : IGameComponent
{
	[Serializable]
	private struct GameConfigMetadata
	{
		public GameConfigType ConfigType;

		public string Name;
	}

	[SerializeField]
	private GameConfigMetadata[] configs;

	private const string INVALID_CONFIG_NAME = "Invalid Config";

	private GameManager gameManager;

	private Dictionary<GameConfigType, SynchronizedField<int>> configFields = new Dictionary<GameConfigType, SynchronizedField<int>>();

	public event Action ConfigChangeEvent;

	public void OnAwake(GameManager gameManager)
	{
		this.gameManager = gameManager;
	}

	public void OnStart()
	{
	}

	public void OnDestroy()
	{
	}

	public void OnUpdate()
	{
	}

	public void OnPlayerDisconnected(PhotonPlayer player)
	{
	}

	public void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
	}

	public void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
	}

	private SynchronizedField<int> GetConfigField(GameConfigType config)
	{
		SynchronizedField<int> value;
		if (!configFields.TryGetValue(config, out value))
		{
			value = new SynchronizedField<int>(gameManager, "CONFIG" + (int)config, -1, SetterPermissionMode.MASTER);
			configFields[config] = value;
		}
		return value;
	}

	public void SetConfig(GameConfigType config, int value)
	{
		gameManager.photonView.RPC("RpcMasterSetConfig", PhotonTargets.MasterClient, (int)config, value);
	}

	public int GetConfig(GameConfigType config)
	{
		return GetConfigField(config).Get();
	}

	public void MasterSetConfig(GameConfigType config, int value)
	{
		if (PhotonNetwork.isMasterClient)
		{
			GetConfigField(config).ForceSet(value);
			BroadcastConfigChange();
		}
	}

	public string GetConfigName(GameConfigType config)
	{
		int configIndex = GetConfigIndex(config);
		if (configIndex >= 0)
		{
			return configs[configIndex].Name;
		}
		return "Invalid Config";
	}

	private int GetConfigIndex(GameConfigType config)
	{
		int result = -1;
		for (int i = 0; i < configs.Length; i++)
		{
			if (configs[i].ConfigType == config)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private void BroadcastConfigChange()
	{
		gameManager.photonView.RPC("RpcBroadcastConfigChange", PhotonTargets.All);
	}

	public void FireConfigChangeEvent()
	{
		if (this.ConfigChangeEvent != null)
		{
			this.ConfigChangeEvent();
		}
	}
}
