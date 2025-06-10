using UnityEngine;

public class GameUITeamDataModel
{
	public bool Active;

	public bool IsLocal;

	public Color Color = Color.white;

	public int Score;

	public GameUIPlayerDataModel[] PlayerModels;

	public static void DeepCopy(GameUITeamDataModel source, GameUITeamDataModel dest)
	{
		dest.Active = source.Active;
		dest.IsLocal = source.IsLocal;
		dest.Color = source.Color;
		dest.Score = source.Score;
		if (dest.PlayerModels == null || dest.PlayerModels.Length != source.PlayerModels.Length)
		{
			dest.PlayerModels = new GameUIPlayerDataModel[source.PlayerModels.Length];
		}
		for (int i = 0; i < source.PlayerModels.Length; i++)
		{
			if (dest.PlayerModels[i] == null)
			{
				dest.PlayerModels[i] = new GameUIPlayerDataModel();
			}
			GameUIPlayerDataModel.DeepCopy(source.PlayerModels[i], dest.PlayerModels[i]);
		}
	}
}
