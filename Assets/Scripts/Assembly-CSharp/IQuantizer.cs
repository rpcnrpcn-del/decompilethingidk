using UnityEngine;

public interface IQuantizer
{
	bool UseColor32 { get; }

	Color[] Palette { get; }

	void Train(Color[] colors);

	void Train(Color32[] colors);
}
