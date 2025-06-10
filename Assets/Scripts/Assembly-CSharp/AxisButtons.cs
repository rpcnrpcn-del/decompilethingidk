using System;
using UnityEngine;

[Serializable]
public abstract class AxisButtons
{
	public enum Direction
	{
		NONE = 0,
		LEFT = 1,
		RIGHT = 2,
		UP = 3,
		DOWN = 4
	}

	[Serializable]
	protected struct DirectionDescription
	{
		public Direction Direction;

		public float MinAngleDegrees;

		public float MaxAngleDegrees;
	}

	protected struct DirectionButtonState
	{
		private bool _isPressed;

		public bool IsPressed
		{
			get
			{
				return _isPressed;
			}
			set
			{
				DirectionDown = false;
				DirectionPressed = false;
				DirectionUp = false;
				if (!_isPressed && value)
				{
					DirectionDown = true;
				}
				else if (_isPressed && !value)
				{
					DirectionUp = true;
				}
				_isPressed = value;
				if (_isPressed)
				{
					DirectionPressed = true;
				}
			}
		}

		public bool DirectionDown { get; private set; }

		public bool DirectionPressed { get; private set; }

		public bool DirectionUp { get; private set; }
	}

	[SerializeField]
	protected DirectionDescription[] possibleDirections;

	protected DirectionButtonState[] directionStates;

	public AxisButtons()
	{
		Direction[] array = (Direction[])Enum.GetValues(typeof(Direction));
		directionStates = new DirectionButtonState[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			int num = (int)array[i];
			directionStates[num] = default(DirectionButtonState);
			directionStates[num].IsPressed = false;
		}
	}

	public abstract void Update(Vector2 axis, bool inDeadZone, bool hasValidInput = true);

	public bool GetPressed(Direction buttonDirection)
	{
		return directionStates[(int)buttonDirection].DirectionPressed;
	}

	public bool GetDown(Direction buttonDirection)
	{
		return directionStates[(int)buttonDirection].DirectionDown;
	}

	public bool GetUp(Direction buttonDirection)
	{
		return directionStates[(int)buttonDirection].DirectionUp;
	}

	protected Direction GetDirection(Vector2 axis)
	{
		Direction result = Direction.NONE;
		if (possibleDirections != null && axis.sqrMagnitude >= Mathf.Epsilon)
		{
			float num = Vector2.right.AngleSignedVector2(axis.normalized);
			if (num < 0f)
			{
				num = 360f + num;
			}
			for (int i = 0; i < possibleDirections.Length; i++)
			{
				bool flag = possibleDirections[i].MaxAngleDegrees < possibleDirections[i].MinAngleDegrees;
				if ((num <= possibleDirections[i].MaxAngleDegrees && num >= possibleDirections[i].MinAngleDegrees) || (flag && num >= possibleDirections[i].MinAngleDegrees && num - 360f <= possibleDirections[i].MaxAngleDegrees) || (flag && num <= possibleDirections[i].MaxAngleDegrees && num + 360f >= possibleDirections[i].MinAngleDegrees))
				{
					result = possibleDirections[i].Direction;
					break;
				}
			}
		}
		return result;
	}
}
