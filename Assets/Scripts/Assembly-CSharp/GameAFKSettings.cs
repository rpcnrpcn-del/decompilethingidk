using System;
using UnityEngine;

[Serializable]
public class GameAFKSettings : IGameComponent
{
	[SerializeField]
	private bool supportsCustomAfkSettings;

	[SerializeField]
	private float afkKickToIntroTimeout = 10f;

	[SerializeField]
	private float afkWarningStartTimeout = 10f;

	[SerializeField]
	private bool supportsGhosting = true;

	private bool settingsPushed;

	public bool SupportsGhosting
	{
		get
		{
			return supportsGhosting;
		}
	}

	public void OnAwake(GameManager gameManager)
	{
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

	public void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
		if (!localPlayerIsSpectator && supportsCustomAfkSettings && !settingsPushed)
		{
			Player.LocalPlayer.PushAfkKickToIntroTimeout(afkKickToIntroTimeout);
			Player.LocalPlayer.PushAfkWarningStartTimeout(afkWarningStartTimeout);
			settingsPushed = true;
		}
		Player.LocalPlayer.GhostModeEnabled = supportsGhosting;
	}

	public void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
		if (settingsPushed)
		{
			Player.LocalPlayer.PopAfkKickToIntroTimeout();
			Player.LocalPlayer.PopAfkWarningStartTimeout();
			settingsPushed = false;
		}
		Player.LocalPlayer.GhostModeEnabled = true;
	}

	public void OnPlayerDisconnected(PhotonPlayer player)
	{
	}
}
