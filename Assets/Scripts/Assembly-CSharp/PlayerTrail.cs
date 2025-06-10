using UnityEngine;

public class PlayerTrail : LineRendererTrail
{
	[SerializeField]
	[Tooltip("Rate that trail will fade from tip to head.  In units of m/s.")]
	private float fadeOutRate = 10f;

	public void PlayTeleportTrail(Vector3 playerHeadOffset, Vector3 startPlayerPosition, Vector3 endPlayerPosition)
	{
		Clear();
		Vector3 vector = endPlayerPosition - startPlayerPosition;
		float magnitude = vector.magnitude;
		if (!(magnitude <= Mathf.Epsilon))
		{
			Vector3 vector2 = vector / magnitude;
			int num = Mathf.RoundToInt(magnitude / minSpacing);
			float num2 = magnitude / (float)num;
			float num3 = magnitude / fadeOutRate;
			float num4 = num3 / (float)num;
			playerHeadOffset -= Vector3.up * widths.y / 2f;
			startPlayerPosition += playerHeadOffset;
			endPlayerPosition += playerHeadOffset;
			for (int i = 0; i < num; i++)
			{
				AddNewPosition(startPlayerPosition + vector2 * i * num2, Vector3.up, Time.time + (float)i * num4);
			}
		}
	}
}
