using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class OutfitItem : MonoBehaviour, IComparable<OutfitItem>
{
	[Serializable]
	public class OutfitSelectionGroup
	{
		public string name = string.Empty;

		[Range(0f, 1f)]
		public float noneDecalRarity = 1f;

		public List<AvatarMask> masks;

		public List<AvatarColorSwatch> swatches;

		public List<AvatarDecal> decals;

		public bool IsValid
		{
			get
			{
				return masks.Count > 0 && swatches.Count > 0;
			}
		}
	}

	private static int highlightShaderParamId;

	private const float HIGHLIGHT_ON = 1f;

	private const float HIGHLIGHT_OFF = 0f;

	public SkinnedMeshRenderer SkinnedMeshRenderer;

	[Tooltip("This guid is auto generated at the time of the prefab creation and should never change if you do not want to break an existing players outfit selection.")]
	[SerializeField]
	private string guidString = Guid.NewGuid().ToString();

	public OutfitManager.OutfitType Type = OutfitManager.OutfitType.None;

	[Tooltip("if this outfitItem will be displayed on the rack or not.")]
	public bool DisplayOnRack = true;

	[Tooltip("Defines if this outfitItem is effected by the global hair color changes.")]
	public bool UsesHairColor;

	[Tooltip("Used take off clothes, when disabled this outfitItem can not be removed by hand.")]
	[SerializeField]
	private bool outfitTriggerEnabled = true;

	[Tooltip("Used for outfitItems enabled outside dorm room")]
	[SerializeField]
	private bool triggerEnabledOutsideDormroom;

	[Header("Outfit Progression")]
	public int Level = 1;

	[Tooltip("0:Impossible, 1:Common")]
	[Range(0f, 1f)]
	public float Rarity = 1f;

	[Tooltip("0:Impossible, 1:Common")]
	[Range(0.01f, 1f)]
	public float NoMaskRarity = 1f;

	public OutfitSelectionGroup[] SelectionGroups;

	[Header("Outfit Tool Settings")]
	[Tooltip("This will be used as a sorting suggestion on the rack for the same type of clothes.")]
	[SerializeField]
	protected int preferredIndex;

	[Tooltip("We will group same type of outfits with the same preferred rack, to the corresponding rack if one exists. If not add them to the paging of the first available rack.")]
	public string PreferredRackName = string.Empty;

	public float RackHeightOffset;

	[Tooltip("[Deprecated] Lock UI offset from OutfitTool")]
	[FormerlySerializedAs("lockUIOffset")]
	public Vector3 outfitToolUIControlOffset = new Vector3(0f, 0.3f, 0f);

	[Tooltip("Anchor to which this object's UI is attached")]
	public Transform uiAnchor;

	private Material _defaultMaterial;

	private bool _highlight;

	private Guid _guid = Guid.Empty;

	[NonSerialized]
	public Player.BodyPart CurrentBodyPart = Player.BodyPart.None;

	private OutfitTrigger outfitTrigger;

	private bool _enabled = true;

	private bool _visible = true;

	private Material DefaultMaterial
	{
		get
		{
			if (_defaultMaterial == null)
			{
				_defaultMaterial = new Material(SkinnedMeshRenderer.sharedMaterial);
			}
			return _defaultMaterial;
		}
	}

	public bool Highlight
	{
		get
		{
			return _highlight;
		}
		set
		{
			if (_highlight != value)
			{
				_highlight = value;
				SkinnedMeshRenderer.material.SetFloat(highlightShaderParamId, (!_highlight) ? 0f : 1f);
			}
		}
	}

	public Guid Guid
	{
		get
		{
			if (_guid == Guid.Empty)
			{
				_guid = new Guid(guidString);
			}
			return _guid;
		}
	}

	public int RackIndex { get; set; }

	public Player Owner { get; set; }

	public bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			_enabled = value;
			if (_enabled)
			{
				Visible = true;
			}
			else
			{
				Visible = Visible;
			}
		}
	}

	public bool Visible
	{
		get
		{
			return _visible && Enabled;
		}
		set
		{
			_visible = value;
			SkinnedMeshRenderer.enabled = Visible;
		}
	}

	public OutfitSelection Selection { get; private set; }

	private bool ShouldCreateTrigger
	{
		get
		{
			bool flag = Owner != null && Owner.isLocal;
			bool flag2 = outfitTriggerEnabled && (PUNNetworkManager.Instance.IsInDormRoom || triggerEnabledOutsideDormroom);
			return flag && flag2;
		}
	}

	private void Awake()
	{
		if (uiAnchor != null && uiAnchor.childCount > 0)
		{
			Debug.LogError("UI Anchor shouldn't have a child. Check the prefab");
		}
		Visible = false;
		Selection = new OutfitSelection(this);
		highlightShaderParamId = Shader.PropertyToID("_Highlight");
		SkinnedMeshRenderer.material = DefaultMaterial;
	}

	private void Start()
	{
		Visible = true;
		if (UsesHairColor)
		{
			Owner.PlayerOutfit.HairColorChanged += PlayerOutfit_HairColorChanged;
			if (Owner.PlayerOutfit.HairColor != null)
			{
				PlayerOutfit_HairColorChanged(Owner.PlayerOutfit.HairColor.color);
			}
		}
		if (ShouldCreateTrigger)
		{
			GameObject gameObject = new GameObject();
			outfitTrigger = gameObject.AddComponent<OutfitTrigger>();
			outfitTrigger.SetOutfitOwner(this);
			outfitTrigger.EnabledOutsideChangingRoom = triggerEnabledOutsideDormroom;
			Owner.SetParentPlayerRoot(gameObject.transform);
			if (!PUNNetworkManager.Instance.IsInDormRoom)
			{
				OutfitTool outfitTool = OutfitTool.Find(Owner, Selection, OutfitTool.ToolPurpose.Doffing);
				if (outfitTool == null)
				{
					OutfitManager.Instance.LocalSpawnOutfitTool(Selection, OutfitTool.ToolPurpose.Doffing);
				}
			}
		}
		if ((CurrentBodyPart == Player.BodyPart.LeftHand && !Owner.LeftHand.MeshesVisible) || (CurrentBodyPart == Player.BodyPart.RightHand && !Owner.RightHand.MeshesVisible) || !Owner.IsVisible)
		{
			base.gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		if (UsesHairColor && Owner != null)
		{
			Owner.PlayerOutfit.HairColorChanged -= PlayerOutfit_HairColorChanged;
		}
		if (outfitTrigger != null)
		{
			UnityEngine.Object.Destroy(outfitTrigger.gameObject);
		}
	}

	private void PlayerOutfit_HairColorChanged(Color color)
	{
		if (UsesHairColor)
		{
			SkinnedMeshRenderer.material.color = color;
		}
	}

	public void SetTargetSkinnedMesh(SkinnedMeshRenderer targetRenderer)
	{
		Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
		Transform[] bones = targetRenderer.bones;
		foreach (Transform transform in bones)
		{
			dictionary[transform.name] = transform;
		}
		Transform[] bones2 = SkinnedMeshRenderer.bones;
		for (int j = 0; j < bones2.Length; j++)
		{
			string text = bones2[j].name;
			if (!dictionary.TryGetValue(text, out bones2[j]))
			{
				Debug.LogError("Failed to get bone: " + text + " - can not attach to " + targetRenderer, this);
				return;
			}
		}
		SkinnedMeshRenderer.bones = bones2;
		Transform parent = SkinnedMeshRenderer.rootBone.transform.parent;
		Vector3 localScale = parent.localScale;
		localScale.z = Mathf.Abs(localScale.z) * Mathf.Sign(targetRenderer.transform.lossyScale.z);
		parent.localScale = localScale;
	}

	public void ApplySwatch(AvatarColorSwatch swatch)
	{
		if (Selection != null)
		{
			Selection.colorSwatch = swatch;
		}
		AvatarColorSwatch.Apply(swatch, SkinnedMeshRenderer.material, DefaultMaterial);
	}

	public void ApplyMask(AvatarMask mask)
	{
		if (Selection != null)
		{
			Selection.mask = mask;
		}
		AvatarMask.Apply(mask, SkinnedMeshRenderer.material, DefaultMaterial);
	}

	public void ApplyDecal(AvatarDecal decal)
	{
		if (Selection != null)
		{
			Selection.decal = decal;
		}
		AvatarDecal.Apply(decal, SkinnedMeshRenderer.material, DefaultMaterial);
	}

	public int CompareTo(OutfitItem other)
	{
		int type = (int)Type;
		int num = type.CompareTo((int)other.Type);
		return (num == 0) ? preferredIndex.CompareTo(other.preferredIndex) : num;
	}
}
