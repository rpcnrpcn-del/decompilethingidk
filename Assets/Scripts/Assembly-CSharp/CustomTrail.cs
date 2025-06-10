using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class CustomTrail<T> : MonoBehaviour where T : Renderer
{
	[Header("Visuals")]
	[SerializeField]
	protected Material[] materials;

	[SerializeField]
	private bool castShadows;

	[SerializeField]
	private bool receiveShadows;

	[Header("Layout")]
	[SerializeField]
	protected float lifetime = 5f;

	[SerializeField]
	protected Vector2 alphas = new Vector2(0f, 1f);

	[SerializeField]
	protected Vector2 widths = new Vector2(0.2f, 0.8f);

	[SerializeField]
	protected float minSpacing = 0.01f;

	[Header("End Caps")]
	[SerializeField]
	protected bool displayStartCap;

	[SerializeField]
	protected Renderer startCap;

	[SerializeField]
	protected bool displayEndCap;

	[SerializeField]
	protected Renderer endCap;

	protected static int ParticleMaterialColorId = -1;

	protected float fadeAlpha = 1f;

	protected float widthScale = 1f;

	protected float minSpacingSqr;

	protected bool lineDirty = true;

	protected T[] renderers;

	protected List<float> pointTimeStamps = new List<float>();

	protected List<Vector3> points = new List<Vector3>();

	protected List<Vector3> tangents = new List<Vector3>();

	protected List<float> segmentLengths = new List<float>();

	public abstract Color Color { get; set; }

	public abstract Vector2 Widths { get; set; }

	public bool Running
	{
		get
		{
			return points.Count > 0;
		}
	}

	public float Length
	{
		get
		{
			float num = 0f;
			for (int i = 0; i < segmentLengths.Count; i++)
			{
				num += segmentLengths[i];
			}
			return num;
		}
	}

	protected bool RenderersEnabled
	{
		set
		{
			if (renderers != null)
			{
				for (int i = 0; i < renderers.Length; i++)
				{
					renderers[i].enabled = value;
				}
			}
			if (startCap != null)
			{
				startCap.enabled = value;
			}
			if (endCap != null)
			{
				endCap.enabled = value;
			}
		}
	}

	protected abstract void ApplyPoints();

	public virtual void Clear()
	{
		pointTimeStamps.Clear();
		points.Clear();
		tangents.Clear();
		segmentLengths.Clear();
		lineDirty = true;
		UpdateLine();
	}

	protected virtual void Awake()
	{
		ParticleMaterialColorId = Shader.PropertyToID("_TintColor");
		minSpacingSqr = minSpacing * minSpacing;
		if (materials == null || materials.Length == 0)
		{
			Debug.LogError("Trail on " + base.gameObject.name + " is missing materials.");
			return;
		}
		renderers = new T[materials.Length];
		T[] componentsInChildren = GetComponentsInChildren<T>(true);
		int i;
		for (i = 0; i < renderers.Length && i < componentsInChildren.Length; i++)
		{
			renderers[i] = componentsInChildren[i];
		}
		GameObject gameObject = base.gameObject;
		for (; i < renderers.Length; i++)
		{
			if (i > 0)
			{
				gameObject = new GameObject(materials[i].name);
				gameObject.transform.SetParent(base.transform, false);
			}
			renderers[i] = gameObject.AddComponent<T>();
		}
		for (; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i]);
		}
		for (i = 0; i < renderers.Length; i++)
		{
			renderers[i].sharedMaterial = materials[i];
			renderers[i].shadowCastingMode = (castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
			renderers[i].receiveShadows = receiveShadows;
		}
		Widths = widths;
		Color = Color.white;
	}

	protected virtual void OnEnable()
	{
		RenderersEnabled = true;
		Clear();
	}

	protected virtual void OnDisable()
	{
		RenderersEnabled = false;
		Clear();
	}

	protected virtual void Update()
	{
		UpdateLine();
	}

	protected void UpdateLine()
	{
		float num = Time.time - lifetime;
		int i;
		for (i = 0; i < pointTimeStamps.Count && pointTimeStamps[i] <= num; i++)
		{
		}
		if (i > 0)
		{
			pointTimeStamps.RemoveRange(0, i);
			points.RemoveRange(0, i);
			tangents.RemoveRange(0, i);
			if (i + 1 < segmentLengths.Count)
			{
				segmentLengths[i + 1] = 0f;
			}
			segmentLengths.RemoveRange(0, i);
			lineDirty = true;
		}
		if (lineDirty)
		{
			ApplyPoints();
			lineDirty = false;
		}
		if (points.Count < 2)
		{
			RenderersEnabled = false;
		}
		else if (points.Count > 1)
		{
			if (displayStartCap && startCap != null)
			{
				startCap.transform.position = points[0];
				Vector3 normalized = (points[0] - points[1]).normalized;
				startCap.transform.rotation = Quaternion.LookRotation(normalized);
			}
			if (displayEndCap && endCap != null)
			{
				endCap.transform.position = points[points.Count - 1];
				Vector3 normalized2 = (points[points.Count - 2] - points[points.Count - 1]).normalized;
				endCap.transform.rotation = Quaternion.LookRotation(normalized2);
			}
		}
	}

	protected virtual void AddNewPosition(Vector3 newPosition, Vector3 newTangent, float addTime)
	{
		if (points.Count > 0)
		{
			segmentLengths.Add((newPosition - points[points.Count - 1]).magnitude);
		}
		else
		{
			segmentLengths.Add(0f);
		}
		points.Add(newPosition);
		tangents.Add(newTangent);
		pointTimeStamps.Add(addTime);
		lineDirty = true;
		RenderersEnabled = true;
	}

	protected void UpdatePoint(int index, Vector3 newPosition, Vector3 newTangent)
	{
		if (index < points.Count)
		{
			points[index] = newPosition;
			tangents[index] = newTangent;
			if (index > 0)
			{
				segmentLengths[index] = (points[index] - points[index - 1]).magnitude;
			}
			if (index + 1 < points.Count)
			{
				segmentLengths[index + 1] = (points[index + 1] - points[index]).magnitude;
			}
		}
	}
}
