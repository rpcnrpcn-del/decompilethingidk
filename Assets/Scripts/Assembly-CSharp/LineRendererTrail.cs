using UnityEngine;

public class LineRendererTrail : CustomTrail<LineRenderer>
{
	private Color _color = Color.white;

	private Vector2 _widths;

	public override Color Color
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
			if (renderers != null)
			{
				Color color = value;
				color.a = alphas.x * fadeAlpha;
				Color color2 = value;
				color2.a = alphas.y * fadeAlpha;
				for (int i = 0; i < renderers.Length; i++)
				{
					renderers[i].startColor = color;
					renderers[i].endColor = color2;
				}
				if (displayStartCap && startCap != null)
				{
					startCap.material.SetColor(CustomTrail<LineRenderer>.ParticleMaterialColorId, color);
				}
				if (displayEndCap && endCap != null)
				{
					endCap.material.SetColor(CustomTrail<LineRenderer>.ParticleMaterialColorId, color2);
				}
			}
		}
	}

	public override Vector2 Widths
	{
		get
		{
			return _widths;
		}
		set
		{
			_widths = value;
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].startWidth = _widths[0] * widthScale;
				renderers[i].endWidth = _widths[1] * widthScale;
			}
			if (displayStartCap && startCap != null)
			{
				float num = _widths[0] * widthScale / 2f / startCap.transform.lossyScale.x;
				startCap.transform.localScale *= num;
			}
			if (displayEndCap && endCap != null)
			{
				float num2 = _widths[1] * widthScale / 2f / endCap.transform.lossyScale.x;
				endCap.transform.localScale *= num2;
			}
		}
	}

	protected override void ApplyPoints()
	{
		if (renderers != null)
		{
			Vector2 mainTextureScale = new Vector2(base.Length, 1f);
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].numPositions = points.Count;
				renderers[i].SetPositions(points.ToArray());
				renderers[i].material.mainTextureScale = mainTextureScale;
			}
		}
	}
}
