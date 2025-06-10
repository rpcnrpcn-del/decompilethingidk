using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Gif.Components;
using UnityEngine;

public class CaptureTheGIF : MonoBehaviour
{
	public enum PaletteMode
	{
		EveryFrame = 0,
		FirstFrame = 1
	}

	private static CaptureTheGIF instance;

	public static bool running;

	public static CaptureTheGIF Instance
	{
		get
		{
			if (!instance)
			{
				GameObject gameObject = new GameObject("CaptureTheGIF");
				gameObject.AddComponent<CaptureTheGIF>();
			}
			return instance;
		}
	}

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("there is already an instance of CaptureTheGIF");
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			instance = this;
		}
	}

	public static FileStream MakeFile(string dirName, string name)
	{
		string text = Path.Combine(Application.persistentDataPath, dirName);
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string text2 = Path.Combine(text, name + ".gif");
		int num = 0;
		while (File.Exists(text2))
		{
			text2 = Path.Combine(text, name + num + ".gif");
			num++;
		}
		Debug.Log("Creating File @: " + text2);
		return File.Create(text2);
	}

	public static RenderTexture Snapshot(int width, int height)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 24);
		foreach (Camera item in Camera.allCameras.OrderBy((Camera c) => c.depth))
		{
			RenderTexture targetTexture = item.targetTexture;
			item.targetTexture = temporary;
			item.Render();
			item.targetTexture = targetTexture;
			item.ResetAspect();
		}
		return temporary;
	}

	public Coroutine Capture(int frames, int width, int height, float frameRate, string dir, string name, bool lockFramerate = false, int paletteSize = 256, PaletteMode paletteMode = PaletteMode.EveryFrame, int maxGifSize = 0)
	{
		return StartCoroutine(CaptureRoutine(frames, width, height, frameRate, dir, name, lockFramerate, paletteSize, paletteMode, maxGifSize));
	}

	private IEnumerator CaptureRoutine(int frames, int width, int height, float frameRate, string dir, string name, bool lockFramerate, int paletteSize, PaletteMode paletteMode, int maxGifSize)
	{
		running = true;
		MemoryStream memStream = new MemoryStream();
		IEnumerator capRoutine = CaptureRoutine(frames, width, height, frameRate, memStream, false, lockFramerate, paletteSize, paletteMode, maxGifSize);
		while (capRoutine.MoveNext())
		{
			yield return capRoutine.Current;
		}
		FileStream file = MakeFile(dir, name);
		try
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				memStream.WriteTo(file);
				file.Flush();
				file.Close();
			});
			thread.Start();
			while (thread.ThreadState == ThreadState.Running)
			{
				yield return null;
			}
			running = false;
		}
		finally
		{
			if (file != null)
			{
				((IDisposable)file).Dispose();
			}
		}
	}

	public Coroutine Capture(int frames, int width, int height, float frameRate, Stream stream, bool closeWhenDone, bool lockFramerate = false, int paletteSize = 256, PaletteMode paletteMode = PaletteMode.EveryFrame, int maxGifSize = 0)
	{
		return StartCoroutine(CaptureRoutine(frames, width, height, frameRate, stream, closeWhenDone, lockFramerate, paletteSize, paletteMode, maxGifSize));
	}

	private IEnumerator CaptureRoutine(int frames, int width, int height, float frameRate, Stream stream, bool closeWhenDone, bool lockFramerate, int paletteSize, PaletteMode paletteMode, int maxGifSize)
	{
		List<RenderTexture> textures = new List<RenderTexture>();
		float t = Time.time;
		float waitUntil = t;
		if (lockFramerate)
		{
			Time.captureFramerate = (int)frameRate;
		}
		for (int f = 0; f < frames; f++)
		{
			if (!lockFramerate)
			{
				while (waitUntil > Time.time)
				{
					yield return null;
				}
			}
			yield return new WaitForEndOfFrame();
			waitUntil += 1f / frameRate;
			textures.Add(Snapshot(width, height));
		}
		Time.captureFramerate = 0;
		float period = 1f / frameRate;
		yield return MakeGif(textures, period, stream, closeWhenDone, paletteSize, paletteMode, maxGifSize);
	}

	public Coroutine MakeGif(List<RenderTexture> textures, float frameLength, Stream output, bool closeWhenDone, int paletteSize = 256, PaletteMode paletteMode = PaletteMode.EveryFrame, int maxGifSize = 0)
	{
		return StartCoroutine(MakeGifRoutine(textures, frameLength, output, closeWhenDone, paletteSize, paletteMode, maxGifSize));
	}

	public Coroutine MakeGif(List<Texture2D> textures, float frameLength, Stream output, bool closeWhenDone, int paletteSize = 256, PaletteMode paletteMode = PaletteMode.EveryFrame, int maxGifSize = 0)
	{
		return StartCoroutine(MakeGifRoutine(textures, frameLength, output, closeWhenDone, paletteSize, paletteMode, maxGifSize));
	}

	private static IEnumerator MakeGifRoutine(List<Texture2D> textures, float frameLength, Stream output, bool closeWhenDone, int paletteSize, PaletteMode paletteMode, int maxGifSize)
	{
		AnimatedGifEncoder gifEncoder = new AnimatedGifEncoder();
		gifEncoder.SetPaletteSize(paletteSize);
		gifEncoder.SetQuality(10);
		gifEncoder.SetStaticColorPaletteMode(paletteMode == PaletteMode.FirstFrame);
		gifEncoder.SetRepeat(0);
		gifEncoder.SetDelay((int)(frameLength * 1000f));
		gifEncoder.SetMaxSize(maxGifSize);
		gifEncoder.Start(output);
		ManualResetEvent imageStart = new ManualResetEvent(false);
		Image image = null;
		bool done = false;
		bool processed = false;
		Thread worker = new Thread((ThreadStart)delegate
		{
			while (!done)
			{
				imageStart.WaitOne();
				imageStart.Reset();
				try
				{
					if (!gifEncoder.AddFrame(image))
					{
						done = true;
					}
				}
				catch (Exception message)
				{
					Debug.Log(message);
					break;
				}
				Thread.MemoryBarrier();
				processed = true;
			}
		});
		worker.Start();
		foreach (Texture2D tex in textures)
		{
			if (done)
			{
				break;
			}
			image = new Image(tex);
			processed = false;
			imageStart.Set();
			while (!processed && !done)
			{
				yield return null;
			}
		}
		done = true;
		textures.Clear();
		gifEncoder.Finish();
		output.Flush();
		if (closeWhenDone)
		{
			output.Close();
		}
	}

	private static IEnumerator MakeGifRoutine(List<RenderTexture> textures, float frameLength, Stream output, bool closeWhenDone, int paletteSize, PaletteMode paletteMode, int maxGifSize)
	{
		AnimatedGifEncoder gifEncoder = new AnimatedGifEncoder();
		gifEncoder.SetPaletteSize(paletteSize);
		gifEncoder.SetQuality(10);
		gifEncoder.SetStaticColorPaletteMode(paletteMode == PaletteMode.FirstFrame);
		gifEncoder.SetRepeat(0);
		gifEncoder.SetDelay((int)(frameLength * 1000f));
		gifEncoder.SetMaxSize(maxGifSize);
		gifEncoder.Start(output);
		int w = textures[0].width;
		int h = textures[0].height;
		Texture2D tex = new Texture2D(w, h, TextureFormat.ARGB32, false, true);
		ManualResetEvent imageStart = new ManualResetEvent(false);
		Image image = null;
		bool done = false;
		bool processed = false;
		Thread worker = new Thread((ThreadStart)delegate
		{
			while (!done)
			{
				imageStart.WaitOne();
				imageStart.Reset();
				try
				{
					if (!gifEncoder.AddFrame(image))
					{
						done = true;
					}
				}
				catch (Exception message)
				{
					Debug.Log(message);
					break;
				}
				Thread.MemoryBarrier();
				processed = true;
			}
		});
		worker.Start();
		for (int picCount = 0; picCount < textures.Count; picCount++)
		{
			if (done)
			{
				break;
			}
			RenderTexture tempTex = textures[picCount];
			RenderTexture oldActive = RenderTexture.active;
			RenderTexture.active = tempTex;
			tex.ReadPixels(new Rect(0f, 0f, w, h), 0, 0);
			RenderTexture.active = oldActive;
			RenderTexture.ReleaseTemporary(tempTex);
			image = new Image(tex);
			processed = false;
			imageStart.Set();
			while (!processed && !done)
			{
				yield return null;
			}
		}
		done = true;
		textures.Clear();
		gifEncoder.Finish();
		UnityEngine.Object.DestroyImmediate(tex);
		output.Flush();
		if (closeWhenDone)
		{
			output.Close();
		}
	}
}
