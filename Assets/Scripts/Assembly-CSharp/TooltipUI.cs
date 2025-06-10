using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
	public static List<TooltipUI> All = new List<TooltipUI>();

	[SerializeField]
	private TextMesh textMesh;

	[SerializeField]
	private Transform background;

	[SerializeField]
	private Vector2 backgroundHorizontalPadding = new Vector2(0.05f, 0.05f);

	[SerializeField]
	private AnimationCurve sizeOverDistanceCurve;

	private Renderer textRenderer;

	private Renderer backgroundRenderer;

	private float defaultTextAlpha;

	private float defaultBackgroundAlpha;

	private float fadeSpeed = 10f;

	public string Message
	{
		get
		{
			return textMesh.text;
		}
		set
		{
			textMesh.text = value;
			StartCoroutine(RunUpdateBoundsSize());
		}
	}

	private void Awake()
	{
		All.Add(this);
		base.gameObject.SetActive(false);
		textRenderer = textMesh.GetComponent<Renderer>();
		textRenderer.material.renderQueue = 10000;
		backgroundRenderer = background.GetComponent<Renderer>();
		backgroundRenderer.material.renderQueue = 9999;
		defaultTextAlpha = textRenderer.material.color.a;
		defaultBackgroundAlpha = backgroundRenderer.material.color.a;
	}

	private void OnDestroy()
	{
		All.Remove(this);
	}

	private void OnDisable()
	{
		if (textRenderer != null)
		{
			textRenderer.SetColorAlpha(0f);
			backgroundRenderer.SetColorAlpha(0f);
		}
	}

	private IEnumerator RunUpdateBoundsSize()
	{
		yield return null;
		Vector3 scale = base.transform.localScale;
		base.transform.localScale = Vector3.one;
		Quaternion rotation = textMesh.transform.rotation;
		textMesh.transform.rotation = Quaternion.identity;
		Bounds bounds = textRenderer.bounds;
		Vector3 boundsSize = bounds.size;
		textMesh.transform.rotation = rotation;
		base.transform.localScale = scale;
		background.transform.localScale = new Vector3(boundsSize.x + backgroundHorizontalPadding.x, boundsSize.y + backgroundHorizontalPadding.y, boundsSize.z);
		Vector3 localBackgroundPosition = textMesh.transform.InverseTransformPoint(bounds.center);
		localBackgroundPosition.z = 0f;
		background.transform.localPosition = localBackgroundPosition;
	}

	public static TooltipUI Acquire(string message, Vector3 initialPosition, Quaternion initialRotation)
	{
		foreach (TooltipUI item in All)
		{
			if (item != null && !item.gameObject.activeSelf)
			{
				float magnitude = (Camera.main.transform.position - initialPosition).magnitude;
				float num = item.sizeOverDistanceCurve.Evaluate(magnitude);
				item.transform.position = initialPosition;
				item.transform.rotation = initialRotation;
				item.transform.localScale = new Vector3(num, num, num);
				item.gameObject.SetActive(true);
				item.Message = message.Replace("\\n", Environment.NewLine);
				item.StartCoroutine(item.RunFadeIn());
				return item;
			}
		}
		return null;
	}

	public void Release()
	{
		base.gameObject.SetActive(false);
	}

	private IEnumerator RunFadeIn()
	{
		float t = 0f;
		while (t < 1f)
		{
			t = Mathf.SmoothStep(t, 1f, Time.deltaTime * fadeSpeed);
			textRenderer.SetColorAlpha(defaultTextAlpha * t);
			backgroundRenderer.SetColorAlpha(defaultBackgroundAlpha * t);
			yield return null;
		}
	}
}
