using UnityEngine;

public class GameInvisibleWall : MonoBehaviour
{
	private struct CollisionPoint
	{
		public bool Active;

		public Vector3 Center;

		public float Radius;

		public float Lifetime;

		public int ImpactShaderPropertyId;

		public int AlphaShaderPropertyId;
	}

	private const int MAX_COLLISION_COUNT = 8;

	private CollisionPoint[] collisionPoints = new CollisionPoint[8];

	private void Awake()
	{
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			if (collider.gameObject.GetComponent<CollisionForwarder>() == null)
			{
				collider.gameObject.AddComponent<CollisionForwarder>().CollisionEnter += OnCollisionEnter;
			}
		}
		for (int j = 0; j < collisionPoints.Length; j++)
		{
			collisionPoints[j] = new CollisionPoint
			{
				Active = false,
				Center = Vector3.zero,
				Radius = 0f,
				Lifetime = 0f,
				ImpactShaderPropertyId = Shader.PropertyToID("_ImpactPoint" + j),
				AlphaShaderPropertyId = Shader.PropertyToID("_ImpactPointAlpha" + j)
			};
		}
	}

	private void Update()
	{
		for (int i = 0; i < collisionPoints.Length; i++)
		{
			if (collisionPoints[i].Active)
			{
				collisionPoints[i].Lifetime += Time.deltaTime / InvisibleWallVisualSettings.ImpactDuration;
				if (collisionPoints[i].Lifetime >= 1f)
				{
					collisionPoints[i].Active = false;
				}
			}
			float w = InvisibleWallVisualSettings.EvaluateImpactGrowthCurve(collisionPoints[i].Lifetime) * collisionPoints[i].Radius;
			float value = InvisibleWallVisualSettings.EvaluateImpactAlphaCurve(collisionPoints[i].Lifetime);
			if (!collisionPoints[i].Active)
			{
				value = 0f;
				w = 0f;
			}
			Shader.SetGlobalVector(vec: new Vector4(collisionPoints[i].Center.x, collisionPoints[i].Center.y, collisionPoints[i].Center.z, w), nameID: collisionPoints[i].ImpactShaderPropertyId);
			Shader.SetGlobalFloat(collisionPoints[i].AlphaShaderPropertyId, value);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		int num = 0;
		for (int i = 0; i < collisionPoints.Length; i++)
		{
			if (!collisionPoints[i].Active)
			{
				num = i;
				break;
			}
		}
		collisionPoints[num].Active = true;
		collisionPoints[num].Center = ((collision.contacts.Length <= 0) ? collision.collider.transform.position : collision.contacts[0].point);
		collisionPoints[num].Radius = InvisibleWallVisualSettings.ConvertImpactVelocityToImpactRadius(collision.relativeVelocity);
		collisionPoints[num].Lifetime = 0f;
	}
}
