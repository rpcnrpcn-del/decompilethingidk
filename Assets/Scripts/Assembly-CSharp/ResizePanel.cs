using UnityEngine;
using UnityEngine.EventSystems;

public class ResizePanel : MonoBehaviour, IPointerDownHandler, IDragHandler, IEventSystemHandler
{
	public Vector2 minSize = new Vector2(100f, 100f);

	public Vector2 maxSize = new Vector2(400f, 400f);

	private RectTransform panelRectTransform;

	private Vector2 originalLocalPointerPosition;

	private Vector2 originalSizeDelta;

	private void Awake()
	{
		panelRectTransform = base.transform.parent.GetComponent<RectTransform>();
	}

	public void OnPointerDown(PointerEventData data)
	{
		originalSizeDelta = panelRectTransform.sizeDelta;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, data.position, data.pressEventCamera, out originalLocalPointerPosition);
	}

	public void OnDrag(PointerEventData data)
	{
		if (!(panelRectTransform == null))
		{
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, data.position, data.pressEventCamera, out localPoint);
			Vector3 vector = localPoint - originalLocalPointerPosition;
			Vector2 vector2 = originalSizeDelta + new Vector2(vector.x, 0f - vector.y);
			vector2 = new Vector2(Mathf.Clamp(vector2.x, minSize.x, maxSize.x), Mathf.Clamp(vector2.y, minSize.y, maxSize.y));
			panelRectTransform.sizeDelta = vector2;
		}
	}
}
