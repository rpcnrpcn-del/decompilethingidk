using UnityEngine;

public class TriggerListener : MonoBehaviour
{
	private DrawingSurface surface;

	private void Awake()
	{
		surface = GetComponentInParent<DrawingSurface>();
	}

	private void OnTriggerEnter(Collider other)
	{
		surface.OnSurfaceTriggerEnter(other);
	}

	private void OnTriggerExit(Collider other)
	{
		surface.OnSurfaceTriggerExit(other);
	}
}
