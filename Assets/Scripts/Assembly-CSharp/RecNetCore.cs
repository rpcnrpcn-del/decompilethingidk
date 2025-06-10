using RecNet;
using UnityEngine;

internal class RecNetCore : MonoBehaviour
{
	private void OnDestroy()
	{
		Core.ShutdownPushNotificationChannel();
	}
}
