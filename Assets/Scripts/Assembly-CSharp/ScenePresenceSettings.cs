using System;
using UnityEngine;

[Serializable]
public class ScenePresenceSettings : IRecRoomSceneComponent
{
	[SerializeField]
	private string richPresenceString;

	[SerializeField]
	private string localPlayerStatus;

	public void SetLocalPlayerSettings()
	{
		Player.LocalPlayer.PushRichPresence(richPresenceString);
		Player.LocalPlayer.PushLocalPlayerStatus(localPlayerStatus);
	}

	public void OnStart(RecRoomSceneManager sceneManager)
	{
	}

	public void OnDestroy()
	{
	}

	public void Update()
	{
	}
}
