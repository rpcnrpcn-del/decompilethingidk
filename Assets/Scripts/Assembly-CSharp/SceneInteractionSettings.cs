using System;
using UnityEngine;

[Serializable]
public class SceneInteractionSettings : IRecRoomSceneComponent
{
	[SerializeField]
	private bool customInteractionSettings;

	[Header("Gravity Pickup")]
	[SerializeField]
	private float gravityPickupFingerOffset = 0.05f;

	[SerializeField]
	private float gravityPickupCapsuleRadius = 0.15f;

	[SerializeField]
	private float gravityPickupSphereRadius = 0.25f;

	[SerializeField]
	private PlayerHand.PickupDirectionMode gravityPickupMode;

	[Header("Physical Pickup")]
	[SerializeField]
	private float physicalPickupSphereRadius = 0.11f;

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
		if (customInteractionSettings)
		{
			Player.LocalPlayer.LeftHand.GravityPickupFingerOffset = gravityPickupFingerOffset;
			Player.LocalPlayer.LeftHand.GravityPickupDirectionMode = gravityPickupMode;
			Player.LocalPlayer.LeftHand.GravityPickupCapsuleRadius = gravityPickupCapsuleRadius;
			Player.LocalPlayer.LeftHand.GravityPickupSphereRadius = gravityPickupSphereRadius;
			Player.LocalPlayer.LeftHand.PhysicalPickupSphereRadius = physicalPickupSphereRadius;
			Player.LocalPlayer.RightHand.GravityPickupFingerOffset = gravityPickupFingerOffset;
			Player.LocalPlayer.RightHand.GravityPickupDirectionMode = gravityPickupMode;
			Player.LocalPlayer.RightHand.GravityPickupCapsuleRadius = gravityPickupCapsuleRadius;
			Player.LocalPlayer.RightHand.GravityPickupSphereRadius = gravityPickupSphereRadius;
			Player.LocalPlayer.RightHand.PhysicalPickupSphereRadius = physicalPickupSphereRadius;
		}
	}
}
