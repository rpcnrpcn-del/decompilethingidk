using System;
using System.Collections.Generic;
using UnityEngine;

public class ControllerButtonLabels : MonoBehaviour
{
	public enum ButtonType
	{
		Touchpad = 0,
		Menu = 1,
		Trigger = 2
	}

	[Serializable]
	private struct ButtonLabel
	{
		public ButtonType ButtonType;

		public string AttachPointName;

		public string Text;

		public Vector3 LocalPosition;

		public Vector3 RotationEuler;

		public float LocalScale;
	}

	[SerializeField]
	private Label buttonLabelPrefab;

	[SerializeField]
	private Color buttonColor = new Color(0.6f, 0.6f, 0.6f);

	[SerializeField]
	private ButtonLabel[] buttonLabels;

	private Dictionary<ButtonType, Label> buttonLabelMap = new Dictionary<ButtonType, Label>();

	private void Awake()
	{
		SteamVR_Utils.Event.Listen("render_model_loaded", OnRenderModelLoaded);
		ButtonLabel[] array = buttonLabels;
		for (int i = 0; i < array.Length; i++)
		{
			ButtonLabel buttonLabel = array[i];
			Label label = UnityEngine.Object.Instantiate(buttonLabelPrefab);
			label.transform.SetParent(base.transform);
			label.Text = buttonLabel.Text;
			label.ButtonColor = buttonColor;
			label.gameObject.SetActive(false);
			buttonLabelMap.Add(buttonLabel.ButtonType, label);
		}
	}

	private void OnDestroy()
	{
		SteamVR_Utils.Event.Remove("render_model_loaded", OnRenderModelLoaded);
	}

	public void SetButtonLabelVisibility(ButtonType button, bool visible)
	{
		Label value = null;
		if (buttonLabelMap.TryGetValue(button, out value))
		{
			value.Visible = visible;
		}
	}

	private void OnRenderModelLoaded(params object[] args)
	{
		if (((SteamVR_RenderModel)args[0]).gameObject != base.gameObject)
		{
			return;
		}
		ButtonLabel[] array = buttonLabels;
		for (int i = 0; i < array.Length; i++)
		{
			ButtonLabel buttonLabel = array[i];
			Label value = null;
			if (!buttonLabelMap.TryGetValue(buttonLabel.ButtonType, out value))
			{
				continue;
			}
			Transform transform = base.transform.Find(buttonLabel.AttachPointName);
			if (transform != null)
			{
				value.ButtonRenderer = transform.gameObject.GetComponent<Renderer>();
				transform = transform.Find("attach");
				if (transform != null)
				{
					value.transform.SetParent(transform, false);
					value.transform.localRotation = Quaternion.Euler(buttonLabel.RotationEuler);
					value.transform.localPosition = buttonLabel.LocalPosition;
					value.transform.localScale.Scale(new Vector3(buttonLabel.LocalScale, buttonLabel.LocalScale, 1f));
					value.gameObject.SetActive(true);
				}
			}
		}
	}
}
