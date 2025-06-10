using System;
using System.Collections.Generic;

namespace ExitGames.Client.Photon.Voice
{
	internal class VoiceClient
	{
		internal IVoiceFrontend frontend;

		private int prevRtt;

		private byte globalAudioGroup;

		private byte voiceIdCnt;

		private Dictionary<byte, LocalVoice> localVoices = new Dictionary<byte, LocalVoice>();

		private Dictionary<int, List<LocalVoice>> localVoicesPerChannel = new Dictionary<int, List<LocalVoice>>();

		private Dictionary<int, Dictionary<int, Dictionary<byte, RemoteVoice>>> remoteVoices = new Dictionary<int, Dictionary<int, Dictionary<byte, RemoteVoice>>>();

		private Random rnd = new Random();

		public int FramesLost { get; internal set; }

		public int FramesReceived { get; private set; }

		public int FramesSent
		{
			get
			{
				int num = 0;
				foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
				{
					num += localVoice.Value.FramesSent;
				}
				return num;
			}
		}

		public int FramesSentBytes
		{
			get
			{
				int num = 0;
				foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
				{
					num += localVoice.Value.FramesSentBytes;
				}
				return num;
			}
		}

		public int RoundTripTime { get; private set; }

		public int RoundTripTimeVariance { get; private set; }

		public bool SuppressInfoDuplicateWarning { get; set; }

		public IEnumerable<LocalVoice> LocalVoices
		{
			get
			{
				LocalVoice[] array = new LocalVoice[localVoices.Count];
				localVoices.Values.CopyTo(array, 0);
				return array;
			}
		}

		public IEnumerable<RemoteVoiceInfo> RemoteVoiceInfos
		{
			get
			{
				foreach (KeyValuePair<int, Dictionary<int, Dictionary<byte, RemoteVoice>>> channelVoices in remoteVoices)
				{
					foreach (KeyValuePair<int, Dictionary<byte, RemoteVoice>> playerVoices in channelVoices.Value)
					{
						foreach (KeyValuePair<byte, RemoteVoice> voice in playerVoices.Value)
						{
							yield return new RemoteVoiceInfo(channelVoices.Key, playerVoices.Key, voice.Key, voice.Value.Info);
						}
					}
				}
			}
		}

		internal byte GlobalAudioGroup
		{
			get
			{
				return globalAudioGroup;
			}
			set
			{
				globalAudioGroup = value;
				foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
				{
					localVoice.Value.AudioGroup = globalAudioGroup;
				}
			}
		}

		internal VoiceClient(IVoiceFrontend frontend)
		{
			this.frontend = frontend;
		}

		public IEnumerable<LocalVoice> LocalVoicesInChannel(int channelId)
		{
			List<LocalVoice> value;
			if (localVoicesPerChannel.TryGetValue(channelId, out value))
			{
				LocalVoice[] array = new LocalVoice[value.Count];
				value.CopyTo(array, 0);
				return array;
			}
			return new LocalVoice[0];
		}

		internal void DebugReturn(DebugLevel level, string message)
		{
			frontend.DebugReturn(level, message);
		}

		public void Service()
		{
			foreach (KeyValuePair<byte, LocalVoice> localVoice in localVoices)
			{
				localVoice.Value.service();
			}
		}

		public LocalVoice CreateLocalVoice(IAudioStream audioStream, VoiceInfo voiceInfo, int channelId)
		{
			byte b = 0;
			if (voiceIdCnt == byte.MaxValue)
			{
				bool[] array = new bool[256];
				foreach (KeyValuePair<byte, LocalVoice> localVoice2 in localVoices)
				{
					array[localVoice2.Value.id] = true;
				}
				for (byte b2 = 1; b2 != 0; b2++)
				{
					if (!array[b2])
					{
						b = b2;
						break;
					}
				}
			}
			else
			{
				voiceIdCnt++;
				b = voiceIdCnt;
			}
			if (b != 0)
			{
				LocalVoice localVoice = new LocalVoice(frontend, b, audioStream, voiceInfo, channelId);
				localVoices[b] = localVoice;
				List<LocalVoice> value;
				if (!localVoicesPerChannel.TryGetValue(channelId, out value))
				{
					value = new List<LocalVoice>();
					localVoicesPerChannel[channelId] = value;
				}
				value.Add(localVoice);
				frontend.DebugReturn(DebugLevel.INFO, "[PV] Local voice #" + localVoice.id + " at channel " + channelStr(channelId) + " added: src_f=" + audioStream.SamplingRate + " enc_f=" + localVoice.info.SamplingRate + " ch=" + localVoice.info.Channels + " d=" + localVoice.info.FrameDurationUs + " s=" + localVoice.info.FrameSize + " b=" + localVoice.info.Bitrate + " ud=" + voiceInfo.UserData);
				if (frontend.IsChannelJoined(channelId))
				{
					frontend.SendVoicesInfo(buildVoicesInfo(new List<LocalVoice> { localVoice }, true), channelId, 0);
				}
				localVoice.AudioGroup = GlobalAudioGroup;
				return localVoice;
			}
			return null;
		}

		public void RemoveLocalVoice(LocalVoice voice)
		{
			localVoices.Remove(voice.id);
			localVoicesPerChannel[voice.channelId].Remove(voice);
			if (frontend.IsChannelJoined(voice.channelId))
			{
				object[] content = buildVoiceRemoveMessage(new List<LocalVoice> { voice });
				frontend.SendVoiceRemove(content, voice.channelId);
			}
			frontend.DebugReturn(DebugLevel.INFO, "[PV] Local voice #" + voice.id + " at channel " + channelStr(voice.channelId) + " removed");
		}

		internal void sendChannelVoicesInfo(int channelId, int targetPlayerId, bool logInfo = true)
		{
			List<LocalVoice> value;
			if (frontend.IsChannelJoined(channelId) && localVoicesPerChannel.TryGetValue(channelId, out value))
			{
				frontend.SendVoicesInfo(buildVoicesInfo(value, logInfo), channelId, targetPlayerId);
			}
		}

		internal void onVoiceEvent(object content0, int channelId, int playerId, int localPlayerId)
		{
			object[] array = (object[])content0;
			if ((byte)array[0] == 0)
			{
				switch ((byte)array[1])
				{
				case 1:
					onVoiceInfo(channelId, playerId, array[2]);
					break;
				case 2:
					onVoiceRemove(channelId, playerId, array[2]);
					break;
				case 10:
					removePlayerVoices(channelId, localPlayerId);
					break;
				default:
					DebugReturn(DebugLevel.ERROR, "[PV] Unknown sevent subcode " + array[1]);
					break;
				}
				return;
			}
			byte b = (byte)array[0];
			byte b2 = (byte)array[1];
			byte[] receivedBytes = (byte[])array[2];
			LocalVoice value;
			int value2;
			if (playerId == localPlayerId && localVoices.TryGetValue(b, out value) && value.eventTimestamps.TryGetValue(b2, out value2))
			{
				int num = Environment.TickCount - value2;
				int num2 = num - prevRtt;
				prevRtt = num;
				if (num2 < 0)
				{
					num2 = -num2;
				}
				RoundTripTimeVariance = (num2 + RoundTripTimeVariance * 19) / 20;
				RoundTripTime = (num + RoundTripTime * 19) / 20;
			}
			onFrame(channelId, playerId, b, b2, receivedBytes);
		}

		internal object[] buildVoicesInfo(ICollection<LocalVoice> voicesToSend, bool logInfo)
		{
			object[] array = new object[voicesToSend.Count];
			object[] result = new object[3]
			{
				(byte)0,
				EventSubcode.VoiceInfo,
				array
			};
			int num = 0;
			foreach (LocalVoice item in voicesToSend)
			{
				array[num] = new Hashtable
				{
					{
						(byte)1,
						item.id
					},
					{
						(byte)2,
						item.info.SamplingRate
					},
					{
						(byte)3,
						item.info.Channels
					},
					{
						(byte)4,
						item.info.FrameDurationUs
					},
					{
						(byte)5,
						item.info.Bitrate
					},
					{
						(byte)10,
						item.info.UserData
					},
					{
						(byte)11,
						item.evNumber
					}
				};
				num++;
				if (logInfo)
				{
					frontend.DebugReturn(DebugLevel.INFO, "[PV] Sending info for voice #" + item.id + " at channel " + channelStr(item.channelId) + ": f=" + item.info.SamplingRate + ", ch=" + item.info.Channels + " d=" + item.info.FrameDurationUs + " s=" + item.info.FrameSize + " b=" + item.info.Bitrate + " ev=" + item.evNumber);
				}
			}
			return result;
		}

		private object[] buildVoiceRemoveMessage(List<LocalVoice> voicesToSend)
		{
			byte[] array = new byte[voicesToSend.Count];
			object[] result = new object[3]
			{
				(byte)0,
				EventSubcode.VoiceRemove,
				array
			};
			int num = 0;
			foreach (LocalVoice item in voicesToSend)
			{
				array[num] = item.id;
				num++;
				frontend.DebugReturn(DebugLevel.INFO, "[PV] Voice #" + item.id + " at channel " + channelStr(item.channelId) + " remove sent");
			}
			return result;
		}

		internal void clearRemoteVoices()
		{
			if (frontend.OnRemoteVoiceRemoveAction != null)
			{
				foreach (KeyValuePair<int, Dictionary<int, Dictionary<byte, RemoteVoice>>> remoteVoice in remoteVoices)
				{
					foreach (KeyValuePair<int, Dictionary<byte, RemoteVoice>> item in remoteVoice.Value)
					{
						foreach (KeyValuePair<byte, RemoteVoice> item2 in item.Value)
						{
							frontend.OnRemoteVoiceRemoveAction(remoteVoice.Key, item.Key, item2.Key);
						}
					}
				}
			}
			remoteVoices.Clear();
			frontend.DebugReturn(DebugLevel.INFO, "[PV] Remote voices cleared");
		}

		internal void clearRemoteVoicesInChannel(int channelId)
		{
			Dictionary<int, Dictionary<byte, RemoteVoice>> value = null;
			if (remoteVoices.TryGetValue(channelId, out value))
			{
				if (frontend.OnRemoteVoiceRemoveAction != null)
				{
					foreach (KeyValuePair<int, Dictionary<byte, RemoteVoice>> item in value)
					{
						foreach (KeyValuePair<byte, RemoteVoice> item2 in item.Value)
						{
							frontend.OnRemoteVoiceRemoveAction(channelId, item.Key, item2.Key);
						}
					}
				}
				remoteVoices.Remove(channelId);
			}
			frontend.DebugReturn(DebugLevel.INFO, "[PV] Remote voices for channel " + channelStr(channelId) + " cleared");
		}

		private void onVoiceInfo(int channelId, int playerId, object payload)
		{
			Dictionary<int, Dictionary<byte, RemoteVoice>> value = null;
			if (!remoteVoices.TryGetValue(channelId, out value))
			{
				value = new Dictionary<int, Dictionary<byte, RemoteVoice>>();
				remoteVoices[channelId] = value;
			}
			Dictionary<byte, RemoteVoice> value2 = null;
			if (!value.TryGetValue(playerId, out value2))
			{
				value2 = (value[playerId] = new Dictionary<byte, RemoteVoice>());
			}
			object[] array = (object[])payload;
			foreach (object obj in array)
			{
				Hashtable hashtable = (Hashtable)obj;
				byte b = (byte)hashtable[(byte)1];
				if (!value2.ContainsKey(b))
				{
					int num = (int)hashtable[(byte)2];
					int num2 = (int)hashtable[(byte)3];
					int num3 = (int)hashtable[(byte)4];
					int num4 = (int)hashtable[(byte)5];
					object obj2 = hashtable[(byte)10];
					byte b2 = (byte)hashtable[(byte)11];
					frontend.DebugReturn(DebugLevel.INFO, string.Concat("[PV] Channel ", channelStr(channelId), " player ", playerStr(playerId), " voice #", b, " info received: f=", num, ", ch=", num2, " d=", num3, " b=", num4, " ud=", obj2, " ev=", b2));
					VoiceInfo voiceInfo = new VoiceInfo(num, num2, num3, num4, obj2);
					value2[b] = new RemoteVoice(this, channelId, playerId, b, voiceInfo, b2);
					if (frontend.OnRemoteVoiceInfoAction != null)
					{
						frontend.OnRemoteVoiceInfoAction(channelId, playerId, b, voiceInfo);
					}
				}
				else if (!SuppressInfoDuplicateWarning)
				{
					frontend.DebugReturn(DebugLevel.WARNING, "[PV] Info duplicate for voice #" + b + " of player " + playerStr(playerId) + " at channel " + channelStr(channelId));
				}
			}
		}

		private void onVoiceRemove(int channelId, int playerId, object payload)
		{
			byte[] array = (byte[])payload;
			Dictionary<int, Dictionary<byte, RemoteVoice>> value = null;
			if (remoteVoices.TryGetValue(channelId, out value))
			{
				Dictionary<byte, RemoteVoice> value2 = null;
				if (value.TryGetValue(playerId, out value2))
				{
					byte[] array2 = array;
					foreach (byte b in array2)
					{
						if (value2.Remove(b))
						{
							frontend.DebugReturn(DebugLevel.INFO, "[PV] Remote voice #" + b + " of player " + playerStr(playerId) + " at channel " + channelStr(channelId) + " removed");
							if (frontend.OnRemoteVoiceRemoveAction != null)
							{
								frontend.OnRemoteVoiceRemoveAction(channelId, playerId, b);
							}
						}
						else
						{
							frontend.DebugReturn(DebugLevel.WARNING, "[PV] Remote voice #" + b + " of player " + playerStr(playerId) + " at channel " + channelStr(channelId) + " not found when trying to remove");
						}
					}
				}
				else
				{
					frontend.DebugReturn(DebugLevel.WARNING, "[PV] Remote voice list of player " + playerStr(playerId) + " at channel " + channelStr(channelId) + " not found when trying to remove voice(s)");
				}
			}
			else
			{
				frontend.DebugReturn(DebugLevel.WARNING, "[PV] Remote voice list of channel " + channelStr(channelId) + " not found when trying to remove voice(s)");
			}
		}

		private void onFrame(int channelId, int playerId, byte voiceId, byte evNumber, byte[] receivedBytes)
		{
			if (frontend.DebugLostPercent > 0 && rnd.Next(100) < frontend.DebugLostPercent)
			{
				frontend.DebugReturn(DebugLevel.WARNING, "[PV] Debug Lost Sim: 1 packet dropped");
				return;
			}
			FramesReceived++;
			Dictionary<int, Dictionary<byte, RemoteVoice>> value = null;
			if (remoteVoices.TryGetValue(channelId, out value))
			{
				Dictionary<byte, RemoteVoice> value2 = null;
				if (value.TryGetValue(playerId, out value2))
				{
					RemoteVoice value3 = null;
					if (value2.TryGetValue(voiceId, out value3))
					{
						value3.receiveBytes(receivedBytes, evNumber);
						return;
					}
					frontend.DebugReturn(DebugLevel.WARNING, "[PV] Frame event for not inited voice #" + voiceId + " of player " + playerStr(playerId) + " at channel " + channelStr(channelId));
				}
				else
				{
					frontend.DebugReturn(DebugLevel.WARNING, "[PV] Frame event for voice #" + voiceId + " of not inited player " + playerStr(playerId) + " at channel " + channelStr(channelId));
				}
			}
			else
			{
				frontend.DebugReturn(DebugLevel.WARNING, "[PV] Frame event for voice #" + voiceId + " of not inited channel " + channelStr(channelId));
			}
		}

		internal bool removePlayerVoices(int channelId, int playerId)
		{
			Dictionary<int, Dictionary<byte, RemoteVoice>> value = null;
			if (remoteVoices.TryGetValue(channelId, out value))
			{
				Dictionary<byte, RemoteVoice> value2 = null;
				if (value.TryGetValue(playerId, out value2))
				{
					value.Remove(playerId);
					foreach (KeyValuePair<byte, RemoteVoice> item in value2)
					{
						if (frontend.OnRemoteVoiceRemoveAction != null)
						{
							frontend.OnRemoteVoiceRemoveAction(channelId, playerId, item.Key);
						}
					}
					return true;
				}
				return false;
			}
			return false;
		}

		internal string channelStr(int channelId)
		{
			string text = frontend.ChannelIdStr(channelId);
			if (text != null)
			{
				return "#" + channelId + "(" + text + ")";
			}
			return "#" + channelId;
		}

		internal string playerStr(int playerId)
		{
			string text = frontend.PlayerIdStr(playerId);
			if (text != null)
			{
				return "#" + playerId + "(" + text + ")";
			}
			return "#" + playerId;
		}
	}
}
