using UnityEngine;

public static class GUIExtensions
{
	private static GUIStyle tabPaneStyle;

	private static GUIStyle defaultSelectedTabStyle;

	private static GUIStyle defaultUnselectedTabStyle;

	public static int BeginTabs(int selected, params string[] labels)
	{
		if (tabPaneStyle == null)
		{
			tabPaneStyle = new GUIStyle(GUI.skin.box);
			tabPaneStyle.margin.top = 0;
		}
		if (defaultSelectedTabStyle == null)
		{
			defaultSelectedTabStyle = new GUIStyle(GUI.skin.box);
			defaultSelectedTabStyle.normal.textColor = GUI.skin.label.normal.textColor;
			defaultSelectedTabStyle.fixedWidth = 100f;
			defaultSelectedTabStyle.alignment = TextAnchor.MiddleLeft;
			defaultSelectedTabStyle.clipping = TextClipping.Clip;
			defaultSelectedTabStyle.margin.bottom = 0;
		}
		if (defaultUnselectedTabStyle == null)
		{
			defaultUnselectedTabStyle = new GUIStyle(GUI.skin.label);
			defaultUnselectedTabStyle.margin = defaultSelectedTabStyle.margin;
			defaultUnselectedTabStyle.padding = defaultSelectedTabStyle.padding;
			defaultUnselectedTabStyle.fixedWidth = defaultSelectedTabStyle.fixedWidth;
			defaultUnselectedTabStyle.alignment = defaultSelectedTabStyle.alignment;
			defaultUnselectedTabStyle.clipping = TextClipping.Clip;
			defaultUnselectedTabStyle.margin.bottom = 0;
		}
		return BeginTabs(selected, labels, tabPaneStyle, defaultSelectedTabStyle, defaultUnselectedTabStyle);
	}

	public static int BeginTabs(int selected, string[] labels, GUIStyle tabPaneStyle, GUIStyle selectedTabStyle, GUIStyle unselectedTabStyle)
	{
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		for (int i = 0; i < labels.Length; i++)
		{
			GUIStyle style = ((i != selected) ? unselectedTabStyle : selectedTabStyle);
			if (GUILayout.Button(labels[i], style))
			{
				selected = i;
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginVertical(tabPaneStyle);
		return selected;
	}

	public static void EndTabs()
	{
		GUILayout.EndVertical();
		GUILayout.EndVertical();
	}
}
