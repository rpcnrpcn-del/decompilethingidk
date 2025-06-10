using System.Collections;
using UnityEngine;

public class ChildControlManager : SingletonMonoBehaviour<ChildControlManager>
{
	[Header("On boot height check")]
	[SerializeField]
	private float minHeightMeters = 1.22f;

	[Header("Second screen warning")]
	[SerializeField]
	private float timeBeforeFadeIn = 1f;

	[SerializeField]
	private float fadeInDuration = 0.5f;

	[SerializeField]
	private float visibleDuration = 0.5f;

	[SerializeField]
	private float fadeOutDuration = 0.5f;

	[SerializeField]
	private Canvas warningCanvas;

	[SerializeField]
	private CanvasRenderer warningElement;

	private void Awake()
	{
		SingletonMonoBehaviour<ChildControlManager>.Instance = this;
		if (warningCanvas != null && warningElement != null)
		{
			StartCoroutine(ShowSecondScreenWarning());
		}
	}

	public void Initialize()
	{
		StartCoroutine(RunHandleInitialLogin());
	}

	private IEnumerator ShowSecondScreenWarning()
	{
		warningElement.SetAlpha(0f);
		yield return new WaitForSeconds(timeBeforeFadeIn);
		warningCanvas.gameObject.SetActive(true);
		float t = 0f;
		while (t < fadeInDuration)
		{
			warningElement.SetAlpha(t / fadeInDuration);
			t += Time.deltaTime;
			yield return null;
		}
		yield return new WaitForSeconds(visibleDuration);
		t = 0f;
		while (t < fadeOutDuration)
		{
			warningElement.SetAlpha(1f - t / fadeInDuration);
			t += Time.deltaTime;
			yield return null;
		}
		warningCanvas.gameObject.SetActive(false);
	}

	private IEnumerator RunHandleInitialLogin()
	{
		while (Player.LocalPlayer == null || Player.LocalPlayer.IsSpawning)
		{
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		float playerHeight = Player.LocalPlayer.CurrentFloorHeightFromHead;
		if (!(playerHeight <= minHeightMeters))
		{
		}
	}
}
