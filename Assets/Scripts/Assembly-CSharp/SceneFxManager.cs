using System;
using UnityEngine;

[Serializable]
public class SceneFxManager : FxManager, IRecRoomSceneComponent
{
	[Header("Ambient Audio")]
	[SerializeField]
	private RecRoomAudioClip ambientAudio;

	[Header("Custom Rolloff")]
	[SerializeField]
	private AudioRolloff customPlayerVoipAudioRolloff;

	[SerializeField]
	private AudioRolloff custom3DSFXAudioRolloff;

	[Header("Teleport Sounds")]
	[SerializeField]
	private RecRoomAudioClip[] customTeleportSounds;

	public void OnStart(RecRoomSceneManager sceneManager)
	{
		base.OnAwake();
		if (ambientAudio != null)
		{
			AudioManager.PlayAmbientAudio(ambientAudio);
		}
		PlayerAudio.PlayerVoipAudioRolloff = customPlayerVoipAudioRolloff;
		PlayerAudio.Custom3DSFXAudioRolloff = custom3DSFXAudioRolloff;
		PlayerAudio.CustomTeleportSounds = customTeleportSounds;
	}

	public override void OnDestroy()
	{
		AudioManager.StopAmbientAudio();
		base.OnDestroy();
	}

	public void Update()
	{
	}

	public void SetLocalPlayerSettings()
	{
	}
}
