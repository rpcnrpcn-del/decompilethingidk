using System;
using ExitGames.Client.Photon;
using UnityEngine;

public class WhiteboardPenDrawingTool : DrawingToolBase<WhiteboardPenDrawingTool.ControlPoint>
{
	public class ControlPoint : IDrawingControlPoint
	{
		public Vector2 Position;

		public Vector2 Tangent;

		public float StrokeRadius = 0.01f;

		private static Vector3 Normal = Vector3.forward;

		private const int byteSize = 20;

		public static readonly byte[] memVector2 = new byte[20];

		Vector2 IDrawingControlPoint.Position
		{
			get
			{
				return Position;
			}
		}

		public Vector2 Binormal
		{
			get
			{
				return Vector3.Cross(Tangent, Normal).normalized;
			}
		}

		public ControlPoint()
		{
			Position = Vector2.zero;
			Tangent = Vector2.zero;
		}

		public ControlPoint(Vector2 position, Vector2 tangent, float strokeRadius)
		{
			Position = position;
			Tangent = tangent;
			StrokeRadius = strokeRadius;
		}

		public ControlPoint(ControlPoint other)
		{
			Position = other.Position;
			Tangent = other.Tangent;
			StrokeRadius = other.StrokeRadius;
		}

		public static ControlPoint Interpolate(ControlPoint lhs, ControlPoint rhs, float t)
		{
			Quaternion a = Quaternion.LookRotation(lhs.Tangent, Normal);
			Quaternion b = Quaternion.LookRotation(rhs.Tangent, Normal);
			Quaternion quaternion = Quaternion.Slerp(a, b, t);
			Vector3 vector = Vector3.Lerp(lhs.Position, rhs.Position, t);
			return new ControlPoint(vector, quaternion * Vector3.forward, lhs.StrokeRadius);
		}

		public override string ToString()
		{
			return Position.ToString();
		}

		public static short Serialize(StreamBuffer outStream, object customobject)
		{
			ControlPoint controlPoint = (ControlPoint)customobject;
			lock (memVector2)
			{
				byte[] array = memVector2;
				int targetOffset = 0;
				Protocol.Serialize(controlPoint.Position.x, array, ref targetOffset);
				Protocol.Serialize(controlPoint.Position.y, array, ref targetOffset);
				Protocol.Serialize(controlPoint.Tangent.x, array, ref targetOffset);
				Protocol.Serialize(controlPoint.Tangent.y, array, ref targetOffset);
				Protocol.Serialize(controlPoint.StrokeRadius, array, ref targetOffset);
				outStream.Write(array, 0, 20);
			}
			return 20;
		}

		public static object Deserialize(StreamBuffer inStream, short length)
		{
			ControlPoint controlPoint = new ControlPoint();
			lock (memVector2)
			{
				inStream.Read(memVector2, 0, 20);
				int offset = 0;
				Protocol.Deserialize(out controlPoint.Position.x, memVector2, ref offset);
				Protocol.Deserialize(out controlPoint.Position.y, memVector2, ref offset);
				Protocol.Deserialize(out controlPoint.Tangent.x, memVector2, ref offset);
				Protocol.Deserialize(out controlPoint.Tangent.y, memVector2, ref offset);
				Protocol.Deserialize(out controlPoint.StrokeRadius, memVector2, ref offset);
				return controlPoint;
			}
		}
	}

	[SerializeField]
	private AnimationCurve radiusCurve;

	[SerializeField]
	private float _strokeRadius = 0.005f;

	protected float StrokeRadius
	{
		get
		{
			if (base.CurrentSurface != null)
			{
				if (distanceToPlane < 0f)
				{
					return _strokeRadius;
				}
				return radiusCurve.Evaluate(distanceToPlane) * _strokeRadius;
			}
			return _strokeRadius;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (radiusCurve == null)
		{
			radiusCurve = AnimationCurve.Linear(0f, StrokeRadius, 0.1f, StrokeRadius);
		}
	}

	protected override Stroke CreateNewStroke(DrawingSurface drawingSurface, ControlPoint firstControlPoint)
	{
		lastControlPoint = firstControlPoint;
		return new Stroke(drawingSurface.PhotonId, firstControlPoint);
	}

	protected override ControlPoint CreateNewControlPoint()
	{
		ControlPoint controlPoint = new ControlPoint(base.NormalizedPlanePos, Vector2.zero, StrokeRadius);
		if (lastControlPoint != null)
		{
			controlPoint.Tangent = (controlPoint.Position - lastControlPoint.Position).normalized;
		}
		float num = Vector3.Angle(controlPoint.Tangent, base.CurrentSurface.transform.up);
		float t = 1f - (Mathf.Cos(2f * num * ((float)Math.PI / 180f)) + 1f) / 2f;
		float strokeRadius = StrokeRadius * Mathf.Lerp(1f, base.CurrentSurface.WHRation, t);
		controlPoint.StrokeRadius = strokeRadius;
		return controlPoint;
	}

	protected override bool UpdateMesh()
	{
		int num = currentStroke.controlPoints.Count;
		if (num == 1)
		{
			ControlPoint controlPoint = currentStroke.controlPoints[0];
			ControlPoint controlPoint2 = new ControlPoint(controlPoint);
			controlPoint2.Position.x -= controlPoint.StrokeRadius / 2f;
			ControlPoint controlPoint3 = new ControlPoint(controlPoint);
			controlPoint3.Position.x += controlPoint.StrokeRadius / 2f;
			controlPoint2.Tangent = (controlPoint3.Position - controlPoint2.Position).normalized;
			controlPoint3.Tangent = -controlPoint2.Tangent;
			AddVertsForControlPoint(controlPoint2);
			AddVertsForControlPoint(controlPoint3);
			num = 2;
		}
		else
		{
			for (int i = 0; i < currentStroke.controlPoints.Count; i++)
			{
				AddVertsForControlPoint(currentStroke.controlPoints[i]);
			}
		}
		int num2 = Mathf.Max(0, num - 1);
		for (int j = 0; j < num2; j++)
		{
			int num3 = j * 2;
			tris.Add(num3);
			tris.Add(num3 + 2);
			tris.Add(num3 + 1);
			tris.Add(num3 + 2);
			tris.Add(num3 + 3);
			tris.Add(num3 + 1);
		}
		drawingMesh.vertices = vertices.ToArray();
		drawingMesh.uv = uvs.ToArray();
		drawingMesh.triangles = tris.ToArray();
		return num2 > 0;
	}

	private void AddVertsForControlPoint(ControlPoint controlPoint, float t = 0f)
	{
		Vector2 vector = controlPoint.Position + controlPoint.Binormal * controlPoint.StrokeRadius;
		Vector2 vector2 = controlPoint.Position + controlPoint.Binormal * (0f - controlPoint.StrokeRadius);
		vertices.Add(vector);
		vertices.Add(vector2);
		Vector2 zero = Vector2.zero;
		zero.x = 1f - t;
		zero.y = 1f;
		uvs.Add(zero);
		zero.y = 0f;
		uvs.Add(zero);
	}
}
