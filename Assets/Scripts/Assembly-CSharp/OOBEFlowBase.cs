using System;
using System.Collections;
using UnityEngine;

public class OOBEFlowBase : MonoBehaviour
{
	protected bool userPickedUpTool;

	protected bool userTeleported;

	protected void PlayerEvents_ToolPickupEvent(Tool tool)
	{
		userPickedUpTool = true;
	}

	protected void PlayerEvents_TeleportEvent()
	{
		userTeleported = true;
	}

	protected IEnumerator RunWaitForPlayerSpawn()
	{
		PhotonNetwork.player.SetCustomProperties("prev_activity", "oobe");
		while (Player.LocalPlayer == null || !Player.LocalPlayer.IsInitialized || !Player.LocalPlayer.HeadActivityDetected)
		{
			yield return null;
		}
	}

	protected IEnumerator RunInterractionFlow(Func<bool> condition, TutorialManager.VOClip instructionVO, TutorialManager.VOClip instructionVO2, TutorialManager.VOClip helpVO, float preHelpVODuration, int maxHelpCount = 10, Transform attentionHelperTransform = null)
	{
		yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(instructionVO);
		if (instructionVO2 != null)
		{
			yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(instructionVO2);
		}
		int count = 0;
		while (count++ < maxHelpCount)
		{
			float timer = preHelpVODuration;
			while (!condition() && timer > 0f)
			{
				timer -= Time.deltaTime;
				yield return null;
			}
			if (!condition())
			{
				if (attentionHelperTransform != null)
				{
					SingletonMonoBehaviour<TutorialManager>.Instance.PlayAttentionHelper(attentionHelperTransform.position);
				}
				yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(helpVO);
			}
			if (condition())
			{
				break;
			}
		}
	}

	protected void VibrateBothHands(Player player)
	{
		if (player.LeftHand != null)
		{
			player.LeftHand.Vibrate(500, 1000);
		}
		if (player.RightHand != null)
		{
			player.RightHand.Vibrate(500, 1000);
		}
	}

	protected IEnumerator RunAnimateInOut(AnimateInOut target, bool enable)
	{
		target.gameObject.SetActive(enable);
		yield return new WaitForSeconds(1f);
	}
}
