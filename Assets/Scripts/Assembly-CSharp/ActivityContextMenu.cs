using UnityEngine;
using UnityEngine.UI;

public class ActivityContextMenu : MonoBehaviour
{
	private GameManager gameManager;

	public Button PlayButton;

	public Button ReplayButton;

	public Button RestartButton;

	private void Start()
	{
		gameManager = RecRoomSceneManager.Instance.GameManager;
		if (gameManager != null)
		{
			gameManager.StateChangeEvent += OnGameStateChanged;
			gameManager.TeamManager.TeamChangeEvent += OnTeamChange;
		}
	}

	private void OnDestroy()
	{
		if (gameManager != null)
		{
			gameManager.StateChangeEvent -= OnGameStateChanged;
			gameManager.TeamManager.TeamChangeEvent -= OnTeamChange;
		}
	}

	private void OnEnable()
	{
		UpdateButtons();
	}

	private void OnGameStateChanged(ushort currentStateId, ushort previousStateId, ushort currentSubStateId, ushort previousSubStateId)
	{
		UpdateButtons();
	}

	private void OnTeamChange()
	{
		UpdateButtons();
	}

	private void UpdateButtons()
	{
		bool flag = gameManager != null;
		bool interactable = flag && gameManager.ReadyToPlay;
		PlayButton.gameObject.SetActive(flag && gameManager.CurrentState == GameStates.PRE_GAME);
		PlayButton.interactable = interactable;
		ReplayButton.gameObject.SetActive(false);
		RestartButton.gameObject.SetActive(flag && gameManager.CurrentState == GameStates.GAME_RUNNING);
		RestartButton.interactable = interactable;
	}
}
