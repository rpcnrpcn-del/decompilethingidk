using UnityEngine;

public class ScaleAnimationControl : MonoBehaviour
{
	public AnimationCurve scaleCurve;

	private Vector3 initialScale = Vector3.one;

	private float lastKeyTime;

	private void OnEnable()
	{
		if (scaleCurve == null || scaleCurve.length < 1)
		{
			base.enabled = false;
			return;
		}
		initialScale = base.transform.localScale;
		lastKeyTime = scaleCurve[scaleCurve.length - 1].time;
		if (lastKeyTime <= 0f)
		{
			base.enabled = false;
		}
	}

	private void OnDisable()
	{
		base.transform.localScale = initialScale;
	}

	private void Update()
	{
		base.transform.localScale = initialScale * scaleCurve.Evaluate(Time.time % lastKeyTime);
	}
}
