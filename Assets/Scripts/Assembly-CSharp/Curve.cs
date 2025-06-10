using System.Collections.Generic;
using UnityEngine;

public class Curve
{
	public Vector3 Position { get; private set; }

	public Quaternion Rotation { get; private set; }

	public List<ControlPoint> ControlPoints { get; private set; }

	public List<ControlPoint> IncompleteControlPoints { get; private set; }

	public bool Finished { get; private set; }

	public ControlPoint LatestControlPoint
	{
		get
		{
			return (ControlPoints.Count <= 0) ? null : ControlPoints[ControlPoints.Count - 1];
		}
	}

	public ControlPoint PreviousControlPoint
	{
		get
		{
			return (ControlPoints.Count <= 1) ? null : ControlPoints[ControlPoints.Count - 2];
		}
	}

	public ControlPoint PreviousPreviousControlPoint
	{
		get
		{
			return (ControlPoints.Count <= 2) ? null : ControlPoints[ControlPoints.Count - 3];
		}
	}

	public ControlPoint LastFinishedControlPoint
	{
		get
		{
			int num = ControlPoints.Count - IncompleteControlPoints.Count - 1;
			if (num < 0)
			{
				return null;
			}
			return ControlPoints[num];
		}
	}

	public Curve(Vector3 position, Quaternion rotation)
	{
		Position = position;
		Rotation = rotation;
		Finished = false;
		ControlPoints = new List<ControlPoint>();
		IncompleteControlPoints = new List<ControlPoint>();
	}

	public void AddControlPoint(Vector3 position)
	{
		if (Finished)
		{
			return;
		}
		ControlPoints.Add(new ControlPoint());
		UpdateLatestControlPoint(position);
		IncompleteControlPoints.Clear();
		for (int i = ControlPoints.Count - 2; i <= ControlPoints.Count - 1; i++)
		{
			if (i >= 0)
			{
				IncompleteControlPoints.Add(ControlPoints[i]);
			}
		}
	}

	public void UpdateLatestControlPoint(Vector3 position)
	{
		if (Finished)
		{
			return;
		}
		ControlPoint latestControlPoint = LatestControlPoint;
		latestControlPoint.Position = position;
		ControlPoint previousControlPoint = PreviousControlPoint;
		if (previousControlPoint == null)
		{
			latestControlPoint.Tangent = Rotation * Vector3.forward;
			latestControlPoint.Normal = Rotation * Vector3.up;
			return;
		}
		Vector3 normalized = (latestControlPoint.Position - previousControlPoint.Position).normalized;
		if (normalized.magnitude >= Mathf.Epsilon)
		{
			previousControlPoint.Tangent = normalized;
		}
		ControlPoint previousPreviousControlPoint = PreviousPreviousControlPoint;
		if (previousPreviousControlPoint != null)
		{
			Vector3 axis = Vector3.Cross(previousPreviousControlPoint.Tangent, previousControlPoint.Tangent);
			if (axis.magnitude <= Mathf.Epsilon)
			{
				previousControlPoint.Normal = previousPreviousControlPoint.Normal;
			}
			else
			{
				axis.Normalize();
				float angle = Vector3.Angle(previousPreviousControlPoint.Tangent, previousControlPoint.Tangent);
				previousControlPoint.Normal = (Quaternion.AngleAxis(angle, axis) * previousPreviousControlPoint.Normal).normalized;
			}
		}
		else
		{
			float num = Mathf.Abs(Vector3.Dot(previousControlPoint.Tangent, previousControlPoint.Normal));
			if (num >= Mathf.Epsilon)
			{
				if (num >= 1f - Mathf.Epsilon)
				{
					previousControlPoint.Normal = Rotation * Vector3.right;
				}
				previousControlPoint.Normal = Vector3.Cross(previousControlPoint.Binormal, previousControlPoint.Tangent).normalized;
			}
		}
		latestControlPoint.Tangent = previousControlPoint.Tangent;
		latestControlPoint.Normal = previousControlPoint.Normal;
	}

	public void Finish()
	{
		Finished = true;
		IncompleteControlPoints.Clear();
	}
}
