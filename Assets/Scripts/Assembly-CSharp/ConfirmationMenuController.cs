using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationMenuController : MenuController
{
	[Header("Confirmation")]
	[SerializeField]
	private Text titleText;

	[SerializeField]
	private Text bodyText;

	[SerializeField]
	private Text yesText;

	[SerializeField]
	private Text noText;

	public bool? Confirm { get; private set; }

	public int? ConfirmationValue { get; protected set; }

	public object[] Data { get; set; }

	public override bool Visible
	{
		set
		{
			base.Visible = value;
			if (!value && base.Owner != null)
			{
				base.Owner.PlayerUI.Menu.MarkDirty();
				if (this.ConfirmAction != null)
				{
					this.ConfirmAction(this);
				}
			}
		}
	}

	public event Action<ConfirmationMenuController> ConfirmAction;

	public void ShowConfirmation()
	{
		Visible = true;
		base.Owner.PlayerUI.Menu.OverrideDefaultMenu = true;
		(base.transform as RectTransform).SetAsLastSibling();
		Confirm = null;
		ConfirmationValue = null;
		Data = null;
	}

	public void ShowConfirmation(string title, string body, string yesButton, string noButton)
	{
		if (this.ConfirmAction != null)
		{
			Confirm = null;
			ConfirmationValue = null;
			this.ConfirmAction(this);
		}
		if (titleText != null)
		{
			titleText.text = title;
		}
		if (bodyText != null)
		{
			bodyText.text = body;
		}
		if (yesText != null)
		{
			yesText.text = yesButton;
		}
		if (noText != null)
		{
			noText.text = noButton;
		}
		ShowConfirmation();
	}

	public virtual void ButtonPress_Confirm(bool confirm)
	{
		Confirm = confirm;
		Visible = false;
	}

	public virtual void ButtonPress_Value(int value)
	{
		ConfirmationValue = value;
		Visible = false;
	}

	public Coroutine RunConfirmation()
	{
		return StartCoroutine(RunConfirmationCoroutine());
	}

	public Coroutine RunConfirmation(string title, string body, string yesButton, string noButton)
	{
		return StartCoroutine(RunConfirmationCoroutine(title, body, yesButton, noButton));
	}

	private IEnumerator RunConfirmationCoroutine()
	{
		ShowConfirmation();
		while (Visible)
		{
			yield return null;
		}
	}

	private IEnumerator RunConfirmationCoroutine(string title, string body, string yesButton, string noButton)
	{
		ShowConfirmation(title, body, yesButton, noButton);
		while (Visible)
		{
			yield return null;
		}
	}
}
