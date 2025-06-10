using System;
using UnityEngine.Serialization;

[Serializable]
public class OutfitSelection : IEquatable<OutfitSelection>
{
	public string name;

	[FormerlySerializedAs("outfit")]
	public OutfitItem outfitItem;

	public AvatarColorSwatch colorSwatch;

	public AvatarMask mask;

	public AvatarDecal decal;

	public bool IsNew { get; set; }

	public string FriendlyName
	{
		get
		{
			string text = outfitItem.name;
			if (mask != null)
			{
				text = text + " " + mask.name;
			}
			if (colorSwatch != null)
			{
				text = text + " " + colorSwatch.name;
			}
			if (decal != null)
			{
				text = text + " " + decal.name;
			}
			return text;
		}
	}

	public int? UnlockedLevel { get; set; }

	public int Level
	{
		get
		{
			return (!UnlockedLevel.HasValue) ? (outfitItem.Level + ((colorSwatch != null) ? colorSwatch.Level : 0) + ((mask != null) ? mask.Level : 0) + ((decal != null) ? decal.Level : 0)) : UnlockedLevel.Value;
		}
	}

	private OutfitSelection()
	{
	}

	public OutfitSelection(OutfitItem o, AvatarColorSwatch cs = null, AvatarMask m = null, AvatarDecal d = null)
	{
		name = o.name;
		outfitItem = o;
		colorSwatch = cs;
		mask = m;
		decal = d;
		IsNew = false;
	}

	public bool Equals(OutfitSelection other)
	{
		return other != null && other.outfitItem != null && outfitItem != null && outfitItem.Guid == other.outfitItem.Guid && ((colorSwatch == null && other.colorSwatch == null) || (colorSwatch != null && other.colorSwatch != null && colorSwatch.Guid == other.colorSwatch.Guid)) && ((mask == null && other.mask == null) || (mask != null && other.mask != null && mask.Guid == other.mask.Guid)) && ((decal == null && other.decal == null) || (decal != null && other.decal != null && decal.Guid == other.decal.Guid));
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj as OutfitSelection);
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	public static bool operator !=(OutfitSelection os1, OutfitSelection os2)
	{
		if (object.ReferenceEquals(os1, null))
		{
			return !object.ReferenceEquals(os2, null);
		}
		return !os1.Equals(os2);
	}

	public static bool operator ==(OutfitSelection os1, OutfitSelection os2)
	{
		if (object.ReferenceEquals(os1, null))
		{
			return object.ReferenceEquals(os2, null);
		}
		return os1.Equals(os2);
	}

	public override string ToString()
	{
		return string.Format("{0},{1},{2},{3}", outfitItem.Guid, (!(colorSwatch != null)) ? string.Empty : colorSwatch.Guid, (!(mask != null)) ? string.Empty : mask.Guid, (!(decal != null)) ? string.Empty : decal.Guid);
	}

	public static OutfitSelection Parse(string desc)
	{
		if (string.IsNullOrEmpty(desc))
		{
			return null;
		}
		string[] array = desc.Split(',');
		if (array == null || array.Length != 4)
		{
			return null;
		}
		return Parse(array[0], array[1], array[2], array[3]);
	}

	public static OutfitSelection Parse(string o, string cs, string m, string d)
	{
		Guid oGuid = new Guid(o);
		OutfitItem outfitItem = OutfitManager.Instance.OutfitPrefabs.Find((OutfitItem x) => x.Guid == oGuid);
		if (outfitItem != null)
		{
			AvatarColorSwatch cs2 = ((!string.IsNullOrEmpty(cs)) ? AvatarColorSwatch.All.Find((AvatarColorSwatch x) => x.Guid == cs) : null);
			AvatarMask m2 = ((!string.IsNullOrEmpty(m)) ? AvatarMask.All.Find((AvatarMask x) => x.Guid == m) : null);
			AvatarDecal d2 = ((!string.IsNullOrEmpty(d)) ? AvatarDecal.All.Find((AvatarDecal x) => x.Guid == d) : null);
			return new OutfitSelection(outfitItem, cs2, m2, d2);
		}
		return null;
	}
}
