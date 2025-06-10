using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class WorldSpaceCameraBlinders : MonoBehaviour
{
	[SerializeField]
	private float gradientStartAngle = 90f;

	[SerializeField]
	private float gradientEndAngle = 120f;

	[SerializeField]
	private float gradientMaxAngle = 160f;

	[SerializeField]
	private float gradientWidth = 0.5f;

	[SerializeField]
	private float maximumGradientFade = 0.1f;

	[SerializeField]
	private float minimumVisibleGradientWidth = 0.15f;

	private Camera thisCamera;

	private PostEffectsMaterial postEffects;

	[NonSerialized]
	public bool BlindersEnabled = true;

	[NonSerialized]
	public Transform DesiredForwardTransform;

	private void Awake()
	{
		thisCamera = GetComponent<Camera>();
		postEffects = GetComponentInChildren<PostEffectsMaterial>();
	}

	private void LateUpdate()
	{
		if (BlindersEnabled)
		{
			Vector3 normalized = Vector3.ProjectOnPlane(DesiredForwardTransform.forward, Vector3.up).normalized;
			Vector3 normalized2 = Vector3.ProjectOnPlane(thisCamera.transform.forward, Vector3.up).normalized;
			float num = normalized.AngleSignedVector3(normalized2);
			float num2 = Mathf.Abs(num);
			float num3 = Mathf.InverseLerp(gradientStartAngle, gradientEndAngle, num2);
			if (num3 > 0f)
			{
				if (num2 >= gradientMaxAngle)
				{
					SetFullDarkGradient();
				}
				else if (num >= 0f)
				{
					SetClockwiseGradient(num3);
				}
				else
				{
					SetCounterClockwiseGradient(num3);
				}
				return;
			}
		}
		SetDefaultGradient();
	}

	private void SetDefaultGradient()
	{
		postEffects.SetGradient(0f, 1f, 1f, 1f);
	}

	private void SetFullDarkGradient()
	{
		postEffects.SetGradient(0f, 1f, maximumGradientFade, maximumGradientFade);
	}

	private void SetClockwiseGradient(float gradient)
	{
		float num = 1f;
		float a = num + gradientWidth;
		float num2 = minimumVisibleGradientWidth;
		float b = num2 - gradientWidth;
		postEffects.SetGradient(Mathf.Lerp(num, b, gradient), Mathf.Lerp(a, num2, gradient), 1f, maximumGradientFade);
	}

	private void SetCounterClockwiseGradient(float gradient)
	{
		float num = 0f;
		float a = num - gradientWidth;
		float num2 = 1f - minimumVisibleGradientWidth;
		float b = num2 + gradientWidth;
		postEffects.SetGradient(Mathf.Lerp(num, b, gradient), Mathf.Lerp(a, num2, gradient), 1f, maximumGradientFade);
	}
}
