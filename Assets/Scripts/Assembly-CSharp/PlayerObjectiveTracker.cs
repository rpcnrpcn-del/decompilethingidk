using System;
using System.Collections.Generic;
using RecNet;
using UnityEngine;

public class PlayerObjectiveTracker : MonoBehaviour
{
	private const string OBJECTIVE_DATE_KEY = "OBJECTIVE_DATE";

	private const string OBJECTIVE_PROGRESS_KEY = "OBJECTIVE_PROGRESS";

	private const string OBJECTIVE_COMPLETED_KEY = "OBJECTIVE_COMPLETED";

	private Player thisPlayer;

	private GameManager gameManager;

	private List<ProgressionManager.ObjectiveType> eventBuffer = new List<ProgressionManager.ObjectiveType>();

	private int debugDateIncrement;

	public static event Action AllObjectivesCompleted;

	public static event Action<List<ProgressionManager.ObjectiveType>> EventBufferUpdated;

	public event Action ObjectiveProgressUpdateEvent;

	private void Awake()
	{
		thisPlayer = GetComponent<Player>();
		if (thisPlayer.isLocal)
		{
			thisPlayer.PlayerEvents.GameOverEvent += OnGameOver;
			thisPlayer.PlayerEvents.DodgeballOutEvent += OnDodgeballHit;
			thisPlayer.PlayerEvents.PaddleballScoreEvent += OnPaddleballScore;
			thisPlayer.PlayerEvents.DiscGolfCompletedHoleEvent += OnDiscGolfCompletedHole;
			thisPlayer.PlayerEvents.PaintballFlagCaptureEvent += OnPaintballFlagCapture;
			thisPlayer.PlayerEvents.PaintballHitEvent += OnPaintballHit;
			thisPlayer.PlayerEvents.SoccerGoalEvent += OnSoccerGoal;
		}
	}

	private void Start()
	{
		gameManager = RecRoomSceneManager.Instance.GameManager;
	}

	private void OnDestroy()
	{
		thisPlayer.PlayerEvents.GameOverEvent -= OnGameOver;
		thisPlayer.PlayerEvents.DodgeballOutEvent -= OnDodgeballHit;
		thisPlayer.PlayerEvents.PaddleballScoreEvent -= OnPaddleballScore;
		thisPlayer.PlayerEvents.DiscGolfCompletedHoleEvent -= OnDiscGolfCompletedHole;
		thisPlayer.PlayerEvents.PaintballFlagCaptureEvent -= OnPaintballFlagCapture;
		thisPlayer.PlayerEvents.PaintballHitEvent -= OnPaintballHit;
		thisPlayer.PlayerEvents.SoccerGoalEvent -= OnSoccerGoal;
	}

	public void GetObjectiveDescription(int objectiveIndex, out ProgressionManager.ObjectiveTypeDescription description, out string descriptionText)
	{
		description = null;
		descriptionText = null;
		ProgressionManager.Objective todaysObjective = GetTodaysObjective(objectiveIndex);
		if (todaysObjective != null)
		{
			description = SingletonMonoBehaviour<ProgressionManager>.Instance.GetObjectiveDescription(todaysObjective.ObjectiveType);
			if (description != null)
			{
				descriptionText = string.Format(description.Description, todaysObjective.RequiredScore);
			}
		}
	}

	public bool GetObjectiveProgress(int objectiveIndex, out int progress, out int total)
	{
		ClearStaleProgress();
		bool result = false;
		progress = 0;
		total = 0;
		ProgressionManager.Objective todaysObjective = GetTodaysObjective(objectiveIndex);
		if (todaysObjective != null)
		{
			total = todaysObjective.RequiredScore;
			if (GetCompleted(objectiveIndex))
			{
				result = true;
				progress = total;
			}
			else
			{
				result = false;
				progress = GetProgress(objectiveIndex);
			}
		}
		return result;
	}

	public void CompleteObjective(ProgressionManager.ObjectiveType objective)
	{
		eventBuffer.Clear();
		eventBuffer.Add(objective);
		UpdateObjectiveProgress();
	}

	public bool GetAllObjectivesCompleted()
	{
		ProgressionManager.Objective[] todaysObjectives = GetTodaysObjectives();
		if (todaysObjectives != null)
		{
			for (int i = 0; i < todaysObjectives.Length; i++)
			{
				if (!GetCompleted(i))
				{
					return false;
				}
			}
		}
		return true;
	}

	private void OnGameOver(bool won)
	{
		eventBuffer.Clear();
		if (RecRoomSceneManager.CurrentSceneName == "dodgeball")
		{
			eventBuffer.Add(ProgressionManager.ObjectiveType.DodgeballGames);
			if (won)
			{
				eventBuffer.Add(ProgressionManager.ObjectiveType.DodgeballWins);
			}
		}
		else if (RecRoomSceneManager.CurrentSceneName == "soccer")
		{
			eventBuffer.Add(ProgressionManager.ObjectiveType.SoccerGames);
			if (won)
			{
				eventBuffer.Add(ProgressionManager.ObjectiveType.SoccerWins);
			}
		}
		else if (RecRoomSceneManager.CurrentSceneName == "charades")
		{
			eventBuffer.Add(ProgressionManager.ObjectiveType.CharadesGames);
			if (won)
			{
				eventBuffer.Add(ProgressionManager.ObjectiveType.CharadesWinsPerformer);
			}
		}
		else if (RecRoomSceneManager.CurrentSceneName == "paddleball")
		{
			eventBuffer.Add(ProgressionManager.ObjectiveType.PaddleballGames);
			if (won)
			{
				eventBuffer.Add(ProgressionManager.ObjectiveType.PaddleballWins);
			}
		}
		else if (RecRoomSceneManager.CurrentSceneFriendlyName.StartsWith("Disc Golf", StringComparison.InvariantCultureIgnoreCase))
		{
			eventBuffer.Add(ProgressionManager.ObjectiveType.DiscGolfGames);
			if (won)
			{
				eventBuffer.Add(ProgressionManager.ObjectiveType.DiscGolfWins);
			}
		}
		else if (RecRoomSceneManager.CurrentSceneFriendlyName.StartsWith("Paintball", StringComparison.InvariantCultureIgnoreCase) && gameManager != null)
		{
			if (gameManager.ModeManager.GetMode() == GameMode.MODE_1)
			{
				eventBuffer.Add(ProgressionManager.ObjectiveType.PaintballCTFGames);
				if (won)
				{
					eventBuffer.Add(ProgressionManager.ObjectiveType.PaintballCTFWins);
				}
			}
			else
			{
				eventBuffer.Add(ProgressionManager.ObjectiveType.PaintballTeamBattleGames);
				if (won)
				{
					eventBuffer.Add(ProgressionManager.ObjectiveType.PaintballTeamBattleWins);
				}
			}
			eventBuffer.Add(ProgressionManager.ObjectiveType.PaintballAnyModeGames);
			if (won)
			{
				eventBuffer.Add(ProgressionManager.ObjectiveType.PaintballAnyModeWins);
			}
		}
		UpdateObjectiveProgress();
	}

	private void OnDodgeballHit()
	{
		eventBuffer.Clear();
		eventBuffer.Add(ProgressionManager.ObjectiveType.DodgeballHits);
		UpdateObjectiveProgress();
	}

	private void OnPaddleballScore()
	{
		eventBuffer.Clear();
		eventBuffer.Add(ProgressionManager.ObjectiveType.PaddleballScores);
		UpdateObjectiveProgress();
	}

	private void OnDiscGolfCompletedHole(int score)
	{
		eventBuffer.Clear();
		if (score < 0)
		{
			eventBuffer.Add(ProgressionManager.ObjectiveType.DiscGolfHolesUnderPar);
		}
		UpdateObjectiveProgress();
	}

	private void OnPaintballFlagCapture()
	{
		eventBuffer.Clear();
		eventBuffer.Add(ProgressionManager.ObjectiveType.PaintballFlagCaptures);
		UpdateObjectiveProgress();
	}

	private void OnPaintballHit()
	{
		eventBuffer.Clear();
		eventBuffer.Add(ProgressionManager.ObjectiveType.PaintballAnyModeHits);
		if (gameManager != null)
		{
			if (gameManager.ModeManager.GetMode() == GameMode.MODE_1)
			{
				eventBuffer.Add(ProgressionManager.ObjectiveType.PaintballCTFHits);
			}
			else
			{
				eventBuffer.Add(ProgressionManager.ObjectiveType.PaintballTeamBattleHits);
			}
		}
		UpdateObjectiveProgress();
	}

	private void OnSoccerGoal()
	{
		eventBuffer.Clear();
		eventBuffer.Add(ProgressionManager.ObjectiveType.SoccerGoals);
		UpdateObjectiveProgress();
	}

	private void UpdateObjectiveProgress()
	{
		ClearStaleProgress();
		if (eventBuffer.Count > 0 && PlayerObjectiveTracker.EventBufferUpdated != null)
		{
			PlayerObjectiveTracker.EventBufferUpdated(eventBuffer);
		}
		if (GetAllObjectivesCompleted())
		{
			return;
		}
		bool flag = false;
		ProgressionManager.Objective[] todaysObjectives = GetTodaysObjectives();
		for (int i = 0; i < todaysObjectives.Length; i++)
		{
			if (GetCompleted(i))
			{
				continue;
			}
			foreach (ProgressionManager.ObjectiveType item in eventBuffer)
			{
				if (todaysObjectives[i].ObjectiveType != item)
				{
					continue;
				}
				AddProgress(i, 1);
				flag = true;
				if (GetProgress(i) >= todaysObjectives[i].RequiredScore)
				{
					CompleteObjective(i);
					if (SingletonMonoBehaviour<TutorialManager>.Instance.IsOOBERunning)
					{
						SingletonMonoBehaviour<ProgressionManager>.Instance.LocalCompleteObjective(todaysObjectives[i].ObjectiveType);
					}
					else
					{
						SingletonMonoBehaviour<ProgressionManager>.Instance.LocalCompleteObjective(GetDailyObjectiveType(i));
					}
				}
			}
		}
		if (this.ObjectiveProgressUpdateEvent != null && flag)
		{
			this.ObjectiveProgressUpdateEvent();
		}
	}

	private ProgressionManager.ObjectiveType GetDailyObjectiveType(int index)
	{
		switch (index)
		{
		case 0:
			return ProgressionManager.ObjectiveType.DailyObjective1;
		case 1:
			return ProgressionManager.ObjectiveType.DailyObjective2;
		case 2:
			return ProgressionManager.ObjectiveType.DailyObjective3;
		default:
			return ProgressionManager.ObjectiveType.Default;
		}
	}

	private ProgressionManager.Objective GetTodaysObjective(int index)
	{
		ProgressionManager.Objective[] todaysObjectives = GetTodaysObjectives();
		if (todaysObjectives == null || index < 0 || index >= todaysObjectives.Length)
		{
			return null;
		}
		return todaysObjectives[index];
	}

	private ProgressionManager.Objective[] GetTodaysObjectives()
	{
		if (SingletonMonoBehaviour<TutorialManager>.Instance.IsOOBERunning)
		{
			return SingletonMonoBehaviour<TutorialManager>.Instance.OOBEObjectives;
		}
		DateTime today = DateTime.Today;
		return Config.DailyObjectives[(int)today.DayOfWeek];
	}

	public void ClearStaleProgress(bool forciblyClear = false)
	{
		int currentDate = GetCurrentDate();
		int lastRecordedObjectiveDate = GetLastRecordedObjectiveDate();
		if (currentDate != lastRecordedObjectiveDate || forciblyClear)
		{
			SetLastRecordedObjectiveDate(currentDate);
			SetProgress(0, 0);
			SetCompleted(0, false);
			SetProgress(1, 0);
			SetCompleted(1, false);
			SetProgress(2, 0);
			SetCompleted(2, false);
		}
	}

	private int GetCurrentDate()
	{
		return DateTime.Today.DayOfYear;
	}

	private int GetLastRecordedObjectiveDate()
	{
		return RecroomPrefs.GetInt("OBJECTIVE_DATE", 0);
	}

	private void SetLastRecordedObjectiveDate(int date)
	{
		RecroomPrefs.SetInt("OBJECTIVE_DATE", date);
		RecroomPrefs.Save();
	}

	private void AddProgress(int objectiveIndex, int delta)
	{
		int progress = GetProgress(objectiveIndex);
		SetProgress(objectiveIndex, progress + delta);
	}

	private void SetProgress(int objectiveIndex, int value)
	{
		RecroomPrefs.SetInt(GetObjectiveProgressKey(objectiveIndex), value);
		RecroomPrefs.Save();
	}

	private int GetProgress(int objectiveIndex)
	{
		return RecroomPrefs.GetInt(GetObjectiveProgressKey(objectiveIndex), 0);
	}

	private string GetObjectiveProgressKey(int objectiveIndex)
	{
		return "OBJECTIVE_PROGRESS" + objectiveIndex;
	}

	private void CompleteObjective(int objectiveIndex)
	{
		ProgressionManager.Objective todaysObjective = GetTodaysObjective(objectiveIndex);
		SetCompleted(objectiveIndex, true);
		ProgressionManager.ObjectiveTypeDescription objectiveDescription = SingletonMonoBehaviour<ProgressionManager>.Instance.GetObjectiveDescription(todaysObjective.ObjectiveType);
		if (objectiveDescription == null)
		{
			return;
		}
		if (GetAllObjectivesCompleted())
		{
			if (SingletonMonoBehaviour<TutorialManager>.Instance.IsOOBERunning)
			{
				ScreenSpaceNotificationManager.Instance.PlayDelayed(ScreenSpaceNotificationManager.NotificationType.Vital, "Checklist Complete!", 3f, objectiveDescription.NotificationDelay, OnNotificationPlaying);
				SingletonMonoBehaviour<TutorialManager>.Instance.OOBEObjectivesCompleted();
				ClearStaleProgress(true);
			}
			else
			{
				ScreenSpaceNotificationManager.Instance.PlayDelayed(ScreenSpaceNotificationManager.NotificationType.Vital, "All Daily Challenges Complete!", 3f, objectiveDescription.NotificationDelay, OnNotificationPlaying);
			}
			if (PlayerObjectiveTracker.AllObjectivesCompleted != null)
			{
				PlayerObjectiveTracker.AllObjectivesCompleted();
			}
		}
		else
		{
			string subtitleText = string.Format(objectiveDescription.Description, todaysObjective.RequiredScore);
			if (SingletonMonoBehaviour<TutorialManager>.Instance.IsOOBERunning)
			{
				ScreenSpaceNotificationManager.Instance.PlayDelayed(ScreenSpaceNotificationManager.NotificationType.Vital, "Checklist Item Complete!", subtitleText, 3f, objectiveDescription.NotificationDelay, OnNotificationPlaying);
			}
			else
			{
				ScreenSpaceNotificationManager.Instance.PlayDelayed(ScreenSpaceNotificationManager.NotificationType.Vital, "Daily Challenge Complete!", subtitleText, 3f, objectiveDescription.NotificationDelay, OnNotificationPlaying);
			}
		}
	}

	private void SetCompleted(int objectiveIndex, bool completed)
	{
		RecroomPrefs.SetInt(GetObjectiveCompletedKey(objectiveIndex), completed ? 1 : 0);
		RecroomPrefs.Save();
	}

	private bool GetCompleted(int objectiveIndex)
	{
		return RecroomPrefs.GetInt(GetObjectiveCompletedKey(objectiveIndex), 0) != 0;
	}

	private string GetObjectiveCompletedKey(int objectiveIndex)
	{
		return "OBJECTIVE_COMPLETED" + objectiveIndex;
	}

	private void OnNotificationPlaying()
	{
		thisPlayer.PlayerAudio.OnObjectiveComplete();
	}
}
