using System;
using UnityEngine;

[Serializable]
public class SceneAFKSettings : IRecRoomSceneComponent
{
	[SerializeField]
	private float afkKickToIntroTimeout = 10f;

	[SerializeField]
	private float afkWarningStartTimeout = 10f;

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
		Player.LocalPlayer.PushAfkKickToIntroTimeout(afkKickToIntroTimeout);
		Player.LocalPlayer.PushAfkWarningStartTimeout(afkWarningStartTimeout);
	}
}
