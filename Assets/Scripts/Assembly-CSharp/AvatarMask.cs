using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RecRoom Avatar/Mask", fileName = "AvatarMask")]
public class AvatarMask : ScriptableObject
{
	public static List<AvatarMask> All = new List<AvatarMask>();

	public string Guid = string.Empty;

	public int Level;

	[Tooltip("0:Impossible, 1:Common")]
	[Range(0f, 1f)]
	public float Rarity = 1f;

	public Texture2D Mask;

	public Vector2 Tiling = new Vector2(1f, 1f);

	public Vector2 Offset = new Vector2(0f, 0f);

	private const string matMainTexName = "_MainTex";

	public AvatarMask()
	{
		if (string.IsNullOrEmpty(Guid))
		{
			Guid = System.Guid.NewGuid().ToString();
		}
		All.Add(this);
	}

	~AvatarMask()
	{
		if (All.Contains(this))
		{
			All.Remove(this);
		}
	}

	public static void Apply(AvatarMask mask, Material mat, Material defaultMat = null)
	{
		if (mask != null)
		{
			mat.SetTexture("_MainTex", mask.Mask);
			mat.SetTextureOffset("_MainTex", mask.Offset);
			mat.SetTextureScale("_MainTex", mask.Tiling);
		}
		else if (defaultMat != null)
		{
			mat.SetTexture("_MainTex", defaultMat.GetTexture("_MainTex"));
			mat.SetTextureOffset("_MainTex", defaultMat.GetTextureOffset("_MainTex"));
			mat.SetTextureScale("_MainTex", defaultMat.GetTextureScale("_MainTex"));
		}
	}
}
