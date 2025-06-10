using System;
using System.Collections;
using UnityEngine;

public class TutorialManager : SingletonMonoBehaviour<TutorialManager>
{
	[Serializable]
	public class VOClip
	{
		public RecRoomAudioClip recClip;

		public float preDuration;

		public float postDuration;
	}

	public enum OOBEFlowState
	{
		NotRan = 0,
		Dormroom = 10,
		Lockerroom = 20,
		Complete = 100
	}

	[Header("OOBE")]
	[SerializeField]
	public ProgressionManager.Objective[] OOBEObjectives;

	[Header("Audio")]
	[SerializeField]
	private AudioSource voAudioSource;

	[Header("Attention Helper")]
	[SerializeField]
	private PooledParticle attentionHelperVFXPrefab;

	[SerializeField]
	private RecRoomAudioClip attentionHelperAudio;

	public const string OOBEFLOW_PREF = "Recroom.OOBE";

	public bool IsPlayingVO
	{
		get
		{
			return voAudioSource.isPlaying;
		}
	}

	public static OOBEFlowState OOBEState
	{
		get
		{
			return (OOBEFlowState)RecroomPrefs.GetInt("Recroom.OOBE", 0);
		}
		set
		{
			RecroomPrefs.SetInt("Recroom.OOBE", (int)value);
			RecroomPrefs.Save();
			if (value == OOBEFlowState.Lockerroom && Player.LocalPlayer != null)
			{
				Player.LocalPlayer.ObjectiveTracker.CompleteObjective(ProgressionManager.ObjectiveType.OOBE_GoToLockerRoom);
			}
		}
	}

	public bool IsOOBERunning
	{
		get
		{
			return OOBEState != OOBEFlowState.Complete;
		}
	}

	public bool SuppressActivityNameNotification
	{
		get
		{
			return IsOOBERunning && (PUNNetworkManager.Instance.IsInLockerRoom || PUNNetworkManager.Instance.IsInDormRoom);
		}
	}

	public bool CanRemoveOutfit
	{
		get
		{
			return OOBEState >= OOBEFlowState.Dormroom;
		}
	}

	private void Awake()
	{
		SingletonMonoBehaviour<TutorialManager>.Instance = this;
	}

	public void Initialize()
	{
		if (IsOOBERunning)
		{
			PUNNetworkManager.Instance.OnRecRoomPlayerConnected += PUNNetworkManager_OnRecRoomPlayerConnected;
		}
	}

	private void PUNNetworkManager_OnRecRoomPlayerConnected(Player player)
	{
		if (player != null && player.isLocal)
		{
			GameManager gameManager = RecRoomSceneManager.Instance.GameManager;
			if (gameManager != null && !PUNNetworkManager.Instance.IsInDormRoom)
			{
				player.ObjectiveTracker.CompleteObjective(ProgressionManager.ObjectiveType.OOBE_GoToActivity);
			}
			player.PlayerEvents.MenuVisibleEvent += OnPlayerMenuVisible;
		}
	}

	private void OnPlayerMenuVisible(bool visible)
	{
		if (IsOOBERunning && visible)
		{
			Player.LocalPlayer.ObjectiveTracker.CompleteObjective(ProgressionManager.ObjectiveType.OOBE_OpenMenu);
			Player.LocalPlayer.PlayerEvents.MenuVisibleEvent -= OnPlayerMenuVisible;
		}
	}

	public void OOBEObjectivesCompleted()
	{
		if (IsOOBERunning)
		{
			PUNNetworkManager.Instance.OnRecRoomPlayerConnected -= PUNNetworkManager_OnRecRoomPlayerConnected;
		}
		OOBEState = OOBEFlowState.Complete;
	}

	public IEnumerator RunPlayVO(VOClip vo)
	{
		if (vo.preDuration > 0f)
		{
			yield return new WaitForSeconds(vo.preDuration);
		}
		PlayVO(vo.recClip);
		while (IsPlayingVO)
		{
			yield return null;
		}
		if (vo.postDuration > 0f)
		{
			yield return new WaitForSeconds(vo.postDuration);
		}
	}

	public void PlayVO(RecRoomAudioClip recClip)
	{
		if (voAudioSource.isPlaying)
		{
			voAudioSource.Stop();
		}
		voAudioSource.clip = recClip.audioClip;
		voAudioSource.volume = recClip.volume;
		if (recClip.pitchVariation > 0f)
		{
			voAudioSource.pitch = 1f + UnityEngine.Random.Range(0f - recClip.pitchVariation, recClip.pitchVariation);
		}
		else
		{
			voAudioSource.pitch = 1f;
		}
		voAudioSource.Play();
	}

	public void PlayAttentionHelper(Vector3 position, bool playAudio = true)
	{
		PooledParticle pooledParticle = ObjectPool.Instance.Acquire(attentionHelperVFXPrefab);
		if (pooledParticle != null)
		{
			pooledParticle.transform.position = position;
			pooledParticle.transform.rotation = Quaternion.identity;
			pooledParticle.Play();
		}
		if (playAudio)
		{
			AudioManager.Play3DSFX(attentionHelperAudio, position);
		}
	}
}
