using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RecRoom Avatar/Color Swatch", fileName = "AvatarColorSwatch")]
public class AvatarColorSwatch : ScriptableObject
{
	public static List<AvatarColorSwatch> All = new List<AvatarColorSwatch>();

	public string Guid = string.Empty;

	public int Level;

	[Tooltip("0:Impossible, 1:Common")]
	[Range(0f, 1f)]
	public float Rarity = 1f;

	public Color ColorBase = Color.white;

	public Color ColorR = Color.white;

	public Color ColorG = Color.white;

	public Color ColorB = Color.white;

	public Color ColorDirt = Color.white;

	public bool AddDirt;

	public Color ColorDecal = Color.white;

	private const string matDirtColName = "_Dirt_Col";

	private const string matDirtAddName = "_Dirt_Add";

	private const string matBaseColName = "_Base_Col";

	private const string matRedColName = "_Red_Col";

	private const string matGreenColName = "_Green_Col";

	private const string matBlueColName = "_Blue_Col";

	private const string matDecalColName = "_Decal_Col";

	public AvatarColorSwatch()
	{
		if (string.IsNullOrEmpty(Guid))
		{
			Guid = System.Guid.NewGuid().ToString();
		}
		All.Add(this);
	}

	~AvatarColorSwatch()
	{
		if (All.Contains(this))
		{
			All.Remove(this);
		}
	}

	public static void Apply(AvatarColorSwatch swatch, Material mat, Material defaultMat = null)
	{
		if (swatch != null)
		{
			mat.SetColor("_Base_Col", swatch.ColorBase);
			mat.SetColor("_Red_Col", swatch.ColorR);
			mat.SetColor("_Green_Col", swatch.ColorG);
			mat.SetColor("_Blue_Col", swatch.ColorB);
			mat.SetColor("_Dirt_Col", swatch.ColorDirt);
			mat.SetInt("_Dirt_Add", swatch.AddDirt ? 1 : 0);
			mat.SetColor("_Decal_Col", swatch.ColorDecal);
		}
		else if (defaultMat != null)
		{
			mat.SetColor("_Base_Col", defaultMat.GetColor("_Base_Col"));
			mat.SetColor("_Red_Col", defaultMat.GetColor("_Red_Col"));
			mat.SetColor("_Green_Col", defaultMat.GetColor("_Green_Col"));
			mat.SetColor("_Blue_Col", defaultMat.GetColor("_Blue_Col"));
			mat.SetColor("_Dirt_Col", defaultMat.GetColor("_Dirt_Col"));
			mat.SetInt("_Dirt_Add", defaultMat.GetInt("_Dirt_Add"));
			mat.SetColor("_Decal_Col", defaultMat.GetColor("_Decal_Col"));
		}
	}
}
