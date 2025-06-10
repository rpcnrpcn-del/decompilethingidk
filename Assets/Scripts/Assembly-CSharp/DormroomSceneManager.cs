using System.Collections;
using UnityEngine;

public class DormroomSceneManager : RecRoomSceneManager
{
	[SerializeField]
	private string blockedUserMessage = "You are currently blocked.\nYou can rejoin in {0}mins {1}secs.";

	protected override void Start()
	{
		base.Start();
		int userBlockedDurationSecondsRemaning = SingletonMonoBehaviour<SettingsManager>.Instance.GetUserBlockedDurationSecondsRemaning();
		if (userBlockedDurationSecondsRemaning > 0)
		{
			StartCoroutine(BlockedCoroutine(userBlockedDurationSecondsRemaning));
		}
	}

	private IEnumerator BlockedCoroutine(int durationSeconds)
	{
		TeleportationPortal[] exitTeleportationPortals = Object.FindObjectsOfType<TeleportationPortal>();
		if (!SessionManager.IsDeveloper)
		{
			TeleportationPortal[] array = exitTeleportationPortals;
			foreach (TeleportationPortal teleportationPortal in array)
			{
				if (teleportationPortal.Target == null)
				{
					teleportationPortal.gameObject.SetActive(false);
				}
			}
		}
		while (Player.LocalPlayer == null || Player.LocalPlayer.PlayerUI.Menu == null)
		{
			yield return null;
		}
		PlayerMenu menu = Player.LocalPlayer.PlayerUI.Menu;
		menu.SetHeaderButtonsInteractable(false);
		while (!menu.Visible)
		{
			yield return null;
		}
		if (menu.IsAlertMessageActive)
		{
			yield return new WaitForSeconds(3f);
		}
		for (durationSeconds = SingletonMonoBehaviour<SettingsManager>.Instance.GetUserBlockedDurationSecondsRemaning(); durationSeconds > 0; durationSeconds--)
		{
			menu.ShowAlertMessage(message: string.Format(blockedUserMessage, durationSeconds / 60, durationSeconds % 60), title: string.Empty, persistent: true, notificationEffects: (PlayerMenu.NotificationEffectFlag)0);
			yield return new WaitForSeconds(1f);
		}
		menu.StopShowAlertMessage();
		menu.SetHeaderButtonsInteractable(true);
		menu.Visible = false;
		TeleportationPortal[] array2 = exitTeleportationPortals;
		foreach (TeleportationPortal teleportationPortal2 in array2)
		{
			if (teleportationPortal2.Target == null)
			{
				teleportationPortal2.gameObject.SetActive(true);
			}
		}
	}
}
