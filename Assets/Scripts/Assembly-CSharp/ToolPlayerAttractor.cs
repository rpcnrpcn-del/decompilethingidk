using UnityEngine;

[RequireComponent(typeof(Tool))]
public class ToolPlayerAttractor : MonoBehaviour
{
	[SerializeField]
	private bool enabledOnReset = true;

	[SerializeField]
	private bool enableOnPoolSpawn;

	private Tool thisTool;

	private PulsingBeam pulsingBeam;

	private void Awake()
	{
		pulsingBeam = GetComponentInChildren<PulsingBeam>(true);
		if (pulsingBeam == null)
		{
			Debug.LogError("ToolPlayerAttactor missing required pulsing beam.");
			return;
		}
		thisTool = GetComponent<Tool>();
		thisTool.PickupEvent += OnThisToolPickup;
		thisTool.ResetEvent += OnThisToolReset;
	}

	private void Start()
	{
		pulsingBeam.gameObject.SetActive(enableOnPoolSpawn || !thisTool.SpawnedFromPool);
	}

	private void OnDestroy()
	{
		thisTool.PickupEvent -= OnThisToolPickup;
		thisTool.ResetEvent -= OnThisToolReset;
	}

	private void OnThisToolPickup(Tool tool)
	{
		pulsingBeam.gameObject.SetActive(false);
	}

	private void OnThisToolReset(Tool tool, Vector3 position, Quaternion rotation)
	{
		if (enabledOnReset)
		{
			pulsingBeam.gameObject.SetActive(enableOnPoolSpawn || !thisTool.SpawnedFromPool);
		}
	}
}
