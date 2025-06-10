using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using ExitGames.Client.Photon.Voice;
using UnityEngine;

public class UnityVoiceFrontend : LoadBalancingFrontend
{
	private Dictionary<KeyValuePair<int, byte>, PhotonVoiceSpeaker> voiceSpeakers = new Dictionary<KeyValuePair<int, byte>, PhotonVoiceSpeaker>();

	private bool reconnect;

	public new Action<int, byte, VoiceInfo> OnRemoteVoiceInfoAction { get; set; }

	public new Action<int, byte> OnRemoteVoiceRemoveAction { get; set; }

	public new Action<int, byte, float[]> OnAudioFrameAction { get; set; }

	public new Action<ExitGames.Client.Photon.LoadBalancing.ClientState> OnStateChangeAction { get; set; }

	public new Action<OperationResponse> OnOpResponseAction { get; set; }

	internal UnityVoiceFrontend(PhotonVoiceNetwork network)
	{
		base.OnRemoteVoiceInfoAction = (Action<int, int, byte, VoiceInfo>)Delegate.Combine(base.OnRemoteVoiceInfoAction, new Action<int, int, byte, VoiceInfo>(OnRemoteVoiceInfo));
		base.OnRemoteVoiceRemoveAction = (Action<int, int, byte>)Delegate.Combine(base.OnRemoteVoiceRemoveAction, new Action<int, int, byte>(OnRemoteVoiceRemove));
		base.OnAudioFrameAction = (Action<int, int, byte, float[]>)Delegate.Combine(base.OnAudioFrameAction, new Action<int, int, byte, float[]>(OnAudioFrame));
		base.OnStateChangeAction += OnStateChange;
		base.OnOpResponseAction += OnOpResponse;
		loadBalancingPeer.DebugOut = DebugLevel.INFO;
	}

	public void Reconnect()
	{
		if (base.State == ExitGames.Client.Photon.LoadBalancing.ClientState.Disconnected)
		{
			PhotonVoiceNetwork.Connect();
			return;
		}
		reconnect = true;
		Disconnect();
	}

	public override void DebugReturn(DebugLevel level, string message)
	{
		message = string.Format("PUNVoice: {0}", message);
		if (level == DebugLevel.ERROR)
		{
			Debug.LogError(message);
		}
	}

	public void OnOpResponse(OperationResponse resp)
	{
		if (resp.ReturnCode == 0)
		{
			byte operationCode = resp.OperationCode;
			if (operationCode == 226)
			{
				PhotonVoiceRecorder[] array = UnityEngine.Object.FindObjectsOfType<PhotonVoiceRecorder>();
				PhotonVoiceRecorder[] array2 = array;
				foreach (PhotonVoiceRecorder photonVoiceRecorder in array2)
				{
					photonVoiceRecorder.SendMessage("OnJoinedVoiceRoom");
				}
			}
		}
		if (OnOpResponseAction != null)
		{
			OnOpResponseAction(resp);
		}
	}

	private void linkVoice(int playerId, byte voiceId, VoiceInfo voiceInfo, PhotonVoiceSpeaker speaker)
	{
		speaker.OnVoiceLinked(voiceInfo.SamplingRate, voiceInfo.Channels, voiceInfo.FrameDurationSamples, PhotonVoiceSettings.Instance.PlayDelayMs);
		KeyValuePair<int, byte> key = new KeyValuePair<int, byte>(playerId, voiceId);
		PhotonVoiceSpeaker value;
		if (voiceSpeakers.TryGetValue(key, out value))
		{
			if (value == speaker)
			{
				return;
			}
			if (PhotonVoiceSettings.Instance.DebugInfo)
			{
				Debug.LogFormat("PUNVoice: Player {0} voice #{1} speaker replaced.", playerId, voiceId);
			}
		}
		else if (PhotonVoiceSettings.Instance.DebugInfo)
		{
			Debug.LogFormat("PUNVoice: Player {0} voice #{1} speaker created.", playerId, voiceId);
		}
		voiceSpeakers[key] = speaker;
	}

	public void OnRemoteVoiceInfo(int channelId, int playerId, byte voiceId, VoiceInfo voiceInfo)
	{
		KeyValuePair<int, byte> key = new KeyValuePair<int, byte>(playerId, voiceId);
		if (voiceSpeakers.ContainsKey(key))
		{
			Debug.LogWarningFormat("PUNVoice: Info duplicate for voice #{0} of player {1}", voiceId, playerId);
		}
		PhotonVoiceSpeaker photonVoiceSpeaker = null;
		PhotonVoiceSpeaker[] array = UnityEngine.Object.FindObjectsOfType<PhotonVoiceSpeaker>();
		PhotonVoiceSpeaker[] array2 = array;
		foreach (PhotonVoiceSpeaker photonVoiceSpeaker2 in array2)
		{
			if (photonVoiceSpeaker2.photonView.viewID == (int)voiceInfo.UserData)
			{
				photonVoiceSpeaker = photonVoiceSpeaker2;
				break;
			}
		}
		if (!(photonVoiceSpeaker == null))
		{
			linkVoice(playerId, voiceId, voiceInfo, photonVoiceSpeaker);
		}
		if (OnRemoteVoiceInfoAction != null)
		{
			OnRemoteVoiceInfoAction(playerId, voiceId, voiceInfo);
		}
	}

	public void LinkSpeakerToRemoteVoice(PhotonVoiceSpeaker speaker)
	{
		foreach (RemoteVoiceInfo remoteVoiceInfo in base.RemoteVoiceInfos)
		{
			if (speaker.photonView.viewID == (int)remoteVoiceInfo.Info.UserData)
			{
				linkVoice(remoteVoiceInfo.PlayerId, remoteVoiceInfo.VoiceId, remoteVoiceInfo.Info, speaker);
			}
		}
	}

	public void OnRemoteVoiceRemove(int channelId, int playerId, byte voiceId)
	{
		KeyValuePair<int, byte> key = new KeyValuePair<int, byte>(playerId, voiceId);
		if (!unlinkSpeaker(key))
		{
			Debug.LogWarningFormat("PUNVoice: Voice #{0} of player {1} not found.", voiceId, playerId);
		}
		else if (PhotonVoiceSettings.Instance.DebugInfo)
		{
			Debug.LogFormat("PUNVoice: Player {0} voice # {1} speaker unlinked.", playerId, voiceId);
		}
		if (OnRemoteVoiceRemoveAction != null)
		{
			OnRemoteVoiceRemoveAction(playerId, voiceId);
		}
	}

	private bool unlinkSpeaker(KeyValuePair<int, byte> key)
	{
		PhotonVoiceSpeaker value;
		if (voiceSpeakers.TryGetValue(key, out value))
		{
			value.OnVoiceUnlinked();
		}
		return voiceSpeakers.Remove(key);
	}

	public void UnlinkSpeakerFromRemoteVoice(PhotonVoiceSpeaker speaker)
	{
		List<KeyValuePair<int, byte>> list = new List<KeyValuePair<int, byte>>();
		foreach (KeyValuePair<KeyValuePair<int, byte>, PhotonVoiceSpeaker> voiceSpeaker in voiceSpeakers)
		{
			if (voiceSpeaker.Value == speaker)
			{
				list.Add(voiceSpeaker.Key);
				if (PhotonVoiceSettings.Instance.DebugInfo)
				{
					Debug.LogFormat("PUNVoice: Player {0} voice # {1} speaker unlinked.", voiceSpeaker.Key.Key, voiceSpeaker.Key.Value);
				}
			}
		}
		foreach (KeyValuePair<int, byte> item in list)
		{
			unlinkSpeaker(item);
		}
	}

	public void OnAudioFrame(int channelId, int playerId, byte voiceId, float[] frame)
	{
		PhotonVoiceSpeaker value = null;
		if (voiceSpeakers.TryGetValue(new KeyValuePair<int, byte>(playerId, voiceId), out value))
		{
			value.OnAudioFrame(frame);
		}
		else
		{
			Debug.LogWarningFormat("PUNVoice: Audio Frame event for not existing speaker for voice #{0} of player {1}.", voiceId, playerId);
		}
		if (OnAudioFrameAction != null)
		{
			OnAudioFrameAction(playerId, voiceId, frame);
		}
	}

	public void OnStateChange(ExitGames.Client.Photon.LoadBalancing.ClientState state)
	{
		if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
		{
			Debug.LogFormat("PUNVoice: Voice Client state: {0}", state);
		}
		switch (state)
		{
		case ExitGames.Client.Photon.LoadBalancing.ClientState.JoinedLobby:
			if (PhotonNetwork.inRoom)
			{
				OpJoinOrCreateRoom(string.Format("{0}_voice_", PhotonNetwork.room.name), new ExitGames.Client.Photon.LoadBalancing.RoomOptions
				{
					IsVisible = false
				}, null);
			}
			else
			{
				Debug.LogWarning("PUNVoice: PUN client is not in room yet. Disconnecting voice client.");
				Disconnect();
			}
			break;
		case ExitGames.Client.Photon.LoadBalancing.ClientState.Disconnected:
			if (reconnect)
			{
				PhotonVoiceNetwork.Connect();
			}
			reconnect = false;
			break;
		}
		if (OnStateChangeAction != null)
		{
			OnStateChangeAction(state);
		}
	}
}
