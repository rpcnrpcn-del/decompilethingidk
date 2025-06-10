using UnityEngine;
using UnityEngine.UI;

public class Label : MonoBehaviour
{
	[SerializeField]
	private Text text;

	[SerializeField]
	private Image line;

	private Renderer _buttonRenderer;

	public Color ButtonColor = Color.white;

	private bool _visible = true;

	public Renderer ButtonRenderer
	{
		get
		{
			return _buttonRenderer;
		}
		set
		{
			_buttonRenderer = value;
			if (_buttonRenderer != null)
			{
				_buttonRenderer.material.EnableKeyword("_EMISSION");
			}
		}
	}

	public string Text
	{
		set
		{
			text.text = value;
		}
	}

	public bool Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			_visible = value;
			text.gameObject.SetActive(value);
			line.gameObject.SetActive(value);
			SetButtonHighlight();
		}
	}

	private void OnEnable()
	{
		SetButtonHighlight();
	}

	private void SetButtonHighlight()
	{
		if (ButtonRenderer != null)
		{
			_buttonRenderer.material.SetColor("_EmissionColor", (!Visible) ? Color.black : ButtonColor);
		}
	}
}
