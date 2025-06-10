using UnityEngine;
using UnityEngine.UI;

public class AlertMessageMenuController : MenuController
{
	[SerializeField]
	private Text titleText;

	[SerializeField]
	private Text bodyText;

	[SerializeField]
	private Button dismissButton;

	public bool Dismissable
	{
		get
		{
			return dismissButton.gameObject.activeSelf;
		}
		set
		{
			dismissButton.gameObject.SetActive(value);
		}
	}

	public void ShowAlertMessage(string title, string message, bool persistent = false)
	{
		if (titleText != null)
		{
			titleText.text = title;
		}
		if (bodyText != null)
		{
			bodyText.text = message;
		}
		Visible = true;
		base.Owner.PlayerUI.Menu.OverrideDefaultMenu = true;
		(base.transform as RectTransform).SetAsLastSibling();
		Dismissable = !persistent;
	}
}
