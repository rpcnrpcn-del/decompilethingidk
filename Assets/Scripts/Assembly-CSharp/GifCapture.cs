using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class GifCapture : VideoCapture
{
	private float lastGifCaptureTime;

	private float gifCaptureStartTime;

	private Coroutine gifExportCoroutine;

	private List<RenderTexture> gifCaptureTextures = new List<RenderTexture>();

	public override string ExportDirectory
	{
		get
		{
			return base.ExportDirectory + "Gifs\\";
		}
	}

	public override void StartCapture()
	{
		if (gifExportCoroutine == null)
		{
			base.IsCapturing = true;
			gifCaptureStartTime = Time.unscaledTime;
			lastGifCaptureTime = 0f;
			gifCaptureTextures.Clear();
			FireCaptureStartedEvent();
		}
	}

	public override void FinishCapture()
	{
		if (base.IsCapturing)
		{
			base.IsCapturing = false;
			gifExportCoroutine = StartCoroutine(GifExportCoroutine());
		}
	}

	public override float UpdateFrame(RenderTexture frame)
	{
		float result = 0f;
		if (base.IsCapturing)
		{
			if (Time.unscaledTime - lastGifCaptureTime >= capturePeriod)
			{
				lastGifCaptureTime = Time.unscaledTime;
				RenderTexture temporary = RenderTexture.GetTemporary(frame.width, frame.height, 0);
				Graphics.Blit(frame, temporary);
				gifCaptureTextures.Add(temporary);
			}
			result = (Time.unscaledTime - gifCaptureStartTime) / maxDuration;
		}
		return result;
	}

	private IEnumerator GifExportCoroutine()
	{
		FireExportStartedEvent();
		MemoryStream memStream = new MemoryStream();
		yield return CaptureTheGIF.Instance.MakeGif(gifCaptureTextures, capturePeriod, memStream, false);
		string filename = VideoCapture.CurrentFileName;
		FileStream file = CaptureTheGIF.MakeFile(ExportDirectory, filename);
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
		}
		finally
		{
			if (file != null)
			{
				((IDisposable)file).Dispose();
			}
		}
		FireExportFinishedEvent(filename);
		gifExportCoroutine = null;
	}
}
