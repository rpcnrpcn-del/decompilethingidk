using System;
using UnityEngine;
using UnityEngine.UI;

public class ScreenSpaceNotification : MonoBehaviour
{
	[Serializable]
	public class NotificationText
	{
		public Text Text;

		public Outline Outline;

		public int FontSize
		{
			set
			{
				Text.fontSize = value;
			}
		}

		public Color Color
		{
			set
			{
				Text.color = value;
			}
		}

		public float Alpha
		{
			set
			{
				Color color = Text.color;
				color.a = value;
				Text.color = color;
				Color effectColor = Outline.effectColor;
				effectColor.a = value;
				Outline.effectColor = effectColor;
			}
		}
	}

	public NotificationText Title;

	public NotificationText Subtitle;

	public MeshRenderer DesaturationBackdrop;

	[Header("Animation Config")]
	public FloatValueCurve DesaturationEaseInCurve;

	public FloatValueCurve DesaturationEaseOutCurve;

	public FloatValueCurve AlphaEaseInCurve;

	public FloatValueCurve AlphaEaseOutCurve;

	public Vector3ValueCurve CameraOffsetEaseInCurve;

	public Vector3ValueCurve CameraOffsetEaseOutCurve;

	public Vector3 positionCameraSpace { get; set; }

	private void Awake()
	{
		Title.Alpha = 0f;
		Subtitle.Alpha = 0f;
	}

	private void LateUpdate()
	{
		Vector3 position = SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.position;
		Vector3 up = Vector3.up;
		Vector3 vector = SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.forward;
		Vector3 vector2 = Vector3.Cross(vector, up);
		up = Vector3.Cross(vector2, vector);
		base.transform.position = positionCameraSpace.TransformToWorldSpace(position, vector2, up, vector);
		if ((base.transform.position - position).sqrMagnitude > 0.001f)
		{
			vector = Vector3.ProjectOnPlane(base.transform.position - position, up).normalized;
		}
		base.transform.rotation = Quaternion.LookRotation(vector, up);
	}
}
