using UnityEngine;

public class MainTextureOverride : MonoBehaviour
{
	[SerializeField]
	private Renderer renderer;

	[Header("Texture Overrides")]
	[SerializeField]
	private Texture2D defaultTexture;

	[SerializeField]
	private Texture2D steamVrVive;

	[SerializeField]
	private Texture2D steamVrOculus;

	[SerializeField]
	private Texture2D oculus;

	private void Start()
	{
		Texture2D texture2D = null;
		switch (ControllerIO.CurrentInputMode)
		{
		case ControllerIO.InputMode.SteamVR_Vive:
			texture2D = steamVrVive;
			break;
		case ControllerIO.InputMode.SteamVR_Oculus:
			texture2D = steamVrOculus;
			break;
		case ControllerIO.InputMode.Oculus:
			texture2D = oculus;
			break;
		}
		if (texture2D == null)
		{
			texture2D = defaultTexture;
		}
		if (texture2D != null)
		{
			renderer.material.mainTexture = texture2D;
		}
	}
}
