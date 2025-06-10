using UnityEngine;

public class QuantTest : MonoBehaviour
{
	[SerializeField]
	private Texture2D src;

	public Texture2D dstQuant;

	public Texture2D dstGray;

	private void Start()
	{
		Color32[] pixels = src.GetPixels32();
		NeuQuantQuantizer neuQuantQuantizer = new NeuQuantQuantizer();
		neuQuantQuantizer.Train(pixels);
		Color[] palette = neuQuantQuantizer.Palette;
		int[] array = new int[pixels.Length];
		Color32[] array2 = new Color32[pixels.Length];
		for (int i = 0; i < pixels.Length; i++)
		{
			Color color = pixels[i];
			float num = 100f;
			int num2 = 0;
			for (int j = 0; j < palette.Length; j++)
			{
				Color color2 = palette[j];
				float num3 = Mathf.Abs(color2.r - color.r) + Mathf.Abs(color2.g - color.g) + Mathf.Abs(color2.b - color.b);
				if (num3 < num)
				{
					num = num3;
					num2 = j;
				}
			}
			array[i] = num2;
			pixels[i] = palette[num2];
			array2[i] = new Color32((byte)num2, (byte)num2, (byte)num2, byte.MaxValue);
		}
		dstQuant = new Texture2D(src.width, src.height);
		dstQuant.SetPixels32(pixels);
		dstQuant.Apply();
		dstGray = new Texture2D(src.width, src.height);
		dstGray.SetPixels32(array2);
		dstGray.Apply();
	}
}
