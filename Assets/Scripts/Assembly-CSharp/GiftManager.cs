using System.Collections;
using GAMiniJSON;
using RecNet;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GiftManager : SingletonMonoBehaviour<GiftManager>
{
	public enum DebugGiftType
	{
		Random = 0,
		Item = 1,
		XP = 2
	}

	public delegate void GiftConsumedCallback(bool successful);

	private const string GIFT_DATE_KEY = "GIFT_DATE";

	private const string GIFT_COUNT_KEY = "GIFT_COUNT";

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("After each game you have some chance of getting a gift. This value will halve after each gift.")]
	private float perActivityGiftChance = 0.33f;

	[SerializeField]
	private int giftXP = 100;

	private Avatars.GiftPackage currentGift;

	private Coroutine receiveGiftsCoroutine;

	private GameObject lastGiftBoxObject;

	private bool LocalPlayerCloseToGiftSpawnPosition
	{
		get
		{
			return Player.LocalPlayer != null && (Player.LocalPlayer.Head.transform.position - Player.LocalPlayer.LastSpawnPosition).magnitude <= 2f;
		}
	}

	public int GiftCountToday
	{
		get
		{
			if (RecroomPrefs.CurrentDay == RecroomPrefs.GetInt("GIFT_DATE", 0))
			{
				return RecroomPrefs.GetInt("GIFT_COUNT", 0);
			}
			return 0;
		}
		private set
		{
			RecroomPrefs.SetInt("GIFT_DATE", RecroomPrefs.CurrentDay);
			RecroomPrefs.SetInt("GIFT_COUNT", value);
			RecroomPrefs.Save();
		}
	}

	protected void Awake()
	{
		SingletonMonoBehaviour<GiftManager>.Instance = this;
	}

	public void Initialize()
	{
		SceneManager.sceneLoaded += SceneManager_sceneLoaded;
		SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
		PlayerObjectiveTracker.AllObjectivesCompleted += PlayerObjectiveTracker_AllObjectivesCompleted;
		float result;
		if (float.TryParse(Config.GetConfigSetting("Gift.DropChance"), out result))
		{
			perActivityGiftChance = result;
		}
		int result2;
		if (int.TryParse(Config.GetConfigSetting("Gift.XP"), out result2))
		{
			giftXP = result2;
		}
	}

	private void PlayerObjectiveTracker_AllObjectivesCompleted()
	{
		StartCoroutine(RunGenerateGift());
	}

	private void SceneManager_sceneUnloaded(Scene scene)
	{
		StopReceiveGiftsCoroutine();
	}

	public void StartReceiveGiftsCoroutine()
	{
		StopReceiveGiftsCoroutine();
		receiveGiftsCoroutine = StartCoroutine(ReceiveGiftsFromQueue());
	}

	private void StopReceiveGiftsCoroutine()
	{
		if (receiveGiftsCoroutine != null)
		{
			StopCoroutine(receiveGiftsCoroutine);
			receiveGiftsCoroutine = null;
		}
		if (lastGiftBoxObject != null)
		{
			PhotonNetwork.Destroy(lastGiftBoxObject);
		}
	}

	private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (RecRoomSceneManager.Instance != null && RecRoomSceneManager.Instance.GameManager != null)
		{
			RecRoomSceneManager.Instance.GameManager.StateChangeEvent += GameManager_StateChangeEvent;
		}
		if (PUNNetworkManager.Instance != null && (PUNNetworkManager.Instance.IsInDormRoom || PUNNetworkManager.Instance.IsInLockerRoom))
		{
			StartReceiveGiftsCoroutine();
		}
	}

	private void GameManager_StateChangeEvent(ushort currentStateId, ushort previousStateId, ushort currentSubStateId, ushort previousSubStateId)
	{
		GameManager gameManager = RecRoomSceneManager.Instance.GameManager;
		if (!(gameManager != null))
		{
			return;
		}
		if (gameManager.CurrentState == GameStates.POST_GAME)
		{
			if (IsRandomDropGift())
			{
				StartCoroutine(RunGenerateGift());
			}
		}
		else if (gameManager.CurrentState == GameStates.GAME_RUNNING)
		{
			StopReceiveGiftsCoroutine();
		}
	}

	public IEnumerator RunGenerateGift(DebugGiftType giftType = DebugGiftType.Random)
	{
		OutfitSelection selection = null;
		float itemDropChance = 1f - (float)Mathf.Min(GiftCountToday, 3) * 0.25f;
		if (itemDropChance >= Random.value)
		{
			selection = OutfitManager.Instance.CreateRandomOutfitSelection(Player.LocalPlayer.PlayerProgression.Level + 1);
		}
		else if (Profiles.LocalProfile.XpRequiredToLevelUp <= 0)
		{
			yield break;
		}
		int xp = ((!(selection != null)) ? giftXP : (giftXP / 10));
		Avatars.GiftPackage giftPackage = null;
		yield return Avatars.LocalCreateGiftPackage((!(selection != null)) ? string.Empty : selection.ToString(), xp, delegate(string e, Avatars.GiftPackage gift)
		{
			giftPackage = gift;
		});
		if (giftPackage != null)
		{
			GiftCountToday++;
		}
	}

	private IEnumerator ReceiveGiftsFromQueue()
	{
		while (Player.LocalPlayer == null || Player.LocalPlayer.IsSpawning)
		{
			yield return null;
		}
		Vector3 lastSpawnPosition = Player.LocalPlayer.LastSpawnPosition;
		Vector3 giftPosition = lastSpawnPosition + Player.LocalPlayer.LastSpawnRotation * Vector3.forward * 0.6f;
		Quaternion giftRotation = Quaternion.LookRotation(giftPosition - lastSpawnPosition);
		while (Player.LocalPlayer != null && Avatars.GiftPackages.Count > 0 && LocalPlayerCloseToGiftSpawnPosition)
		{
			yield return DequeueGift(giftPosition, giftRotation);
		}
	}

	private IEnumerator DequeueGift(Vector3 boxPosition, Quaternion boxRotation)
	{
		yield return new WaitForSeconds(1f);
		currentGift = Avatars.GiftPackages[0];
		object[] instantiationData = new object[2]
		{
			Player.LocalPlayer.PhotonPlayer,
			Json.Serialize(currentGift.Serialize())
		};
		lastGiftBoxObject = PhotonNetwork.Instantiate("[GiftBox]", boxPosition, boxRotation, 0, instantiationData);
		while (lastGiftBoxObject != null && LocalPlayerCloseToGiftSpawnPosition)
		{
			yield return null;
		}
		if (lastGiftBoxObject != null)
		{
			PhotonNetwork.Destroy(lastGiftBoxObject);
		}
	}

	public IEnumerator RunConsumeGift(Avatars.GiftPackage giftPackage, GiftConsumedCallback callback)
	{
		if (giftPackage != null && !giftPackage.Consumed)
		{
			OutfitSelection selection = OutfitSelection.Parse(giftPackage.AvatarItemDesc);
			string error = string.Empty;
			yield return Avatars.LocalConsumeGiftPackage(giftPackage, (selection != null) ? selection.Level : 0, delegate(string e)
			{
				error = e;
			});
			callback(string.IsNullOrEmpty(error));
			if (!string.IsNullOrEmpty(error))
			{
				Debug.LogError("Server failed to consume gift " + giftPackage.Id + " : " + error);
			}
			else if (OutfitManager.Instance != null && giftPackage.HasAvatarItem)
			{
				OutfitManager.Instance.AddAvatarItemToUnlockedList(giftPackage.AvatarItemDesc);
			}
		}
		else
		{
			callback(false);
			Debug.LogError("Unable to consume missing or used gift.");
		}
	}

	private bool IsRandomDropGift()
	{
		float value = perActivityGiftChance / Mathf.Pow(2f, GiftCountToday - 1);
		value = Mathf.Clamp(value, 0.01f, 1f);
		return value >= Random.value;
	}
}
