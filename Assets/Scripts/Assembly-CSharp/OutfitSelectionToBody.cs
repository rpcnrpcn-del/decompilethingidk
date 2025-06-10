using System;
using UnityEngine;

[Serializable]
public class OutfitSelectionToBody : IEquatable<OutfitSelectionToBody>
{
	public OutfitSelection selection;

	public Player.BodyPart bodyPart;

	public OutfitSelectionToBody(OutfitSelection s, Player.BodyPart b)
	{
		selection = s;
		bodyPart = b;
	}

	public bool Equals(OutfitSelectionToBody other)
	{
		return selection == other.selection && other.bodyPart == bodyPart;
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj as OutfitSelectionToBody);
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	public override string ToString()
	{
		return selection.ToString() + "," + (int)bodyPart;
	}

	public static OutfitSelectionToBody Parse(string serializedStr)
	{
		string[] array = serializedStr.Split(',');
		if (array == null || array.Length != 5)
		{
			return null;
		}
		try
		{
			OutfitSelection outfitSelection = OutfitSelection.Parse(array[0], array[1], array[2], array[3]);
			int b = int.Parse(array[4]);
			if (outfitSelection != null)
			{
				return new OutfitSelectionToBody(outfitSelection, (Player.BodyPart)b);
			}
		}
		catch
		{
			Debug.LogError("Failed to parse OutfitSelectionToBody with str:" + serializedStr);
		}
		return null;
	}
}
