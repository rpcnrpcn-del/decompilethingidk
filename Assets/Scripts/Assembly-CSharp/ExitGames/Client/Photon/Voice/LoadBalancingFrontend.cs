using System;
using System.Collections.Generic;
using ExitGames.Client.Photon.LoadBalancing;

namespace ExitGames.Client.Photon.Voice
{
	public class LoadBalancingFrontend : LoadBalancingClient, IVoiceFrontend, IVoiceActions
	{
		private const int DefaultVoiceChannel = 1;

		private VoiceClient voiceClient;

		private bool debugEchoMode;

		public int DebugLostPercent { get; set; }

		public bool UseLossCompensation { get; set; }

		public int FramesLost
		{
			get
			{
				return voiceClient.FramesLost;
			}
		}

		public int FramesReceived
		{
			get
			{
				return voiceClient.FramesReceived;
			}
		}

		public int FramesSent
		{
			get
			{
				return voiceClient.FramesSent;
			}
		}

		public int FramesSentBytes
		{
			get
			{
				return voiceClient.FramesSentBytes;
			}
		}

		public int RoundTripTime
		{
			get
			{
				return voiceClient.RoundTripTime;
			}
		}

		public int RoundTripTimeVariance
		{
			get
			{
				return voiceClient.RoundTripTimeVariance;
			}
		}

		public Action<int, int, byte, VoiceInfo> OnRemoteVoiceInfoAction { get; set; }

		public Action<int, int, byte> OnRemoteVoiceRemoveAction { get; set; }

		public Action<int, int, byte, float[]> OnAudioFrameAction { get; set; }

		public IEnumerable<LocalVoice> LocalVoices
		{
			get
			{
				return voiceClient.LocalVoices;
			}
		}

		public IEnumerable<RemoteVoiceInfo> RemoteVoiceInfos
		{
			get
			{
				return voiceClient.RemoteVoiceInfos;
			}
		}

		public bool DebugEchoMode
		{
			get
			{
				return debugEchoMode;
			}
			set
			{
				debugEchoMode = value;
				if (base.State == ExitGames.Client.Photon.LoadBalancing.ClientState.Joined)
				{
					if (debugEchoMode)
					{
						voiceClient.sendChannelVoicesInfo(1, base.LocalPlayer.ID);
						return;
					}
					object[] customEventContent = new object[2]
					{
						(byte)0,
						EventSubcode.DebugEchoRemoveMyVoices
					};
					ExitGames.Client.Photon.LoadBalancing.RaiseEventOptions raiseEventOptions = new ExitGames.Client.Photon.LoadBalancing.RaiseEventOptions();
					raiseEventOptions.TargetActors = new int[1] { base.LocalPlayer.ID };
					OpRaiseEvent(201, customEventContent, true, raiseEventOptions);
				}
			}
		}

		public new Action<EventData> OnEventAction { get; set; }

		public byte GlobalAudioGroup
		{
			get
			{
				return voiceClient.GlobalAudioGroup;
			}
			set
			{
				voiceClient.GlobalAudioGroup = value;
				if (base.State == ExitGames.Client.Photon.LoadBalancing.ClientState.Joined)
				{
					if (voiceClient.GlobalAudioGroup != 0)
					{
						loadBalancingPeer.OpChangeGroups(new byte[0], new byte[1] { voiceClient.GlobalAudioGroup });
					}
					else
					{
						loadBalancingPeer.OpChangeGroups(new byte[0], null);
					}
				}
			}
		}

		public LoadBalancingFrontend()
		{
			base.OnEventAction += onEventActionVoiceClient;
			voiceClient = new VoiceClient(this);
		}

		public IEnumerable<LocalVoice> LocalVoicesInChannel(int channelId)
		{
			return voiceClient.LocalVoicesInChannel(channelId);
		}

		public bool IsChannelJoined(int channelId)
		{
			return base.State == ExitGames.Client.Photon.LoadBalancing.ClientState.Joined;
		}

		public new void Service()
		{
			base.Service();
			voiceClient.Service();
		}

		public LocalVoice CreateLocalVoice(IAudioStream audioStream, VoiceInfo voiceInfo)
		{
			return voiceClient.CreateLocalVoice(audioStream, voiceInfo, 1);
		}

		public void RemoveLocalVoice(LocalVoice voice)
		{
			voiceClient.RemoveLocalVoice(voice);
		}

		public virtual bool ChangeAudioGroups(byte[] groupsToRemove, byte[] groupsToAdd)
		{
			return loadBalancingPeer.OpChangeGroups(groupsToRemove, groupsToAdd);
		}

		public void SendVoicesInfo(object content, int channelId, int targetPlayerId)
		{
			ExitGames.Client.Photon.LoadBalancing.RaiseEventOptions raiseEventOptions = new ExitGames.Client.Photon.LoadBalancing.RaiseEventOptions();
			if (targetPlayerId != 0)
			{
				raiseEventOptions.TargetActors = new int[1] { targetPlayerId };
			}
			else if (DebugEchoMode)
			{
				raiseEventOptions.Receivers = ExitGames.Client.Photon.LoadBalancing.ReceiverGroup.All;
			}
			OpRaiseEvent(201, content, true, raiseEventOptions);
		}

		public void SendVoiceRemove(object content, int channelId)
		{
			ExitGames.Client.Photon.LoadBalancing.RaiseEventOptions raiseEventOptions = new ExitGames.Client.Photon.LoadBalancing.RaiseEventOptions();
			if (DebugEchoMode)
			{
				raiseEventOptions.Receivers = ExitGames.Client.Photon.LoadBalancing.ReceiverGroup.All;
			}
			OpRaiseEvent(201, content, true, raiseEventOptions);
		}

		public void SendFrame(object content, int channelId, byte audioGroup)
		{
			ExitGames.Client.Photon.LoadBalancing.RaiseEventOptions raiseEventOptions = new ExitGames.Client.Photon.LoadBalancing.RaiseEventOptions();
			if (DebugEchoMode)
			{
				raiseEventOptions.Receivers = ExitGames.Client.Photon.LoadBalancing.ReceiverGroup.All;
			}
			raiseEventOptions.InterestGroup = audioGroup;
			OpRaiseEvent(201, content, false, raiseEventOptions);
			loadBalancingPeer.SendOutgoingCommands();
		}

		public string ChannelIdStr(int channelId)
		{
			return null;
		}

		public string PlayerIdStr(int playerId)
		{
			return null;
		}

		private void onEventActionVoiceClient(EventData ev)
		{
			switch (ev.Code)
			{
			case byte.MaxValue:
			{
				int num = (int)ev[254];
				if (num == base.LocalPlayer.ID)
				{
					voiceClient.clearRemoteVoices();
					voiceClient.sendChannelVoicesInfo(1, 0);
					if (voiceClient.GlobalAudioGroup != 0)
					{
						loadBalancingPeer.OpChangeGroups(new byte[0], new byte[1] { voiceClient.GlobalAudioGroup });
					}
				}
				else
				{
					voiceClient.sendChannelVoicesInfo(1, num);
				}
				break;
			}
			case 254:
			{
				int num = (int)ev[254];
				if (num == base.LocalPlayer.ID)
				{
					voiceClient.clearRemoteVoices();
				}
				else
				{
					onPlayerLeave(num);
				}
				break;
			}
			case 201:
				voiceClient.onVoiceEvent(ev[245], 1, (int)ev[254], base.LocalPlayer.ID);
				break;
			}
			if (OnEventAction != null)
			{
				OnEventAction(ev);
			}
		}

		private void onPlayerLeave(int playerId)
		{
			if (voiceClient.removePlayerVoices(1, playerId))
			{
				DebugReturn(DebugLevel.INFO, "[PV] Player " + playerId + " voices removed on leave");
			}
			else
			{
				DebugReturn(DebugLevel.WARNING, "[PV] Voices of player " + playerId + " not found when trying to remove on player leave");
			}
		}
	}
}
