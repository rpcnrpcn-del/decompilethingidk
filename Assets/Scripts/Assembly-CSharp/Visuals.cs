using UnityEngine;

public class Visuals : MonoBehaviour
{
	[SerializeField]
	private Transform visualRoot;

	public bool Visible
	{
		get
		{
			return visualRoot.gameObject.activeSelf;
		}
		set
		{
			visualRoot.gameObject.SetActive(value);
		}
	}
}
