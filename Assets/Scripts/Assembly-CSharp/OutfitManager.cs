using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon;
using RecNet;
using UnityEngine;
using UnityEngine.Serialization;

public class OutfitManager : Photon.MonoBehaviour
{
	public enum OutfitType
	{
		None = -1,
		Hat = 0,
		BackHead = 1,
		Hair = 2,
		Eye = 10,
		Mouth = 20,
		Neck = 100,
		Shirt = 101,
		Belt = 102,
		Pocket = 103,
		TeamJersey = 104,
		Wrist = 200,
		Glove = 201,
		Watch = 202,
		TeamWrist = 203
	}

	public delegate void DownloadUnlockedAvatarItemsCallback(string error);

	[SerializeField]
	private List<OutfitItem> outfitPrefabs = new List<OutfitItem>();

	public Material OutfitGhostMaterial;

	[SerializeField]
	private OutfitToolUIControl outfitToolUIControlPrefab;

	[Header("Highlighting")]
	[SerializeField]
	[Tooltip("The animation scale curve of a highlighted outfit tool. Y-axis is scale, x-axis is period")]
	private AnimationCurve outfitToolScaleAnimationCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	[Header("Selections")]
	[Tooltip("Default unlocked items for all players")]
	[SerializeField]
	[FormerlySerializedAs("outfitSelections")]
	private List<OutfitSelection> unlockedOutfitSelections = new List<OutfitSelection>();

	[SerializeField]
	[Tooltip("Minimum default set of outfits for players.")]
	private List<OutfitSelectionToBody> defaultOutfits = new List<OutfitSelectionToBody>();

	[Tooltip("Minimum default set of outfits for players.")]
	public List<OutfitSelectionToBody> TeamOutfits = new List<OutfitSelectionToBody>();

	[SerializeField]
	[Tooltip("Set of outfits for our moderators.")]
	private List<OutfitSelectionToBody> moderatorOutfits = new List<OutfitSelectionToBody>();

	[Header("Sounds")]
	[SerializeField]
	private RecRoomAudioClip[] equipSounds;

	[SerializeField]
	private RecRoomAudioClip[] unEquipSounds;

	[SerializeField]
	private RecRoomAudioClip[] pageSounds;

	[SerializeField]
	private RecRoomAudioClip lockedOutfitToolClip;

	[Header("Colors")]
	public ColorVault SkinColorVault;

	public ColorVault HairColorVault;

	private Dictionary<OutfitType, List<OutfitSelection>> outfitTypeMap;

	public static OutfitManager Instance { get; private set; }

	public static bool IsInitialized
	{
		get
		{
			return Instance != null;
		}
	}

	public List<OutfitSelectionToBody> ModeratorOutfits
	{
		get
		{
			return moderatorOutfits;
		}
	}

	public List<OutfitItem> OutfitPrefabs
	{
		get
		{
			return outfitPrefabs;
		}
	}

	public List<OutfitSelection> UnlockedOutfitSelections
	{
		get
		{
			return unlockedOutfitSelections;
		}
	}

	public RecRoomAudioClip[] EquipSounds
	{
		get
		{
			return equipSounds;
		}
	}

	public RecRoomAudioClip[] UnEquipSounds
	{
		get
		{
			return unEquipSounds;
		}
	}

	public RecRoomAudioClip[] PageSounds
	{
		get
		{
			return pageSounds;
		}
	}

	public RecRoomAudioClip LockedOutfitToolClip
	{
		get
		{
			return lockedOutfitToolClip;
		}
	}

	public event Action<OutfitTool, string> OutfitToolSpawned;

	public event Action<OutfitSelection> OutfitSelectionUnlocked;

	protected override void Awake()
	{
		base.Awake();
		Instance = this;
		InitializeOutfitTypeMap();
	}

	private void InitializeOutfitTypeMap()
	{
		outfitTypeMap = new Dictionary<OutfitType, List<OutfitSelection>>();
		List<IGrouping<OutfitType, OutfitSelection>> list = (from o in unlockedOutfitSelections
			where o != null && o.outfitItem != null
			group o by o.outfitItem.Type into oType
			select (oType)).Distinct().ToList();
		int playerLevel = ((Profiles.LocalProfile == null) ? 1 : Profiles.LocalProfile.Level);
		foreach (IGrouping<OutfitType, OutfitSelection> item in list)
		{
			OutfitType type = item.Key;
			List<OutfitSelection> value = unlockedOutfitSelections.Where((OutfitSelection o) => o != null && o.outfitItem != null && o.outfitItem.Type == type && o.outfitItem.DisplayOnRack && o.Level <= playerLevel).ToList();
			outfitTypeMap.Add(type, value);
		}
	}

	public OutfitItem GetOutfit(string guidString)
	{
		return GetOutfit(new Guid(guidString));
	}

	public OutfitItem GetOutfit(Guid guid)
	{
		foreach (OutfitItem outfitPrefab in outfitPrefabs)
		{
			if (outfitPrefab != null && outfitPrefab.Guid == guid)
			{
				return outfitPrefab;
			}
		}
		return null;
	}

	public List<OutfitSelectionToBody> GetMissingOutfitsFromDefaultSet(List<OutfitSelectionToBody> currentSelection)
	{
		List<OutfitSelectionToBody> list = new List<OutfitSelectionToBody>();
		foreach (OutfitSelectionToBody defaultOutfit in defaultOutfits)
		{
			bool flag = false;
			foreach (OutfitSelectionToBody item in currentSelection)
			{
				if (item.selection.outfitItem.Type == defaultOutfit.selection.outfitItem.Type && item.selection.outfitItem.CurrentBodyPart == defaultOutfit.bodyPart)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(defaultOutfit);
			}
		}
		return list;
	}

	public static Player.BodyPart GetMatchingBodyPart(OutfitType outfitType)
	{
		Player.BodyPart result = Player.BodyPart.Head;
		switch (outfitType)
		{
		case OutfitType.Hat:
		case OutfitType.BackHead:
		case OutfitType.Hair:
		case OutfitType.Eye:
		case OutfitType.Mouth:
			result = Player.BodyPart.Head;
			break;
		case OutfitType.Neck:
		case OutfitType.Shirt:
		case OutfitType.Belt:
		case OutfitType.Pocket:
		case OutfitType.TeamJersey:
			result = Player.BodyPart.Torso;
			break;
		case OutfitType.Wrist:
		case OutfitType.Glove:
		case OutfitType.Watch:
		case OutfitType.TeamWrist:
			result = Player.BodyPart.LeftHand;
			break;
		}
		return result;
	}

	public static bool IsTeamOutfit(OutfitType outfitType)
	{
		return outfitType == OutfitType.TeamJersey || outfitType == OutfitType.TeamWrist;
	}

	public static bool IsRequiredOutfitType(OutfitType outfitType)
	{
		return outfitType == OutfitType.Shirt;
	}

	public static bool IsMatchingBodyPart(Player.BodyPart bodyPart, OutfitType outfitType)
	{
		Player.BodyPart matchingBodyPart = GetMatchingBodyPart(outfitType);
		return matchingBodyPart == bodyPart || (matchingBodyPart == Player.BodyPart.LeftHand && bodyPart == Player.BodyPart.RightHand);
	}

	public void RandomizePlayerOutfit()
	{
		if (!(Player.LocalPlayer != null) || !(Player.LocalPlayer.PlayerOutfit != null))
		{
			return;
		}
		List<OutfitSelectionToBody> list = new List<OutfitSelectionToBody>();
		foreach (KeyValuePair<OutfitType, List<OutfitSelection>> item in outfitTypeMap)
		{
			OutfitType key = item.Key;
			List<OutfitSelection> value = item.Value;
			if ((IsRequiredOutfitType(key) || !(UnityEngine.Random.value > ((value.Count < 5) ? 0.3f : 0.6f))) && value.Count > 0)
			{
				OutfitSelection outfitSelection = value.Random();
				Player.BodyPart matchingBodyPart = GetMatchingBodyPart(outfitSelection.outfitItem.Type);
				list.Add(new OutfitSelectionToBody(outfitSelection, matchingBodyPart));
				if (matchingBodyPart == Player.BodyPart.LeftHand && (outfitSelection.outfitItem.Type == OutfitType.Wrist || outfitSelection.outfitItem.Type == OutfitType.Glove))
				{
					list.Add(new OutfitSelectionToBody(outfitSelection, Player.BodyPart.RightHand));
				}
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		Player.LocalPlayer.PlayerOutfit.UnEquipAll();
		foreach (OutfitSelectionToBody item2 in list)
		{
			Player.LocalPlayer.PlayerOutfit.SetOutfit(item2);
		}
	}

	public ColorVault.ColorToGuid FindHairColor(string hairColorGuid)
	{
		return FindColorFromVault(HairColorVault, hairColorGuid);
	}

	public ColorVault.ColorToGuid FindSkinColor(string skinColorGuid)
	{
		return FindColorFromVault(SkinColorVault, skinColorGuid);
	}

	private ColorVault.ColorToGuid FindColorFromVault(ColorVault vault, string colorGuid)
	{
		ColorVault.ColorToGuid colorToGuid = vault.Find(colorGuid);
		if (colorToGuid == null)
		{
			colorToGuid = vault.Colors.Random();
			if (!string.IsNullOrEmpty(colorGuid))
			{
				Debug.LogWarning(string.Concat("Outfit Manager FindColor in ", vault, " can't find matching color for the guid ", colorGuid, ". Getting a random one."));
			}
		}
		return colorToGuid;
	}

	public IEnumerator DownloadUnlockedAvatarItems(DownloadUnlockedAvatarItemsCallback callback)
	{
		return Avatars.DowloadUnlockedAvatarItems(delegate(string e, List<OutfitSelection> selections)
		{
			if (selections != null)
			{
				foreach (OutfitSelection selection in selections)
				{
					if (selection != null && !unlockedOutfitSelections.Contains(selection))
					{
						unlockedOutfitSelections.Add(selection);
					}
				}
			}
			if (string.IsNullOrEmpty(e))
			{
				InitializeOutfitTypeMap();
			}
			callback(e);
		});
	}

	public bool AddAvatarItemToUnlockedList(string avatarItemDesc)
	{
		OutfitSelection outfitSelection = OutfitSelection.Parse(avatarItemDesc);
		if (outfitSelection != null && !unlockedOutfitSelections.Contains(outfitSelection))
		{
			unlockedOutfitSelections.Add(outfitSelection);
			InitializeOutfitTypeMap();
			outfitSelection.IsNew = true;
			if (this.OutfitSelectionUnlocked != null)
			{
				this.OutfitSelectionUnlocked(outfitSelection);
			}
			return true;
		}
		return false;
	}

	public OutfitSelection CreateRandomOutfitSelection(int maxLevel)
	{
		OutfitItem randomOutfitItem = GetRandomOutfitItem(maxLevel);
		AvatarMask randomMask = null;
		OutfitItem.OutfitSelectionGroup randomGroup = null;
		GetRandomAvatarMask(randomOutfitItem, out randomMask, out randomGroup, maxLevel);
		if ((bool)randomMask && randomGroup != null)
		{
			AvatarColorSwatch randomAvatarColorSwatch = GetRandomAvatarColorSwatch(randomOutfitItem, randomGroup, randomMask, maxLevel);
			if (randomAvatarColorSwatch != null)
			{
				AvatarDecal randomAvatarDecal = GetRandomAvatarDecal(randomOutfitItem, maxLevel, randomGroup, randomMask, randomAvatarColorSwatch);
				return new OutfitSelection(randomOutfitItem, randomAvatarColorSwatch, randomMask, randomAvatarDecal);
			}
		}
		return new OutfitSelection(randomOutfitItem);
	}

	private OutfitItem GetRandomOutfitItem(int maxLevel)
	{
		IEnumerable<OutfitItem> enumerable = outfitPrefabs.Where((OutfitItem o) => o != null && o.Level <= maxLevel && o.Rarity > 0f && o.DisplayOnRack);
		float num = 0f;
		foreach (OutfitItem item in enumerable)
		{
			num += item.Rarity;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		num = 0f;
		foreach (OutfitItem item2 in enumerable)
		{
			if (num2 >= num && num2 <= num + item2.Rarity)
			{
				return item2;
			}
			num += item2.Rarity;
		}
		Debug.LogError("Failed to find outfit item. This should never happen.");
		return null;
	}

	private void GetRandomAvatarMask(OutfitItem outfitItem, out AvatarMask randomMask, out OutfitItem.OutfitSelectionGroup randomGroup, int maxLevel)
	{
		randomGroup = null;
		randomMask = null;
		var enumerable = outfitItem.SelectionGroups.Where((OutfitItem.OutfitSelectionGroup g) => g.IsValid).SelectMany((OutfitItem.OutfitSelectionGroup g) => from m in g.masks
			where IsValidAvatarMask(m, outfitItem, g, maxLevel)
			select new
			{
				Group = g,
				Mask = m
			});
		float num = 0f;
		foreach (var item in enumerable)
		{
			num += item.Mask.Rarity;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		num = 0f;
		foreach (var item2 in enumerable)
		{
			if (num2 >= num && num2 <= num + item2.Mask.Rarity)
			{
				randomMask = item2.Mask;
				randomGroup = item2.Group;
				break;
			}
			num += item2.Mask.Rarity;
		}
	}

	private bool IsValidAvatarMask(AvatarMask mask, OutfitItem outfitItem, OutfitItem.OutfitSelectionGroup group, int maxLevel)
	{
		if (outfitItem != null && mask != null && group != null && mask.Rarity > 0f)
		{
			int num = outfitItem.Level + mask.Level;
			foreach (AvatarColorSwatch swatch in group.swatches)
			{
				if (swatch.Rarity > 0f && swatch.Level + num <= maxLevel)
				{
					return true;
				}
			}
		}
		return false;
	}

	private AvatarColorSwatch GetRandomAvatarColorSwatch(OutfitItem outfitItem, OutfitItem.OutfitSelectionGroup group, AvatarMask mask, int maxLevel)
	{
		int currentLevel = outfitItem.Level + mask.Level;
		IEnumerable<AvatarColorSwatch> enumerable = group.swatches.Where((AvatarColorSwatch s) => s.Rarity > 0f && s.Level + currentLevel <= maxLevel);
		float num = 0f;
		foreach (AvatarColorSwatch item in enumerable)
		{
			num += item.Rarity;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		num = 0f;
		foreach (AvatarColorSwatch item2 in enumerable)
		{
			if (num2 >= num && num2 <= num + item2.Rarity)
			{
				return item2;
			}
			num += item2.Rarity;
		}
		return null;
	}

	private AvatarDecal GetRandomAvatarDecal(OutfitItem outfitItem, int maxLevel, OutfitItem.OutfitSelectionGroup group, AvatarMask mask, AvatarColorSwatch swatch)
	{
		int currentLevel = outfitItem.Level + mask.Level + swatch.Level;
		IEnumerable<AvatarDecal> enumerable = group.decals.Where((AvatarDecal d) => d.Rarity > 0f && d.Level + currentLevel <= maxLevel);
		float num = group.noneDecalRarity;
		foreach (AvatarDecal item in enumerable)
		{
			num += item.Rarity;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		if (num2 <= group.noneDecalRarity)
		{
			return null;
		}
		num = group.noneDecalRarity;
		foreach (AvatarDecal item2 in enumerable)
		{
			if (num2 >= num && num2 <= num + item2.Rarity)
			{
				return item2;
			}
			num += item2.Rarity;
		}
		return null;
	}

	private OutfitTool SpawnOutfitTool(PhotonPlayer wearerPhotonPlayer, OutfitSelection selection, int photonViewId, OutfitTool.ToolPurpose purpose, string guid)
	{
		Player player = wearerPhotonPlayer.ToPlayer();
		GameObject gameObject = new GameObject(selection.name + "_OutfitTool");
		gameObject.transform.position = new Vector3(0f, -10000f, 0f);
		player.SetParentPlayerRoot(gameObject.transform);
		SkinnedMeshRenderer skinnedMeshRenderer = selection.outfitItem.SkinnedMeshRenderer;
		GameObject gameObject2 = new GameObject("VisualRoot");
		gameObject2.transform.SetParent(gameObject.transform, false);
		gameObject2.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
		GameObject gameObject3 = new GameObject("ToolVisual");
		gameObject3.transform.SetParent(gameObject2.transform, false);
		MeshRenderer meshRenderer = gameObject3.AddComponent<MeshRenderer>();
		MeshFilter meshFilter = gameObject3.AddComponent<MeshFilter>();
		meshFilter.sharedMesh = skinnedMeshRenderer.sharedMesh;
		meshRenderer.materials = skinnedMeshRenderer.sharedMaterials;
		gameObject2.transform.localPosition = gameObject.transform.position - meshRenderer.bounds.center;
		GameObject gameObject4 = UnityEngine.Object.Instantiate(gameObject3);
		gameObject4.transform.SetParent(gameObject2.transform, false);
		gameObject4.gameObject.SetActive(false);
		gameObject4.name = "ToolVisual_ghost";
		MeshRenderer component = gameObject4.GetComponent<MeshRenderer>();
		Collider collider = meshRenderer.GetComponent<Collider>();
		if (collider == null)
		{
			collider = meshRenderer.gameObject.AddComponent<BoxCollider>();
		}
		meshRenderer.gameObject.AddComponent<ToolCollider>();
		BoxCollider boxCollider = gameObject4.gameObject.AddComponent<BoxCollider>();
		boxCollider.isTrigger = true;
		gameObject4.gameObject.AddComponent<ToolCollider>();
		if (selection.mask != null)
		{
			AvatarMask.Apply(selection.mask, meshRenderer.material);
		}
		if (selection.decal != null)
		{
			AvatarDecal.Apply(selection.decal, meshRenderer.material);
		}
		if (selection.colorSwatch != null)
		{
			AvatarColorSwatch.Apply(selection.colorSwatch, meshRenderer.material);
		}
		Material[] array = new Material[component.materials.Length];
		for (int i = 0; i < component.materials.Length; i++)
		{
			array[i] = OutfitGhostMaterial;
		}
		component.materials = array;
		Rigidbody rigidbody = gameObject.gameObject.AddComponent<Rigidbody>();
		rigidbody.useGravity = false;
		PhotonView photonView = gameObject.gameObject.AddComponent<PhotonView>();
		photonView.viewID = photonViewId;
		photonView.synchronization = ViewSynchronization.Unreliable;
		PUNNetworkTransform item = gameObject.gameObject.AddComponent<PUNNetworkTransform>();
		if (photonView.ObservedComponents == null)
		{
			photonView.ObservedComponents = new List<Component>();
		}
		photonView.ObservedComponents.Add(item);
		if (purpose == OutfitTool.ToolPurpose.GiftBox)
		{
			gameObject.gameObject.SetActive(false);
			AnimateInOut animateInOut = gameObject.gameObject.AddComponent<AnimateInOut>();
			animateInOut.SuppressAnimation = true;
			gameObject.gameObject.SetActive(true);
		}
		OutfitTool outfitTool = gameObject.gameObject.AddComponent<OutfitTool>();
		outfitTool.Wearer = wearerPhotonPlayer;
		outfitTool.OutfitTarget = selection;
		outfitTool.DrawerIndex = 0;
		outfitTool.ToolCollider = collider;
		outfitTool.GhostVisual = component;
		outfitTool.GhostCollider = boxCollider;
		outfitTool.Purpose = purpose;
		if (outfitTool.IsWearerLocal)
		{
			OutfitSelection outfitSelection = UnlockedOutfitSelections.FindLast((OutfitSelection s) => s.Equals(selection));
			if (outfitSelection != null)
			{
				selection.UnlockedLevel = outfitSelection.UnlockedLevel;
				outfitTool.IsNew = outfitSelection.IsNew;
			}
			else
			{
				outfitTool.IsNew = true;
			}
		}
		OutfitToolRenderer component2 = outfitTool.GetComponent<OutfitToolRenderer>();
		component2.SetupRenderer(meshRenderer);
		ScaleAnimationControl scaleAnimationControl = meshRenderer.gameObject.AddComponent<ScaleAnimationControl>();
		scaleAnimationControl.enabled = false;
		scaleAnimationControl.scaleCurve = outfitToolScaleAnimationCurve;
		component2.ScaleControl = scaleAnimationControl;
		outfitTool.gameObject.AddComponent<OutfitToolCleanup>();
		gameObject.gameObject.SetLayerRecursively(Layers.DynamicPhysics);
		outfitTool.CurrentState = OutfitTool.OutfitToolState.Disabled;
		bool flag = player.PlayerOutfit.IsWearing(selection);
		outfitTool.EquipState = ((!flag) ? OutfitTool.PlayerEquipState.NotWearing : OutfitTool.PlayerEquipState.Wearing);
		if (player.isLocal && (outfitTool.IsLevelLocked || outfitTool.IsNew))
		{
			OutfitToolUIControl outfitToolUIControl = UnityEngine.Object.Instantiate(outfitToolUIControlPrefab);
			outfitToolUIControl.SetLockLevel(selection.Level);
			if (selection.outfitItem.uiAnchor != null)
			{
				outfitToolUIControl.Offset = selection.outfitItem.uiAnchor.localPosition;
			}
			else
			{
				outfitToolUIControl.Offset = selection.outfitItem.outfitToolUIControlOffset;
				Debug.LogWarning("Using deprecated UIControl offset for " + outfitTool.gameObject.name);
			}
			outfitToolUIControl.transform.SetParent(outfitTool.transform);
			outfitTool.OutfitToolUIControl = outfitToolUIControl;
		}
		if (this.OutfitToolSpawned != null)
		{
			this.OutfitToolSpawned(outfitTool, guid);
		}
		return outfitTool;
	}

	public string LocalSpawnOutfitTool(OutfitSelection selection, OutfitTool.ToolPurpose purpose)
	{
		string text = Guid.NewGuid().ToString();
		int num = PhotonNetwork.AllocateViewID();
		base.photonView.RPC("RpcSpawnOutfitTool", PhotonTargets.AllBuffered, PhotonNetwork.player, selection.ToString(), num, purpose, text);
		return text;
	}

	[PunRPC]
	public void RpcSpawnOutfitTool(PhotonPlayer player, string serializedSelection, int viewId, OutfitTool.ToolPurpose purpose, string guid)
	{
		OutfitSelection outfitSelection = OutfitSelection.Parse(serializedSelection);
		if (outfitSelection != null)
		{
			SpawnOutfitTool(player, outfitSelection, viewId, purpose, guid);
		}
		else
		{
			Debug.LogWarning("RPC SpawnOutfitTool received null selection");
		}
	}
}
