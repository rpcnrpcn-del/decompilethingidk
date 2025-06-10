namespace ExitGames.Client.Photon.Voice
{
	public class LevelMeter
	{
		private float ampSum;

		private float ampPeak;

		private int bufferSize;

		private float[] buffer;

		private int prevValuesPtr;

		private float accumAvgPeakAmpSum;

		private int accumAvgPeakAmpCount;

		public float CurrentAvgAmp
		{
			get
			{
				return ampSum / (float)bufferSize;
			}
		}

		public float CurrentPeakAmp { get; private set; }

		public float AccumAvgPeakAmp
		{
			get
			{
				return (accumAvgPeakAmpCount != 0) ? (accumAvgPeakAmpSum / (float)accumAvgPeakAmpCount) : 0f;
			}
		}

		internal LevelMeter(int samplingRate, int numChannels)
		{
			bufferSize = samplingRate * numChannels / 2;
			buffer = new float[bufferSize];
		}

		public void ResetAccumAvgPeakAmp()
		{
			accumAvgPeakAmpSum = 0f;
			accumAvgPeakAmpCount = 0;
		}

		internal void process(float[] buf)
		{
			foreach (float num in buf)
			{
				float num2 = num;
				if (num2 < 0f)
				{
					num2 = 0f - num2;
				}
				ampSum = ampSum + num2 - buffer[prevValuesPtr];
				buffer[prevValuesPtr] = num2;
				if (ampPeak < num2)
				{
					ampPeak = num2;
				}
				if (prevValuesPtr == 0)
				{
					CurrentPeakAmp = ampPeak;
					ampPeak = 0f;
					accumAvgPeakAmpSum += CurrentPeakAmp;
					accumAvgPeakAmpCount++;
				}
				prevValuesPtr = (prevValuesPtr + 1) % bufferSize;
			}
		}
	}
}
