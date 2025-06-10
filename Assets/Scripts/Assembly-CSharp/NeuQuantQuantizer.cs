using Gif.Components;
using UnityEngine;

public class NeuQuantQuantizer : IQuantizer
{
	public bool UseColor32
	{
		get
		{
			return true;
		}
	}

	public Color[] Palette { get; private set; }

	public void Train(Color[] colors)
	{
		Color32[] array = new Color32[colors.Length];
		for (int i = 0; i < colors.Length; i++)
		{
			array[i] = colors[i];
		}
	}

	public void Train(Color32[] colors)
	{
		byte[] array = new byte[colors.Length * 3];
		for (int i = 0; i < colors.Length; i++)
		{
			array[i * 3] = colors[i].b;
			array[i * 3 + 1] = colors[i].g;
			array[i * 3 + 2] = colors[i].r;
		}
		NeuQuant neuQuant = new NeuQuant(array, array.Length, 10);
		byte[] array2 = neuQuant.Process();
		Color[] array3 = new Color[array2.Length / 3];
		for (int j = 0; j < array3.Length; j++)
		{
			float b = (float)(int)array2[j * 3] / 256f;
			float g = (float)(int)array2[j * 3 + 1] / 256f;
			float r = (float)(int)array2[j * 3 + 2] / 256f;
			array3[j] = new Color(r, g, b);
		}
		Palette = array3;
	}
}
