using System;
using System.Collections.Generic;
using UnityEngine;

public class ColorVault : MonoBehaviour
{
	[Serializable]
	public class ColorToGuid
	{
		public string guidString;

		public Color color;
	}

	[Tooltip("Used only for decorative purposes.")]
	public string Label = "My Vault";

	public List<ColorToGuid> Colors;

	private Dictionary<string, ColorToGuid> colorMap = new Dictionary<string, ColorToGuid>();

	private void Awake()
	{
		foreach (ColorToGuid color in Colors)
		{
			if (colorMap.ContainsKey(color.guidString))
			{
				Debug.LogError(string.Concat(this, " vault has duplicate guid for colors : ", color.guidString));
			}
			else
			{
				colorMap.Add(color.guidString, color);
			}
		}
	}

	public ColorToGuid Find(string guidStr)
	{
		if (colorMap.ContainsKey(guidStr))
		{
			return colorMap[guidStr];
		}
		return null;
	}
}
