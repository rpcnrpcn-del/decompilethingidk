using System;
using RecNet;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MessageSummaryView : Selectable
{
	[SerializeField]
	private Text description;

	[SerializeField]
	private Text senderName;

	[SerializeField]
	private RawImage senderProfileImage;

	public string Description
	{
		get
		{
			return description.text;
		}
		set
		{
			description.text = value;
		}
	}

	public string SenderName
	{
		get
		{
			return senderName.text;
		}
		set
		{
			senderName.text = value;
		}
	}

	public Texture SenderProfileImage
	{
		get
		{
			return senderProfileImage.texture;
		}
		set
		{
			senderProfileImage.texture = value;
		}
	}

	public Message Message { get; set; }

	public event Action<long> OnSelected;

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		Button_ShowDetails();
	}

	public void Button_ShowDetails()
	{
		if (this.OnSelected != null)
		{
			this.OnSelected(Message.Id);
		}
	}
}
