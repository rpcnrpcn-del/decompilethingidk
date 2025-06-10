using UnityEngine;

public class Tooltip : MonoBehaviour
{
	public string Message = "Hello";

	public float ShowDelay = 0.75f;

	private TooltipUI tooltipUI;

	private float hoverStartTime;

	private bool wasHovered;

	private bool isHovered;

	private Transform cursor;

	public bool Hovered { get; set; }

	private void Update()
	{
		if (isHovered != wasHovered)
		{
			if (isHovered)
			{
				hoverStartTime = Time.time;
			}
			else
			{
				Hide();
			}
		}
		if (isHovered && cursor != null)
		{
			Vector3 position = cursor.position;
			Vector3 eulerAngles = cursor.rotation.eulerAngles;
			eulerAngles.z = 0f;
			Quaternion rotation = Quaternion.Euler(eulerAngles);
			if (tooltipUI != null)
			{
				tooltipUI.transform.position = position;
				tooltipUI.transform.rotation = rotation;
				if (!cursor.gameObject.activeSelf)
				{
					Hide();
				}
			}
			else if (Time.time - hoverStartTime >= ShowDelay)
			{
				Show(position, rotation);
			}
		}
		wasHovered = isHovered;
	}

	private void LateUpdate()
	{
		isHovered = false;
	}

	private void OnDisable()
	{
		Hide();
	}

	public void SetHoverPoint(Transform hoveredCursor)
	{
		isHovered = true;
		if (cursor == null)
		{
			cursor = hoveredCursor;
		}
	}

	private void Hide()
	{
		if (tooltipUI != null)
		{
			tooltipUI.Release();
			tooltipUI = null;
		}
		cursor = null;
	}

	private void Show(Vector3 position, Quaternion rotation)
	{
		if (tooltipUI == null)
		{
			tooltipUI = TooltipUI.Acquire(Message, position, rotation);
		}
	}
}
