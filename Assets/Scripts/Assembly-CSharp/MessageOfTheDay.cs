using RecNet;
using UnityEngine;
using UnityEngine.UI;

public class MessageOfTheDay : MonoBehaviour
{
	[SerializeField]
	private Text messageText;

	private void Start()
	{
		messageText.text = Config.MessageOfTheDay;
	}
}
