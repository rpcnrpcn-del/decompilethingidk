using UnityEngine;
using UnityEngine.UI;

public class GameScoreboardPlayerStatColumnView : GameScoreboardElementView
{
	[SerializeField]
	private Text statText;

	public void SetHeaderModel(string headerText)
	{
		statText.text = headerText.ToUpper();
		ApplyColors(false);
	}

	public void SetStatModel(int stat, bool isActive, bool isLocal)
	{
		statText.text = ((!isActive) ? "-" : stat.ToString());
		ApplyColors(isActive && isLocal);
	}
}
