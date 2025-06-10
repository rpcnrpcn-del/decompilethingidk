using System;
using POpusCodec;
using POpusCodec.Enums;

namespace ExitGames.Client.Photon.Voice
{
	internal class RemoteVoice : IDisposable
	{
		private int channelId;

		private int playerId;

		private byte voiceId;

		internal byte lastEvNumber;

		private OpusDecoder opusDecoder;

		private VoiceClient voiceClient;

		internal VoiceInfo Info { get; private set; }

		internal RemoteVoice(VoiceClient client, int channelId, int playerId, byte voiceId, VoiceInfo info, byte lastEventNumber)
		{
			opusDecoder = new OpusDecoder((SamplingRate)info.SamplingRate, (Channels)info.Channels);
			voiceClient = client;
			this.channelId = channelId;
			this.playerId = playerId;
			this.voiceId = voiceId;
			Info = info;
			lastEvNumber = lastEventNumber;
		}

		internal void receiveBytes(byte[] receivedBytes, byte evNumber)
		{
			if (evNumber != lastEvNumber)
			{
				int num = VoiceUtil.byteDiff(evNumber, lastEvNumber);
				if (num != 0)
				{
					voiceClient.frontend.DebugReturn(DebugLevel.ALL, "[PV] evNumer: " + evNumber + " playerVoice.lastEvNumber: " + lastEvNumber + " missing: " + num);
				}
				lastEvNumber = evNumber;
				for (int i = 0; i < num; i++)
				{
					receiveFrame(null);
				}
				voiceClient.FramesLost += num;
			}
			receiveFrame(receivedBytes);
		}

		internal void receiveFrame(byte[] frame)
		{
			float[] arg = decompress(frame);
			if (voiceClient.frontend.OnAudioFrameAction != null)
			{
				voiceClient.frontend.OnAudioFrameAction(channelId, playerId, voiceId, arg);
			}
		}

		internal float[] decompress(byte[] buffer)
		{
			float[] array;
			if (buffer == null)
			{
				array = opusDecoder.DecodePacketFloat(null);
				voiceClient.DebugReturn(DebugLevel.ALL, "[PV] lost packet decoded length: " + array.Length);
			}
			else
			{
				array = opusDecoder.DecodePacketFloat(buffer);
			}
			return array;
		}

		public void Dispose()
		{
			if (opusDecoder != null)
			{
				opusDecoder.Dispose();
			}
		}
	}
}
