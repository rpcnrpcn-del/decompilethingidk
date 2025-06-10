using System;
using UnityEngine;

[Serializable]
public class GameFxManager : FxManager, IGameComponent
{
	[SerializeField]
	private RecRoomAudioClip loadingGameMusic;

	[SerializeField]
	private RecRoomAudioClip playingGameMusic;

	public void OnAwake(GameManager gameManager)
	{
		base.OnAwake();
	}

	public void OnStart()
	{
	}

	public override void OnDestroy()
	{
		StopMusic();
		base.OnDestroy();
	}

	public void OnUpdate()
	{
	}

	public void OnPlayerDisconnected(PhotonPlayer player)
	{
	}

	public void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
	}

	public void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
	}

	public void StartLoadingGameMusic()
	{
		if (loadingGameMusic == null)
		{
			Debug.LogWarning("Missing loading game music.");
		}
		else
		{
			AudioManager.PlayMusic(loadingGameMusic);
		}
	}

	public void StartPlayingGameMusic()
	{
		if (playingGameMusic == null)
		{
			Debug.LogWarning("Missing playing game music.");
		}
		else
		{
			AudioManager.PlayMusic(playingGameMusic);
		}
	}

	public void StopMusic()
	{
		AudioManager.StopMusic();
	}
}
