using UnityEngine;

public class BobbingMotion : MonoBehaviour
{
	[SerializeField]
	private float scaleBobAmount;

	[SerializeField]
	private float scaleBobSpeed;

	private float bobHeight;

	private void Awake()
	{
		scaleBobSpeed += Random.Range(-0.2f, 0.2f);
	}

	private void FixedUpdate()
	{
		bobHeight = Mathf.Sin(Time.time * scaleBobSpeed) * scaleBobAmount;
		base.transform.localPosition = new Vector3(0f, bobHeight, 0f);
	}
}
