using System;
using UnityEngine;

public class MeleeWeaponTrail : DynamicMeshTrail
{
	[NonSerialized]
	public bool CanAddPoints = true;

	private Vector3 previousPosition;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddNewPosition(base.transform.position, base.transform.up, Time.time);
	}

	protected override void Update()
	{
		if (CanAddPoints && (base.transform.position - previousPosition).sqrMagnitude >= minSpacingSqr)
		{
			AddNewPosition(base.transform.position, base.transform.up, Time.time);
		}
		base.Update();
	}

	protected override void AddNewPosition(Vector3 newPosition, Vector3 newTangent, float addTime)
	{
		base.AddNewPosition(newPosition, newTangent, addTime);
		previousPosition = newPosition;
	}
}
