using System;
using System.Collections.Generic;
using UnityEngine;

public class ExtrudedMesh
{
	private const int SEGMENT_VERTEX_COUNT = 3;

	private const int SEGMENT_INDEX_COUNT = 18;

	private const float SEGMENT_ANGLE = (float)Math.PI * 2f / 3f;

	private const float MIN_CORNER_ANGLE = 15f;

	private const float MIN_CORNER_DISTANCE = 0.1f;

	private const float MIN_CORNER_DISTANCE_SQR = 0.010000001f;

	private const int CAP_INDEX_COUNT = 18;

	private List<Vector3> vertices;

	private List<Vector3> normals;

	private List<int> indices;

	private int temporaryVertexCount;

	private int temporaryIndexCount;

	public Transform Parent { get; private set; }

	public Mesh Mesh { get; private set; }

	public ExtrudedMesh(Transform parent)
	{
		Parent = parent;
		Mesh = new Mesh();
		Mesh.MarkDynamic();
		vertices = new List<Vector3>();
		normals = new List<Vector3>();
		indices = new List<int>();
	}

	public void Finish()
	{
		vertices = null;
		normals = null;
		indices = null;
	}

	public void AddPermanentControlPointVertices(ControlPoint controlPoint, ControlPoint lastPermanentControlPoint, float radius, bool isEnd = false)
	{
		ClearTemporaryControlPointVertices();
		AddControlPoint(controlPoint, lastPermanentControlPoint, radius, isEnd);
	}

	public void AddTemporaryControlPointVertices(List<ControlPoint> controlPoints, ControlPoint lastPermanentControlPoint, float radius)
	{
		ClearTemporaryControlPointVertices();
		int count = vertices.Count;
		int count2 = indices.Count;
		for (int i = 0; i < controlPoints.Count; i++)
		{
			ControlPoint controlPoint = controlPoints[i];
			ControlPoint controlPoint2 = null;
			controlPoint2 = ((i != 0) ? controlPoints[i - 1] : lastPermanentControlPoint);
			bool isEnd = i == controlPoints.Count - 1;
			AddControlPoint(controlPoint, controlPoint2, radius, isEnd);
		}
		temporaryVertexCount = vertices.Count - count;
		temporaryIndexCount = indices.Count - count2;
	}

	public void CopyChangesToGPU()
	{
		Mesh.Clear();
		Mesh.SetVertices(vertices);
		Mesh.SetNormals(normals);
		Mesh.SetTriangles(indices, 0);
	}

	private void AddControlPoint(ControlPoint controlPoint, ControlPoint previous, float radius, bool isEnd)
	{
		bool flag = false;
		if (previous != null)
		{
			float sqrMagnitude = (controlPoint.Position - previous.Position).sqrMagnitude;
			float num = Vector3.Angle(previous.Tangent, controlPoint.Tangent);
			flag = sqrMagnitude >= 0.010000001f && num >= 15f;
		}
		if (flag)
		{
			ControlPoint controlPoint2 = new ControlPoint(previous);
			controlPoint2.Position = controlPoint.Position - previous.Tangent * 2f * radius;
			ControlPoint controlPoint3 = ControlPoint.Interpolate(previous, controlPoint, 0.5f);
			controlPoint3.Position = controlPoint.Position;
			ControlPoint controlPoint4 = new ControlPoint(controlPoint);
			controlPoint4.Position = controlPoint.Position + controlPoint.Tangent * 2f * radius;
			ControlPoint controlPoint5 = ControlPoint.Interpolate(controlPoint2, controlPoint3, 0.5f);
			ControlPoint controlPoint6 = ControlPoint.Interpolate(controlPoint3, controlPoint4, 0.5f);
			AddNewRingVertices(controlPoint2, radius, false);
			AddNewRingVertices(controlPoint5, radius, false);
			AddNewRingVertices(controlPoint3, radius, false);
			AddNewRingVertices(controlPoint6, radius, false);
			AddNewRingVertices(controlPoint4, radius, false);
		}
		else
		{
			AddNewRingVertices(controlPoint, radius, isEnd);
		}
	}

	private void AddNewRingVertices(ControlPoint controlPoint, float radius, bool isEnd)
	{
		Vector3 position = Parent.InverseTransformPoint(controlPoint.Position);
		Vector3 right = Parent.InverseTransformVector(controlPoint.Binormal);
		Vector3 up = Parent.InverseTransformVector(controlPoint.Normal);
		Vector3 vector = Parent.InverseTransformVector(controlPoint.Tangent);
		if (vertices.Count == 0)
		{
			AllocateVertices(4);
			SetVertex(0, position, -vector);
			SetRingVertices(1, position, right, up, -vector, radius);
			AllocateIndices(18);
			SetStartCapIndices(0, 1, 0);
			return;
		}
		int num = AllocateVertices(3);
		SetRingVertices(num, position, right, up, vector, radius);
		int indexStartIndex = AllocateIndices(18);
		SetRingIndices(num, indexStartIndex);
		if (isEnd)
		{
			int num2 = AllocateVertices(1);
			SetVertex(num2, position, vector);
			int indexStartIndex2 = AllocateIndices(18);
			SetEndCapIndices(num2, num2 - 3, indexStartIndex2);
		}
	}

	private void ClearTemporaryControlPointVertices()
	{
		FreeVertices(temporaryVertexCount);
		FreeIndices(temporaryIndexCount);
		temporaryVertexCount = 0;
		temporaryIndexCount = 0;
	}

	private void SetVertex(int index, Vector3 position, Vector3 normal)
	{
		vertices[index] = position;
		normals[index] = normal;
	}

	private int AllocateVertices(int vertexCount)
	{
		int count = vertices.Count;
		for (int i = 0; i < vertexCount; i++)
		{
			vertices.Add(Vector3.zero);
		}
		for (int j = 0; j < vertexCount; j++)
		{
			normals.Add(Vector3.zero);
		}
		return count;
	}

	private void FreeVertices(int vertexCount)
	{
		int index = Mathf.Max(0, vertices.Count - vertexCount);
		vertices.RemoveRange(index, vertexCount);
		normals.RemoveRange(index, vertexCount);
	}

	private void SetRingVertices(int ringStartIndex, Vector3 position, Vector3 right, Vector3 up, Vector3 forward, float radius)
	{
		float num = 0f;
		for (int i = 0; i < 3; i++)
		{
			Vector3 vector = position + right * radius * Mathf.Cos(num) + up * radius * Mathf.Sin(num);
			Vector3 normalized = (vector - position).normalized;
			SetVertex(ringStartIndex++, vector, normalized);
			num += (float)Math.PI * 2f / 3f;
		}
	}

	private int AllocateIndices(int indexCount)
	{
		int count = indices.Count;
		for (int i = 0; i < indexCount; i++)
		{
			indices.Add(0);
		}
		return count;
	}

	private void FreeIndices(int indexCount)
	{
		int index = Mathf.Max(0, indices.Count - indexCount);
		indices.RemoveRange(index, indexCount);
	}

	private void SetStartCapIndices(int capIndex, int ringStartIndex, int indexStartIndex)
	{
		for (int i = 0; i < 3; i++)
		{
			int value = ringStartIndex + i;
			int value2 = ringStartIndex + (i + 1) % 3;
			indices[indexStartIndex++] = capIndex;
			indices[indexStartIndex++] = value;
			indices[indexStartIndex++] = value2;
		}
	}

	private void SetEndCapIndices(int capIndex, int ringStartIndex, int indexStartIndex)
	{
		for (int i = 0; i < 3; i++)
		{
			int value = ringStartIndex + i;
			int value2 = ringStartIndex + (i + 1) % 3;
			indices[indexStartIndex++] = value;
			indices[indexStartIndex++] = capIndex;
			indices[indexStartIndex++] = value2;
		}
	}

	private void SetRingIndices(int newRingStartIndex, int indexStartIndex)
	{
		int num = newRingStartIndex - 3;
		for (int i = 0; i < 3; i++)
		{
			int num2 = num + i;
			int num3 = num + (i + 1) % 3;
			int value = num2 + 3;
			int value2 = num3 + 3;
			indices[indexStartIndex++] = num2;
			indices[indexStartIndex++] = value2;
			indices[indexStartIndex++] = num3;
			indices[indexStartIndex++] = value2;
			indices[indexStartIndex++] = num2;
			indices[indexStartIndex++] = value;
		}
	}
}
