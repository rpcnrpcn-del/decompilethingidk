using System.Collections.Generic;
using UnityEngine;

public class BezierPath
{
	private const int TOTAL_POINTS_PER_BEZIER_CURVE = 4;

	private const int UNIQUE_POINTS_PER_BEZIER_CURVE = 3;

	private const int LENGTH_SEGMENTS = 5;

	private const float LENGTH_SEGMENT_FRACTION = 0.2f;

	private List<Vector3> points;

	private List<float> lengthAtCurve;

	public int CurveCount { get; private set; }

	public float PathLength { get; private set; }

	public BezierPath()
	{
		points = new List<Vector3>();
		lengthAtCurve = new List<float>();
		CurveCount = 0;
		PathLength = 0f;
	}

	public void AddCurve(Vector3 start, Vector3 controlPoint1, Vector3 controlPoint2, Vector3 end)
	{
		if (points.Count == 0)
		{
			points.Add(start);
		}
		else if (Vector3.Distance(start, points[points.Count - 1]) > Mathf.Epsilon)
		{
			Debug.LogError("Tried to add discontinuity to Bezier Path.");
		}
		AddCurve(controlPoint1, controlPoint2, end);
	}

	public void AddCurve(Vector3 controlPoint1, Vector3 controlPoint2, Vector3 end)
	{
		if (points.Count == 0)
		{
			Debug.LogError("Tried to add a Bezier Path curve without a starting point.");
		}
		points.Add(controlPoint1);
		points.Add(controlPoint2);
		points.Add(end);
		CurveCount++;
		PathLength += CurveLength(CurveCount - 1);
		lengthAtCurve.Add(PathLength);
	}

	public void Clear()
	{
		points.Clear();
		lengthAtCurve.Clear();
		CurveCount = 0;
		PathLength = 0f;
	}

	public Vector3 EvaluatePath(float t)
	{
		if (CurveCount <= 0)
		{
			return Vector3.zero;
		}
		int num = -1;
		float t2 = 0f;
		float num2 = Mathf.Clamp01(t) * PathLength;
		for (int i = 0; i < CurveCount; i++)
		{
			if (num2 <= lengthAtCurve[i])
			{
				num = i;
				float num3 = ((i != 0) ? lengthAtCurve[i - 1] : 0f);
				float num4 = lengthAtCurve[i] - num3;
				t2 = (num2 - num3) / num4;
				break;
			}
		}
		if (num < 0)
		{
			num = CurveCount - 1;
			t2 = 1f;
		}
		return EvaluateBezierCurve(num, t2);
	}

	public void DrawGizmos()
	{
		for (int i = 0; i < CurveCount; i++)
		{
			DrawGizmosBezierCurve(i);
		}
	}

	private Vector3 EvaluateBezierCurve(int curveIndex, float t)
	{
		Vector3 vector = points[curveIndex * 3];
		Vector3 vector2 = points[curveIndex * 3 + 1];
		Vector3 vector3 = points[curveIndex * 3 + 2];
		Vector3 vector4 = points[curveIndex * 3 + 3];
		float num = 1f - t;
		return vector * num * num * num + vector2 * 3f * num * num * t + vector3 * 3f * num * t * t + vector4 * t * t * t;
	}

	private float CurveLength(int curveIndex)
	{
		float num = 0f;
		float num2 = 0f;
		Vector3 vector = EvaluateBezierCurve(curveIndex, num2);
		for (int i = 0; i < 5; i++)
		{
			num2 += 0.2f;
			Vector3 vector2 = EvaluateBezierCurve(curveIndex, num2);
			num += (vector2 - vector).magnitude;
			vector = vector2;
		}
		return num;
	}

	private void DrawGizmosBezierCurve(int curveIndex)
	{
		Vector3 vector = points[curveIndex * 3];
		Vector3 to = points[curveIndex * 3 + 1];
		Vector3 vector2 = points[curveIndex * 3 + 2];
		Vector3 to2 = points[curveIndex * 3 + 3];
		Gizmos.color = Color.white;
		Gizmos.DrawLine(vector, to);
		Gizmos.DrawLine(vector2, to2);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(vector, to2);
	}
}
