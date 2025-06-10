using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ShowSliderValue : MonoBehaviour
{
	public void UpdateLabel(float value)
	{
		Text component = GetComponent<Text>();
		if (component != null)
		{
			component.text = Mathf.RoundToInt(value * 100f) + "%";
		}
	}
}
