using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonalBubble : MonoBehaviour
{
	[Flags]
	public enum BubbleMask
	{
		Hand_Local = 1,
		Hand_Remote = 2,
		Tool = 4
	}

	[Flags]
	public enum ToolMask
	{
		Ball = 1,
		BoxingGlove = 2,
		Bucket = 4,
		Card = 8,
		CardBox = 0x10,
		Clipboard = 0x20,
		Cone = 0x40,
		Dart = 0x80,
		Extrusion = 0x100,
		DiscGolfDisc = 0x200,
		Gun = 0x400,
		HandheldCameraTool = 0x800,
		HelpClipboard = 0x1000,
		LaundryBin = 0x2000,
		MeshExtruderTool = 0x4000,
		OutfitTool = 0x8000,
		PaddleballBall = 0x10000,
		PaddleballPaddle = 0x20000,
		Radio = 0x40000,
		WaterBottle = 0x80000,
		WaterJug = 0x100000
	}

	[SerializeField]
	[EnumFlags]
	private BubbleMask bubbleFlags;

	[SerializeField]
	[EnumFlags]
	private ToolMask excludedTools;

	[Tooltip("How long after leaving this zone we can show your hand back again? In seconds!")]
	[SerializeField]
	private float turnVisibleTimeout = 0.15f;

	[SerializeField]
	private BoxCollider boxCollider;

	private Player thisPlayer;

	private GameManager gameManager;

	private Vector3 boxSize = Vector3.one;

	private const int MAX_RAYCAST_HIT = 256;

	private static Collider[] hits = new Collider[256];

	private Dictionary<Type, ToolMask> toolMaskMap;

	private void Awake()
	{
		thisPlayer = GetComponentInParent<Player>();
		boxSize = boxCollider.size.MultiplyComponents(boxCollider.transform.lossyScale) / 2f;
		InitToolMaskMap();
	}

	private void Start()
	{
		gameManager = RecRoomSceneManager.Instance.GameManager;
		if (PUNNetworkManager.Instance.IsInDormRoom)
		{
			base.enabled = false;
		}
	}

	private void FixedUpdate()
	{
		if (SingletonMonoBehaviour<SettingsManager>.Instance.PersonalBubble && (gameManager == null || gameManager.CurrentState != GameStates.GAME_RUNNING) && Player.LocalPlayer != null)
		{
			Vector3 center = boxCollider.transform.TransformPoint(boxCollider.center);
			int num = Physics.OverlapBoxNonAlloc(center, boxSize, hits, boxCollider.transform.rotation, 227948032);
			for (int i = 0; i < num; i++)
			{
				CheckCollider(hits[i]);
			}
		}
	}

	private void CheckCollider(Collider col)
	{
		GameObject gameObject = col.gameObject;
		bool flag = IsMaskActive(BubbleMask.Hand_Local);
		bool flag2 = IsMaskActive(BubbleMask.Hand_Remote);
		bool flag3 = IsMaskActive(BubbleMask.Tool);
		if ((flag && gameObject.IsInLayerMask(LayerMasks.LocalPlayerPhysics)) || (flag2 && gameObject.IsInLayerMask(LayerMasks.RemotePlayerPhysics)))
		{
			PlayerCollider component = gameObject.GetComponent<PlayerCollider>();
			if (component != null && component.ThisPlayer != null && component.BodyPart.IsHand())
			{
				bool isLocal = component.ThisPlayer.isLocal;
				if ((isLocal && flag) || (!isLocal && flag2))
				{
					PlayerHand playerHand = ((component.BodyPart != Player.BodyPart.LeftHand) ? component.ThisPlayer.RightHand : component.ThisPlayer.LeftHand);
					playerHand.SetInPersonalBubble(turnVisibleTimeout);
				}
			}
		}
		else if (flag3 && gameObject.IsInLayerMask(LayerMasks.ToolPhysics))
		{
			ToolCollider component2 = gameObject.GetComponent<ToolCollider>();
			if (component2 != null && component2.Tool != null && !IsToolExcluded(component2.Tool))
			{
				bool inPersonalBubble = !thisPlayer.isLocal || IsMaskActive(BubbleMask.Hand_Local);
				component2.Tool.InPersonalBubble = inPersonalBubble;
			}
		}
	}

	private bool IsMaskActive(BubbleMask mask)
	{
		return (mask & bubbleFlags) == mask;
	}

	private bool IsToolExcluded(Tool tool)
	{
		ToolMask toolMask = ToToolMask(tool);
		return toolMask != 0 && (toolMask & excludedTools) == toolMask;
	}

	private ToolMask ToToolMask(Tool tool)
	{
		Type type = tool.GetType();
		ToolMask value = (ToolMask)0;
		toolMaskMap.TryGetValue(type, out value);
		return value;
	}

	private void InitToolMaskMap()
	{
		if (toolMaskMap != null)
		{
			return;
		}
		toolMaskMap = new Dictionary<Type, ToolMask>();
		foreach (ToolMask value in Enum.GetValues(typeof(ToolMask)))
		{
			string typeName = value.ToString();
			Type type = Type.GetType(typeName);
			if (type != null && !toolMaskMap.ContainsKey(type))
			{
				toolMaskMap.Add(type, value);
			}
		}
	}
}
