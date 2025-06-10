using System.Collections;
using UnityEngine;

public class VignetteImageEffect : MonoBehaviour
{
	[SerializeField]
	private float defaultIntensity = 0.82f;

	[SerializeField]
	private float softness = 0.1f;

	private PostEffectsMaterial postEffects;

	private Coroutine fadeCoroutine;

	public bool Faded { get; private set; }

	private void Awake()
	{
		postEffects = GetComponentInChildren<PostEffectsMaterial>();
		if (postEffects == null)
		{
			Debug.LogError("PostEffectsMaterial missing from VignetteImageEffects.");
		}
	}

	private void Start()
	{
		postEffects.VignetteSoftness = softness;
	}

	public void FadeIn(float duration)
	{
		FadeIn(duration, defaultIntensity);
	}

	public void FadeIn(float duration, float maxIntensity)
	{
		StartFadeCoroutine(duration, maxIntensity);
		Faded = true;
	}

	public void FadeOut(float duration)
	{
		StartFadeCoroutine(duration, 0f);
		Faded = false;
	}

	public void SetVignette(float intensity)
	{
		postEffects.VignetteIntensity = Mathf.Clamp01(intensity) * defaultIntensity;
	}

	private void StartFadeCoroutine(float duration, float target)
	{
		StopFadeCoroutine();
		fadeCoroutine = StartCoroutine(FadeCoroutine(duration, target));
	}

	private void StopFadeCoroutine()
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
	}

	private IEnumerator FadeCoroutine(float duration, float targetIntensity)
	{
		float initialIntensity = postEffects.VignetteIntensity;
		for (float timer = 0f; timer <= duration; timer += Time.deltaTime)
		{
			float t = Mathf.Clamp01(timer / duration);
			postEffects.VignetteIntensity = Mathf.Lerp(initialIntensity, targetIntensity, t);
			yield return null;
		}
		postEffects.VignetteIntensity = targetIntensity;
		fadeCoroutine = null;
	}
}
