using System.Collections.Generic;
using UnityEngine;

public class StaticBatchManager : MonoBehaviour
{
	public float SpacePartitionCubeSize = 10f;

	public bool ForceLOD0;

	[HideInInspector]
	public List<MeshRenderer> originalAssets;

	[HideInInspector]
	public List<LODGroup> originalLodGroups;

	[HideInInspector]
	public List<GameObject> staticBatches;
}
