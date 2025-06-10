using UnityEngine;

namespace UTJ
{
	public static class FrameCapturerUtils
	{
		public static Mesh CreateFullscreenQuad()
		{
			Vector3[] vertices = new Vector3[4]
			{
				new Vector3(1f, 1f, 0f),
				new Vector3(-1f, 1f, 0f),
				new Vector3(-1f, -1f, 0f),
				new Vector3(1f, -1f, 0f)
			};
			int[] triangles = new int[6] { 0, 1, 2, 2, 3, 0 };
			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			return mesh;
		}
	}
}
