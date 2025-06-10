using UnityEngine;
using UnityEngine.VR;

public class VRRenderScale : MonoBehaviour
{
	[Range(0.1f, 4f)]
	public float renderScale = 1f;

	private void Update()
	{
		VRSettings.renderScale = renderScale;
	}
}
