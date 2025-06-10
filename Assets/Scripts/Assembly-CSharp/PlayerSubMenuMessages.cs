using RecNet;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSubMenuMessages : PlayerSubMenu
{
	[Header("Notifications")]
	[SerializeField]
	private Text notificationCountText;

	[SerializeField]
	private Image notificationCountBackground;

	protected override void Awake()
	{
		base.Awake();
		Messages.OnMessageListUpdated += OnMessageListUpdated;
	}

	protected override void OnDestroy()
	{
		Messages.OnMessageListUpdated -= OnMessageListUpdated;
		base.OnDestroy();
	}

	private void OnMessageListUpdated(Messages.UpdateType updateType)
	{
		if (notificationCountText != null && notificationCountBackground != null)
		{
			int num = Mathf.Min(Messages.MessageList.Count, 99);
			if (num > 0)
			{
				notificationCountText.text = num.ToString();
				notificationCountBackground.enabled = true;
			}
			else
			{
				notificationCountText.text = string.Empty;
				notificationCountBackground.enabled = false;
			}
		}
	}
}
