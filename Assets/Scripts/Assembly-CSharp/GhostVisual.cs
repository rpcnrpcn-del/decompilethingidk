using UnityEngine;

public class GhostVisual : MonoBehaviour
{
	public Renderer[] Renderers { get; private set; }

	private void Awake()
	{
		Renderers = GetComponentsInChildren<Renderer>(true);
	}

	public void SetAlpha(float newAlpha)
	{
		Renderer[] renderers = Renderers;
		foreach (Renderer renderer in renderers)
		{
			if (renderer.material.HasProperty("_Color"))
			{
				renderer.SetColorAlpha(newAlpha);
			}
		}
	}
}
