using System;
using System.Collections.Generic;
using POpusCodec;
using POpusCodec.Enums;

namespace ExitGames.Client.Photon.Voice
{
	public class LocalVoice : IDisposable
	{
		public static LocalVoice Dummy = new LocalVoice();

		private int voiceDetectorCalibrateCount;

		internal VoiceInfo info;

		private OpusEncoder opusEncoder;

		internal byte id;

		internal int channelId;

		internal byte evNumber;

		private IVoiceFrontend frontend;

		private IAudioStream audioStream;

		private int sourceSamplingRateHz;

		private float[] frameBuffer;

		internal int sourceFrameSize;

		private float[] sourceFrameBuffer;

		internal Dictionary<byte, int> eventTimestamps = new Dictionary<byte, int>();

		public byte AudioGroup { get; set; }

		public bool Transmit { get; set; }

		public bool IsTransmitting
		{
			get
			{
				return Transmit && (!VoiceDetector.On || VoiceDetector.Detected);
			}
		}

		public int FramesSent { get; private set; }

		public int FramesSentBytes { get; private set; }

		public VoiceDetector VoiceDetector { get; private set; }

		public LevelMeter LevelMeter { get; private set; }

		public bool VoiceDetectorCalibrating
		{
			get
			{
				return voiceDetectorCalibrateCount > 0;
			}
		}

		internal LocalVoice()
		{
			LevelMeter = new LevelMeter(0, 0);
			VoiceDetector = new VoiceDetector(0, 0);
		}

		internal LocalVoice(IVoiceFrontend client, byte id, IAudioStream audioStream, VoiceInfo voiceInfo, int channelId)
		{
			info = voiceInfo;
			this.channelId = channelId;
			opusEncoder = new OpusEncoder((SamplingRate)voiceInfo.SamplingRate, (Channels)voiceInfo.Channels, voiceInfo.Bitrate, OpusApplicationType.Voip, (Delay)(voiceInfo.FrameDurationUs * 2 / 1000));
			frontend = client;
			this.id = id;
			this.audioStream = audioStream;
			sourceSamplingRateHz = audioStream.SamplingRate;
			sourceFrameSize = info.FrameSize * sourceSamplingRateHz / (int)opusEncoder.InputSamplingRate;
			frameBuffer = new float[info.FrameSize];
			if (sourceFrameSize == info.FrameSize)
			{
				sourceFrameBuffer = frameBuffer;
			}
			else
			{
				sourceSamplingRateHz = audioStream.SamplingRate;
				sourceFrameBuffer = new float[sourceFrameSize];
				frontend.DebugReturn(DebugLevel.WARNING, "[PV] Local voice #" + this.id + " audio source frequency " + sourceSamplingRateHz + " and encoder sampling rate " + (int)opusEncoder.InputSamplingRate + " do not match. Resampling will occur before encoding.");
			}
			LevelMeter = new LevelMeter(sourceSamplingRateHz, info.Channels);
			VoiceDetector = new VoiceDetector(sourceSamplingRateHz, info.Channels);
		}

		public void VoiceDetectorCalibrate(int durationMs)
		{
			voiceDetectorCalibrateCount = sourceSamplingRateHz * (int)opusEncoder.InputChannels * durationMs / 1000;
			LevelMeter.ResetAccumAvgPeakAmp();
		}

		internal void service()
		{
			while (processStream())
			{
			}
		}

		private bool readStream()
		{
			if (!audioStream.GetData(sourceFrameBuffer))
			{
				return false;
			}
			LevelMeter.process(sourceFrameBuffer);
			if (voiceDetectorCalibrateCount != 0)
			{
				voiceDetectorCalibrateCount -= sourceFrameBuffer.Length;
				if (voiceDetectorCalibrateCount <= 0)
				{
					voiceDetectorCalibrateCount = 0;
					VoiceDetector.Threshold = LevelMeter.AccumAvgPeakAmp * 2f;
				}
			}
			if (VoiceDetector.On)
			{
				VoiceDetector.process(sourceFrameBuffer);
				if (!VoiceDetector.Detected)
				{
					return false;
				}
			}
			if (sourceFrameBuffer != frameBuffer)
			{
				VoiceUtil.Resample(sourceFrameBuffer, frameBuffer, (int)opusEncoder.InputChannels);
			}
			return true;
		}

		private bool processStream()
		{
			if (frontend.IsChannelJoined(channelId) && Transmit)
			{
				if (readStream())
				{
					ArraySegment<byte> arraySegment = compress(frameBuffer);
					FramesSent++;
					FramesSentBytes += arraySegment.Count;
					byte b = evNumber++;
					object[] content = new object[3] { id, b, arraySegment };
					frontend.SendFrame(content, channelId, AudioGroup);
					eventTimestamps[evNumber] = Environment.TickCount;
					return true;
				}
				return false;
			}
			return false;
		}

		private ArraySegment<byte> compress(float[] buffer)
		{
			return opusEncoder.Encode(buffer);
		}

		public void Dispose()
		{
			if (opusEncoder != null)
			{
				opusEncoder.Dispose();
			}
		}
	}
}
