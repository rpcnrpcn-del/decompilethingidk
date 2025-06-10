using UnityEngine;

public class ImpactDecal : MonoBehaviour
{
	[SerializeField]
	private float lifetime = 2f;

	[SerializeField]
	private float fadeOutTime = 1f;

	[SerializeField]
	private float bias = 0.01f;

	[SerializeField]
	private Vector2 defaultMinMaxScale = new Vector2(0.8f, 1.2f);

	private Renderer[] renderers;

	private Vector3 defaultScale = Vector3.one;

	private float enableTime;

	private Transform targetParent;

	private Quaternion targetLocalRotation = Quaternion.identity;

	private Vector3 targetLocalPosition = Vector3.zero;

	private Color _color;

	public Color Color
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
			Renderer[] array = renderers;
			foreach (Renderer renderer in array)
			{
				renderer.material.color = _color;
			}
		}
	}

	private void Awake()
	{
		renderers = GetComponentsInChildren<Renderer>();
		defaultScale = base.transform.localScale;
	}

	private void OnEnable()
	{
		enableTime = -1f;
	}

	private void OnDisable()
	{
		if (base.gameObject.activeSelf)
		{
			ReleaseDecal();
		}
	}

	private void ReleaseDecal()
	{
		ObjectPool.Instance.Release(this);
		targetParent = null;
	}

	private void Update()
	{
		if (enableTime < 0f)
		{
			return;
		}
		float num = Time.time - enableTime;
		if (num > lifetime)
		{
			float num2 = num - lifetime;
			Color color = Color;
			color.a = 1f - Mathf.Clamp01(num2 / fadeOutTime);
			Color = color;
			if (num2 >= fadeOutTime)
			{
				ReleaseDecal();
			}
		}
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		if (targetParent != null)
		{
			base.transform.position = targetParent.TransformPoint(targetLocalPosition);
			base.transform.rotation = targetParent.TransformRotation(targetLocalRotation);
		}
	}

	public void Splat(Vector3 position, Vector3 normal, GameObject hitGameObject)
	{
		Splat(position, normal, hitGameObject, defaultMinMaxScale.x, defaultMinMaxScale.y);
	}

	public void Splat(Vector3 position, Vector3 normal, GameObject hitGameObject, float minScale, float maxScale)
	{
		targetParent = hitGameObject.transform;
		Quaternion rotation = Quaternion.LookRotation(-normal, Vector3.up) * Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
		targetLocalRotation = targetParent.InverseTransformRotation(rotation);
		targetLocalPosition = targetParent.InverseTransformPoint(position + normal * bias);
		Vector3 vector = defaultScale * Random.Range(minScale, maxScale);
		Vector3 lossyScale = base.transform.lossyScale;
		if (lossyScale != vector)
		{
			Vector3 b = new Vector3(base.transform.localScale.x / lossyScale.x, base.transform.localScale.y / lossyScale.y, base.transform.localScale.z / lossyScale.z);
			base.transform.localScale = Vector3.Scale(vector, b);
		}
		base.enabled = true;
		enableTime = Time.time;
		UpdatePosition();
	}
}
