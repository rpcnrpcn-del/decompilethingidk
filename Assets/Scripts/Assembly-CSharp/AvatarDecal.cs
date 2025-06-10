using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RecRoom Avatar/Decal", fileName = "AvatarDecal")]
public class AvatarDecal : ScriptableObject
{
	public static List<AvatarDecal> All = new List<AvatarDecal>();

	public string Guid = string.Empty;

	public int Level;

	[Tooltip("0:Impossible, 1:Common")]
	[Range(0f, 1f)]
	public float Rarity = 1f;

	public Texture2D Decal;

	public Vector2 Tiling = new Vector2(1f, 1f);

	public Vector2 Offset = new Vector2(0f, 0f);

	private const string matDecalName = "_Decal_Tex";

	public AvatarDecal()
	{
		if (string.IsNullOrEmpty(Guid))
		{
			Guid = System.Guid.NewGuid().ToString();
		}
		All.Add(this);
	}

	~AvatarDecal()
	{
		if (All.Contains(this))
		{
			All.Remove(this);
		}
	}

	public static void Apply(AvatarDecal decal, Material material, Material defaultMat = null)
	{
		if (decal != null)
		{
			material.SetTexture("_Decal_Tex", decal.Decal);
			material.SetTextureOffset("_Decal_Tex", decal.Offset);
			material.SetTextureScale("_Decal_Tex", decal.Tiling);
		}
		else if (defaultMat != null)
		{
			material.SetTexture("_Decal_Tex", defaultMat.GetTexture("_Decal_Tex"));
			material.SetTextureOffset("_Decal_Tex", defaultMat.GetTextureOffset("_Decal_Tex"));
			material.SetTextureScale("_Decal_Tex", defaultMat.GetTextureScale("_Decal_Tex"));
		}
	}
}
