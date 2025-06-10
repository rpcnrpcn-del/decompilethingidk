using UnityEngine;

public class OutlineRenderer : ToolRenderer
{
	[SerializeField]
	private GameObject highlightRenderer;

	public override HighlightMode Mode
	{
		get
		{
			return base.Mode;
		}
		set
		{
			if (value == HighlightMode.Locked)
			{
				base.Mode = HighlightMode.Locked;
			}
			else
			{
				base.Mode = HighlightMode.None;
			}
			if (highlightRenderer != null)
			{
				highlightRenderer.gameObject.SetActive(value == HighlightMode.Physical || value == HighlightMode.Magnet || value == HighlightMode.SmartTool);
			}
		}
	}

	protected override void UpdateGhostedVisuals()
	{
	}
}
