using RecNet;
using UnityEngine;

public class RecNetMessageReceivedAnalytics : MonoBehaviour
{
	private void Awake()
	{
		Messages.OnMessageAdded += AnalyticsHelper.OnMessageReceived;
	}

	private void OnDestroy()
	{
		Messages.OnMessageAdded -= AnalyticsHelper.OnMessageReceived;
	}
}
