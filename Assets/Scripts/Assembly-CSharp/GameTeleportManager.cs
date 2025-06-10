using System;
using UnityEngine;

[Serializable]
public class GameTeleportManager : IGameComponent
{
	[Serializable]
	private struct TeleportRegionTagMetadata
	{
		public TeleportRegionTagType TagType;

		public string Name;
	}

	[Serializable]
	private struct TeleportCooldownMetadata
	{
		public TeleportCooldownType cooldownType;

		public float customTeleportCooldown;
	}

	[Header("Cooldown")]
	[SerializeField]
	private TeleportCooldownMetadata[] customTeleportCooldowns;

	[SerializeField]
	private TeleportCooldownType defaultCooldownType = TeleportCooldownType.DEFAULT;

	[Header("Min Teleport Buffer")]
	[SerializeField]
	private bool supportsCustomTeleportBufferDistance;

	[SerializeField]
	private float customTeleportBufferDistance = 0.5f;

	[Header("Teleport Region Tags")]
	[SerializeField]
	private TeleportRegionTagMetadata[] teleportRegionTags;

	private const string TELEPORT_REGION_TAG_KEY = "TELEPORT_REGION_TAG";

	private GameManager gameManager;

	private GameTeleportRestriction[] teleportRestrictions;

	private TeleportRegionTagType localPlayerTeleportRegionTag = TeleportRegionTagType.DEFAULT;

	public TeleportCooldownType DefaultCooldownType
	{
		get
		{
			return defaultCooldownType;
		}
	}

	public void OnAwake(GameManager gameManager)
	{
		this.gameManager = gameManager;
	}

	public void OnStart()
	{
		teleportRestrictions = UnityEngine.Object.FindObjectsOfType<GameTeleportRestriction>();
		UpdateRestrictions();
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

	public void SetLocalPlayerTeleportRegionTag(TeleportRegionTagType tag)
	{
		localPlayerTeleportRegionTag = tag;
		UpdateRestrictions();
	}

	public string GetTagName(TeleportRegionTagType tagType)
	{
		for (int i = 0; i < teleportRegionTags.Length; i++)
		{
			if (teleportRegionTags[i].TagType == tagType)
			{
				return teleportRegionTags[i].Name;
			}
		}
		return null;
	}

	private TeleportRegionTagType GetLocalPlayerTeleportRegionTag()
	{
		return localPlayerTeleportRegionTag;
	}

	public void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
		SetLocalPlayerTeleportRegionTag(TeleportRegionTagType.DEFAULT);
		if (!localPlayerIsSpectator)
		{
			SetLocalPlayerCustomTeleportCooldown(defaultCooldownType);
			if (supportsCustomTeleportBufferDistance)
			{
				Player.LocalPlayer.PlayerLocomotion.HasOverrideTeleportBuffer = true;
				Player.LocalPlayer.PlayerLocomotion.OverrideTeleportBuffer = customTeleportBufferDistance;
			}
		}
		UpdateRestrictions();
	}

	public void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
		if (!localPlayerIsSpectator)
		{
			SetLocalPlayerCustomTeleportCooldown(TeleportCooldownType.DEFAULT);
			Player.LocalPlayer.PlayerLocomotion.HasOverrideTeleportBuffer = false;
		}
		UpdateRestrictions();
	}

	private void UpdateRestrictions()
	{
		bool isGameRunning = gameManager.CurrentState == GameStates.GAME_RUNNING;
		bool flag = gameManager.TeamManager.IsPlayerSpectator(PhotonNetwork.player);
		GameTeam localPlayerTeam = ((!flag) ? gameManager.TeamManager.GetPlayerTeam(PhotonNetwork.player) : GameTeam.INVALID);
		for (int i = 0; i < teleportRestrictions.Length; i++)
		{
			teleportRestrictions[i].UpdateRestrictions(isGameRunning, flag, localPlayerTeam, GetLocalPlayerTeleportRegionTag());
		}
	}

	public void SetLocalPlayerCustomTeleportCooldown(TeleportCooldownType cooldownType)
	{
		if (customTeleportCooldowns.Length <= 0)
		{
			return;
		}
		if (cooldownType != TeleportCooldownType.DEFAULT)
		{
			float num = -1f;
			for (int i = 0; i < customTeleportCooldowns.Length; i++)
			{
				if (customTeleportCooldowns[i].cooldownType == cooldownType)
				{
					num = customTeleportCooldowns[i].customTeleportCooldown;
				}
			}
			if (num >= 0f)
			{
				Player.LocalPlayer.PlayerLocomotion.AddTeleportCooldown(cooldownType, num);
			}
		}
		else
		{
			for (int j = 0; j < customTeleportCooldowns.Length; j++)
			{
				Player.LocalPlayer.PlayerLocomotion.RemoveTeleportCooldown(customTeleportCooldowns[j].cooldownType);
			}
		}
	}

	public void RemoveLocalPlayerCustomTeleportCooldown(TeleportCooldownType cooldownType)
	{
		Player.LocalPlayer.PlayerLocomotion.RemoveTeleportCooldown(cooldownType);
	}
}
