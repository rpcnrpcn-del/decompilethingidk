using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DebugUtils
{
	public class ValueOverTime
	{
		private class ValueOverTimeElement
		{
			public Color Color;

			public List<Vector2> ValueHistories = new List<Vector2>();

			public int Average
			{
				get
				{
					if (ValueHistories.Count > 0)
					{
						return Mathf.RoundToInt(ValueHistories.Sum((Vector2 p) => p.y) / (float)ValueHistories.Count);
					}
					return 0;
				}
			}

			public float Min
			{
				get
				{
					if (ValueHistories.Count > 0)
					{
						return ValueHistories.Min((Vector2 p) => p.y);
					}
					return float.MaxValue;
				}
			}

			public float Max
			{
				get
				{
					if (ValueHistories.Count > 0)
					{
						return ValueHistories.Max((Vector2 p) => p.y);
					}
					return float.MinValue;
				}
			}
		}

		private string name;

		private float graphTimeLength;

		private float? overrideMin;

		private float? overrideMax;

		private List<ValueOverTimeElement> elements = new List<ValueOverTimeElement>();

		private GUIStyle defaultJustify;

		private GUIStyle bottomJustify;

		public ValueOverTime(string name, Color color, float graphTimeLength, float? overrideMin = null, float? overrideMax = null)
			: this(name, 1, new Color[1] { color }, graphTimeLength, overrideMin, overrideMax)
		{
		}

		public ValueOverTime(string name, int elementCount, Color[] elementColors, float graphTimeLength, float? overrideMin = null, float? overrideMax = null)
		{
			this.name = name;
			this.graphTimeLength = graphTimeLength;
			this.overrideMin = overrideMin;
			this.overrideMax = overrideMax;
			for (int i = 0; i < elementCount && i < elementColors.Length; i++)
			{
				elements.Add(new ValueOverTimeElement
				{
					Color = elementColors[i]
				});
			}
		}

		public void AddDataPoint(float value)
		{
			AddDataPoint(new float[1] { value });
		}

		public void AddDataPoint(float[] values)
		{
			for (int i = 0; i < values.Length && i < elements.Count; i++)
			{
				while (elements[i].ValueHistories.Count > 0 && Time.realtimeSinceStartup - elements[i].ValueHistories[0].x > graphTimeLength)
				{
					elements[i].ValueHistories.RemoveAt(0);
				}
				elements[i].ValueHistories.Add(new Vector2(Time.realtimeSinceStartup, values[i]));
			}
		}

		public void DrawGraph(Rect screenRect)
		{
			if (bottomJustify == null)
			{
				bottomJustify = new GUIStyle(GUI.skin.label);
				bottomJustify.alignment = TextAnchor.LowerLeft;
			}
			if (defaultJustify == null)
			{
				defaultJustify = new GUIStyle(GUI.skin.label);
			}
			if (Event.current.type == EventType.Repaint && elements.Count > 0)
			{
				GUI.Box(screenRect, string.Format("{0} (Avg: {1})", name, elements[elements.Count - 1].Average), GUI.skin.box);
				screenRect.xMin += 1f;
				screenRect.xMax -= 1f;
				screenRect.yMin += 1f;
				screenRect.yMax -= 1f;
				Rect valueRect = new Rect
				{
					xMin = Time.realtimeSinceStartup - graphTimeLength,
					xMax = Time.realtimeSinceStartup
				};
				DrawLine(new Vector2(valueRect.xMin, 0f), new Vector2(valueRect.xMax, 0f), valueRect, screenRect, Color.black);
				for (int i = 0; i < elements.Count; i++)
				{
					valueRect.yMin = Mathf.Floor((!overrideMin.HasValue) ? Mathf.Min(-1f, elements[i].Min) : overrideMin.Value);
					valueRect.yMax = Mathf.Ceil((!overrideMax.HasValue) ? Mathf.Max(1f, elements[i].Max) : overrideMax.Value);
					defaultJustify.normal.textColor = elements[i].Color;
					bottomJustify.normal.textColor = elements[i].Color;
					GUI.Label(screenRect, ((int)valueRect.yMax/*cast due to .constrained prefix*/).ToString(), defaultJustify);
					GUI.Label(screenRect, ((int)valueRect.yMin/*cast due to .constrained prefix*/).ToString(), bottomJustify);
					DrawPolyLine(elements[i].ValueHistories, valueRect, screenRect, elements[i].Color);
				}
			}
		}

		private void DrawLine(Vector2 pointA, Vector2 pointB, Rect valueRect, Rect screenRect, Color color)
		{
			Drawing.DrawLine(ScaleToRect(pointA, valueRect, screenRect), ScaleToRect(pointB, valueRect, screenRect), color, 1f, false);
		}

		private void DrawPolyLine(IEnumerable<Vector2> points, Rect valueRect, Rect screenRect, Color color)
		{
			Drawing.DrawPolyLine(points.Select((Vector2 p) => ScaleToRect(p, valueRect, screenRect)), color, 1f, true);
		}

		private Vector2 ScaleToRect(Vector2 point, Rect valueRect, Rect screenRect)
		{
			point.x = Mathf.Lerp(screenRect.xMin, screenRect.xMax, (point.x - valueRect.xMin) / valueRect.width);
			point.y = Mathf.Lerp(screenRect.yMax, screenRect.yMin, (point.y - valueRect.yMin) / valueRect.height);
			return point;
		}
	}
}
