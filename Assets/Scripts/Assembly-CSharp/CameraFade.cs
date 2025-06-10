using System.Collections;
using UnityEngine;
using Valve.VR;

public class CameraFade : MonoBehaviour
{
	[SerializeField]
	private float fadeOutDelay = 0.4f;

	[SerializeField]
	private float fadeInDelay = 0.4f;

	[SerializeField]
	private float fadeOutDuration = 1f;

	[SerializeField]
	private float fadeInDuration = 0.25f;

	[SerializeField]
	private AnimateInOut effect;

	[Header("Audio")]
	[SerializeField]
	private RecRoomAudioClip RezOutAudio;

	[SerializeField]
	private RecRoomAudioClip RezInAudio;

	private FadeImageEffect fadeEffect;

	private Coroutine fadeCoroutine;

	public static CameraFade Instance { get; private set; }

	public float TotalFadeInDuration
	{
		get
		{
			return fadeInDelay + fadeInDuration;
		}
	}

	public float TotalFadeOutDuration
	{
		get
		{
			return fadeOutDelay + fadeOutDuration;
		}
	}

	public bool IsFading
	{
		get
		{
			return fadeCoroutine != null;
		}
	}

	private void Awake()
	{
		Instance = this;
		fadeEffect = GetComponentInChildren<FadeImageEffect>();
	}

	public Coroutine Fade(bool fadeIn, bool prepForLevelSwitch = false, bool suppressEffect = false, float overrideDuration = 0f)
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine(FadeCoroutine(fadeIn, prepForLevelSwitch, suppressEffect, overrideDuration));
		return fadeCoroutine;
	}

	private IEnumerator FadeCoroutine(bool fadeIn, bool prepForLevelSwitch, bool suppressEffect, float overrideDuration)
	{
		if (!suppressEffect)
		{
			AnimateInOutManager.SuppressAnimations = false;
			effect.StopEffect();
			effect.PlayEffect(fadeIn);
			if (RezInAudio.audioClip != null && RezOutAudio.audioClip != null)
			{
				AudioManager.Play2DSFX((!fadeIn) ? RezOutAudio : RezInAudio);
			}
		}
		if (fadeIn)
		{
			if (OpenVR.Compositor != null)
			{
				OpenVR.Compositor.SuspendRendering(false);
			}
			yield return new WaitForSeconds(fadeInDelay);
			if (OpenVR.Compositor != null)
			{
				OpenVR.Compositor.FadeToColor(0.05f, 0f, 0f, 0f, 0f, false);
			}
			yield return fadeEffect.FadeIn((overrideDuration != 0f) ? overrideDuration : fadeInDuration);
		}
		else
		{
			if (!suppressEffect)
			{
				yield return new WaitForSeconds(fadeOutDelay);
			}
			yield return fadeEffect.FadeOut((overrideDuration != 0f) ? overrideDuration : fadeOutDuration);
			if (prepForLevelSwitch && OpenVR.Compositor != null)
			{
				OpenVR.Compositor.FadeToColor(0.05f, 0f, 0f, 0f, 1f, false);
				OpenVR.Compositor.SuspendRendering(true);
			}
		}
		fadeCoroutine = null;
	}
}
