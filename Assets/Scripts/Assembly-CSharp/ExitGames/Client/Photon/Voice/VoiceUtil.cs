using System;
using System.Text;
using POpusCodec.Enums;

namespace ExitGames.Client.Photon.Voice
{
	public static class VoiceUtil
	{
		internal static byte byteDiff(byte latest, byte last)
		{
			return (byte)(latest - (last + 1));
		}

		internal static void Resample(float[] src, float[] dst, int channels)
		{
			for (int i = 0; i < dst.Length; i += channels)
			{
				int num = i * src.Length / dst.Length;
				for (int j = 0; j < channels; j++)
				{
					dst[i + j] = src[num + j];
				}
			}
		}

		internal static string tostr<T>(T[] x, int lim = 10)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < ((x.Length >= lim) ? lim : x.Length); i++)
			{
				stringBuilder.Append("-");
				stringBuilder.Append(x[i]);
			}
			return stringBuilder.ToString();
		}

		internal static int bestEncoderSampleRate(int f)
		{
			int num = int.MaxValue;
			int result = 48000;
			foreach (object value in Enum.GetValues(typeof(SamplingRate)))
			{
				int num2 = Math.Abs((int)value - f);
				if (num2 < num)
				{
					num = num2;
					result = (int)value;
				}
			}
			return result;
		}
	}
}
