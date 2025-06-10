using UnityEngine;

public class CharadesRoundTimer : MonoBehaviour
{
	[SerializeField]
	private TextMesh[] timerTextMeshes;

	public float Timer
	{
		set
		{
			string text = value.ToTimeString();
			TextMesh[] array = timerTextMeshes;
			foreach (TextMesh textMesh in array)
			{
				textMesh.text = text;
			}
		}
	}

	private void Awake()
	{
		Timer = 0f;
	}
}
