using UnityEngine;

public static class SA_TweenExtensions
{
	public static void MoveTo(this GameObject go, Vector3 position, float time)
	{
		SA_ValuesTween sA_ValuesTween = go.AddComponent<SA_ValuesTween>();
		sA_ValuesTween.DestoryGameObjectOnComplete = false;
		sA_ValuesTween.VectorTo(go.transform.position, position, time);
	}
}
