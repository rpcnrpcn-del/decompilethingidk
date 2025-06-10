using UnityEngine;
using UnityEngine.UI;

public class JumbotronPlayerEntry : MonoBehaviour
{
	private const string WAITING_FOR_PLAYER_STRING = "Waiting for player...";

	[SerializeField]
	private Text playerName;

	[SerializeField]
	private Image background;

	public void Show(PhotonPlayer player, GameTeam team)
	{
		playerName.gameObject.SetActive(true);
		playerName.text = player.name;
		playerName.alignment = TextAnchor.MiddleLeft;
		background.gameObject.SetActive(true);
		background.color = GameTeamSettings.GetTeamColor(team);
	}

	public void Hide()
	{
		playerName.gameObject.SetActive(false);
		background.gameObject.SetActive(false);
	}

	public void ShowWaiting(GameTeam team)
	{
		playerName.gameObject.SetActive(true);
		playerName.text = "Waiting for player...";
		playerName.alignment = TextAnchor.MiddleCenter;
		background.gameObject.SetActive(true);
		background.color = GameTeamSettings.GetTeamColor(team);
	}
}
