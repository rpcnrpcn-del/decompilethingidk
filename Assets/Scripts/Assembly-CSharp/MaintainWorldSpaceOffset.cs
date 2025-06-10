using UnityEngine;

public class MaintainWorldSpaceOffset : MonoBehaviour
{
	[SerializeField]
	private Transform origin;

	[SerializeField]
	private Vector3 worldSpaceOffset = new Vector3(0f, 0.5f, 0f);

	private void Update()
	{
		base.transform.position = origin.position + worldSpaceOffset;
	}
}
