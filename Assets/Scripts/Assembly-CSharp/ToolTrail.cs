using System.Collections;
using UnityEngine;

public class ToolTrail : MonoBehaviour
{
	[Header("Held Trails")]
	[SerializeField]
	private bool visibleWhileHeld = true;

	[SerializeField]
	private float heldTime = 2f;

	[Header("Release Trails")]
	[SerializeField]
	private bool visibleUponRelease = true;

	[SerializeField]
	private float releaseTime = 2f;

	[SerializeField]
	private bool fadeOutUponRelease;

	[SerializeField]
	private float releaseFadeOutDuration = 1f;

	[Header("Force Applied Trails")]
	[SerializeField]
	private bool visibleAfterForceApplied = true;

	[SerializeField]
	private float forceAppliedTime = 2f;

	[SerializeField]
	private bool fadeOutAfterForceApplied = true;

	[SerializeField]
	private float forceAppliedFadeOutDuration = 1f;

	private Tool thisTool;

	private TrailRenderer trailRenderer;

	private Coroutine releaseFadeCoroutine;

	private void Awake()
	{
		thisTool = GetComponentInParent<Tool>();
		thisTool.ReleaseEvent += OnRelease;
		thisTool.PickupEvent += OnPickup;
		thisTool.ForceApplied += OnForceApplied;
		trailRenderer = GetComponent<TrailRenderer>();
	}

	private void OnDisable()
	{
		StopReleaseFadeCoroutine();
	}

	private void OnDestroy()
	{
		thisTool.ReleaseEvent -= OnRelease;
		thisTool.PickupEvent -= OnPickup;
		thisTool.ForceApplied -= OnForceApplied;
	}

	private void OnRelease(Tool tool)
	{
		if (!visibleUponRelease)
		{
			trailRenderer.time = 0f;
			return;
		}
		trailRenderer.time = releaseTime;
		if (fadeOutUponRelease)
		{
			StartReleaseFadeCoroutine(releaseFadeOutDuration, releaseTime);
		}
	}

	private void OnPickup(Tool tool)
	{
		StopReleaseFadeCoroutine();
		if (!visibleWhileHeld)
		{
			trailRenderer.time = 0f;
		}
		else
		{
			trailRenderer.time = heldTime;
		}
	}

	private void OnForceApplied(Player hitPlayer, Tool tool, Vector3 position, Vector3 force)
	{
		StopReleaseFadeCoroutine();
		if (!visibleAfterForceApplied)
		{
			trailRenderer.time = 0f;
			return;
		}
		trailRenderer.time = forceAppliedTime;
		if (fadeOutAfterForceApplied)
		{
			StartReleaseFadeCoroutine(forceAppliedFadeOutDuration, forceAppliedTime);
		}
	}

	private void StartReleaseFadeCoroutine(float duration, float maxTime)
	{
		StopReleaseFadeCoroutine();
		if (thisTool != null && thisTool.gameObject.activeSelf)
		{
			releaseFadeCoroutine = StartCoroutine(ReleaseFadeCoroutine(duration, maxTime));
		}
	}

	private void StopReleaseFadeCoroutine()
	{
		if (releaseFadeCoroutine != null)
		{
			StopCoroutine(releaseFadeCoroutine);
			releaseFadeCoroutine = null;
		}
	}

	private IEnumerator ReleaseFadeCoroutine(float duration, float maxTime)
	{
		float timeRemaining = duration;
		while (timeRemaining >= 0f)
		{
			yield return null;
			timeRemaining -= Time.deltaTime;
			float t = timeRemaining / duration;
			trailRenderer.time = maxTime * t;
		}
		trailRenderer.time = 0f;
		releaseFadeCoroutine = null;
	}
}
