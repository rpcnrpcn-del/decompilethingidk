using UnityEngine;

public class CaptureGifOnStart : MonoBehaviour
{
	public string dir = "gifs";

	public new string name = "gif";

	public int frames = 30;

	public float frameRate = 10f;

	public int height = 640;

	public float aspect = 1.3333334f;

	[Range(2f, 256f)]
	public int paletteSize = 256;

	public CaptureTheGIF.PaletteMode paletteMode;

	public int maxGifSize;

	private void Start()
	{
		CaptureTheGIF.Instance.Capture(frames, (int)(aspect * (float)height), height, frameRate, dir, name, false, paletteSize, paletteMode, maxGifSize);
	}
}
