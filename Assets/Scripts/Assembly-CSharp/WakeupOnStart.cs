using Photon;
using UnityEngine;

public class WakeupOnStart : Photon.MonoBehaviour
{
	private void Start()
	{
		if (base.hasAuthority)
		{
			Rigidbody component = GetComponent<Rigidbody>();
			if (component != null)
			{
				component.WakeUp();
				component.isKinematic = false;
			}
		}
	}
}
