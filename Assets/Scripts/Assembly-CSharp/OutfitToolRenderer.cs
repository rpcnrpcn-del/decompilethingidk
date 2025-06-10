using UnityEngine;

public class OutfitToolRenderer : ToolRenderer
{
	private static int highlightShaderParamId;

	private const float HIGHLIGHT_MAGNET = 1f;

	private const float HIGHLIGHT_PHYSICAL = 1f;

	private const float HIGHLIGHT_LOCKED = 0.5f;

	private const float HIGHLIGHT_OFF = 0f;

	public ScaleAnimationControl ScaleControl { get; set; }

	public override HighlightMode Mode
	{
		get
		{
			return mode;
		}
		set
		{
			if (mode != value)
			{
				mode = value;
				float highlight = 0f;
				switch (mode)
				{
				case HighlightMode.Physical:
					highlight = 1f;
					ScaleControl.enabled = true;
					break;
				case HighlightMode.Magnet:
					highlight = 1f;
					ScaleControl.enabled = true;
					break;
				case HighlightMode.Locked:
					highlight = 0.5f;
					ScaleControl.enabled = false;
					break;
				default:
					ScaleControl.enabled = false;
					break;
				}
				ForAllMaterialsInAllRenderers(delegate(Material mat)
				{
					mat.SetFloat(highlightShaderParamId, highlight);
				});
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		highlightShaderParamId = Shader.PropertyToID("_Highlight");
	}
}
