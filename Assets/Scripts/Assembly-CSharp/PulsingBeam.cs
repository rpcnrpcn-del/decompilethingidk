using UnityEngine;

public class PulsingBeam : MonoBehaviour
{
	[SerializeField]
	private bool gravityAligned;

	[SerializeField]
	private float scalePulseAmount;

	[SerializeField]
	private float scalePulseSpeed;

	private float startingScale;

	private void Awake()
	{
		startingScale = base.transform.localScale.x;
		scalePulseSpeed += Random.Range(-0.2f, 0.2f);
	}

	private void FixedUpdate()
	{
		if (gravityAligned)
		{
			base.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
		}
		Vector3 localScale = base.transform.localScale;
		localScale.z = (localScale.x = startingScale + Mathf.Sin(Time.time * scalePulseSpeed) * scalePulseAmount);
		base.transform.localScale = localScale;
	}
}
