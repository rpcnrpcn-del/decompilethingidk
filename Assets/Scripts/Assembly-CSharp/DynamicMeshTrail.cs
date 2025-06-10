using System.Collections.Generic;
using UnityEngine;

public class DynamicMeshTrail : CustomTrail<MeshRenderer>
{
	private MeshFilter[] meshFilters;

	private List<Vector3> vertices = new List<Vector3>();

	private List<Color> vertexColors = new List<Color>();

	private List<Vector2> uvs = new List<Vector2>();

	private List<int> indices = new List<int>();

	private Mesh[] doubleBufferedMeshes;

	private int meshIndex;

	private Color color;

	private Vector2 _widths;

	private Mesh Mesh
	{
		get
		{
			return doubleBufferedMeshes[meshIndex];
		}
	}

	public override Color Color
	{
		get
		{
			return color;
		}
		set
		{
			color = value;
		}
	}

	public override Vector2 Widths
	{
		get
		{
			return _widths;
		}
		set
		{
			_widths = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		doubleBufferedMeshes = new Mesh[2];
		for (int i = 0; i < doubleBufferedMeshes.Length; i++)
		{
			doubleBufferedMeshes[i] = new Mesh();
			doubleBufferedMeshes[i].MarkDynamic();
		}
		if (renderers != null)
		{
			meshFilters = new MeshFilter[renderers.Length];
			for (int j = 0; j < renderers.Length; j++)
			{
				meshFilters[j] = renderers[j].gameObject.AddComponent<MeshFilter>();
				meshFilters[j].mesh = Mesh;
			}
		}
	}

	protected override void ApplyPoints()
	{
		if (meshFilters != null)
		{
			meshIndex = (meshIndex + 1) % doubleBufferedMeshes.Length;
			vertices.Clear();
			uvs.Clear();
			vertexColors.Clear();
			indices.Clear();
			Color a = color;
			a.a = alphas[0] * fadeAlpha;
			Color b = color;
			b.a = alphas[1] * fadeAlpha;
			float num = Widths[0] * widthScale;
			float num2 = Widths[1] * widthScale;
			Vector2 zero = Vector2.zero;
			for (int i = 0; i < points.Count; i++)
			{
				float num3 = Mathf.InverseLerp(pointTimeStamps[i] + lifetime, pointTimeStamps[i], Time.time);
				float num4 = Mathf.Lerp(num, num2, num3) * 0.5f;
				vertices.Add(base.transform.InverseTransformPoint(points[i] + tangents[i] * num4));
				vertices.Add(base.transform.InverseTransformPoint(points[i] - tangents[i] * num4));
				Color item = Color.Lerp(a, b, num3);
				vertexColors.Add(item);
				vertexColors.Add(item);
				zero.x = 1f - num3;
				zero.y = 1f;
				uvs.Add(zero);
				zero.y = 0f;
				uvs.Add(zero);
			}
			int num5 = Mathf.Max(0, points.Count - 1);
			for (int j = 0; j < num5; j++)
			{
				int num6 = j * 2;
				indices.Add(num6);
				indices.Add(num6 + 2);
				indices.Add(num6 + 1);
				indices.Add(num6 + 2);
				indices.Add(num6 + 3);
				indices.Add(num6 + 1);
			}
			Mesh.Clear(true);
			Mesh.vertices = vertices.ToArray();
			Mesh.colors = vertexColors.ToArray();
			Mesh.uv = uvs.ToArray();
			Mesh.triangles = indices.ToArray();
			for (int k = 0; k < meshFilters.Length; k++)
			{
				meshFilters[k].mesh = Mesh;
			}
			if (displayStartCap && startCap != null)
			{
				startCap.material.color = a;
				float num7 = num / 2f / startCap.transform.lossyScale.x;
				startCap.transform.localScale *= num7;
			}
			if (displayEndCap && endCap != null)
			{
				endCap.material.SetColor(CustomTrail<MeshRenderer>.ParticleMaterialColorId, b);
				float num8 = num2 / 2f / endCap.transform.lossyScale.x;
				endCap.transform.localScale *= num8;
			}
		}
	}
}
