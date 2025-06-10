using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ThrobbingUIElement : MonoBehaviour
{
	private RectTransform thisTransform;

	[SerializeField]
	private Image elementImage;

	[SerializeField]
	private Sprite inactiveSprite;

	[SerializeField]
	private Sprite activeSprite;

	private float startingScale;

	[SerializeField]
	private float maxThrobScale = 2f;

	[SerializeField]
	private float throbFrequency = 1f;

	private Color startingColor;

	[SerializeField]
	private Color throbColorOverlay;

	private bool active;

	public bool Active
	{
		get
		{
			return active;
		}
		set
		{
			active = value;
			if (active)
			{
				if (elementImage != null)
				{
					elementImage.sprite = activeSprite;
				}
			}
			else if (elementImage != null)
			{
				elementImage.sprite = inactiveSprite;
			}
			if (thisTransform != null)
			{
				thisTransform.localScale = new Vector3(startingScale, startingScale);
			}
			elementImage.color = startingColor;
		}
	}

	private void Awake()
	{
		thisTransform = GetComponent<RectTransform>();
		if (elementImage == null)
		{
			elementImage = GetComponentInChildren<Image>();
		}
		if (elementImage != null)
		{
			startingColor = elementImage.color;
		}
		startingScale = thisTransform.localScale.x;
	}

	private void Update()
	{
		if (Active)
		{
			float num = startingScale + Mathf.Abs(Mathf.Sin((float)Math.PI * throbFrequency * Time.time)) * (maxThrobScale * startingScale);
			thisTransform.localScale = new Vector3(num, num);
			if (elementImage != null)
			{
				elementImage.color = Color.Lerp(startingColor, throbColorOverlay, Mathf.Abs(Mathf.Sin((float)Math.PI * throbFrequency * Time.time)));
			}
		}
	}
}
