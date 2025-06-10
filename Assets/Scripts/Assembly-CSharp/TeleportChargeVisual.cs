using UnityEngine;
using UnityEngine.UI;

public class TeleportChargeVisual : MonoBehaviour
{
	[Header("Curves")]
	[SerializeField]
	private ColorValueCurve colorCurve;

	[SerializeField]
	private FloatValueCurve scaleCurve;

	[Header("Component References")]
	[SerializeField]
	private Slider sliderToSet;

	[SerializeField]
	private Image imageToColor;

	[SerializeField]
	private Transform transformToScale;

	public float Charge
	{
		set
		{
			value = Mathf.Clamp01(value);
			if (sliderToSet != null)
			{
				sliderToSet.value = Mathf.Clamp01(value);
			}
			if (imageToColor != null)
			{
				imageToColor.color = colorCurve.Evaluate(value);
			}
			if (transformToScale != null)
			{
				transformToScale.localScale = Vector3.one * scaleCurve.Evaluate(value);
			}
		}
	}
}
