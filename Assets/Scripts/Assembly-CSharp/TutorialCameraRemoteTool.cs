using UnityEngine;

public class TutorialCameraRemoteTool : Tool
{
	[Header("Tutorial Camera Remote Settings")]
	[SerializeField]
	private TutorialCameraTool camera;

	public override void OnInputDown()
	{
		base.OnInputDown();
		camera.ToggleRecording();
	}
}
