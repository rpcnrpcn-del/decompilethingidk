using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSpaceNotificationManager : MonoBehaviour
{
	public enum NotificationType
	{
		Vital = 0,
		Major = 1,
		Medium = 2,
		Minor = 3
	}

	private class NotificationDescriptor
	{
		public NotificationType Type;

		public string Title;

		public string Subtitle;

		public float Duration;

		public Action Callback;
	}

	private static ScreenSpaceNotificationManager instance;

	[SerializeField]
	private ScreenSpaceNotification vitalNotification;

	[SerializeField]
	private ScreenSpaceNotification majorNotification;

	[SerializeField]
	private ScreenSpaceNotification mediumNotification;

	[SerializeField]
	private ScreenSpaceNotification minorNotification;

	private Queue<NotificationDescriptor> vitalNotificationQueue;

	private NotificationDescriptor nextNotificationDescriptor;

	private NotificationDescriptor currentNotificationDescriptor;

	private ScreenSpaceNotification currentNotification;

	public static ScreenSpaceNotificationManager Instance
	{
		get
		{
			return instance;
		}
		protected set
		{
			if (instance == null)
			{
				instance = value;
			}
			else
			{
				Debug.LogError("Created more than one singleton of ScreenSpaceNotificationManager");
			}
		}
	}

	private bool Paused
	{
		get
		{
			return Player.LocalPlayer != null && Player.LocalPlayer.IsSpawning;
		}
	}

	private void Awake()
	{
		Instance = this;
		vitalNotificationQueue = new Queue<NotificationDescriptor>();
		vitalNotification.gameObject.SetActive(false);
		majorNotification.gameObject.SetActive(false);
		mediumNotification.gameObject.SetActive(false);
		minorNotification.gameObject.SetActive(false);
	}

	private void Start()
	{
		StartCoroutine(NotificationDisplayCoroutine());
	}

	public void Play(NotificationType style, string titleText, float duration, Action callback = null)
	{
		Play(style, titleText, null, duration, callback);
	}

	public void Play(NotificationType style, string titleText, string subtitleText, float duration, Action callback = null)
	{
		NotificationDescriptor notificationDescriptor = new NotificationDescriptor();
		notificationDescriptor.Type = style;
		notificationDescriptor.Title = titleText;
		notificationDescriptor.Subtitle = subtitleText;
		notificationDescriptor.Duration = duration;
		notificationDescriptor.Callback = callback;
		NotificationDescriptor item = notificationDescriptor;
		if (style == NotificationType.Vital)
		{
			nextNotificationDescriptor = null;
			vitalNotificationQueue.Enqueue(item);
		}
		else if (vitalNotificationQueue.Count == 0 && (currentNotificationDescriptor == null || currentNotificationDescriptor.Type >= style) && (nextNotificationDescriptor == null || nextNotificationDescriptor.Type >= style))
		{
			nextNotificationDescriptor = item;
		}
	}

	public void PlayDelayed(NotificationType style, string titleText, float duration, float delay, Action callback = null)
	{
		PlayDelayed(style, titleText, null, duration, delay, callback);
	}

	public void PlayDelayed(NotificationType style, string titleText, string subtitleText, float duration, float delay, Action callback = null)
	{
		StartCoroutine(EnqueueNotificationCoroutine(style, titleText, subtitleText, duration, delay, callback));
	}

	private IEnumerator EnqueueNotificationCoroutine(NotificationType style, string titleText, string subtitleText, float duration, float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		Play(style, titleText, subtitleText, duration, callback);
	}

	private IEnumerator NotificationDisplayCoroutine()
	{
		while (true)
		{
			yield return new WaitUntil(() => AnyNotificationsPending());
			currentNotificationDescriptor = GetNextNotification();
			currentNotification = GetNotification(currentNotificationDescriptor.Type);
			currentNotification.gameObject.SetActive(true);
			currentNotification.Title.Text.text = currentNotificationDescriptor.Title;
			currentNotification.Title.Alpha = 0f;
			currentNotification.Subtitle.Text.text = currentNotificationDescriptor.Subtitle;
			currentNotification.Subtitle.Alpha = 0f;
			currentNotification.DesaturationBackdrop.material.SetFloat("_DesaturationAmount", 0f);
			if (currentNotificationDescriptor.Callback != null)
			{
				currentNotificationDescriptor.Callback();
			}
			yield return AnimateNotification(true);
			yield return WaitForNotificationDuration();
			yield return AnimateNotification(false);
			currentNotification.gameObject.SetActive(false);
			currentNotification = null;
			currentNotificationDescriptor = null;
		}
	}

	private void ConfigureNotification()
	{
	}

	private IEnumerator AnimateNotification(bool animateIn)
	{
		float duration = ((!animateIn) ? currentNotification.CameraOffsetEaseOutCurve.Duration : currentNotification.CameraOffsetEaseInCurve.Duration);
		Vector3ValueCurve posCurve = ((!animateIn) ? currentNotification.CameraOffsetEaseOutCurve : currentNotification.CameraOffsetEaseInCurve);
		FloatValueCurve alphaCurve = ((!animateIn) ? currentNotification.AlphaEaseOutCurve : currentNotification.AlphaEaseInCurve);
		FloatValueCurve desatCurve = ((!animateIn) ? currentNotification.DesaturationEaseOutCurve : currentNotification.DesaturationEaseInCurve);
		float timer = 0f;
		while (timer < posCurve.Duration && !HigherPriorityNotificationsPending())
		{
			float t = timer / duration;
			currentNotification.positionCameraSpace = posCurve.Evaluate(t);
			currentNotification.Title.Alpha = alphaCurve.Evaluate(t);
			currentNotification.Subtitle.Alpha = alphaCurve.Evaluate(t);
			currentNotification.DesaturationBackdrop.material.SetFloat("_DesaturationAmount", desatCurve.Evaluate(t));
			if (!Paused)
			{
				timer += Time.deltaTime;
			}
			yield return null;
		}
		float finalAlpha = ((!animateIn || HigherPriorityNotificationsPending()) ? 0f : 1f);
		float finalDesat = ((!animateIn || HigherPriorityNotificationsPending()) ? 0f : 1f);
		currentNotification.Title.Alpha = finalAlpha;
		currentNotification.Subtitle.Alpha = finalAlpha;
		currentNotification.DesaturationBackdrop.material.SetFloat("_DesaturationAmount", finalDesat);
	}

	private IEnumerator WaitForNotificationDuration()
	{
		float waitDuration = Mathf.Max(0f, currentNotificationDescriptor.Duration - currentNotification.CameraOffsetEaseInCurve.Duration - currentNotification.AlphaEaseOutCurve.Duration);
		float timer = 0f;
		while (timer < waitDuration && !HigherPriorityNotificationsPending())
		{
			if (!Paused)
			{
				timer += Time.deltaTime;
			}
			yield return null;
		}
	}

	private ScreenSpaceNotification GetNotification(NotificationType type)
	{
		switch (type)
		{
		case NotificationType.Vital:
			return vitalNotification;
		case NotificationType.Major:
			return majorNotification;
		case NotificationType.Medium:
			return mediumNotification;
		case NotificationType.Minor:
			return minorNotification;
		default:
			return null;
		}
	}

	private bool AnyNotificationsPending()
	{
		return vitalNotificationQueue.Count > 0 || nextNotificationDescriptor != null;
	}

	private bool HigherPriorityNotificationsPending()
	{
		if (currentNotificationDescriptor.Type != NotificationType.Vital)
		{
			return vitalNotificationQueue.Count > 0 || (nextNotificationDescriptor != null && nextNotificationDescriptor.Type <= currentNotificationDescriptor.Type);
		}
		return false;
	}

	private NotificationDescriptor GetNextNotification()
	{
		if (vitalNotificationQueue.Count > 0)
		{
			return vitalNotificationQueue.Dequeue();
		}
		NotificationDescriptor result = nextNotificationDescriptor;
		nextNotificationDescriptor = null;
		return result;
	}
}
