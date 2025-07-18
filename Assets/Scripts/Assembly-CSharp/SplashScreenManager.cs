using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreenManager : SingletonMonoBehaviour<SplashScreenManager>
{
	[SerializeField]
	private Texture2D tonemappingLut;

	[SerializeField]
	private Canvas errorMessageCanvas;

	[SerializeField]
	private Text errorMessageTextBox;

	[SerializeField]
	private float minSplashDuration = 5f;

	private float startTime;

	private void Awake()
	{
		SingletonMonoBehaviour<SplashScreenManager>.Instance = this;
		startTime = Time.time;
		errorMessageCanvas.gameObject.SetActive(false);
		SingletonMonoBehaviour<CameraRig>.Instance.SetTonemappingLUT(tonemappingLut);
	}

	public void ShowErrorMessage(string message)
	{
		errorMessageTextBox.text = message;
		errorMessageCanvas.gameObject.SetActive(true);
	}

	public IEnumerator WaitForMinimumSplashDuration()
	{
		float elapsed = Time.time - startTime;
		float remaining = minSplashDuration - elapsed;
		if (remaining > 0f)
        {
            Debug.Log("[SplashScreenManager] " + remaining.ToString());
            yield return new WaitForSeconds(remaining);
		}
	}
}
