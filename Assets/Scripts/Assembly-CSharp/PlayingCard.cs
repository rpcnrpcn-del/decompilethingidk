using UnityEngine;

public class PlayingCard : GroupableTool
{
	public enum PlayingCardValue
	{
		ACE = 0,
		TWO = 1,
		THREE = 2,
		FOUR = 3,
		FIVE = 4,
		SIX = 5,
		SEVEN = 6,
		EIGHT = 7,
		NINE = 8,
		TEN = 9,
		JACK = 10,
		QUEEN = 11,
		KING = 12
	}

	public enum PlayingCardSuit
	{
		HEARTS = 0,
		SPADES = 1,
		CLUBS = 2,
		DIAMONDS = 3
	}

	[Header("Playing Card")]
	[SerializeField]
	private PlayingCardSuit suit = PlayingCardSuit.CLUBS;

	[SerializeField]
	private PlayingCardValue value;

	private SynchronizedField<int> _suit;

	private SynchronizedField<int> _value;

	private PlayingCardVisual visual;

	private GroupableTool insertionPreviewCard;

	private bool wasSleeping;

	public PlayingCardSuit Suit
	{
		get
		{
			return (PlayingCardSuit)_suit.Get();
		}
		set
		{
			_suit.ForceSet((int)value);
		}
	}

	public PlayingCardValue Value
	{
		get
		{
			return (PlayingCardValue)_value.Get();
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
			return "[PlayingCardGroup]";
		}
	}

	protected override void Awake()
	{
		base.Awake();
		visual = GetComponentInChildren<PlayingCardVisual>();
		_suit = new SynchronizedField<int>(this, "SUIT", (int)suit, SetterPermissionMode.MASTER_OR_AUTHORITY, OnSuitChange);
		_value = new SynchronizedField<int>(this, "VALUE", (int)value, SetterPermissionMode.AUTHORITY, OnValueChange);
		OnSuitChange();
		OnValueChange();
		base.Rigidbody.isKinematic = true;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (insertionPreviewCard != null && insertionPreviewCard.IsHeld)
		{
			bool flag = (base.FrontAddHighlightEnabled = IsAbove(insertionPreviewCard));
			base.BackAddHighlightEnabled = !flag;
		}
		bool flag3 = base.Rigidbody.IsSleeping();
		if (!base.IsHeld && flag3 && !wasSleeping)
		{
			if (!base.Rigidbody.isKinematic)
			{
				base.Rigidbody.isKinematic = true;
			}
			if (base.hoveredGroup != null && ((base.hoveredGroup is PlayingCardGroup && (base.hoveredGroup as PlayingCardGroup).IsDeck) || (base.hoveredGroupableTool is PlayingCard && (base.hoveredGroupableTool as PlayingCard).IsHeld)))
			{
				TryAddToHoveredGroup();
			}
		}
		wasSleeping = flag3;
	}

	public override bool CanAddTool(GroupableTool tool)
	{
		return tool != null && tool.GetComponent<PlayingCard>() != null;
	}

	public override void DisplayAddToolHighlight(GroupableTool toolToAdd)
	{
		insertionPreviewCard = null;
		if (toolToAdd == null)
		{
			base.DisplayAddToolHighlight(toolToAdd);
		}
		else
		{
			insertionPreviewCard = toolToAdd;
		}
	}

	private bool IsAbove(GroupableTool newTool)
	{
		return Vector3.Dot(newTool.transform.position - base.transform.position, base.transform.up) >= 0f;
	}

	private void OnSuitChange()
	{
		visual.Suit = Suit;
	}

	private void OnValueChange()
	{
		visual.Value = Value;
	}
}
