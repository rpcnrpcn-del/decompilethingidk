using System;

public class GameUIPlayerDataModel
{
	public bool Active;

	public bool IsLocal;

	public string Name = string.Empty;

	public int[] Stats;

	public static void DeepCopy(GameUIPlayerDataModel source, GameUIPlayerDataModel dest)
	{
		dest.Active = source.Active;
		dest.IsLocal = source.IsLocal;
		dest.Name = source.Name;
		if (dest.Stats == null || dest.Stats.Length != source.Stats.Length)
		{
			dest.Stats = new int[source.Stats.Length];
		}
		Array.Copy(source.Stats, dest.Stats, source.Stats.Length);
	}
}
