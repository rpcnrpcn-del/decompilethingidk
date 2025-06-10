using System.Collections.Generic;
using UnityEngine;

public class GameViewDebug : MonoBehaviour
{
	private struct Line
	{
		public Vector3 Start;

		public Vector3 End;

		public Color Color;
	}

	private static GameViewDebug _instance;

	private static Material _lineMaterial;

	private List<Line> lines = new List<Line>();

	private static GameViewDebug instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Camera.main.gameObject.AddComponent<GameViewDebug>();
			}
			return _instance;
		}
	}

	private static Material lineMaterial
	{
		get
		{
			if (_lineMaterial == null)
			{
				_lineMaterial = new Material("Shader \"Lines/Colored Blended\" {SubShader { Pass {     Blend SrcAlpha OneMinusSrcAlpha     ZWrite Off Cull Off Fog { Mode Off }     BindChannels {      Bind \"vertex\", vertex Bind \"color\", color }} } }");
				_lineMaterial.hideFlags = HideFlags.HideAndDontSave;
				_lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
			}
			return _lineMaterial;
		}
	}

	public static void DrawLine(Vector3 start, Vector3 end)
	{
		DrawLine(start, end, Color.white);
	}

	public static void DrawLine(Vector3 start, Vector3 end, Color color)
	{
		instance.lines.Add(new Line
		{
			Start = start,
			End = end,
			Color = color
		});
	}

	private void OnPostRender()
	{
		GL.Begin(1);
		lineMaterial.SetPass(0);
		for (int i = 0; i < lines.Count; i++)
		{
			GL.Color(lines[i].Color);
			GL.Vertex3(lines[i].Start.x, lines[i].Start.y, lines[i].Start.z);
			GL.Vertex3(lines[i].End.x, lines[i].End.y, lines[i].End.z);
		}
		GL.End();
		lines.Clear();
	}
}
