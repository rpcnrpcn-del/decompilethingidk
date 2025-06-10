using UnityEngine;

public class FlagToolRenderer : ToolRenderer
{
	[SerializeField]
	private ModelMaterial[] highlightRenderers;

	public override HighlightMode Mode
	{
		get
		{
			return mode;
		}
		set
		{
			mode = value;
			Color highlightMaterialColor = ToolRenderer.noHighlightColor;
			if (mode == HighlightMode.Physical)
			{
				highlightMaterialColor = ToolRenderer.physicalPickupHighlightColor;
			}
			else if (mode == HighlightMode.Magnet)
			{
				highlightMaterialColor = ToolRenderer.magnetPickupHighlightColor;
			}
			else if (mode == HighlightMode.Locked)
			{
				highlightMaterialColor = ToolRenderer.lockedToolPickupHighlightColor;
			}
			SetHighlightMaterialColor(highlightMaterialColor);
		}
	}

	private void SetHighlightMaterialColor(Color highlightColor)
	{
		ModelMaterial[] array = highlightRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			ModelMaterial modelMaterial = array[i];
			modelMaterial.Renderer.materials[modelMaterial.MaterialIndex].SetColor(ToolRenderer.emissionColorId, highlightColor);
		}
	}
}
