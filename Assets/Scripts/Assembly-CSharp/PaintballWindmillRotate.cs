using UnityEngine;

public class PaintballWindmillRotate : MonoBehaviour
{
	[SerializeField]
	private float degreesPerSecond;

	private void Update()
	{
		base.transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.Self);
	}
}
