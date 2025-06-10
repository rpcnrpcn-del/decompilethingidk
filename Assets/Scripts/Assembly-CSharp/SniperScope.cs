using UnityEngine;

public class SniperScope : MonoBehaviour
{
	[SerializeField]
	private Renderer scopeRenderer;

	[SerializeField]
	private Camera scopeCamera;

	[SerializeField]
	private float fov = 5f;

	[SerializeField]
	private int resolution = 256;

	private RenderTexture renderTexture;

	private Tool tool;

	private bool RenderingEnabled
	{
		set
		{
			if (scopeCamera != null)
			{
				scopeCamera.enabled = value;
			}
			if (tool != null && tool.ToolRenderer != null)
			{
				for (int i = 0; i < tool.ToolRenderer.Renderers.Count; i++)
				{
					tool.ToolRenderer.Renderers[i].gameObject.layer = ((!value) ? 10 : 16);
				}
			}
		}
	}

	private void Awake()
	{
		renderTexture = new RenderTexture(resolution, resolution, 24);
		if (scopeCamera == null || scopeRenderer == null)
		{
			Debug.LogError("SniperScope: Missing camera or renderer.");
			return;
		}
		scopeCamera.fieldOfView = fov;
		scopeCamera.targetTexture = renderTexture;
		scopeRenderer.material.mainTexture = renderTexture;
		tool = GetComponent<Tool>();
		if (tool != null)
		{
			tool.PickupEvent += OnToolPickup;
			tool.ReleaseEvent += OnToolRelease;
			RenderingEnabled = false;
		}
		else
		{
			RenderingEnabled = true;
		}
	}

	private void OnDestroy()
	{
		if (tool != null)
		{
			tool.PickupEvent -= OnToolPickup;
			tool.ReleaseEvent -= OnToolRelease;
		}
	}

	private void OnToolPickup(Tool tool)
	{
		if (tool.Owner.isLocal)
		{
			RenderingEnabled = true;
		}
	}

	private void OnToolRelease(Tool tool)
	{
		RenderingEnabled = false;
	}
}
