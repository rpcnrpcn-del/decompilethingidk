using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

public class EraserDrawingTool : DrawingToolBase<EraserDrawingTool.ControlPoint>
{
	public class ControlPoint : IDrawingControlPoint
	{
		public Vector3 center = Vector3.zero;

		public Vector3 tr = Vector3.zero;

		public Vector3 tl = Vector3.zero;

		public Vector3 bl = Vector3.zero;

		public Vector3 br = Vector3.zero;

		private Vector3[] allPoints;

		private int[] indices = new int[4] { 0, 1, 2, 3 };

		private Vector3 surfaceNormal;

		private const int byteSize = 40;

		public static readonly byte[] memVector2 = new byte[40];

		public Vector2 Position
		{
			get
			{
				return center;
			}
		}

		public Vector2 Tangent
		{
			get
			{
				return Vector2.one;
			}
			set
			{
			}
		}

		public ControlPoint()
		{
			SetAllPoints();
		}

		public ControlPoint(Vector3 center, Vector3 tr, Vector3 tl, Vector3 bl, Vector3 br, Vector3 surfaceNormal)
		{
			this.center = center;
			this.tr = tr;
			this.tl = tl;
			this.bl = bl;
			this.br = br;
			this.surfaceNormal = surfaceNormal;
			SetAllPoints();
		}

		private void SetAllPoints()
		{
			allPoints = new Vector3[4] { tr, tl, bl, br };
		}

		public void AddVertsAndUVs(List<Vector3> verts, List<Vector2> uvs)
		{
			verts.Add(tr);
			verts.Add(tl);
			verts.Add(bl);
			verts.Add(br);
			Vector2 item = Vector3.one;
			uvs.Add(item);
			item.x = 0f;
			uvs.Add(item);
			item.y = 0f;
			uvs.Add(item);
			item.x = 1f;
			uvs.Add(item);
		}

		public void AddTris(List<int> tris, int startIndex)
		{
			tris.Add(startIndex);
			tris.Add(startIndex + 2);
			tris.Add(startIndex + 1);
			tris.Add(startIndex);
			tris.Add(startIndex + 3);
			tris.Add(startIndex + 2);
		}

		public void AddInitialQuad(List<Vector3> verts, List<Vector2> uvs, List<int> tris)
		{
			AddVertsAndUVs(verts, uvs);
			AddTris(tris, 0);
		}

		public List<int> EdgeVerts(ControlPoint other, bool trailing = false)
		{
			List<int> list = new List<int>(indices);
			float[] xDist = new float[4];
			float[] yDist = new float[4];
			Vector3 vector = other.center - center;
			Vector3 vector2 = Vector3.Lerp(center, other.center, 0.5f);
			Vector3 normalized = vector.normalized;
			Vector3 onNormal = Vector3.Cross(normalized, surfaceNormal);
			for (int i = 0; i < allPoints.Length; i++)
			{
				Vector3 vector3 = Vector3.Project(allPoints[i] - vector2, onNormal);
				Debug.DrawRay(allPoints[i], vector3, Color.cyan);
				float num = Mathf.Sign(Vector3.Cross(normalized, vector3).y);
				xDist[i] = vector3.magnitude * num;
				Vector3 vector4 = Vector3.Project(vector2 - allPoints[i], normalized);
				Debug.DrawRay(allPoints[i], vector4 * 0.5f, Color.magenta);
				yDist[i] = vector4.magnitude;
			}
			list.Sort((int a, int b) => yDist[a].CompareTo(yDist[b]));
			List<int> range = list.GetRange(0, 3);
			range.Sort((int a, int b) => trailing ? xDist[a].CompareTo(xDist[b]) : xDist[b].CompareTo(xDist[a]));
			int num2 = 1;
			foreach (int item in range)
			{
				Debug.DrawRay(allPoints[item], Vector3.up * 0.25f * num2++, Color.blue);
			}
			return range;
		}

		public static short Serialize(StreamBuffer outStream, object customobject)
		{
			ControlPoint controlPoint = (ControlPoint)customobject;
			lock (memVector2)
			{
				byte[] array = memVector2;
				int targetOffset = 0;
				Protocol.Serialize(controlPoint.center.x, array, ref targetOffset);
				Protocol.Serialize(controlPoint.center.y, array, ref targetOffset);
				Protocol.Serialize(controlPoint.tr.x, array, ref targetOffset);
				Protocol.Serialize(controlPoint.tr.y, array, ref targetOffset);
				Protocol.Serialize(controlPoint.tl.x, array, ref targetOffset);
				Protocol.Serialize(controlPoint.tl.y, array, ref targetOffset);
				Protocol.Serialize(controlPoint.bl.x, array, ref targetOffset);
				Protocol.Serialize(controlPoint.bl.y, array, ref targetOffset);
				Protocol.Serialize(controlPoint.br.x, array, ref targetOffset);
				Protocol.Serialize(controlPoint.br.y, array, ref targetOffset);
				outStream.Write(array, 0, 40);
			}
			return 40;
		}

		public static object Deserialize(StreamBuffer inStream, short length)
		{
			ControlPoint controlPoint = new ControlPoint();
			lock (memVector2)
			{
				inStream.Read(memVector2, 0, 40);
				int offset = 0;
				Protocol.Deserialize(out controlPoint.center.x, memVector2, ref offset);
				Protocol.Deserialize(out controlPoint.center.y, memVector2, ref offset);
				Protocol.Deserialize(out controlPoint.tr.x, memVector2, ref offset);
				Protocol.Deserialize(out controlPoint.tr.y, memVector2, ref offset);
				Protocol.Deserialize(out controlPoint.tl.x, memVector2, ref offset);
				Protocol.Deserialize(out controlPoint.tl.y, memVector2, ref offset);
				Protocol.Deserialize(out controlPoint.bl.x, memVector2, ref offset);
				Protocol.Deserialize(out controlPoint.bl.y, memVector2, ref offset);
				Protocol.Deserialize(out controlPoint.br.x, memVector2, ref offset);
				Protocol.Deserialize(out controlPoint.br.y, memVector2, ref offset);
			}
			controlPoint.SetAllPoints();
			return controlPoint;
		}
	}

	public Transform TR;

	public Transform TL;

	public Transform BL;

	public Transform BR;

	protected override Stroke CreateNewStroke(DrawingSurface drawingSurface, ControlPoint firstControlPoint)
	{
		lastControlPoint = firstControlPoint;
		return new Stroke(drawingSurface.PhotonId, firstControlPoint);
	}

	protected override ControlPoint CreateNewControlPoint()
	{
		Vector2 vector = base.CurrentSurface.WorldToNormalizedBoardPos(base.transform.position);
		Vector2 vector2 = base.CurrentSurface.WorldToNormalizedBoardPos(TL.transform.position);
		Vector2 vector3 = base.CurrentSurface.WorldToNormalizedBoardPos(TR.transform.position);
		Vector2 vector4 = base.CurrentSurface.WorldToNormalizedBoardPos(BR.transform.position);
		Vector2 vector5 = base.CurrentSurface.WorldToNormalizedBoardPos(BL.transform.position);
		return new ControlPoint(vector, vector3, vector2, vector5, vector4, base.CurrentSurface.Normal);
	}

	protected override bool UpdateMesh()
	{
		vertices.Clear();
		uvs.Clear();
		tris.Clear();
		for (int i = 0; i < currentStroke.controlPoints.Count; i++)
		{
			ControlPoint controlPoint = currentStroke.controlPoints[i];
			if (i == 0)
			{
				controlPoint.AddInitialQuad(vertices, uvs, tris);
				continue;
			}
			controlPoint.AddVertsAndUVs(vertices, uvs);
			ControlPoint controlPoint2 = currentStroke.controlPoints[i - 1];
			List<int> list = controlPoint2.EdgeVerts(controlPoint, true);
			List<int> list2 = controlPoint.EdgeVerts(controlPoint2);
			int num = (i - 1) * 4;
			int num2 = i * 4;
			tris.Add(num2 + list[1]);
			tris.Add(num + list2[0]);
			tris.Add(num2 + list[0]);
			tris.Add(num + list2[1]);
			tris.Add(num + list2[0]);
			tris.Add(num2 + list[1]);
			tris.Add(num2 + list[2]);
			tris.Add(num + list2[1]);
			tris.Add(num2 + list[1]);
			tris.Add(num + list2[2]);
			tris.Add(num + list2[1]);
			tris.Add(num2 + list[2]);
			controlPoint.AddTris(tris, num);
		}
		drawingMesh.vertices = vertices.ToArray();
		drawingMesh.uv = uvs.ToArray();
		drawingMesh.triangles = tris.ToArray();
		return true;
	}
}
