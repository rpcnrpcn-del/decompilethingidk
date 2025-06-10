using System.Collections.Generic;
using UnityEngine;

public class FaceStretchInteraction : MonoBehaviour
{
	private static Color visualColor = new Color(0f, 1f, 0.8f, 1f);

	private static float visualScale = 0.05f;

	private static float colliderRadius = 0.025f;

	private static float defaultAlpha = 0.1f;

	private static float highlightAlpha = 0.7f;

	private Renderer visualRenderer;

	private List<PlayerHand> interactingHands = new List<PlayerHand>();

	private void OnTriggerEnter(Collider collider)
	{
		PlayerHand componentInParents = collider.gameObject.GetComponentInParents<PlayerHand>();
		if (componentInParents != null)
		{
			if (interactingHands.Count == 0)
			{
				Color color = visualRenderer.material.color;
				color.a = highlightAlpha;
				visualRenderer.material.color = color;
			}
			if (!interactingHands.Contains(componentInParents))
			{
				interactingHands.Add(componentInParents);
				componentInParents.Vibrate(1, 1000);
			}
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		PlayerHand componentInParents = collider.gameObject.GetComponentInParents<PlayerHand>();
		if (componentInParents != null)
		{
			if (interactingHands.Contains(componentInParents))
			{
				interactingHands.Remove(componentInParents);
			}
			if (interactingHands.Count == 0)
			{
				Color color = visualRenderer.material.color;
				color.a = defaultAlpha;
				visualRenderer.material.color = color;
			}
		}
	}

	private void Awake()
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		gameObject.transform.SetParent(base.transform, false);
		gameObject.transform.localScale = Vector3.one * visualScale;
		Object.Destroy(gameObject.GetComponent<Collider>());
		SphereCollider sphereCollider = base.gameObject.AddComponent<SphereCollider>();
		sphereCollider.radius = colliderRadius;
		sphereCollider.isTrigger = true;
		visualRenderer = gameObject.GetComponent<Renderer>();
		visualRenderer.material.shader = Shader.Find("Transparent/Diffuse");
		Color color = visualColor;
		color.a = defaultAlpha;
		visualRenderer.material.color = color;
	}

	private void Update()
	{
		foreach (PlayerHand interactingHand in interactingHands)
		{
			if (interactingHand.ControllerIO != null && interactingHand.ControllerIO.TriggerButtonPressed)
			{
				base.transform.position = interactingHand.transform.position;
			}
		}
	}
}
