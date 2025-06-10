using UnityEngine;

public class PokerChipVisual : MonoBehaviour
{
	[SerializeField]
	private Renderer[] chipRenderers;

	public PokerChip.PokerChipValue Value
	{
		set
		{
			Color color = PokerChipVisualSettings.LoadChipColor(value);
			for (int i = 0; i < chipRenderers.Length; i++)
			{
				chipRenderers[i].material.SetColor("_Green_Col", color);
				chipRenderers[i].material.SetColor("_Base_Col", color);
			}
		}
	}
}
