using RecNet;
using UnityEngine;
using UnityEngine.UI;

public class GameJoinFailedMessageDetailMenuController : MessageDetailMenuController
{
	[SerializeField]
	private Text messageText;

	private string MessageText
	{
		get
		{
			return messageText.text;
		}
		set
		{
			messageText.text = value;
		}
	}

	public override void SetMessage(Message message)
	{
		base.SetMessage(message);
	}
}
