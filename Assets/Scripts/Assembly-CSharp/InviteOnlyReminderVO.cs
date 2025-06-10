using System.Collections;
using UnityEngine;

public class InviteOnlyReminderVO : MonoBehaviour
{
	private IEnumerator Start()
	{
		if (PhotonNetwork.otherPlayers.Length != 0)
		{
			yield break;
		}
		yield return new WaitForSeconds(4f);
		if (PhotonNetwork.otherPlayers.Length != 0)
		{
			yield break;
		}
		PlayWaitingForPlayersOneFeedback();
		yield return new WaitForSeconds(8f);
		if (PhotonNetwork.otherPlayers.Length == 0)
		{
			PlayWaitingForPlayersTwoFeedback();
			yield return new WaitForSeconds(60f);
			while (PhotonNetwork.otherPlayers.Length == 0)
			{
				PlayWaitingForPlayersThreeFeedback();
				yield return new WaitForSeconds(3f);
				PlayWaitingForPlayersFourFeedback();
				yield return new WaitForSeconds(60f);
			}
		}
	}

	private void PlayWaitingForPlayersOneFeedback()
	{
		RecRoomSceneManager.Instance.FxManager.PlaySFX((!PUNNetworkManager.Instance.IsActivityInviteOnly) ? FxType.WAITING_FOR_PLAYERS_ONE : FxType.WAITING_FOR_PLAYERS_ONE_INVITE_ONLY);
	}

	private void PlayWaitingForPlayersTwoFeedback()
	{
		RecRoomSceneManager.Instance.FxManager.PlaySFX((!PUNNetworkManager.Instance.IsActivityInviteOnly) ? FxType.WAITING_FOR_PLAYERS_TWO : FxType.WAITING_FOR_PLAYERS_TWO_INVITE_ONLY);
	}

	private void PlayWaitingForPlayersThreeFeedback()
	{
		RecRoomSceneManager.Instance.FxManager.PlaySFX((!PUNNetworkManager.Instance.IsActivityInviteOnly) ? FxType.WAITING_FOR_PLAYERS_THREE : FxType.WAITING_FOR_PLAYERS_THREE_INVITE_ONLY);
	}

	private void PlayWaitingForPlayersFourFeedback()
	{
		RecRoomSceneManager.Instance.FxManager.PlaySFX((!PUNNetworkManager.Instance.IsActivityInviteOnly) ? FxType.WAITING_FOR_PLAYERS_FOUR : FxType.WAITING_FOR_PLAYERS_FOUR_INVITE_ONLY);
	}
}
