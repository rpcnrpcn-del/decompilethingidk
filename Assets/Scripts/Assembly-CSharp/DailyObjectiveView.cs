using UnityEngine;
using UnityEngine.UI;

public class DailyObjectiveView : MonoBehaviour
{
	[SerializeField]
	private Text description;

	[SerializeField]
	private Text progressLabel;

	[SerializeField]
	private Slider progressSlider;

	[SerializeField]
	private Image checkImage;

	[SerializeField]
	private Tooltip tooltip;

	public void UpdateProgress(int progress, int total, bool completed, string helpText, string tooltipText)
	{
		if (completed)
		{
			checkImage.gameObject.SetActive(true);
			progressSlider.value = 1f;
			progressLabel.gameObject.SetActive(false);
		}
		else
		{
			checkImage.gameObject.SetActive(false);
			progressSlider.value = (((float)total != 0f) ? Mathf.Clamp01((float)progress / (float)total) : 0f);
			progressLabel.gameObject.SetActive(true);
			progressLabel.text = "(" + progress + "/" + total + ")";
		}
		description.text = helpText;
		tooltip.Message = tooltipText;
	}
}
