using UnityEngine;

public class ExampleGUI : MonoBehaviour
{
	private static readonly string[] Captions = new string[2] { "Default Terrain Shader", "Advanced Terrain Shader" };

	private int selected = 1;

	public Material[] materials;

	public Terrain terrain;

	public void OnGUI()
	{
		GUILayout.BeginArea(new Rect(5f, 5f, 200f, 200f));
		GUILayout.BeginVertical("box");
		int num = GUILayout.SelectionGrid(selected, Captions, 1);
		if (num != selected)
		{
			terrain.materialTemplate = materials[num];
			selected = num;
		}
		GUILayout.EndVertical();
		if (selected == 1)
		{
			GUILayout.BeginVertical("box");
			GUILayout.Label("Blend Depth");
			materials[1].SetFloat("_Depth", GUILayout.HorizontalSlider(materials[1].GetFloat("_Depth"), 0.001f, 1f));
			GUILayout.EndVertical();
		}
		GUILayout.EndArea();
	}
}
