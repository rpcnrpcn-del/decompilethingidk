using System;
using UnityEngine;

public class ClockFace : MonoBehaviour
{
	[SerializeField]
	private Transform secondHand;

	[SerializeField]
	private Transform minuteHand;

	[SerializeField]
	private Transform hourHand;

	private bool HandsActive
	{
		set
		{
			secondHand.gameObject.SetActive(value);
			minuteHand.gameObject.SetActive(value);
			hourHand.gameObject.SetActive(value);
		}
	}

	private void OnEnable()
	{
		HandsActive = true;
	}

	private void OnDisable()
	{
		HandsActive = false;
	}

	private void Update()
	{
		DateTime now = DateTime.Now;
		secondHand.localRotation = GetHandRotation(now.Second, 60);
		minuteHand.localRotation = GetHandRotation(now.Minute, 60);
		hourHand.localRotation = GetHandRotation(now.Hour, 12);
	}

	private Quaternion GetHandRotation(int timeValue, int maxTimeValue)
	{
		float num = Mathf.InverseLerp(0f, maxTimeValue, timeValue % maxTimeValue);
		return Quaternion.Euler(0f, 0f, (0f - num) * 360f);
	}
}
