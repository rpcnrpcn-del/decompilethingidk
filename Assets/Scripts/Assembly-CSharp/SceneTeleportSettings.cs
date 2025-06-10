using System;
using UnityEngine;

[Serializable]
public class SceneTeleportSettings : IRecRoomSceneComponent
{
	[Header("Cooldown")]
	[SerializeField]
	private float cooldown = 0.1f;

	[Header("Max Teleport Distance")]
	[SerializeField]
	private float maxTeleportDistance = 12f;

	[SerializeField]
	private float maxTeleportAngle = 45f;

	public void OnStart(RecRoomSceneManager sceneManager)
	{
	}

	public void OnDestroy()
	{
	}

	public void Update()
	{
	}

	public void SetLocalPlayerSettings()
	{
		Player.LocalPlayer.PlayerLocomotion.AddTeleportCooldown(TeleportCooldownType.DEFAULT, cooldown);
		Player.LocalPlayer.PlayerLocomotion.BaseMaxTeleportDistance = maxTeleportDistance;
		Player.LocalPlayer.PlayerLocomotion.MaxTeleportPitchDegrees = maxTeleportAngle;
	}
}
