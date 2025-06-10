using System.Collections;
using UnityEngine;

public class LockerroomOOBEFlow : OOBEFlowBase
{
	[Header("VO")]
	[SerializeField]
	private TutorialManager.VOClip welcomeToLockerroomVO;

	[SerializeField]
	private TutorialManager.VOClip whenYouAreReadyToPlayFollowGoPlayVO;

	[SerializeField]
	private TutorialManager.VOClip letsGoPlayVO;

	[SerializeField]
	private RecRoomAudioClip lockerRoomNotificationClip;

	private void Start()
	{
		if (TutorialManager.OOBEState <= TutorialManager.OOBEFlowState.Lockerroom)
		{
			StartCoroutine(RunOOBEFlow());
		}
	}

	private IEnumerator RunOOBEFlow()
	{
		yield return RunWaitForPlayerSpawn();
		Player player = Player.LocalPlayer;
		yield return new WaitForSeconds(0.75f);
		ScreenSpaceNotificationManager.Instance.Play(ScreenSpaceNotificationManager.NotificationType.Major, RecRoomSceneManager.CurrentSceneFriendlyName, 3f);
		AudioManager.Play2DSFX(lockerRoomNotificationClip);
		yield return new WaitForSeconds(0.75f);
		if (TutorialManager.OOBEState < TutorialManager.OOBEFlowState.Lockerroom)
		{
			yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(welcomeToLockerroomVO);
			yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(letsGoPlayVO);
		}
		while (!ActivityBounds.PointInBounds(player.Head.transform.position))
		{
			yield return null;
		}
		TutorialManager.OOBEState = TutorialManager.OOBEFlowState.Lockerroom;
		yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(whenYouAreReadyToPlayFollowGoPlayVO);
	}
}
