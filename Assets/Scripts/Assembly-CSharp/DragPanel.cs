using UnityEngine;
using UnityEngine.EventSystems;

public class DragPanel : MonoBehaviour, IPointerDownHandler, IDragHandler, IEventSystemHandler
{
	private Vector2 originalLocalPointerPosition;

	private Vector3 originalPanelLocalPosition;

	private RectTransform panelRectTransform;

	private RectTransform parentRectTransform;

	private void Awake()
	{
		panelRectTransform = base.transform.parent as RectTransform;
		parentRectTransform = panelRectTransform.parent as RectTransform;
	}

	public void OnPointerDown(PointerEventData data)
	{
		originalPanelLocalPosition = panelRectTransform.localPosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, data.position, data.pressEventCamera, out originalLocalPointerPosition);
	}

	public void OnDrag(PointerEventData data)
	{
		if (!(panelRectTransform == null) && !(parentRectTransform == null))
		{
			Vector2 localPoint;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, data.position, data.pressEventCamera, out localPoint))
			{
				Vector3 vector = localPoint - originalLocalPointerPosition;
				panelRectTransform.localPosition = originalPanelLocalPosition + vector;
			}
			ClampToWindow();
		}
	}

	private void ClampToWindow()
	{
		Vector3 localPosition = panelRectTransform.localPosition;
		Vector3 vector = parentRectTransform.rect.min - panelRectTransform.rect.min;
		Vector3 vector2 = parentRectTransform.rect.max - panelRectTransform.rect.max;
		localPosition.x = Mathf.Clamp(panelRectTransform.localPosition.x, vector.x, vector2.x);
		localPosition.y = Mathf.Clamp(panelRectTransform.localPosition.y, vector.y, vector2.y);
		panelRectTransform.localPosition = localPosition;
	}
}
