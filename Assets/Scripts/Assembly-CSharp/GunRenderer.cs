using UnityEngine;

public class GunRenderer : ToolRenderer
{
	[SerializeField]
	private ModelMaterial[] lockLightRenderers;

	public override HighlightMode Mode
	{
		get
		{
			return base.Mode;
		}
		set
		{
			SetLockMaterialColor(value == HighlightMode.Locked);
			if (value != HighlightMode.Locked)
			{
				base.Mode = value;
			}
			else
			{
				mode = value;
			}
		}
	}

	private void SetLockMaterialColor(bool locked)
	{
		ModelMaterial[] array = lockLightRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			ModelMaterial modelMaterial = array[i];
			modelMaterial.Renderer.materials[modelMaterial.MaterialIndex].SetFloat("_Emit_Lerp", (!locked) ? 0f : 1f);
		}
	}
}
