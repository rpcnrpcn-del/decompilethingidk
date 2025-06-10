using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TimeTravelGifRecorder : MonoBehaviour
{
	public KeyCode key = KeyCode.P;

	public string dir = "gifs";

	public new string name = "gif";

	public int width = 640;

	public int height = 480;

	public int frames = 30;

	public int frameRate = 15;

	[Range(2f, 256f)]
	public int paletteSize = 256;

	public CaptureTheGIF.PaletteMode paletteMode;

	public int maxGifSize;

	private List<RenderTexture> snapshots = new List<RenderTexture>();

	private float lastSnapshot;

	private void Start()
	{
		lastSnapshot = Time.time;
	}

	private void Update()
	{
		if (Time.time >= lastSnapshot + (float)(1 / frameRate))
		{
			lastSnapshot = Time.time;
			Snapshot();
		}
		if (Input.GetKeyDown(key))
		{
			FileStream output = CaptureTheGIF.MakeFile(dir, name);
			CaptureTheGIF.Instance.MakeGif(snapshots.ToList(), 1 / frameRate, output, true, paletteSize, paletteMode, maxGifSize);
			snapshots.Clear();
		}
	}

	private void Snapshot()
	{
		snapshots.Add(CaptureTheGIF.Snapshot(width, height));
		if (snapshots.Count > frames)
		{
			RenderTexture.ReleaseTemporary(snapshots[0]);
			snapshots.RemoveAt(0);
		}
	}
}
