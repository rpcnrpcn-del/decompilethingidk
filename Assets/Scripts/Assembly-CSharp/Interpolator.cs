using UnityEngine;

public class Interpolator : MonoBehaviour
{
	[Header("Movement")]
	public bool UseMovementEaseIn;

	[Tooltip("At what minimum distance should we apply easeIn")]
	public float MovementEaseInDistance = 0.5f;

	[Tooltip("What percent of the original speed easeIn should effect.")]
	[Range(0f, 0.99f)]
	public float MovementEaseInPercent = 0.8f;

	[Tooltip("Meters/second")]
	[SerializeField]
	private float movementSpeed = 2f;

	private bool isAnimatingPosition;

	private Vector3 targetPosition = Vector3.zero;

	[Header("Rotation")]
	[Tooltip("Degrees/second")]
	[SerializeField]
	private float rotationSpeed = 90f;

	private bool isAnimatingRotation;

	private Quaternion targetRotation = Quaternion.identity;

	public bool IsRunning
	{
		get
		{
			return base.enabled;
		}
	}

	public Vector3 TargetPosition
	{
		get
		{
			if (isAnimatingPosition)
			{
				return targetPosition;
			}
			return base.transform.position;
		}
		set
		{
			targetPosition = value;
			isAnimatingPosition = true;
			base.enabled = true;
		}
	}

	public Quaternion TargetRotation
	{
		get
		{
			if (isAnimatingRotation)
			{
				return targetRotation;
			}
			return base.transform.rotation;
		}
		set
		{
			targetRotation = value;
			isAnimatingRotation = true;
			base.enabled = true;
		}
	}

	private void Update()
	{
		EvaluateMovementFrame();
		EvaluateRotationFrame();
		if (!isAnimatingPosition && !isAnimatingRotation)
		{
			base.enabled = false;
		}
	}

	private void EvaluateMovementFrame()
	{
		if (isAnimatingPosition)
		{
			float num = Time.deltaTime * movementSpeed;
			if (UseMovementEaseIn)
			{
				float num2 = Mathf.Clamp01((targetPosition - base.transform.position).magnitude / MovementEaseInDistance) * MovementEaseInPercent + (1f - MovementEaseInPercent);
				num *= num2;
			}
			base.transform.position = Vector3.MoveTowards(base.transform.position, targetPosition, num);
			float magnitude = (targetPosition - base.transform.position).magnitude;
			if (magnitude < num)
			{
				isAnimatingPosition = false;
				base.transform.position = targetPosition;
			}
		}
	}

	private void EvaluateRotationFrame()
	{
		if (isAnimatingRotation)
		{
			float num = Time.deltaTime * rotationSpeed;
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, targetRotation, num);
			float num2 = Quaternion.Angle(targetRotation, base.transform.rotation);
			if (num2 < num)
			{
				isAnimatingRotation = false;
				base.transform.rotation = targetRotation;
			}
		}
	}

	public void StopInterpolation()
	{
		isAnimatingRotation = false;
		isAnimatingPosition = false;
	}

	public void SnapToTarget()
	{
		base.transform.position = TargetPosition;
		base.transform.rotation = TargetRotation;
		StopInterpolation();
	}
}
