using UnityEngine;

public class SoccerGarageDoor : MonoBehaviour
{
	[SerializeField]
	private Transform visualRoot;

	public bool Open
	{
		get
		{
			return !visualRoot.gameObject.activeSelf;
		}
		set
		{
			visualRoot.gameObject.SetActive(!value);
		}
	}
}
