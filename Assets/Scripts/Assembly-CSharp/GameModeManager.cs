using System;
using UnityEngine;

[Serializable]
public class GameModeManager : IGameComponent
{
	[Serializable]
	private class GameModeMetadata
	{
		public GameMode Mode = GameMode.INVALID;

		public string Name;

		public bool Default;

		public string ScoreName;

		public bool HasScoreWinCondition;

		public int ScoreToWin;
	}

	[SerializeField]
	private GameModeMetadata[] modes;

	private const string MODE_KEY = "MODE";

	private const string INVALID_MODE_NAME = "Invalid Mode";

	private const string INVALID_SCORE_NAME = "Score";

	private GameManager gameManager;

	private SynchronizedField<int> _mode;

	private GameMode Mode
	{
		get
		{
			return (GameMode)_mode.Get();
		}
		set
		{
			_mode.ForceSet((int)value);
		}
	}

	public bool SupportsModeSwitching
	{
		get
		{
			return modes != null && modes.Length > 1;
		}
	}

	public event Action ModeChangeEvent;

	public void OnAwake(GameManager gameManager)
	{
		this.gameManager = gameManager;
		_mode = new SynchronizedField<int>(gameManager, "MODE", -1, SetterPermissionMode.MASTER);
		if (PhotonNetwork.isMasterClient)
		{
			MasterSetMode(GetDefaultMode());
		}
	}

	public void OnStart()
	{
	}

	public void OnDestroy()
	{
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

	public void SwitchMode()
	{
		int modeIndex = GetModeIndex(GetMode());
		if (modeIndex >= 0)
		{
			modeIndex = (modeIndex + 1) % modes.Length;
			SetMode(modes[modeIndex].Mode);
		}
	}

	public void SetMode(GameMode mode)
	{
		gameManager.photonView.RPC("RpcMasterSetMode", PhotonTargets.All, (int)mode);
	}

	public void MasterSetMode(GameMode mode)
	{
		if (PhotonNetwork.isMasterClient)
		{
			Mode = mode;
			BroadcastModeChange();
		}
	}

	public GameMode GetMode()
	{
		return Mode;
	}

	public GameMode GetDefaultMode()
	{
		GameMode result = GameMode.INVALID;
		for (int i = 0; i < modes.Length; i++)
		{
			if (modes[i].Default)
			{
				result = modes[i].Mode;
				break;
			}
		}
		return result;
	}

	public string GetModeName(GameMode mode)
	{
		int modeIndex = GetModeIndex(mode);
		if (modeIndex >= 0)
		{
			return modes[modeIndex].Name;
		}
		return "Invalid Mode";
	}

	public string GetModeScoreName(GameMode mode)
	{
		int modeIndex = GetModeIndex(mode);
		if (modeIndex >= 0)
		{
			return modes[modeIndex].ScoreName;
		}
		return "Score";
	}

	public bool GetModeHasScoreWinCondition(GameMode mode)
	{
		int modeIndex = GetModeIndex(mode);
		if (modeIndex >= 0)
		{
			return modes[modeIndex].HasScoreWinCondition;
		}
		return false;
	}

	public int GetModeScoreWinCondition(GameMode mode)
	{
		int modeIndex = GetModeIndex(mode);
		if (modeIndex >= 0)
		{
			return modes[modeIndex].ScoreToWin;
		}
		return 0;
	}

	public string GetCurrentModeName()
	{
		return GetModeName(GetMode());
	}

	public string GetCurrentModeScoreName()
	{
		return GetModeScoreName(GetMode());
	}

	public bool GetCurrentModeHasScoreWinCondition()
	{
		return GetModeHasScoreWinCondition(GetMode());
	}

	public int GetCurrentModeScoreWinCondition()
	{
		return GetModeScoreWinCondition(GetMode());
	}

	private int GetModeIndex(GameMode mode)
	{
		int result = -1;
		for (int i = 0; i < modes.Length; i++)
		{
			if (modes[i].Mode == mode)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private void BroadcastModeChange()
	{
		gameManager.photonView.RPC("RpcBroadcastModeChange", PhotonTargets.All);
	}

	public void FireModeChangeEvent()
	{
		if (this.ModeChangeEvent != null)
		{
			this.ModeChangeEvent();
		}
	}
}
