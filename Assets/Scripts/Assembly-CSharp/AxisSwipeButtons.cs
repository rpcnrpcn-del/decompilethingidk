using System;
using UnityEngine;

[Serializable]
public class AxisSwipeButtons : AxisButtons
{
	[SerializeField]
	private float maxSwipeTime = 1f;

	[SerializeField]
	private float minSwipeDistance = 0.5f;

	private bool isSwiping;

	private bool swipeIsValid = true;

	private Vector2 swipeDisplacement = Vector2.zero;

	private Vector2 latestPosition = Vector2.zero;

	private float swipeStartTime;

	public override void Update(Vector2 axis, bool inDeadZone, bool inputValid = true)
	{
		swipeIsValid &= inputValid;
		Direction direction = Direction.NONE;
		if (!isSwiping && !inDeadZone)
		{
			swipeIsValid = true;
			latestPosition = axis;
			swipeDisplacement = Vector2.zero;
			swipeStartTime = Time.time;
		}
		else if (isSwiping && !inDeadZone)
		{
			swipeDisplacement += axis - latestPosition;
			latestPosition = axis;
		}
		else if (isSwiping && inDeadZone && swipeIsValid && Time.time - swipeStartTime <= maxSwipeTime && swipeDisplacement.magnitude >= minSwipeDistance)
		{
			direction = GetDirection(swipeDisplacement);
		}
		isSwiping = !inDeadZone;
		for (int i = 0; i < directionStates.Length; i++)
		{
			if (i == (int)direction)
			{
				directionStates[i].IsPressed = true;
			}
			else
			{
				directionStates[i].IsPressed = false;
			}
		}
	}
}
