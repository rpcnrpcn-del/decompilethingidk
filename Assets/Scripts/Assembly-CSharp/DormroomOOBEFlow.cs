using System.Collections;
using UnityEngine;

public class DormroomOOBEFlow : OOBEFlowBase
{
	[Header("Scene references")]
	public StickyNote teleportNote;

	public StickyNote grabNote;

	public StickyNote menuNote;

	public Tool[] interactionTools;

	public TeleportationPortal lockerRoomDoor;

	[SerializeField]
	private Transform doorHandle;

	[SerializeField]
	private Transform youAreHereIndicator;

	[SerializeField]
	private Transform changingRoomMenu;

	[SerializeField]
	private Transform outfitDrawerMenu;

	[Header("Logo and Maps")]
	public AnimateInOut Logo;

	public AnimateInOut Map3D;

	[Header("VO")]
	[SerializeField]
	private TutorialManager.VOClip welcomeVO;

	[SerializeField]
	private TutorialManager.VOClip thisIsYourDormVO;

	[SerializeField]
	private TutorialManager.VOClip letsGoOverBasicsVO;

	[SerializeField]
	private TutorialManager.VOClip useTeleportButtonVO;

	[SerializeField]
	private TutorialManager.VOClip useTeleportButtonHelpVO;

	[SerializeField]
	private TutorialManager.VOClip greatVO;

	[SerializeField]
	private TutorialManager.VOClip weHaveNeatStuffVO;

	[SerializeField]
	private TutorialManager.VOClip giveItaShotVO;

	[SerializeField]
	private TutorialManager.VOClip pickupToolHelpVO;

	[SerializeField]
	private TutorialManager.VOClip lockToolVO;

	[SerializeField]
	private TutorialManager.VOClip lockToolHelpVO;

	[SerializeField]
	private TutorialManager.VOClip unlockToolVO;

	[SerializeField]
	private TutorialManager.VOClip unlockToolHelpVO;

	[SerializeField]
	private TutorialManager.VOClip firstDayChecklistVO;

	[SerializeField]
	private TutorialManager.VOClip seeTheChecklistVO;

	[SerializeField]
	private TutorialManager.VOClip checkYourProgressVO;

	[SerializeField]
	private TutorialManager.VOClip closeMenuHelpVO;

	[SerializeField]
	private TutorialManager.VOClip customizeAvatarVO;

	[SerializeField]
	private TutorialManager.VOClip feelFreeToPlayAroundVO;

	[SerializeField]
	private TutorialManager.VOClip whenReadyTeleportThroughTheDoorVO;

	[SerializeField]
	private TutorialManager.VOClip useThumbstickToTurnAroundVO;

	[Header("Audio")]
	[SerializeField]
	private RecRoomAudioClip logoRezClip;

	[SerializeField]
	private AudioSource introAudio;

	private Tool[] tools;

	private void Start()
	{
		if (TutorialManager.OOBEState < TutorialManager.OOBEFlowState.Dormroom)
		{
			StartCoroutine(RunOOBEFlow());
		}
	}

	private IEnumerator RunOOBEFlow()
	{
		SetupDormForOOBE();
		yield return RunWaitForPlayerSpawn();
		Player player = Player.LocalPlayer;
		player.PlayerUI.MenuInteractionAllowed = false;
		yield return new WaitForSeconds(1.5f);
		AudioManager.Play3DSFX(logoRezClip, Map3D.transform.position);
		StartCoroutine(RunAnimateInOut(Logo, true));
		yield return RunAnimateInOut(Map3D, true);
		SingletonMonoBehaviour<TutorialManager>.Instance.PlayAttentionHelper(youAreHereIndicator.position, false);
		yield return new WaitForSeconds(1.75f);
		yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(welcomeVO);
		StartCoroutine(RunAnimateInOut(Map3D, false));
		yield return RunAnimateInOut(Logo, false);
		if (introAudio != null)
		{
			introAudio.Play();
		}
		yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(thisIsYourDormVO);
		yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(letsGoOverBasicsVO);
		VibrateBothHands(player);
		ShowNotes(true, false, false);
		userTeleported = false;
		player.PlayerEvents.TeleportEvent += base.PlayerEvents_TeleportEvent;
		yield return RunInterractionFlow(() => userTeleported, useTeleportButtonVO, null, useTeleportButtonHelpVO, 5f, 5, teleportNote.transform);
		player.PlayerEvents.TeleportEvent -= base.PlayerEvents_TeleportEvent;
		yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(greatVO);
		yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(weHaveNeatStuffVO);
		ShowNotes(false, true, false);
		VibrateBothHands(player);
		Tool[] array = interactionTools;
		foreach (Tool tool in array)
		{
			tool.gameObject.SetActive(true);
			SingletonMonoBehaviour<ToolCleanupManager>.Instance.RemoveTool(tool);
			yield return new WaitForSeconds(0.2f);
		}
		userPickedUpTool = false;
		player.PlayerEvents.ToolPickupEvent += base.PlayerEvents_ToolPickupEvent;
		yield return RunInterractionFlow(() => userPickedUpTool, giveItaShotVO, null, pickupToolHelpVO, 5f, 5, interactionTools[0].transform);
		player.PlayerEvents.ToolPickupEvent -= base.PlayerEvents_ToolPickupEvent;
		yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(greatVO);
		yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(firstDayChecklistVO);
		player.ObjectiveTracker.ClearStaleProgress(true);
		player.PlayerUI.MenuInteractionAllowed = true;
		VibrateBothHands(player);
		ShowNotes(false, false, true);
		yield return RunInterractionFlow(() => player.PlayerUI.Menu.Visible, seeTheChecklistVO, null, seeTheChecklistVO, 5f, 10, menuNote.transform);
		TutorialManager.OOBEState = TutorialManager.OOBEFlowState.Dormroom;
		yield return RunInterractionFlow(() => !player.PlayerUI.Menu.Visible, checkYourProgressVO, null, closeMenuHelpVO, 5f);
		changingRoomMenu.gameObject.SetActive(true);
		outfitDrawerMenu.gameObject.SetActive(true);
		ShowNotes(false, false, false);
		Tool[] array2 = interactionTools;
		foreach (Tool tool2 in array2)
		{
			tool2.gameObject.SetActive(false);
		}
		yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(feelFreeToPlayAroundVO);
		StartCoroutine(RunStopIntroAudio(8));
		yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(whenReadyTeleportThroughTheDoorVO);
		lockerRoomDoor.gameObject.SetActive(true);
		StartCoroutine(RunSpawnTools());
		yield return new WaitForSeconds(1f);
		SingletonMonoBehaviour<TutorialManager>.Instance.PlayAttentionHelper(doorHandle.position);
		if (PlatformManager.Instance.CurrentHardwareType == PlatformManager.HardwareType.OCULUS)
		{
			yield return SingletonMonoBehaviour<TutorialManager>.Instance.RunPlayVO(useThumbstickToTurnAroundVO);
		}
	}

	private IEnumerator RunStopIntroAudio(int duration)
	{
		if (introAudio != null)
		{
			yield return SingletonMonoBehaviour<AudioManager>.Instance.RunChangeAudioVolume(introAudio, introAudio.volume, 0f, duration);
			introAudio.Stop();
		}
	}

	private IEnumerator RunSpawnTools()
	{
		Tool[] array = tools;
		foreach (Tool tool in array)
		{
			tool.gameObject.SetActive(true);
			yield return new WaitForSeconds(0.15f);
		}
	}

	private void SetupDormForOOBE()
	{
		Map3D.gameObject.SetActive(false);
		Logo.gameObject.SetActive(false);
		if (tools == null)
		{
			tools = Object.FindObjectsOfType<Tool>();
		}
		Tool[] array = tools;
		foreach (Tool tool in array)
		{
			tool.gameObject.SetActive(false);
		}
		ShowNotes(false, false, false);
		Tool[] array2 = interactionTools;
		foreach (Tool tool2 in array2)
		{
			tool2.gameObject.SetActive(false);
		}
		lockerRoomDoor.gameObject.SetActive(false);
		changingRoomMenu.gameObject.SetActive(false);
		outfitDrawerMenu.gameObject.SetActive(false);
	}

	private void ShowNotes(bool teleport, bool grab, bool menu)
	{
		teleportNote.gameObject.SetActive(teleport);
		grabNote.gameObject.SetActive(grab);
		menuNote.gameObject.SetActive(menu);
	}
}
