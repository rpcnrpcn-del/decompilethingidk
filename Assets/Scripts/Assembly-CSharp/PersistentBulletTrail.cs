using UnityEngine;

public class PersistentBulletTrail : BulletTrail
{
	[SerializeField]
	private AnimationCurve alphaFadeOutCurve;

	[SerializeField]
	private AnimationCurve widthFadeOutCurve;

	[SerializeField]
	private float finalWidthScale = 4f;

	private float startTime;

	protected override void OnEnable()
	{
		base.OnEnable();
		fadeAlpha = 1f;
		Color = Color;
		widthScale = 1f;
		Widths = Widths;
		startTime = Time.time;
	}

	protected override void Update()
	{
		base.Update();
		float time = Mathf.InverseLerp(startTime, startTime + lifetime, Time.time);
		fadeAlpha = Mathf.Lerp(1f, 0f, alphaFadeOutCurve.Evaluate(time));
		Color = Color;
		widthScale = Mathf.Lerp(1f, finalWidthScale, widthFadeOutCurve.Evaluate(time));
		Widths = Widths;
	}
}
