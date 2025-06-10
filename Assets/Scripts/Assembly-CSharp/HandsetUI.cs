using UnityEngine;
using UnityEngine.UI;

public class HandsetUI : MonoBehaviour
{
	public Animator visualAnimator;

	public Text gameTime;

	public Text redTeamScore;

	public Text blueTeamScore;

	private float showDelay = 0.5f;

	private float showTimer;

	private LaserTeleporter laserTeleporter;

	private GameManager gameManager;

	private bool showing;

	public void Start()
	{
		gameManager = RecRoomSceneManager.Instance.GameManager;
		showTimer = 0f;
		laserTeleporter = GetComponent<LaserTeleporter>();
		if (laserTeleporter.Player.isLocal && gameManager != null)
		{
			gameManager.StatsManager.StatsChangeEvent += OnScoreChange;
			base.enabled = true;
			Reset();
		}
		else
		{
			base.enabled = false;
		}
	}

	private void OnScoreChange()
	{
		GameTeam[] allTeams = gameManager.TeamManager.GetAllTeams();
		if (allTeams != null && allTeams.Length > 0)
		{
			GameTeam[] array = allTeams;
			foreach (GameTeam gameTeam in array)
			{
				int score = gameManager.StatsManager.GetScore(gameTeam);
				if (gameTeam == GameTeam.TEAM_1)
				{
					SetBlueTeamScore(score);
				}
				else
				{
					SetRedTeamScore(score);
				}
			}
		}
		else
		{
			Reset();
		}
	}

	private void Update()
	{
		if (!showing && laserTeleporter.TeleportLaserVisible)
		{
			showTimer += Time.deltaTime;
			if (showTimer > showDelay)
			{
				if (visualAnimator != null)
				{
					visualAnimator.SetTrigger("Show");
				}
				showing = true;
			}
		}
		else if (!laserTeleporter.TeleportLaserVisible)
		{
			showing = false;
			showTimer = 0f;
		}
		if (showing)
		{
			SetTimer(gameManager.GameTimerManager.TimeRemaining);
		}
	}

	private void Reset()
	{
		SetRedTeamScore(0);
		SetBlueTeamScore(0);
		SetTimer(0f);
	}

	public void SetTimer(float time)
	{
		if (gameTime != null)
		{
			gameTime.text = time.ToTimeString();
		}
	}

	public void SetRedTeamScore(int score)
	{
		if (redTeamScore != null)
		{
			redTeamScore.text = score.ToString();
		}
	}

	public void SetBlueTeamScore(int score)
	{
		if (blueTeamScore != null)
		{
			blueTeamScore.text = score.ToString();
		}
	}
}
