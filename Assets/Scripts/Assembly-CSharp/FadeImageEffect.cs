using System.Collections;
using UnityEngine;

public class FadeImageEffect : MonoBehaviour
{
	private PostEffectsMaterial postEffects;

	private float _fade;

	private bool _blind;

	private bool _fading;

	public float Fade
	{
		get
		{
			return _fade;
		}
		set
		{
			_fade = value;
			UpdateMaterial();
		}
	}

	public bool Blind
	{
		get
		{
			return _blind;
		}
		set
		{
			_blind = value;
			UpdateMaterial();
		}
	}

	private bool Fading
	{
		get
		{
			return _fading;
		}
		set
		{
			_fading = value;
			UpdateMaterial();
		}
	}

	private void Awake()
	{
		postEffects = GetComponentInChildren<PostEffectsMaterial>();
	}

	private void UpdateMaterial()
	{
		float fade = ((!Blind) ? Fade : 0f);
		postEffects.Fade = fade;
	}

	public Coroutine FadeIn(float duration)
	{
		Fading = true;
		StopAllCoroutines();
		return StartCoroutine(FadeCoroutine(true, duration));
	}

	public Coroutine FadeOut(float duration)
	{
		Fading = true;
		StopAllCoroutines();
		return StartCoroutine(FadeCoroutine(false, duration));
	}

	private IEnumerator FadeCoroutine(bool fadeIn, float duration)
	{
		float initialFade = ((!fadeIn) ? 1f : 0f);
		float finalFade = ((!fadeIn) ? 0f : 1f);
		float timer = 0f;
		while (timer < duration)
		{
			float t = timer / duration;
			Fade = Mathf.Lerp(initialFade, finalFade, t);
			timer += Time.deltaTime;
			yield return null;
		}
		Fade = finalFade;
		Fading = false;
	}
}
