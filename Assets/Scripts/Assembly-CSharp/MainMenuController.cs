public class MainMenuController : MenuController
{
	public void ButtonPress_Play()
	{
		if (gameManager != null)
		{
			gameManager.LocalPlayerStartGame();
		}
	}

	public void ButtonPress_Replay()
	{
	}

	public void ButtonPress_Restart()
	{
		if (gameManager != null)
		{
			gameManager.LocalPlayerStopGame();
		}
	}
}
