using UnityEngine;

public class ActiveStateToggler : MonoBehaviour
{
	public void ToggleActive()
	{
		base.gameObject.SetActive(!base.gameObject.activeSelf);
	}
}
