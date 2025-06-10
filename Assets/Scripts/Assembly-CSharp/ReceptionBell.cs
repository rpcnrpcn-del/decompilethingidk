using UnityEngine;

public class ReceptionBell : MonoBehaviour
{
	[SerializeField]
	private AudioSource audioSource;

	private void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.IsInLayerMask(LayerMasks.ToolAndAnyPlayerPhysics))
		{
			audioSource.Play();
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.IsInLayerMask(LayerMasks.AnyPlayerPhysics))
		{
			PlayerCollider component = col.gameObject.GetComponent<PlayerCollider>();
			if (component != null && component.BodyPart.IsHand())
			{
				audioSource.Play();
			}
		}
	}
}
