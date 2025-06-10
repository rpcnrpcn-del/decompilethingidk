using System;
using UnityEngine;

[Serializable]
public class GamePresenceSettings : IGameComponent
{
	[Header("Rich Presence")]
	[SerializeField]
	private bool customRichPresenceString;

	[SerializeField]
	private string richPresenceString;

	[Header("Local Player Status")]
	[SerializeField]
	private bool customLocalPlayerStatus;

	[SerializeField]
	private string localPlayerStatus;

	private string _currentPresenceString = string.Empty;

	public string CurrentPresenceString
	{
		get
		{
			return _currentPresenceString;
		}
		set
		{
			_currentPresenceString = value;
			if (this.PresenceChangeEvent != null)
			{
				this.PresenceChangeEvent();
			}
		}
	}

	public event Action PresenceChangeEvent;

	public void OnAwake(GameManager gameManager)
	{
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

	public void InitializeLocalPlayer(bool localPlayerIsSpectator)
	{
		if (!localPlayerIsSpectator)
		{
			if (customRichPresenceString)
			{
				Player.LocalPlayer.PushRichPresence(richPresenceString);
			}
			if (customLocalPlayerStatus)
			{
				Player.LocalPlayer.PushLocalPlayerStatus(localPlayerStatus);
			}
		}
	}

	public void ResetLocalPlayer(bool localPlayerIsSpectator)
	{
		if (!localPlayerIsSpectator)
		{
			if (customRichPresenceString)
			{
				Player.LocalPlayer.PopRichPresence();
			}
			if (customLocalPlayerStatus)
			{
				Player.LocalPlayer.PopLocalPlayerStatus();
			}
		}
	}

	public void OnPlayerDisconnected(PhotonPlayer player)
	{
	}
}
