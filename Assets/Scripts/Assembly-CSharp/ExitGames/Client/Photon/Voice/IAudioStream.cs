namespace ExitGames.Client.Photon.Voice
{
	public interface IAudioStream
	{
		int SamplingRate { get; }

		bool GetData(float[] buffer);
	}
}
