using System.Threading;
using UnityEngine;

public class TextureScale
{
	public class ThreadData
	{
		public int start;

		public int end;

		public ThreadData(int s, int e)
		{
			start = s;
			end = e;
		}
	}

	private enum Method
	{
		Point = 0,
		Bilinear = 1,
		Average = 2
	}

	private static Color[] texColors;

	private static Color[] newColors;

	private static int w;

	private static float ratioX;

	private static float ratioY;

	private static int w2;

	private static int finishCount;

	private static Mutex mutex;

	public static void Point(Texture2D tex, int newWidth, int newHeight)
	{
		ThreadedScale(tex, newWidth, newHeight, Method.Point);
	}

	public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
	{
		ThreadedScale(tex, newWidth, newHeight, Method.Bilinear);
	}

	public static void Average(Texture2D tex, int newWidth, int newHeight)
	{
		ThreadedScale(tex, newWidth, newHeight, Method.Average);
	}

	private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, Method method)
	{
		texColors = tex.GetPixels();
		newColors = new Color[newWidth * newHeight];
		switch (method)
		{
		case Method.Point:
			ratioX = (float)tex.width / (float)newWidth;
			ratioY = (float)tex.height / (float)newHeight;
			break;
		case Method.Bilinear:
			ratioX = 1f / ((float)newWidth / (float)(tex.width - 1));
			ratioY = 1f / ((float)newHeight / (float)(tex.height - 1));
			break;
		default:
			ratioX = (float)tex.width / (float)newWidth;
			ratioY = (float)tex.height / (float)newHeight;
			break;
		}
		w = tex.width;
		w2 = newWidth;
		int num = Mathf.Min(SystemInfo.processorCount, newHeight);
		int num2 = newHeight / num;
		finishCount = 0;
		if (mutex == null)
		{
			mutex = new Mutex(false);
		}
		if (num > 1)
		{
			int num3 = 0;
			ThreadData parameter;
			for (num3 = 0; num3 < num - 1; num3++)
			{
				parameter = new ThreadData(num2 * num3, num2 * (num3 + 1));
				ParameterizedThreadStart start;
				switch (method)
				{
				case Method.Point:
					start = PointScale;
					break;
				case Method.Bilinear:
					start = BilinearScale;
					break;
				default:
					start = AverageScale;
					break;
				}
				Thread thread = new Thread(start);
				thread.Start(parameter);
			}
			parameter = new ThreadData(num2 * num3, newHeight);
			switch (method)
			{
			case Method.Point:
				PointScale(parameter);
				break;
			case Method.Bilinear:
				BilinearScale(parameter);
				break;
			default:
				AverageScale(parameter);
				break;
			}
			while (finishCount < num)
			{
				Thread.Sleep(1);
			}
		}
		else
		{
			ThreadData obj = new ThreadData(0, newHeight);
			switch (method)
			{
			case Method.Point:
				PointScale(obj);
				break;
			case Method.Bilinear:
				BilinearScale(obj);
				break;
			default:
				AverageScale(obj);
				break;
			}
		}
		tex.Resize(newWidth, newHeight);
		tex.SetPixels(newColors);
		tex.Apply();
	}

	public static void BilinearScale(object obj)
	{
		ThreadData threadData = (ThreadData)obj;
		for (int i = threadData.start; i < threadData.end; i++)
		{
			int num = (int)Mathf.Floor((float)i * ratioY);
			int num2 = num * w;
			int num3 = (num + 1) * w;
			int num4 = i * w2;
			for (int j = 0; j < w2; j++)
			{
				int num5 = (int)Mathf.Floor((float)j * ratioX);
				float value = (float)j * ratioX - (float)num5;
				newColors[num4 + j] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[num2 + num5], texColors[num2 + num5 + 1], value), ColorLerpUnclamped(texColors[num3 + num5], texColors[num3 + num5 + 1], value), (float)i * ratioY - (float)num);
			}
		}
		mutex.WaitOne();
		finishCount++;
		mutex.ReleaseMutex();
	}

	public static void PointScale(object obj)
	{
		ThreadData threadData = (ThreadData)obj;
		for (int i = threadData.start; i < threadData.end; i++)
		{
			int num = (int)(ratioY * (float)i) * w;
			int num2 = i * w2;
			for (int j = 0; j < w2; j++)
			{
				newColors[num2 + j] = texColors[(int)((float)num + ratioX * (float)j)];
			}
		}
		mutex.WaitOne();
		finishCount++;
		mutex.ReleaseMutex();
	}

	private static Color GridAverage(int x, int y, float ratioX, float ratioY)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		int num5 = 0;
		int num6 = (int)(0f - ratioX) / 2;
		int num7 = (int)(0f - ratioY) / 2;
		int num8 = (int)ratioX / 2;
		int num9 = (int)ratioY / 2;
		for (int i = num6; i <= num8; i++)
		{
			for (int j = num7; j <= num9; j++)
			{
				int num10 = y + j + (int)(ratioX * (float)(x + i));
				if (num10 >= 0 && num10 < texColors.Length)
				{
					num += texColors[num10].r;
					num2 += texColors[num10].g;
					num3 += texColors[num10].b;
					num4 += texColors[num10].a;
					num5++;
				}
			}
		}
		num /= (float)num5;
		num2 /= (float)num5;
		num3 /= (float)num5;
		num4 /= (float)num5;
		return new Color(num, num2, num3, num4);
	}

	public static void AverageScale(object obj)
	{
		ThreadData threadData = (ThreadData)obj;
		for (int i = threadData.start; i < threadData.end; i++)
		{
			int y = (int)(ratioY * (float)i) * w;
			int num = i * w2;
			for (int j = 0; j < w2; j++)
			{
				newColors[num + j] = GridAverage(j, y, ratioX, ratioY);
			}
		}
		mutex.WaitOne();
		finishCount++;
		mutex.ReleaseMutex();
	}

	private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
	{
		return new Color(c1.r + (c2.r - c1.r) * value, c1.g + (c2.g - c1.g) * value, c1.b + (c2.b - c1.b) * value, c1.a + (c2.a - c1.a) * value);
	}
}
