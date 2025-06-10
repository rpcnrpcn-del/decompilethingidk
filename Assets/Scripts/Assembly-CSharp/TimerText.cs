using UnityEngine;
using UnityEngine.UI;

public class TimerText : MonoBehaviour
{
	[SerializeField]
	private TextMesh[] timerTextMeshes;

	[SerializeField]
	private Text[] timerTexts;

	private GameManager gameManager;

	private void Start()
	{
		OnTimerUpdate(0f);
		gameManager = RecRoomSceneManager.Instance.GameManager;
	}

	private void Update()
	{
		float timer = 0f;
		if (gameManager != null)
		{
			timer = gameManager.GameTimerManager.TimeRemaining;
		}
		OnTimerUpdate(timer);
	}

	private void OnTimerUpdate(float timer)
	{
		string text = timer.ToTimeString();
		Text[] array = timerTexts;
		foreach (Text text2 in array)
		{
			text2.text = text;
		}
		TextMesh[] array2 = timerTextMeshes;
		foreach (TextMesh textMesh in array2)
		{
			textMesh.text = text;
		}
	}
}
