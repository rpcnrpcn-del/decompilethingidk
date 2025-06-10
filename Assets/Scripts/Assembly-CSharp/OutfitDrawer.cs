using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OutfitDrawer : MonoBehaviour
{
	[Serializable]
	public struct ItemSpacingConfig
	{
		[FormerlySerializedAs("MinItems")]
		public int AtLeastItemCount;

		public float MinItemCircleRadius;

		public float MaxItemCircleRadius;

		public int MaxItemsPerRing;

		public float RingHeightOffset;

		[Range(0f, 1f)]
		public float ItemScaleFactor;
	}

	private static List<OutfitDrawer> All = new List<OutfitDrawer>();

	public string Name = string.Empty;

	public OutfitManager.OutfitType OutfitType = OutfitManager.OutfitType.None;

	[SerializeField]
	protected Transform startTransform;

	[Header("Drawer Item Spacing")]
	[SerializeField]
	[Tooltip("The total front gap in degress. Items will be offset frontGapDegrees/2 on either side of center")]
	private int frontGapDegrees = 90;

	private static ItemSpacingConfig defaultSpacingConfig = new ItemSpacingConfig
	{
		AtLeastItemCount = 0,
		MinItemCircleRadius = 0.5f,
		MaxItemCircleRadius = 1.35f,
		MaxItemsPerRing = 15,
		RingHeightOffset = 0.7f,
		ItemScaleFactor = 1f
	};

	[SerializeField]
	private ItemSpacingConfig[] itemSpacingConfigs = new ItemSpacingConfig[1] { defaultSpacingConfig };

	[Header("Drawer UI")]
	public Button SelectButton;

	public Image NewIcon;

	public Interpolator DrawerInterpolator;

	public Vector3 DrawerOpenOffset = new Vector3(0f, 0f, -0.15f);

	private RecRoomAudioClip _pagingSound;

	private bool toolVisibilityDirty = true;

	private bool _isSelected;

	protected int maxIndex = -1;

	[HideInInspector]
	public List<OutfitTool> OutfitTools;

	private Vector3 defaultDrawerPosition = Vector3.zero;

	private Vector3 defaultButtonDrawerOffset = Vector3.zero;

	private float targetAngle;

	private float currentAngle;

	private float lastSwipeTime;

	private float swipeInterval = 0.3f;

	public ItemSpacingConfig CurrentConfig
	{
		get
		{
			int count = OutfitTools.Count;
			ItemSpacingConfig result = defaultSpacingConfig;
			for (int num = itemSpacingConfigs.Length - 1; num >= 0; num--)
			{
				ItemSpacingConfig itemSpacingConfig = itemSpacingConfigs[num];
				if (itemSpacingConfig.AtLeastItemCount < count)
				{
					result = itemSpacingConfig;
					break;
				}
			}
			return result;
		}
	}

	public float ItemScaleFactor
	{
		get
		{
			return CurrentConfig.ItemScaleFactor;
		}
	}

	public RecRoomAudioClip PagingSound
	{
		get
		{
			return _pagingSound;
		}
		set
		{
			_pagingSound = value;
		}
	}

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (_isSelected == value)
			{
				return;
			}
			_isSelected = value;
			if (_isSelected)
			{
				foreach (OutfitDrawer item in All)
				{
					if (item != this)
					{
						item.IsSelected = false;
					}
				}
				currentAngle = 0f;
				targetAngle = 0f;
				base.transform.rotation = Quaternion.identity;
			}
			toolVisibilityDirty = true;
			UpdateToolVisibility();
		}
	}

	public int OutfitCount
	{
		get
		{
			return OutfitTools.Count;
		}
	}

	public Transform StartTransform
	{
		get
		{
			return startTransform;
		}
	}

	public Vector3 OutfitSpawnPosition
	{
		get
		{
			if (SelectButton != null)
			{
				return SelectButton.transform.position + SelectButton.transform.forward * 0.1f + Vector3.up * 0.05f;
			}
			return base.transform.position;
		}
	}

	protected void Awake()
	{
		OutfitTools = new List<OutfitTool>();
		if (startTransform == null)
		{
			startTransform = base.transform;
		}
		if (DrawerInterpolator != null && SelectButton != null)
		{
			defaultDrawerPosition = DrawerInterpolator.transform.position;
			defaultButtonDrawerOffset = SelectButton.transform.position - DrawerInterpolator.transform.position;
		}
		All.Add(this);
	}

	private void Update()
	{
		if (DrawerInterpolator != null && SelectButton != null)
		{
			SelectButton.transform.position = defaultButtonDrawerOffset + DrawerInterpolator.transform.position;
		}
	}

	private void UpdateHandSwipeGesture()
	{
		if (IsSelected && Player.LocalPlayer != null && Player.LocalPlayer.IsInChangingRoom)
		{
			UpdatePerHandSwipe(Player.LocalPlayer.RightHand);
			UpdatePerHandSwipe(Player.LocalPlayer.LeftHand);
		}
		if (targetAngle != currentAngle)
		{
			currentAngle = Mathf.SmoothStep(currentAngle, targetAngle, Time.deltaTime * 8f);
			base.transform.rotation = Quaternion.AngleAxis(currentAngle, Vector3.up);
		}
	}

	private void UpdatePerHandSwipe(PlayerHand hand)
	{
		if (Time.time - lastSwipeTime > swipeInterval && hand != null && hand.IsVisible && !hand.IsHoldingTool)
		{
			Vector3 recentLinearVelocity = hand.TrackedVelocity.RecentLinearVelocity;
			float magnitude = recentLinearVelocity.magnitude;
			float num = Vector3.Angle(hand.PalmDirection, recentLinearVelocity);
			float num2 = Vector3.Angle(Vector3.up, recentLinearVelocity);
			float num3 = Vector3.Angle(Vector3.up, hand.transform.forward);
			if (magnitude > 2f && num <= 30f && Mathf.Abs(90f - num2) <= 15f && Mathf.Abs(90f - num3) <= 15f)
			{
				float num4 = Mathf.Sign(Vector3.Dot(Player.LocalPlayer.Head.transform.right, recentLinearVelocity)) * 90f;
				targetAngle += num4;
				lastSwipeTime = Time.time;
				AudioManager.Play3DSFX(_pagingSound, base.transform.position);
			}
		}
	}

	protected void OnDestroy()
	{
		All.Remove(this);
	}

	public void UpdateToolVisibility()
	{
		if (toolVisibilityDirty)
		{
			StopAllCoroutines();
			StartCoroutine(RunUpdateToolVisibility());
			toolVisibilityDirty = false;
		}
	}

	private IEnumerator RunUpdateToolVisibility()
	{
		if (DrawerInterpolator != null)
		{
			DrawerInterpolator.TargetPosition = defaultDrawerPosition + ((!_isSelected) ? Vector3.zero : DrawerOpenOffset);
		}
		if (!IsSelected)
		{
			foreach (OutfitTool item in OutfitTools)
			{
				item.CurrentState = OutfitTool.OutfitToolState.AnimatingOut;
				yield return null;
			}
			yield break;
		}
		if (SelectButton != null)
		{
			for (int i = OutfitTools.Count - 1; i >= 0; i--)
			{
				OutfitTools[i].transform.position = OutfitSpawnPosition;
				OutfitTools[i].CurrentState = OutfitTool.OutfitToolState.AnimatingIn;
				yield return new WaitForSeconds(0.075f);
			}
		}
		else
		{
			for (int num = OutfitTools.Count - 1; num >= 0; num--)
			{
				OutfitTools[num].CurrentState = OutfitTool.OutfitToolState.InOpenDrawer;
			}
		}
	}

	public void UpdateNewIcon()
	{
		if (!(NewIcon != null))
		{
			return;
		}
		bool active = false;
		foreach (OutfitTool outfitTool in OutfitTools)
		{
			if (outfitTool.IsNew)
			{
				active = true;
				break;
			}
		}
		NewIcon.gameObject.SetActive(active);
	}

	public Vector3 GetPosition(OutfitItem outfitItem, int index, int maxCount)
	{
		float num = 0f;
		if (outfitItem != null)
		{
			outfitItem.RackIndex = index;
			num = outfitItem.RackHeightOffset;
		}
		int num2 = maxCount / CurrentConfig.MaxItemsPerRing;
		float t = Mathf.Clamp((float)maxCount / (float)CurrentConfig.MaxItemsPerRing, 0f, 1f);
		float num3 = Mathf.Lerp(CurrentConfig.MinItemCircleRadius, CurrentConfig.MaxItemCircleRadius, t);
		int num4 = index / CurrentConfig.MaxItemsPerRing;
		float num5 = (360 - frontGapDegrees) / CurrentConfig.MaxItemsPerRing;
		Quaternion quaternion = Quaternion.AngleAxis((float)(index % CurrentConfig.MaxItemsPerRing) * num5 + num5 * 0.5f + (float)(frontGapDegrees / 2), Vector3.up);
		int num6 = num2 / 2;
		int num7 = num4 - num6;
		Vector3 vector = Vector3.up * (CurrentConfig.RingHeightOffset * (float)num7);
		Vector3 position = startTransform.position + quaternion * (base.transform.forward * num3) + num * Vector3.up + vector;
		return StartTransform.InverseTransformPoint(position);
	}

	private void OnDrawGizmosSelected()
	{
		Quaternion quaternion = Quaternion.AngleAxis(frontGapDegrees / 2, Vector3.up);
		Quaternion quaternion2 = Quaternion.AngleAxis(-(frontGapDegrees / 2), Vector3.up);
		Debug.DrawLine(startTransform.position, startTransform.position + quaternion * (base.transform.forward * CurrentConfig.MaxItemCircleRadius), Color.green);
		Debug.DrawLine(startTransform.position, startTransform.position + quaternion2 * (base.transform.forward * CurrentConfig.MaxItemCircleRadius), Color.green);
		Gizmos.DrawWireSphere(startTransform.position, CurrentConfig.MaxItemCircleRadius);
	}

	protected Quaternion GetRotation(OutfitItem outfitItem, Vector3 position)
	{
		Vector3 vector = StartTransform.TransformPoint(position);
		Vector3 forward = startTransform.position - vector;
		return Quaternion.LookRotation(forward);
	}

	public bool IsOutfitToolVisible(OutfitTool tool)
	{
		return IsSelected;
	}

	public void UpdateOutfitPlacement()
	{
		for (int i = 0; i < OutfitTools.Count; i++)
		{
			OutfitTools[i].DrawerIndex = i;
			OutfitTools[i].DrawerPosition = GetPosition(OutfitTools[i].OutfitTarget.outfitItem, i, OutfitTools.Count);
			OutfitTools[i].DrawerRotation = GetRotation(OutfitTools[i].OutfitTarget.outfitItem, OutfitTools[i].DrawerPosition);
		}
		UpdateNewIcon();
	}

	public void AddSelection(OutfitTool outfitTool)
	{
		outfitTool.transform.position = startTransform.position;
		outfitTool.transform.rotation = startTransform.rotation;
		outfitTool.transform.SetParent(StartTransform, true);
		outfitTool.Drawer = this;
		OutfitTools.Add(outfitTool);
		toolVisibilityDirty = true;
		UpdateToolVisibility();
	}
}
