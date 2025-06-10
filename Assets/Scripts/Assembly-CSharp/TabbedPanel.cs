using System;
using UnityEngine;
using UnityEngine.UI;

public class TabbedPanel : MonoBehaviour
{
	private bool _isSelected;

	public Button HeaderButton;

	public Text HeaderText;

	public Transform Body;

	public bool DefaultSelected;

	[SerializeField]
	private Color selectedButtonColor = Color.cyan;

	private Color defaultColor = Color.clear;

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			_isSelected = value;
			if (this.OnSelected != null)
			{
				this.OnSelected(this, value);
			}
			Body.gameObject.SetActive(value);
			HeaderButton.image.color = ((!value) ? defaultColor : selectedButtonColor);
		}
	}

	public event Action<TabbedPanel, bool> OnSelected;

	private void Awake()
	{
		defaultColor = HeaderButton.image.color;
	}

	private void Start()
	{
		IsSelected = DefaultSelected;
	}

	public void Button_SetSelected()
	{
		IsSelected = true;
	}
}
