using System.Collections;
using UnityEngine;
using UnityStandardAssets.CinematicEffects;

public class MonochromeImageEffect : MonoBehaviour
{
	private TonemappingColorGrading colorGrading;

	private float MonochromeAmount
	{
		set
		{
			colorGrading.MonochromeAmount = value;
		}
	}

	private void Awake()
	{
		colorGrading = GetComponent<TonemappingColorGrading>();
		if (colorGrading == null)
		{
			Debug.LogError("Monochrome Image Effect requires TonemappingColorGrading script.");
		}
	}

	public Coroutine FadeIn(float duration)
	{
		StopAllCoroutines();
		return StartCoroutine(FadeCoroutine(true, duration));
	}

	public Coroutine FadeOut(float duration)
	{
		StopAllCoroutines();
		return StartCoroutine(FadeCoroutine(false, duration));
	}

	private IEnumerator FadeCoroutine(bool fadeIn, float duration)
	{
		float initialAmount = ((!fadeIn) ? 1f : 0f);
		float finalAmount = ((!fadeIn) ? 0f : 1f);
		float timer = 0f;
		while (timer < duration)
		{
			float t = timer / duration;
			MonochromeAmount = Mathf.Lerp(initialAmount, finalAmount, t);
			timer += Time.deltaTime;
			yield return null;
		}
		MonochromeAmount = finalAmount;
	}
}
