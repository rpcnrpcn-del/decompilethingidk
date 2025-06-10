using System;
using System.Collections;
using System.Collections.Generic;
using GAMiniJSON;
using Photon;
using RecNet;
using UnityEngine;
using UnityEngine.UI;

public class GiftBox : Photon.MonoBehaviour
{
	public enum GiftBoxState
	{
		Closed = 0,
		Opening = 1,
		Open = 2,
		Failed = 3
	}

	public enum GiftBoxContents
	{
		Unspecified = -1,
		XP = 0,
		RepeatItem = 1,
		NewItem = 2
	}

	[SerializeField]
	private PooledParticle xpBoxOpenedParticlePrefab;

	[SerializeField]
	private PooledParticle itemBoxOpenedParticlePrefab;

	[SerializeField]
	private Transform boxOpenedParticleRoot;

	[SerializeField]
	private Transform toolAnchor;

	[SerializeField]
	private Text xpText;

	[SerializeField]
	private Text nameTagText;

	[SerializeField]
	private float remotePlayerXpBonusDistance = 7f;

	[SerializeField]
	private Animator boxAnimator;

	[SerializeField]
	private Animator toolAnchorAnimator;

	[SerializeField]
	private float timeBeforeDerez = 2f;

	[SerializeField]
	private RecRoomAudioClip boxAppearAudio;

	[Tooltip("XP box | Already owned item | New item")]
	[SerializeField]
	private RecRoomAudioClip[] boxOpenAudio = new RecRoomAudioClip[3];

	private Avatars.GiftPackage giftPackage;

	private TearStripTab tearStrip;

	private PhotonPlayer giftOwner;

	private OutfitTool giftTool;

	private SynchronizedField<int> _boxState;

	private SynchronizedField<int> _boxContents;

	public Transform ToolAnchor
	{
		get
		{
			return toolAnchor;
		}
	}

	public bool IsOwnerLocal
	{
		get
		{
			return giftOwner != null && giftOwner.isLocal;
		}
	}

	public OutfitSelection OutfitSelection { get; private set; }

	public GiftBoxState BoxState
	{
		get
		{
			return (GiftBoxState)_boxState.Get();
		}
		private set
		{
			_boxState.ForceSet((int)value);
		}
	}

	public GiftBoxContents BoxContents
	{
		get
		{
			return (GiftBoxContents)_boxContents.Get();
		}
		private set
		{
			_boxContents.ForceSet((int)value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_boxState = new SynchronizedField<int>(this, "GIFTBOX_STATE", 0, SetterPermissionMode.AUTHORITY, OnBoxStateChanged);
		_boxContents = new SynchronizedField<int>(this, "GIFTBOX_CONTENTS", -1, SetterPermissionMode.AUTHORITY);
		object[] instantiationData = base.photonView.instantiationData;
		giftOwner = (PhotonPlayer)instantiationData[0];
		string json = (string)instantiationData[1];
		try
		{
			Dictionary<string, object> dict = Json.Deserialize(json) as Dictionary<string, object>;
			giftPackage = new Avatars.GiftPackage();
			giftPackage.Deserialize(dict);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			giftPackage = null;
		}
		if (giftPackage != null)
		{
			OutfitSelection = OutfitSelection.Parse(giftPackage.AvatarItemDesc);
			if (IsOwnerLocal)
			{
				if (OutfitSelection != null)
				{
					OutfitManager.Instance.LocalSpawnOutfitTool(OutfitSelection, OutfitTool.ToolPurpose.GiftBox);
					BoxContents = (OutfitManager.Instance.UnlockedOutfitSelections.Contains(OutfitSelection) ? GiftBoxContents.RepeatItem : GiftBoxContents.NewItem);
				}
				else
				{
					BoxContents = GiftBoxContents.XP;
				}
			}
		}
		tearStrip = GetComponentInChildren<TearStripTab>();
		if (tearStrip != null)
		{
			tearStrip.TornOff += OnStripTornOff;
		}
		BoxState = GiftBoxState.Closed;
		if (xpText != null)
		{
			xpText.text = string.Empty;
		}
		if (nameTagText != null)
		{
			nameTagText.text = giftOwner.name.Truncate(16, "...");
		}
	}

	private void Start()
	{
		if (boxOpenAudio != null)
		{
			AudioManager.Play3DSFX(boxAppearAudio, base.transform);
		}
	}

	protected override void OnDestroy()
	{
		if (tearStrip != null)
		{
			tearStrip.TornOff -= OnStripTornOff;
		}
		if (giftTool != null && giftTool.IsWearerLocal)
		{
			giftTool.PickupAttemptEvent -= OnOutfitToolPickupAttempt;
		}
		base.OnDestroy();
	}

	private void OnBoxStateChanged()
	{
		boxAnimator.SetInteger("State", (int)BoxState);
		switch (BoxState)
		{
		case GiftBoxState.Opening:
			if (IsOwnerLocal)
			{
				StartCoroutine(RunOpenBox());
			}
			if (nameTagText != null)
			{
				nameTagText.enabled = false;
			}
			break;
		case GiftBoxState.Open:
		case GiftBoxState.Failed:
			if (BoxState == GiftBoxState.Open)
			{
				ApplyGiftPackageXp();
			}
			toolAnchorAnimator.SetTrigger("Spawn");
			giftTool = OutfitTool.Find(giftOwner, OutfitSelection, OutfitTool.ToolPurpose.GiftBox);
			if (giftTool != null)
			{
				giftTool.Box = this;
				giftTool.OnlyOwnerCanPickup = true;
				giftTool.CurrentState = OutfitTool.OutfitToolState.InOpenBox;
				if (IsOwnerLocal)
				{
					giftTool.PickupAttemptEvent += OnOutfitToolPickupAttempt;
				}
			}
			else
			{
				if (xpText != null && giftPackage != null)
				{
					xpText.text = giftPackage.Xp + " XP";
					xpText.canvas.worldCamera = ViveControllerInput.Instance.ControllerCamera;
				}
				if (IsOwnerLocal)
				{
					StartCoroutine(DismissBox());
				}
			}
			PlayBoxOpenEffects();
			break;
		case GiftBoxState.Closed:
			break;
		}
	}

	private void ApplyGiftPackageXp()
	{
		if (giftPackage != null && giftPackage.Xp > 0 && Player.LocalPlayer != null && (IsOwnerLocal || (Player.LocalPlayer.CurrentFloorPosition - base.transform.position).magnitude <= remotePlayerXpBonusDistance))
		{
			int additionalXp = Mathf.CeilToInt((float)giftPackage.Xp * ((!IsOwnerLocal) ? 0.1f : 1f));
			SingletonMonoBehaviour<ProgressionManager>.Instance.LocalCompleteObjective(ProgressionManager.ObjectiveType.Default, additionalXp);
		}
	}

	private void OnStripTornOff(TearStripTab tearStrip)
	{
		BoxState = GiftBoxState.Opening;
		if (tearStrip != null)
		{
			tearStrip.TornOff -= OnStripTornOff;
		}
	}

	private void OnOutfitToolPickupAttempt(OutfitTool tool)
	{
		if (IsOwnerLocal)
		{
			tool.OnlyOwnerCanPickup = false;
			toolAnchorAnimator.enabled = false;
			StartCoroutine(DismissBox());
		}
	}

	private IEnumerator RunOpenBox()
	{
		yield return SingletonMonoBehaviour<GiftManager>.Instance.RunConsumeGift(giftPackage, delegate(bool successful)
		{
			if (successful)
			{
				BoxState = GiftBoxState.Open;
			}
			else
			{
				BoxState = GiftBoxState.Failed;
			}
		});
	}

	private void PlayBoxOpenEffects()
	{
		PooledParticle pooledParticle = null;
		if ((BoxContents == GiftBoxContents.NewItem || BoxContents == GiftBoxContents.RepeatItem) && itemBoxOpenedParticlePrefab != null)
		{
			pooledParticle = ObjectPool.Instance.Acquire(itemBoxOpenedParticlePrefab);
		}
		else if (BoxContents == GiftBoxContents.XP)
		{
			pooledParticle = ObjectPool.Instance.Acquire(xpBoxOpenedParticlePrefab);
		}
		if (pooledParticle != null)
		{
			pooledParticle.transform.position = boxOpenedParticleRoot.position;
			pooledParticle.transform.rotation = boxOpenedParticleRoot.rotation;
			pooledParticle.Play();
		}
		if (BoxContents != GiftBoxContents.Unspecified)
		{
			RecRoomAudioClip recRoomAudioClip = boxOpenAudio[(int)BoxContents];
			if (recRoomAudioClip != null)
			{
				AudioManager.Play2DSFX(recRoomAudioClip);
			}
		}
	}

	private IEnumerator DismissBox()
	{
		yield return new WaitForSeconds(timeBeforeDerez);
		PhotonNetwork.Destroy(base.gameObject);
	}
}
