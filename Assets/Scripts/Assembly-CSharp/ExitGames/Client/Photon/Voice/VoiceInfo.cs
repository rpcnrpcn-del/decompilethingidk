namespace ExitGames.Client.Photon.Voice
{
	public class VoiceInfo
	{
		public int SamplingRate { get; private set; }

		public int Channels { get; private set; }

		public int FrameDurationUs { get; private set; }

		public int Bitrate { get; private set; }

		public object UserData { get; private set; }

		public int FrameDurationSamples
		{
			get
			{
				return (int)((long)SamplingRate * (long)FrameDurationUs / 1000000);
			}
		}

		public int FrameSize
		{
			get
			{
				return FrameDurationSamples * Channels;
			}
		}

		public VoiceInfo(int samplingRate, int channels, int frameDurationUs, int bitrate, object userdata)
		{
			SamplingRate = samplingRate;
			Channels = channels;
			FrameDurationUs = frameDurationUs;
			Bitrate = bitrate;
			UserData = userdata;
		}
	}
}
