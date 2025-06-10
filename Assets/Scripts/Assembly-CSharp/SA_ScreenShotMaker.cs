using System;
using System.Collections;
using UnityEngine;

public class SA_ScreenShotMaker : SA_Singleton<SA_ScreenShotMaker>
{
	public Action<Texture2D> OnScreenshotReady;

	public void GetScreenshot()
	{
		StartCoroutine(SaveScreenshot());
	}

	private IEnumerator SaveScreenshot()
	{
		yield return new WaitForEndOfFrame();
		int width = Screen.width;
		int height = Screen.height;
		Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
		tex.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
		tex.Apply();
		if (OnScreenshotReady != null)
		{
			OnScreenshotReady(tex);
		}
	}
}
