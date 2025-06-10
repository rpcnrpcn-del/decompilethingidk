using System;
using UnityEngine;

[Serializable]
public class AxisPressButtons : AxisButtons
{
	public override void Update(Vector2 axis, bool inDeadZone, bool hasValidInput = true)
	{
		Direction direction = GetDirection(axis);
		for (int i = 0; i < directionStates.Length; i++)
		{
			if (i == (int)direction)
			{
				directionStates[i].IsPressed = !inDeadZone;
			}
			else
			{
				directionStates[i].IsPressed = false;
			}
		}
	}
}
