using System;
using UnityEngine;

public class HeadGestureDetect : MonoBehaviour
{
	public enum Gesture
	{
		TiltUp = 0,
		TiltDown = 1,
		NodUp = 2,
		NodDown = 3,
		FullNod = 4,
		TurnLeft = 5,
		TurnRight = 6,
		ShakeLeft = 7,
		ShakeRight = 8,
		FullShake = 9,
		LeanLeft = 10,
		LeanRight = 11,
		NodLeft = 12,
		NodRight = 13,
		FullSideNod = 14
	}

	[SerializeField]
	private float nodSensitivity = 10f;

	[SerializeField]
	private float shakeSensitivity = 10f;

	[SerializeField]
	private float leanSensitivity = 10f;

	[SerializeField]
	private float nodNeutralZone = 5f;

	[SerializeField]
	private float shakeNeutralZone = 5f;

	[SerializeField]
	private float leanNeutralZone = 5f;

	[SerializeField]
	private float timeOut = 3f;

	[SerializeField]
	private Transform trackedHead;

	[Tooltip("Enabling this will fire all the different gestures and not just the full gestures.")]
	public bool DetailedDetection;

	private float countdown;

	private float baselineX;

	private float baselineY;

	private float baselineZ;

	private float pitch;

	private float yaw;

	private float roll;

	private float doSomethingTime;

	private bool tiltUp;

	private bool tiltDown;

	private bool nodUp;

	private bool nodDown;

	private bool turnLeft;

	private bool turnRight;

	private bool shakeLeft;

	private bool shakeRight;

	private bool leanLeft;

	private bool leanRight;

	private bool nodLeft;

	private bool nodRight;

	private int nods;

	private int shakes;

	private int sideNods;

	private float pitchAng;

	private float yawAng;

	private float rollAng;

	public event Action<Gesture> GestureDetected;

	private void Start()
	{
		if (trackedHead == null)
		{
			trackedHead = base.transform;
		}
		baselineX = Angle(trackedHead.localEulerAngles.x);
		baselineY = Angle(trackedHead.localEulerAngles.y);
		baselineZ = Angle(trackedHead.localEulerAngles.z);
		doSomethingTime = Time.time;
	}

	private void Update()
	{
		countdown = Time.time - doSomethingTime;
		pitch = Angle(trackedHead.localEulerAngles.x);
		yaw = Angle(trackedHead.localEulerAngles.y);
		roll = Angle(trackedHead.localEulerAngles.z);
		pitchAng = AngleBetween(pitch, baselineX);
		yawAng = AngleBetween(yaw, baselineY);
		rollAng = AngleBetween(roll, baselineZ);
		if (tiltUp && countdown > timeOut)
		{
			tiltUp = false;
			baselineX = pitch;
			return;
		}
		if (tiltDown && countdown > timeOut)
		{
			tiltDown = false;
			baselineX = pitch;
			return;
		}
		if (turnLeft && countdown > timeOut)
		{
			turnLeft = false;
			baselineY = yaw;
			return;
		}
		if (turnRight && countdown > timeOut)
		{
			turnRight = false;
			baselineY = yaw;
			return;
		}
		if (nods > 0 && countdown > timeOut)
		{
			nods = 0;
		}
		if (shakes > 0 && countdown > timeOut)
		{
			shakes = 0;
		}
		if (sideNods > 0 && countdown > timeOut)
		{
			sideNods = 0;
		}
		if (!tiltUp && pitchAng < 0f - nodSensitivity)
		{
			tiltUp = true;
			doSomethingTime = Time.time;
			if (DetailedDetection && this.GestureDetected != null)
			{
				this.GestureDetected(Gesture.TiltUp);
			}
		}
		if (tiltUp && pitchAng > nodNeutralZone && countdown < timeOut)
		{
			tiltUp = false;
			nodUp = true;
			doSomethingTime = Time.time;
			if (DetailedDetection && this.GestureDetected != null)
			{
				this.GestureDetected(Gesture.NodUp);
			}
		}
		if (!tiltDown && pitchAng > nodSensitivity)
		{
			tiltDown = true;
			doSomethingTime = Time.time;
			if (DetailedDetection && this.GestureDetected != null)
			{
				this.GestureDetected(Gesture.TiltDown);
			}
		}
		if (tiltDown && pitchAng < nodNeutralZone && countdown < timeOut)
		{
			tiltDown = false;
			doSomethingTime = Time.time;
			nodDown = true;
			if (DetailedDetection && this.GestureDetected != null)
			{
				this.GestureDetected(Gesture.NodDown);
			}
		}
		if (nodDown && nodUp)
		{
			nods++;
			if (this.GestureDetected != null)
			{
				this.GestureDetected(Gesture.FullNod);
			}
			doSomethingTime = Time.time;
			nodUp = false;
			nodDown = false;
		}
		if (!turnRight && yawAng > shakeSensitivity)
		{
			turnRight = true;
			doSomethingTime = Time.time;
			if (DetailedDetection && this.GestureDetected != null)
			{
				this.GestureDetected(Gesture.TurnRight);
			}
		}
		if (turnRight && yawAng < shakeNeutralZone && countdown < timeOut)
		{
			turnRight = false;
			shakeRight = true;
			doSomethingTime = Time.time;
			if (DetailedDetection && this.GestureDetected != null)
			{
				this.GestureDetected(Gesture.ShakeRight);
			}
		}
		if (!turnLeft && yawAng < 0f - shakeSensitivity)
		{
			turnLeft = true;
			doSomethingTime = Time.time;
			if (DetailedDetection && this.GestureDetected != null)
			{
				this.GestureDetected(Gesture.TurnLeft);
			}
		}
		if (turnLeft && yawAng > 0f - shakeNeutralZone)
		{
			turnLeft = false;
			doSomethingTime = Time.time;
			shakeLeft = true;
			if (DetailedDetection && this.GestureDetected != null)
			{
				this.GestureDetected(Gesture.ShakeLeft);
			}
		}
		if (shakeLeft && shakeRight)
		{
			shakes++;
			if (this.GestureDetected != null)
			{
				this.GestureDetected(Gesture.FullShake);
			}
			doSomethingTime = Time.time;
			shakeLeft = false;
			shakeRight = false;
		}
		if (!leanRight && rollAng < 0f - leanSensitivity)
		{
			leanRight = true;
			doSomethingTime = Time.time;
			if (DetailedDetection && this.GestureDetected != null)
			{
				this.GestureDetected(Gesture.LeanRight);
			}
		}
		if (leanRight && rollAng > leanNeutralZone && countdown < timeOut)
		{
			leanRight = false;
			nodRight = true;
			doSomethingTime = Time.time;
			if (DetailedDetection && this.GestureDetected != null)
			{
				this.GestureDetected(Gesture.NodRight);
			}
		}
		if (!leanLeft && rollAng > leanSensitivity)
		{
			leanLeft = true;
			doSomethingTime = Time.time;
			if (DetailedDetection && this.GestureDetected != null)
			{
				this.GestureDetected(Gesture.LeanLeft);
			}
		}
		if (leanLeft && rollAng < leanNeutralZone)
		{
			leanLeft = false;
			doSomethingTime = Time.time;
			nodLeft = true;
			if (DetailedDetection && this.GestureDetected != null)
			{
				this.GestureDetected(Gesture.NodLeft);
			}
		}
		if (nodLeft && nodRight)
		{
			sideNods++;
			if (this.GestureDetected != null)
			{
				this.GestureDetected(Gesture.FullSideNod);
			}
			doSomethingTime = Time.time;
			nodLeft = false;
			nodRight = false;
		}
	}

	private float Angle(float a)
	{
		if (a > 180f)
		{
			a -= 360f;
		}
		return a;
	}

	private float AngleBetween(float a, float b)
	{
		float num = a - b;
		if (num > 180f || num < -180f)
		{
			num = 360f - num;
		}
		return num;
	}
}
