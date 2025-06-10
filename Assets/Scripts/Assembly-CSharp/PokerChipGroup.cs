using UnityEngine;

public class PokerChipGroup : ToolGroup
{
	[SerializeField]
	private Transform stackOffsetTransform;

	[SerializeField]
	private float minDeckDrawChipDistance = 0.5f;

	private bool tryToDropChip;

	private bool wasSleeping;

	private GroupableTool insertionPreviewChip;

	private bool IsUpsideDown
	{
		get
		{
			return Vector3.Dot(base.transform.up, Vector3.up) <= 0f;
		}
	}

	private GroupableTool TopChip
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

	private GroupableTool BottomChip
	{
		get
		{
			if (base.Size == 0)
			{
				return null;
			}
			if (IsUpsideDown)
			{
				return tools[tools.Count - 1];
			}
			return tools[0];
		}
	}

	private Vector3 RandomCardPositionOffset
	{
		get
		{
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			return new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y) * 0.001f;
		}
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (insertionPreviewChip != null && (insertionPreviewChip.IsHeld || (insertionPreviewChip.IsInGroup && insertionPreviewChip.Group.IsHeld)))
		{
			int num = 0;
			bool flag = true;
			int desiredNewToolIndex = GetDesiredNewToolIndex(insertionPreviewChip, Vector3.zero, Quaternion.identity);
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
			TryAddToHoveredGroup();
		}
		wasSleeping = flag2;
		if (tryToDropChip)
		{
			GroupableTool bottomChip = BottomChip;
			if (bottomChip != null)
			{
				AuthorityRemoveTool(bottomChip);
			}
			tryToDropChip = false;
		}
	}

	public override void OnInputDown()
	{
		base.OnInputDown();
		if (base.hasAuthority)
		{
			tryToDropChip = true;
		}
	}

	public override bool CanAddGroup(ToolGroup group)
	{
		return group != null && group.GetComponent<PokerChipGroup>() != null;
	}

	public override bool CanAddTool(GroupableTool tool)
	{
		return tool != null && tool.GetComponent<PokerChip>() != null;
	}

	public override GroupableTool GetHighlightedToolInGroup(GroupableTool tool, Player player, bool physicalPickup, Vector3 pickupPosition)
	{
		GroupableTool result = null;
		if (!base.IsHeld)
		{
			if ((pickupPosition - base.transform.position).sqrMagnitude >= minDeckDrawChipDistance * minDeckDrawChipDistance)
			{
				result = TopChip;
			}
		}
		else if (base.IsHeld && base.Owner == player && tool == TopChip)
		{
			result = tool;
		}
		return result;
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
		if (tools.Count > 0)
		{
			Vector3 vector = base.transform.InverseTransformPoint(stackOffsetTransform.position);
			Quaternion rotation = base.transform.InverseTransformRotation(stackOffsetTransform.rotation);
			Vector3 vector2 = Vector3.zero;
			Quaternion quaternion = Quaternion.identity;
			int num = tools.Count / 2;
			for (int i = num; i < tools.Count; i++)
			{
				tools[i].UpdateGroupLayout(vector2 + RandomCardPositionOffset, quaternion, vector2, quaternion);
				vector2 = vector.TransformToWorldSpace(vector2, quaternion);
				quaternion = quaternion.TransformRotation(rotation);
			}
			vector = -1f * vector;
			rotation = Quaternion.Inverse(rotation);
			vector2 = Vector3.zero;
			quaternion = Quaternion.identity;
			for (int num2 = num - 1; num2 >= 0; num2--)
			{
				vector2 = vector.TransformToWorldSpace(vector2, quaternion);
				quaternion = quaternion.TransformRotation(rotation);
				tools[num2].UpdateGroupLayout(vector2 + RandomCardPositionOffset, quaternion, vector2, quaternion);
			}
		}
	}

	public override void OnToolInGroupHoverStart(GroupableTool tool, bool physicalHover)
	{
		base.OnToolInGroupHoverStart(tool, physicalHover);
		if (!base.DelayedUpdateCoroutineIsRunning)
		{
			UpdateGroupLayout();
		}
	}

	public override void OnToolInGroupHoverEnd(GroupableTool tool)
	{
		base.OnToolInGroupHoverEnd(tool);
		if (!base.DelayedUpdateCoroutineIsRunning)
		{
			UpdateGroupLayout();
		}
	}
}
