using System;
using System.IO;
using UnityEngine;

public class PngCapture : VideoCapture
{
	public override string ExportDirectory
	{
		get
		{
			return base.ExportDirectory + "Pngs\\";
		}
	}

	public override void StartCapture()
	{
		base.IsCapturing = true;
		FireCaptureStartedEvent();
	}

	public override void FinishCapture()
	{
		base.IsCapturing = false;
	}

	public override float UpdateFrame(RenderTexture frame)
	{
		if (!base.IsCapturing)
		{
			return 0f;
		}
		base.IsCapturing = false;
		FireExportStartedEvent();
		string fullFilename = GetFullFilename(VideoCapture.CurrentFileName);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = frame;
		Texture2D texture2D = new Texture2D(frame.width, frame.height, TextureFormat.ARGB32, false);
		try
		{
			texture2D.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
			byte[] bytes = texture2D.EncodeToPNG();
			File.WriteAllBytes(fullFilename, bytes);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		UnityEngine.Object.Destroy(texture2D);
		RenderTexture.active = active;
		FireExportFinishedEvent(fullFilename);
		return 1f;
	}

	private string GetFullFilename(string filename)
	{
		return Path.Combine(ExportDirectory, filename + ".png");
	}
}
