using System;
using UnityEngine;

[Serializable]
public class ScenePhysicsSettings : IRecRoomSceneComponent
{
	[SerializeField]
	private Vector3 gravity = new Vector3(0f, -9.81f, 0f);

	[SerializeField]
	private float bounceThreshold = 2f;

	public void OnStart(RecRoomSceneManager sceneManager)
	{
		Physics.gravity = gravity;
		Physics.bounceThreshold = bounceThreshold;
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
