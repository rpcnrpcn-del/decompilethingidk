using System.Linq;
using UnityEngine;

public class PlayingCardGroup : ToolGroup
{
	private struct LayoutTransform
	{
		public Vector3 Position;

		public Quaternion Rotation;

		public static LayoutTransform Default
		{
			get
			{
				LayoutTransform result = default(LayoutTransform);
				result.Position = Vector3.zero;
				result.Rotation = Quaternion.identity;
				return result;
			}
		}

		public void Clear()
		{
			Position = Vector3.zero;
			Rotation = Quaternion.identity;
		}

		public LayoutTransform Mirror(Vector3 surfaceNormal)
		{
			LayoutTransform result = default(LayoutTransform);
			result.Position = 2f * Vector3.Project(Position, surfaceNormal) - Position;
			result.Rotation = Quaternion.Inverse(Rotation);
			return result;
		}
	}

	[Header("Playing Card Group")]
	[SerializeField]
	private RecRoomAudioClip shuffleSound;

	[Header("Deck Select")]
	[SerializeField]
	private float deckDwellTime = 1.5f;

	[Header("Deck Offset Transforms")]
	[SerializeField]
	private Transform deckNeighborTransform;

	[SerializeField]
	private Transform deckHighlightTransform;

	[SerializeField]
	private Transform entireDeckHighlightTransform;

	[Header("Stack Offset Transforms")]
	[SerializeField]
	private Transform stackNeighborTransform;

	[SerializeField]
	private Transform stackHighlightTransform;

	[SerializeField]
	private Transform stackAfterHighlightTransform;

	[SerializeField]
	private Transform entireStackHighlightTransform;

	private bool wasSleeping;

	private GroupableTool hoveredCard;

	private bool hoverEntireGroup;

	private GroupableTool insertionPreviewCard;

	private SynchronizedField<bool> _isDeck;

	private bool dwellingOnDeck;

	private float deckDwellStartTime;

	private int hoveredCardIndex = -1;

	private LayoutTransform neighborTransform;

	private LayoutTransform defaultCardTransform;

	private LayoutTransform cardHighlightTransform;

	private LayoutTransform entireGroupHighlightTransform;

	private LayoutTransform cardBeforeHighlightTransform;

	private LayoutTransform cardAfterHighlightTransform;

	public bool IsDeck
	{
		get
		{
			return _isDeck.Get();
		}
		set
		{
			_isDeck.ForceSet(value);
		}
	}

	private bool IsUpsideDown
	{
		get
		{
			return Vector3.Dot(base.transform.up, Vector3.up) <= 0f;
		}
	}

	private GroupableTool TopCard
	{
		get
		{
			if (base.Size == 0)
			{
				return null;
			}
			if (IsUpsideDown)
			{
				return tools[0];
			}
			return tools[tools.Count - 1];
		}
	}

	private bool DwellingLongEnoughToSelectDeck
	{
		get
		{
			return dwellingOnDeck && Time.time - deckDwellStartTime >= deckDwellTime;
		}
	}

	private Vector3 RandomCardPositionOffset
	{
		get
		{
			Vector3 result = Vector3.zero;
			if (IsDeck)
			{
				Vector2 insideUnitCircle = Random.insideUnitCircle;
				result = new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y) * 0.001f;
			}
			return result;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_isDeck = new SynchronizedField<bool>(this, "IS_DECK", false, SetterPermissionMode.AUTHORITY, OnIsDeckChange);
	}

	public void AuthorityShuffle()
	{
		int[] array = tools.Select((GroupableTool x) => x.photonView.viewID).ToArray();
		UnityExtensions.ShuffleArray(array);
		AuthorityReorderTools(array);
		base.photonView.RPC("RpcPlayShuffleSound", PhotonTargets.All);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (insertionPreviewCard != null && (insertionPreviewCard.IsHeld || (insertionPreviewCard.IsInGroup && insertionPreviewCard.Group.IsHeld)))
		{
			int num = 0;
			bool flag = true;
			int desiredNewToolIndex = GetDesiredNewToolIndex(insertionPreviewCard, Vector3.zero, Quaternion.identity);
			if (desiredNewToolIndex > 0)
			{
				num = desiredNewToolIndex - 1;
				flag = false;
			}
			for (int i = 0; i < tools.Count; i++)
			{
				if (i != num)
				{
					tools[i].FrontAddHighlightEnabled = false;
					tools[i].BackAddHighlightEnabled = false;
				}
				else
				{
					tools[i].FrontAddHighlightEnabled = !flag;
					tools[i].BackAddHighlightEnabled = flag;
				}
			}
		}
		bool flag2 = base.Rigidbody.IsSleeping();
		if (!base.IsHeld && flag2 && !wasSleeping)
		{
			if (!base.Rigidbody.isKinematic)
			{
				base.Rigidbody.isKinematic = true;
			}
			if (base.hoveredGroup != null && (base.hoveredGroup.IsHeld || (base.hoveredGroup is PlayingCardGroup && (base.hoveredGroup as PlayingCardGroup).IsDeck)))
			{
				TryAddToHoveredGroup();
			}
		}
		wasSleeping = flag2;
	}

	public override void OnInputDown()
	{
		base.OnInputDown();
		if (base.hasAuthority)
		{
			IsDeck = !IsDeck;
		}
	}

	public override void OnShake()
	{
		base.OnShake();
		AuthorityShuffle();
		if (base.HolderHand != null)
		{
			base.HolderHand.Vibrate(200, 1000);
		}
	}

	public override void DisplayAddGroupHighlight(ToolGroup groupToAdd)
	{
		insertionPreviewCard = null;
		PlayingCardGroup playingCardGroup = ((!(groupToAdd != null)) ? null : groupToAdd.GetComponent<PlayingCardGroup>());
		if (playingCardGroup == null || playingCardGroup.Size == 0)
		{
			base.DisplayAddGroupHighlight(groupToAdd);
		}
		else
		{
			DisplayAddToolHighlight(playingCardGroup.tools[0]);
		}
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

	public override bool CanAddGroup(ToolGroup group)
	{
		return group != null && group.GetComponent<PlayingCardGroup>() != null;
	}

	public override bool CanAddTool(GroupableTool tool)
	{
		return tool != null && tool.GetComponent<PlayingCard>() != null;
	}

	protected override void OnCloned(ToolGroup oldGroup)
	{
		base.OnCloned(oldGroup);
		PlayingCardGroup component = oldGroup.GetComponent<PlayingCardGroup>();
		if (component != null)
		{
			IsDeck = component.IsDeck;
		}
	}

	public override GroupableTool GetHighlightedToolInGroup(GroupableTool tool, Player player, bool physicalPickup, Vector3 pickupPosition)
	{
		GroupableTool groupableTool = null;
		if (IsDeck)
		{
			return GetHighlightedToolInDeck(tool, player, physicalPickup, pickupPosition);
		}
		return GetHighlightedToolInStack(tool, player, physicalPickup, pickupPosition);
	}

	private GroupableTool GetHighlightedToolInDeck(GroupableTool tool, Player player, bool physicalPickup, Vector3 pickupPosition)
	{
		GroupableTool result = null;
		if (!base.IsHeld)
		{
			if (!dwellingOnDeck)
			{
				dwellingOnDeck = true;
				deckDwellStartTime = Time.time;
			}
			if (!DwellingLongEnoughToSelectDeck)
			{
				result = TopCard;
			}
		}
		else if (base.IsHeld && base.Owner == player && tool == TopCard)
		{
			result = tool;
		}
		return result;
	}

	private GroupableTool GetHighlightedToolInStack(GroupableTool tool, Player player, bool physicalPickup, Vector3 pickupPosition)
	{
		GroupableTool result = null;
		if (physicalPickup && base.IsHeld && base.Owner == player)
		{
			result = tool;
		}
		return result;
	}

	public override void GetHandSpacePickupTransform(Transform handTransform, PlayerHand.HandType handType, out Vector3 position, out Quaternion rotation)
	{
		base.GetHandSpacePickupTransform(handTransform, handType, out position, out rotation);
		if (IsDeck)
		{
			rotation = handTransform.InverseTransformRotation(base.transform.rotation);
		}
	}

	public override void OnHoverStart(bool physicalHover)
	{
		base.OnHoverStart(physicalHover);
		hoverEntireGroup = true;
		if (!base.DelayedUpdateCoroutineIsRunning)
		{
			UpdateGroupLayout();
		}
	}

	public override void OnHoverEnd()
	{
		base.OnHoverEnd();
		hoverEntireGroup = false;
		if (!base.DelayedUpdateCoroutineIsRunning)
		{
			UpdateGroupLayout();
		}
		if (dwellingOnDeck)
		{
			dwellingOnDeck = false;
		}
	}

	public override void OnToolInGroupHoverStart(GroupableTool tool, bool physicalHover)
	{
		base.OnToolInGroupHoverStart(tool, physicalHover);
		hoveredCard = tool;
		if (!base.DelayedUpdateCoroutineIsRunning)
		{
			UpdateGroupLayout();
		}
	}

	public override void OnToolInGroupHoverEnd(GroupableTool tool)
	{
		base.OnToolInGroupHoverEnd(tool);
		hoveredCard = null;
		if (dwellingOnDeck && !DwellingLongEnoughToSelectDeck)
		{
			dwellingOnDeck = false;
		}
		if (!base.DelayedUpdateCoroutineIsRunning)
		{
			UpdateGroupLayout();
		}
	}

	protected override void OnRemoveTool(GroupableTool tool)
	{
		base.OnRemoveTool(tool);
		if (dwellingOnDeck)
		{
			dwellingOnDeck = false;
		}
	}

	private void OnIsDeckChange()
	{
		StopDelayedUpdateOnRemove();
		UpdateGroupLayout();
		UpdateGroupCollider();
	}

	protected override int GetDesiredNewToolIndex(GroupableTool newTool, Vector3 localPosition, Quaternion localRotation)
	{
		int num = -1;
		bool isUpsideDown = IsUpsideDown;
		Vector3 rhs = ((!isUpsideDown) ? base.transform.up : (-base.transform.up));
		bool flag = Vector3.Dot(newTool.transform.position - base.transform.position, rhs) >= 0f;
		if (isUpsideDown)
		{
			return (!flag) ? tools.Count : 0;
		}
		return flag ? tools.Count : 0;
	}

	protected override void UpdateGroupLayout()
	{
		if (tools.Count <= 0)
		{
			return;
		}
		defaultCardTransform = LayoutTransform.Default;
		if (IsDeck)
		{
			neighborTransform = CreateLocalLayoutTransform(deckNeighborTransform);
			cardBeforeHighlightTransform = defaultCardTransform;
			cardAfterHighlightTransform = defaultCardTransform;
			cardHighlightTransform = CreateLocalLayoutTransform(deckHighlightTransform);
			entireGroupHighlightTransform = CreateLocalLayoutTransform(entireDeckHighlightTransform);
			if (IsUpsideDown)
			{
				cardHighlightTransform = cardHighlightTransform.Mirror(Vector3.forward);
				entireGroupHighlightTransform = entireGroupHighlightTransform.Mirror(Vector3.forward);
			}
		}
		else
		{
			neighborTransform = CreateLocalLayoutTransform(stackNeighborTransform);
			cardHighlightTransform = CreateLocalLayoutTransform(stackHighlightTransform);
			cardAfterHighlightTransform = CreateLocalLayoutTransform(stackAfterHighlightTransform);
			cardBeforeHighlightTransform = cardAfterHighlightTransform.Mirror(Vector3.forward);
			entireGroupHighlightTransform = CreateLocalLayoutTransform(entireStackHighlightTransform);
			if (IsUpsideDown)
			{
				entireGroupHighlightTransform = entireGroupHighlightTransform.Mirror(Vector3.forward);
			}
		}
		LayoutTransform groupLocalLayout = new LayoutTransform
		{
			Position = Vector3.zero,
			Rotation = Quaternion.identity
		};
		hoveredCardIndex = ((!(hoveredCard != null)) ? (-1) : tools.FindIndex((GroupableTool t) => t == hoveredCard));
		if (base.IsHeld)
		{
			int num = tools.Count / 2;
			UpdateGroupLayoutForCard(num, defaultCardTransform, ref groupLocalLayout);
			for (int num2 = num + 1; num2 < tools.Count; num2++)
			{
				UpdateGroupLayoutForCard(num2, neighborTransform, ref groupLocalLayout);
			}
			neighborTransform = neighborTransform.Mirror(Vector3.forward);
			groupLocalLayout.Clear();
			for (int num3 = num - 1; num3 >= 0; num3--)
			{
				UpdateGroupLayoutForCard(num3, neighborTransform, ref groupLocalLayout);
			}
		}
		else if (IsUpsideDown)
		{
			neighborTransform = neighborTransform.Mirror(Vector3.forward);
			UpdateGroupLayoutForCard(tools.Count - 1, defaultCardTransform, ref groupLocalLayout);
			for (int num4 = tools.Count - 2; num4 >= 0; num4--)
			{
				UpdateGroupLayoutForCard(num4, neighborTransform, ref groupLocalLayout);
			}
		}
		else
		{
			UpdateGroupLayoutForCard(0, defaultCardTransform, ref groupLocalLayout);
			for (int num5 = 1; num5 < tools.Count; num5++)
			{
				UpdateGroupLayoutForCard(num5, neighborTransform, ref groupLocalLayout);
			}
		}
	}

	private void UpdateGroupLayoutForCard(int cardIndex, LayoutTransform neighborTransform, ref LayoutTransform groupLocalLayout)
	{
		LayoutTransform thisCardLayoutTransform = GetThisCardLayoutTransform(cardIndex);
		thisCardLayoutTransform = IntegrateLocalLayoutTransform(neighborTransform, thisCardLayoutTransform, ref groupLocalLayout);
		tools[cardIndex].UpdateGroupLayout(thisCardLayoutTransform.Position + RandomCardPositionOffset, thisCardLayoutTransform.Rotation, groupLocalLayout.Position, groupLocalLayout.Rotation);
	}

	private LayoutTransform GetThisCardLayoutTransform(int thisCardIndex)
	{
		if (hoverEntireGroup)
		{
			return entireGroupHighlightTransform;
		}
		if (hoveredCardIndex < 0)
		{
			return defaultCardTransform;
		}
		if (thisCardIndex < hoveredCardIndex)
		{
			return cardBeforeHighlightTransform;
		}
		if (thisCardIndex > hoveredCardIndex)
		{
			return cardAfterHighlightTransform;
		}
		return cardHighlightTransform;
	}

	private LayoutTransform CreateLocalLayoutTransform(Transform worldTransform)
	{
		return new LayoutTransform
		{
			Position = base.transform.InverseTransformPoint(worldTransform.position),
			Rotation = base.transform.InverseTransformRotation(worldTransform.rotation)
		};
	}

	private LayoutTransform IntegrateLocalLayoutTransform(LayoutTransform neighborTransform, LayoutTransform thisCardTransform, ref LayoutTransform integratedLayoutTransform)
	{
		integratedLayoutTransform.Position = neighborTransform.Position.TransformToWorldSpace(integratedLayoutTransform.Position, integratedLayoutTransform.Rotation);
		integratedLayoutTransform.Rotation = integratedLayoutTransform.Rotation.TransformRotation(neighborTransform.Rotation);
		thisCardTransform.Position = thisCardTransform.Position.TransformToWorldSpace(integratedLayoutTransform.Position, integratedLayoutTransform.Rotation);
		thisCardTransform.Rotation = integratedLayoutTransform.Rotation.TransformRotation(thisCardTransform.Rotation);
		return thisCardTransform;
	}

	[PunRPC]
	private void RpcPlayShuffleSound()
	{
		if (shuffleSound != null)
		{
			AudioManager.Play3DSFX(shuffleSound, base.transform.position);
		}
	}
}
