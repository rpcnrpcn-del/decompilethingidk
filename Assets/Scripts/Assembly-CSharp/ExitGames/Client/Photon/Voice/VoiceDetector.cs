namespace ExitGames.Client.Photon.Voice
{
	public class VoiceDetector
	{
		private int activityDelay;

		private int autoSilenceCounter;

		private int valuesCountPerSec;

		private int activityDelayValuesCount;

		public bool On { get; set; }

		public float Threshold { get; set; }

		public bool Detected { get; private set; }

		public int ActivityDelayMs
		{
			get
			{
				return activityDelay;
			}
			set
			{
				activityDelay = value;
				activityDelayValuesCount = value * valuesCountPerSec / 1000;
			}
		}

		internal VoiceDetector(int samplingRate, int numChannels)
		{
			valuesCountPerSec = samplingRate * numChannels;
			Threshold = 0.01f;
			ActivityDelayMs = 500;
		}

		internal void process(float[] buffer)
		{
			if (On)
			{
				foreach (float num in buffer)
				{
					if (num > Threshold)
					{
						Detected = true;
						autoSilenceCounter = 0;
					}
					else
					{
						autoSilenceCounter++;
					}
				}
				if (autoSilenceCounter > activityDelayValuesCount)
				{
					Detected = false;
				}
			}
			else
			{
				Detected = false;
			}
		}
	}
}
