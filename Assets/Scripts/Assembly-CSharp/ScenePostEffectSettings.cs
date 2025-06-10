using System;
using UnityEngine;

[Serializable]
public class ScenePostEffectSettings : IRecRoomSceneComponent
{
	[SerializeField]
	private Texture tonemappingLUT;

	public void OnStart(RecRoomSceneManager sceneManager)
	{
		SingletonMonoBehaviour<CameraRig>.Instance.SetTonemappingLUT(tonemappingLUT);
	}

	public void OnDestroy()
	{
	}

	public void Update()
	{
	}

	public void SetLocalPlayerSettings()
	{
	}
}
