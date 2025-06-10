using System.Collections.Generic;
using UnityEngine;

public class PlayerLineTrail : MonoBehaviour
{
	[SerializeField]
	private int maxHistoricalSegments = 3;

	[SerializeField]
	private float vertexSpacing = 0.25f;

	[SerializeField]
	private Vector2 alphas = new Vector2(0f, 1f);

	[SerializeField]
	private Vector2 widths = new Vector2(0.2f, 0.8f);

	private LineRenderer lineRenderer;

	private BezierPath bezierPath = new BezierPath();

	private Queue<Vector3> points = new Queue<Vector3>();

	public Color Color
	{
		set
		{
			if (lineRenderer != null)
			{
				Color startColor = value;
				startColor.a = alphas.x;
				Color endColor = value;
				endColor.a = alphas.y;
				lineRenderer.startColor = startColor;
				lineRenderer.endColor = endColor;
			}
		}
	}

	private void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
		if (lineRenderer != null)
		{
			lineRenderer.startWidth = widths.x;
			lineRenderer.endWidth = widths.y;
		}
		Color = Color.white;
		lineRenderer.enabled = false;
	}

	public void Clear()
	{
		points.Clear();
		UpdatePath();
	}

	public void SetLatestPlayerPosition(Vector3 sourcePlayerPosition, Vector3 destinationPlayerPosition)
	{
		Vector3 vector = Vector3.up * widths.y / 2f;
		if (points.Count == 0)
		{
			points.Enqueue(sourcePlayerPosition + vector);
		}
		while (points.Count > maxHistoricalSegments)
		{
			points.Dequeue();
		}
		points.Enqueue(destinationPlayerPosition + vector);
		UpdatePath();
	}

	private void UpdatePath()
	{
		bezierPath.Clear();
		Vector3[] array = points.ToArray();
		for (int i = 0; i < array.Length - 1; i++)
		{
			Vector3 vector = array[i];
			Vector3 vector2 = array[i + 1];
			Vector3 vector3 = vector2 - vector;
			float magnitude = vector3.magnitude;
			Vector3 normalized = vector3.normalized;
			Vector3 zero = Vector3.zero;
			zero = ((i + 1 != array.Length - 1) ? (vector2 - array[i + 2]).normalized : (-normalized));
			Vector3 controlPoint = vector + normalized * 0.5f * magnitude;
			Vector3 controlPoint2 = vector2 + zero * 0.5f * magnitude;
			if (bezierPath.CurveCount == 0)
			{
				bezierPath.AddCurve(vector, controlPoint, controlPoint2, vector2);
			}
			else
			{
				bezierPath.AddCurve(controlPoint, controlPoint2, vector2);
			}
		}
		if (bezierPath.CurveCount == 0)
		{
			lineRenderer.enabled = false;
			return;
		}
		int num = Mathf.RoundToInt(bezierPath.PathLength / vertexSpacing);
		Vector3[] array2 = new Vector3[num];
		for (int j = 0; j < num; j++)
		{
			array2[j] = bezierPath.EvaluatePath((float)j / (float)(num - 1));
		}
		lineRenderer.enabled = true;
		lineRenderer.numPositions = num;
		lineRenderer.SetPositions(array2);
	}
}
