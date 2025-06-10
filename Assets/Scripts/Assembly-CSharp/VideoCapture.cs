using System;
using System.IO;
using UnityEngine;

public abstract class VideoCapture : MonoBehaviour
{
	[SerializeField]
	protected int frameRate = 15;

	[SerializeField]
	protected float maxDuration = 30f;

	[HideInInspector]
	public Camera RecordingCamera;

	protected float capturePeriod;

	public bool IsCapturing { get; protected set; }

	public virtual string ExportDirectory
	{
		get
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Rec Room\\";
		}
	}

	public static string CurrentFileName
	{
		get
		{
			DateTime now = DateTime.Now;
			return string.Format("{0:D4}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
		}
	}

	public event Action CaptureStartedEvent;

	public event Action ExportStartedEvent;

	public event Action<string> ExportFinishedEvent;

	public abstract void StartCapture();

	public abstract void FinishCapture();

	public abstract float UpdateFrame(RenderTexture frame);

	protected virtual void Awake()
	{
		capturePeriod = 1f / (float)frameRate;
	}

	protected virtual void Start()
	{
		if (!Directory.Exists(ExportDirectory))
		{
			Directory.CreateDirectory(ExportDirectory);
		}
	}

	protected void FireCaptureStartedEvent()
	{
		if (this.CaptureStartedEvent != null)
		{
			this.CaptureStartedEvent();
		}
	}

	protected void FireExportStartedEvent()
	{
		if (this.ExportStartedEvent != null)
		{
			this.ExportStartedEvent();
		}
	}

	protected void FireExportFinishedEvent(string filename)
	{
		if (this.ExportFinishedEvent != null)
		{
			this.ExportFinishedEvent(filename);
		}
	}
}
