using UnityEngine;

public class PokerChip : GroupableTool
{
	public enum PokerChipValue
	{
		ONE = 0,
		FIVE = 1,
		TEN = 2,
		TWENTYFIVE = 3,
		ONEHUNDRED = 4
	}

	[Header("Poker Chip")]
	[SerializeField]
	private PokerChipValue value = PokerChipValue.FIVE;

	private PokerChipVisual visual;

	private bool wasSleeping;

	private SynchronizedField<int> _value;

	public PokerChipValue Value
	{
		get
		{
			return (PokerChipValue)_value.Get();
		}
		set
		{
			_value.ForceSet((int)value);
		}
	}

	protected override string GroupPrefabName
	{
		get
		{
			return "[PokerChipGroup]";
		}
	}

	protected override void Awake()
	{
		base.Awake();
		visual = GetComponentInChildren<PokerChipVisual>();
		_value = new SynchronizedField<int>(this, "VALUE", (int)value, SetterPermissionMode.AUTHORITY, OnValueChange);
		OnValueChange();
		base.Rigidbody.isKinematic = true;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		bool flag = base.Rigidbody.IsSleeping();
		if (!base.IsHeld && flag && !wasSleeping)
		{
			if (!base.Rigidbody.isKinematic)
			{
				base.Rigidbody.isKinematic = true;
			}
			TryAddToHoveredGroup();
		}
		wasSleeping = flag;
	}

	public override bool CanAddTool(GroupableTool tool)
	{
		return tool != null && tool.GetComponent<PokerChip>() != null;
	}

	private void OnValueChange()
	{
		visual.Value = Value;
	}
}
