using System;
using UnityEngine;

public class GroupSearchCustomGeometry : GroupSearch
{
	[SerializeField]
	private Vector3 buffer = new Vector3(0.02f, 0.02f, 0.02f);

	[NonSerialized]
	public Vector3 Center;

	[NonSerialized]
	public Vector3 Size;

	protected override void GetOverlapGeometry(out Vector3 center, out Vector3 size)
	{
		center = Center;
		size = Size + buffer;
	}
}
