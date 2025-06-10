using System;
using System.Collections;
using System.Collections.Generic;
using RecNet;
using UnityEngine;

public class ProgressionManager : SingletonMonoBehaviour<ProgressionManager>
{
	public enum ObjectiveType
	{
		Default = -1,
		FirstSessionOfDay = 1,
		DailyObjective1 = 10,
		DailyObjective2 = 11,
		DailyObjective3 = 12,
		OOBE_OpenMenu = 20,
		OOBE_GoToLockerRoom = 21,
		OOBE_GoToActivity = 22,
		CharadesGames = 100,
		CharadesWinsPerformer = 101,
		CharadesWinsGuesser = 102,
		DiscGolfWins = 200,
		DiscGolfGames = 201,
		DiscGolfHolesUnderPar = 202,
		DodgeballWins = 300,
		DodgeballGames = 301,
		DodgeballHits = 302,
		PaddleballGames = 400,
		PaddleballWins = 401,
		PaddleballScores = 402,
		PaintballAnyModeGames = 500,
		PaintballAnyModeWins = 501,
		PaintballAnyModeHits = 502,
		PaintballCTFWins = 600,
		PaintballCTFGames = 601,
		PaintballCTFHits = 602,
		PaintballFlagCaptures = 603,
		PaintballTeamBattleWins = 700,
		PaintballTeamBattleGames = 701,
		PaintballTeamBattleHits = 702,
		SoccerWins = 800,
		SoccerGames = 801,
		SoccerGoals = 802
	}

	[Serializable]
	public class ObjectiveTypeDescription
	{
		public string Name;

		public ObjectiveType ObjectiveType;

		public string Description;

		public string ToolTip;

		public float NotificationDelay;
	}

	[Serializable]
	public class Objective
	{
		public ObjectiveType ObjectiveType = ObjectiveType.Default;

		public int RequiredScore;
	}

	[Serializable]
	public class RangeBucket
	{
		public string Name;

		[Range(0f, 1f)]
		public float Multiplier = 1f;

		public int Max = 100;
	}

	[Header("Objectives")]
	[SerializeField]
	private ObjectiveTypeDescription[] objectiveTypeDescriptions;

	private Dictionary<ObjectiveType, ObjectiveTypeDescription> objectiveDescriptionMap = new Dictionary<ObjectiveType, ObjectiveTypeDescription>();

	private const string DAILY_LOGIN_KEY = "DAILY_LOGIN_DATE";

	public float LocalXPProgression
	{
		get
		{
			int xpRequiredToLevelUp = Profiles.LocalProfile.XpRequiredToLevelUp;
			if (xpRequiredToLevelUp <= 0)
			{
				return 1f;
			}
			return (float)Profiles.LocalProfile.XP / (float)xpRequiredToLevelUp;
		}
	}

	public bool IsFirstDailyLogin
	{
		get
		{
			return RecroomPrefs.CurrentDay != RecroomPrefs.GetInt("DAILY_LOGIN_DATE", 0);
		}
		private set
		{
			RecroomPrefs.SetInt("DAILY_LOGIN_DATE", RecroomPrefs.CurrentDay + (value ? (-1) : 0));
			RecroomPrefs.Save();
		}
	}

	private void Awake()
	{
		SingletonMonoBehaviour<ProgressionManager>.Instance = this;
		PlayerObjectiveTracker.EventBufferUpdated += PlayerObjectiveTracker_EventBufferUpdated;
	}

	public void Initialize()
	{
		ObjectiveTypeDescription[] array = objectiveTypeDescriptions;
		foreach (ObjectiveTypeDescription objectiveTypeDescription in array)
		{
			if (!objectiveDescriptionMap.ContainsKey(objectiveTypeDescription.ObjectiveType))
			{
				objectiveDescriptionMap.Add(objectiveTypeDescription.ObjectiveType, objectiveTypeDescription);
			}
			else
			{
				Debug.LogError("objectiveTypeDescriptions has duplicate entries for " + objectiveTypeDescription.ObjectiveType);
			}
		}
		StartCoroutine(RunHandleInitialLogin());
	}

	private IEnumerator RunHandleInitialLogin()
	{
		while (Player.LocalPlayer == null || Player.LocalPlayer.IsSpawning)
		{
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		if (IsFirstDailyLogin)
		{
			LocalCompleteObjective(ObjectiveType.FirstSessionOfDay);
			IsFirstDailyLogin = false;
			if (SingletonMonoBehaviour<TutorialManager>.Instance != null && TutorialManager.OOBEState == TutorialManager.OOBEFlowState.Complete)
			{
				yield return SingletonMonoBehaviour<GiftManager>.Instance.RunGenerateGift();
			}
		}
		else
		{
			LocalCompleteObjective(ObjectiveType.Default, 1);
		}
		yield return new WaitForSeconds(1f);
		AnalyticsHelper.InitialPlayerLogin(Player.LocalPlayer.CurrentFloorHeightFromHead);
	}

	public void LocalCompleteObjective(ObjectiveType type, int additionalXp = 0)
	{
		if (type != ObjectiveType.Default || additionalXp > 0)
		{
			Profiles.LocalCompleteObjective(type, additionalXp, Player.LocalPlayer.PlayerParty.PartySize > 1);
		}
	}

	public ObjectiveTypeDescription GetObjectiveDescription(ObjectiveType objectiveType)
	{
		ObjectiveTypeDescription value;
		objectiveDescriptionMap.TryGetValue(objectiveType, out value);
		return value;
	}

	private void PlayerObjectiveTracker_EventBufferUpdated(List<ObjectiveType> eventBuffer)
	{
		foreach (ObjectiveType item in eventBuffer)
		{
			ObjectiveTypeDescription objectiveDescription = GetObjectiveDescription(item);
			if (objectiveDescription != null)
			{
				LocalCompleteObjective(objectiveDescription.ObjectiveType);
			}
		}
	}
}
