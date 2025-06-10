using System;
using System.Collections.Generic;
using Photon;
using RecNet;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerOutfit : Photon.MonoBehaviour
{
	public static readonly string PlayerHairColorProperty = "PlayerHairColorProperty";

	public static readonly string PlayerSkinColorProperty = "PlayerSkinColorProperty";

	public static readonly string PlayerOutFitProperty = "PlayerOutFitProperty";

	public bool PersistOutfitSelection = true;

	[SerializeField]
	private SkinnedMeshRenderer face;

	[SerializeField]
	private SkinnedMeshRenderer eyes;

	[SerializeField]
	private SkinnedMeshRenderer mouth;

	[SerializeField]
	private SkinnedMeshRenderer torso;

	[SerializeField]
	private SkinnedMeshRenderer leftHand;

	[SerializeField]
	private SkinnedMeshRenderer rightHand;

	[HideInInspector]
	public Player ThisPlayer;

	private ColorVault.ColorToGuid _hairColor;

	private ColorVault.ColorToGuid _skinColor;

	private Color? _teamIndicator;

	private List<OutfitSelectionToBody> outfitSelections = new List<OutfitSelectionToBody>();

	private PlayerData playerData;

	private bool recnetLocalAvatarDirty;

	private bool snapShotReady;

	private string savedSelectionStr;

	private string savedHairColorGuid = string.Empty;

	private string savedSkinColorGuid = string.Empty;

	public ColorVault.ColorToGuid HairColor
	{
		get
		{
			return _hairColor;
		}
		private set
		{
			_hairColor = value;
			if (base.isLocal)
			{
				AvatarColor.SetSelected(AvatarColor.ColorType.Hair, _hairColor);
				if (ThisPlayer.IsInChangingRoom)
				{
					AnalyticsHelper.AvatarHairColor(_hairColor);
				}
			}
			if (this.HairColorChanged != null)
			{
				this.HairColorChanged(_hairColor.color);
			}
		}
	}

	public ColorVault.ColorToGuid SkinColor
	{
		get
		{
			return _skinColor;
		}
		private set
		{
			_skinColor = value;
			if (base.isLocal)
			{
				AvatarColor.SetSelected(AvatarColor.ColorType.Skin, _skinColor);
				if (ThisPlayer.IsInChangingRoom)
				{
					AnalyticsHelper.AvatarSkinColor(_skinColor);
				}
			}
			face.material.color = _skinColor.color;
			torso.material.color = _skinColor.color;
			leftHand.material.color = _skinColor.color;
			rightHand.material.color = _skinColor.color;
		}
	}

	public Color? TeamIndicator
	{
		get
		{
			return _teamIndicator;
		}
		set
		{
			if (_teamIndicator != value)
			{
				_teamIndicator = value;
				UpdateTeamOutfits();
			}
		}
	}

	public bool TeamOutfitsSupported
	{
		get
		{
			return RecRoomSceneManager.Instance.GameManager != null && RecRoomSceneManager.Instance.GameManager.TeamManager.SupportsTeamOutfits;
		}
	}

	public event Action<Color> HairColorChanged;

	protected override void Awake()
	{
		base.Awake();
		playerData = GetComponent<PlayerData>();
	}

	private void Start()
	{
		playerData.RegisterCallback(PlayerSkinColorProperty, OnPlayerSkinColorChanged);
		playerData.RegisterCallback(PlayerHairColorProperty, OnPlayerHairColorChanged);
		playerData.RegisterCallback(PlayerOutFitProperty, OnPlayerOutfitChanged);
		if (base.isLocal)
		{
			bool flag = true;
			bool flag2 = true;
			bool flag3 = true;
			if (PersistOutfitSelection)
			{
				string skinColor = Avatars.LocalAvatar.SkinColor;
				if (!string.IsNullOrEmpty(skinColor))
				{
					playerData.SetData(PlayerSkinColorProperty, skinColor);
					flag2 = false;
				}
				string hairColor = Avatars.LocalAvatar.HairColor;
				if (!string.IsNullOrEmpty(hairColor))
				{
					playerData.SetData(PlayerHairColorProperty, hairColor);
					flag3 = false;
				}
				string text = Avatars.LocalAvatar.OutfitSelections;
				if (!string.IsNullOrEmpty(text))
				{
					playerData.SetData(PlayerOutFitProperty, text);
					flag = false;
				}
			}
			if (flag3)
			{
				SetHairColor(OutfitManager.Instance.FindHairColor(string.Empty).guidString);
			}
			if (flag2)
			{
				SetSkinColor(OutfitManager.Instance.FindSkinColor(string.Empty).guidString);
			}
			if (flag)
			{
				OutfitManager.Instance.RandomizePlayerOutfit();
			}
			EquipDefaultOutfitSetForMissing();
			EquipTeamOutfits();
		}
		else
		{
			OnPlayerOutfitChanged(null, null);
			OnPlayerSkinColorChanged(null, null);
			OnPlayerHairColorChanged(null, null);
		}
		ThisPlayer.VisibilityChanged += ThisPlayer_VisibilityChanged;
		ThisPlayer.IsInChangingRoomUpdated += ThisPlayer_IsInChangingRoomUpdated;
	}

	private void Update()
	{
		if (base.isLocal && recnetLocalAvatarDirty)
		{
			recnetLocalAvatarDirty = false;
			Avatars.SaveLocalAvatarSettings();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		OutfitSelectionToBody[] array = outfitSelections.ToArray();
		foreach (OutfitSelectionToBody outfitSelectionToBody in array)
		{
			if (outfitSelectionToBody != null && outfitSelectionToBody.selection.outfitItem != null)
			{
				UnityEngine.Object.Destroy(outfitSelectionToBody.selection.outfitItem.gameObject);
			}
		}
		outfitSelections.Clear();
		ThisPlayer.VisibilityChanged -= ThisPlayer_VisibilityChanged;
		ThisPlayer.IsInChangingRoomUpdated -= ThisPlayer_IsInChangingRoomUpdated;
	}

	public OutfitSelection GetSelection(Player.BodyPart bodyPart)
	{
		foreach (OutfitSelectionToBody outfitSelection in outfitSelections)
		{
			if (outfitSelection.bodyPart == bodyPart)
			{
				return outfitSelection.selection;
			}
		}
		return null;
	}

	private void EquipTeamOutfits()
	{
		foreach (OutfitSelectionToBody teamOutfit in OutfitManager.Instance.TeamOutfits)
		{
			SetOutfit(teamOutfit);
		}
	}

	private void ThisPlayer_VisibilityChanged(bool visible)
	{
		if (base.isLocal)
		{
			return;
		}
		foreach (OutfitSelectionToBody outfitSelection in outfitSelections)
		{
			outfitSelection.selection.outfitItem.gameObject.SetActive(visible);
		}
	}

	private void ThisPlayer_IsInChangingRoomUpdated(bool entered)
	{
		if (base.isLocal && entered)
		{
			SnapShotSelection();
		}
	}

	private void OnPlayerSkinColorChanged(PlayerData sender, string key)
	{
		string data = playerData.GetData(PlayerSkinColorProperty, string.Empty);
		ColorVault.ColorToGuid skinColor = OutfitManager.Instance.FindSkinColor(data);
		SkinColor = skinColor;
	}

	private void OnPlayerHairColorChanged(PlayerData sender, string key)
	{
		string data = playerData.GetData(PlayerHairColorProperty, string.Empty);
		ColorVault.ColorToGuid hairColor = OutfitManager.Instance.FindHairColor(data);
		HairColor = hairColor;
	}

	private void OnPlayerOutfitChanged(PlayerData sender, string key)
	{
		string data = playerData.GetData(PlayerOutFitProperty, string.Empty);
		ParseOutfitString(data);
	}

	private void EquipDefaultOutfitSetForMissing()
	{
		List<OutfitSelectionToBody> missingOutfitsFromDefaultSet = OutfitManager.Instance.GetMissingOutfitsFromDefaultSet(outfitSelections);
		foreach (OutfitSelectionToBody item in missingOutfitsFromDefaultSet)
		{
			SetOutfit(item);
		}
	}

	private void UpdateTeamOutfits()
	{
		bool flag = TeamIndicator.HasValue && TeamOutfitsSupported;
		foreach (OutfitSelectionToBody outfitSelection in outfitSelections)
		{
			if (OutfitManager.IsTeamOutfit(outfitSelection.selection.outfitItem.Type))
			{
				outfitSelection.selection.outfitItem.Enabled = flag;
				if (flag)
				{
					outfitSelection.selection.outfitItem.SkinnedMeshRenderer.material.color = TeamIndicator.Value;
				}
			}
			else
			{
				Player.BodyPart matchingBodyPart = OutfitManager.GetMatchingBodyPart(outfitSelection.selection.outfitItem.Type);
				if (matchingBodyPart.IsHand())
				{
					outfitSelection.selection.outfitItem.Enabled = !flag;
				}
			}
		}
	}

	public string GetOutfitNameList()
	{
		string text = string.Empty;
		for (int i = 0; i < outfitSelections.Count; i++)
		{
			text = text + outfitSelections[i].selection.outfitItem.name + ((i == outfitSelections.Count - 1) ? string.Empty : ",");
		}
		return text;
	}

	public void UnEquipAll()
	{
		OutfitSelectionToBody[] array = outfitSelections.ToArray();
		foreach (OutfitSelectionToBody selectionToBody in array)
		{
			UnEquipOutfit(selectionToBody);
		}
	}

	public void ResetOutfitSelectionToDefault()
	{
		UnEquipAll();
		EquipDefaultOutfitSetForMissing();
	}

	public void SetHandVisible(PlayerHand.HandType type, bool isVisible)
	{
		int num;
		switch (type)
		{
		case PlayerHand.HandType.Unknown:
			return;
		case PlayerHand.HandType.Left:
			num = 2;
			break;
		default:
			num = 3;
			break;
		}
		Player.BodyPart bodyPart = (Player.BodyPart)num;
		foreach (OutfitSelectionToBody outfitSelection in outfitSelections)
		{
			if (outfitSelection.selection.outfitItem.CurrentBodyPart == bodyPart)
			{
				outfitSelection.selection.outfitItem.gameObject.SetActive(isVisible && ThisPlayer.IsVisible);
			}
		}
	}

	public void SetSkinColor(string colorGuid)
	{
		playerData.SetData(PlayerSkinColorProperty, colorGuid);
		if (PersistOutfitSelection)
		{
			Avatars.LocalAvatar.SkinColor = colorGuid;
			recnetLocalAvatarDirty = true;
		}
	}

	public void SetHairColor(string colorGuid)
	{
		playerData.SetData(PlayerHairColorProperty, colorGuid);
		if (PersistOutfitSelection)
		{
			Avatars.LocalAvatar.HairColor = colorGuid;
			recnetLocalAvatarDirty = true;
		}
	}

	public bool SetOutfit(OutfitSelection selection, Player.BodyPart bodyPart, bool playAudio = false)
	{
		return SetOutfit(new OutfitSelectionToBody(selection, bodyPart), playAudio);
	}

	public bool SetOutfit(OutfitSelectionToBody selectionToBody, bool playAudio = false)
	{
		if (outfitSelections.Contains(selectionToBody))
		{
			return false;
		}
		OutfitSelectionToBody[] array = outfitSelections.ToArray();
		foreach (OutfitSelectionToBody outfitSelectionToBody in array)
		{
			if (outfitSelectionToBody.selection.outfitItem.Type == selectionToBody.selection.outfitItem.Type && outfitSelectionToBody.selection.outfitItem.CurrentBodyPart == selectionToBody.bodyPart && outfitSelectionToBody.selection != selectionToBody.selection)
			{
				UnEquipOutfit(outfitSelectionToBody);
			}
		}
		string data = SerializeOutfitSelections(selectionToBody);
		playerData.SetData(PlayerOutFitProperty, data);
		if (PersistOutfitSelection && !OutfitManager.IsTeamOutfit(selectionToBody.selection.outfitItem.Type))
		{
			Avatars.LocalAvatar.OutfitSelections = data;
			recnetLocalAvatarDirty = true;
		}
		if (playAudio)
		{
			PlayOutfitAudio(OutfitManager.Instance.EquipSounds, selectionToBody.bodyPart);
		}
		ThisPlayer.PlayerEvents.OutfitEquipped(selectionToBody.bodyPart);
		return true;
	}

	public bool RemoveOutfit(OutfitSelection selection, bool playAudio = false)
	{
		OutfitSelectionToBody outfitSelectionToBody = new OutfitSelectionToBody(selection, selection.outfitItem.CurrentBodyPart);
		if (!outfitSelections.Contains(outfitSelectionToBody))
		{
			return false;
		}
		UnEquipOutfit(outfitSelectionToBody);
		string data = SerializeOutfitSelections();
		playerData.SetData(PlayerOutFitProperty, data);
		if (PersistOutfitSelection)
		{
			Avatars.LocalAvatar.OutfitSelections = data;
			recnetLocalAvatarDirty = true;
		}
		if (playAudio)
		{
			PlayOutfitAudio(OutfitManager.Instance.UnEquipSounds, selection.outfitItem.CurrentBodyPart);
		}
		return true;
	}

	private void PlayOutfitAudio(RecRoomAudioClip[] audioClips, Player.BodyPart bodyPart)
	{
		Vector3 position;
		switch (bodyPart)
		{
		case Player.BodyPart.LeftHand:
			position = ThisPlayer.LeftHand.transform.position;
			break;
		case Player.BodyPart.RightHand:
			position = ThisPlayer.RightHand.transform.position;
			break;
		case Player.BodyPart.Torso:
			position = ThisPlayer.Body.transform.position;
			break;
		default:
			position = ThisPlayer.Head.transform.position;
			break;
		}
		AudioManager.PlayRandom3DSFX(audioClips, position);
	}

	private string SerializeOutfitSelections(OutfitSelectionToBody selectionToBody = null)
	{
		string text = string.Empty;
		foreach (OutfitSelectionToBody outfitSelection in outfitSelections)
		{
			text = text + outfitSelection.ToString() + ";";
		}
		if (selectionToBody != null)
		{
			text += selectionToBody.ToString();
		}
		return text;
	}

	private List<OutfitSelectionToBody> ParseOutfitMapString(string newSelectionsStr)
	{
		List<OutfitSelectionToBody> list = new List<OutfitSelectionToBody>();
		string[] array = newSelectionsStr.Split(';');
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!string.IsNullOrEmpty(text))
			{
				OutfitSelectionToBody outfitSelectionToBody = OutfitSelectionToBody.Parse(text);
				if (outfitSelectionToBody != null)
				{
					list.Add(outfitSelectionToBody);
				}
				else
				{
					Debug.LogWarning("PlayerOutfit outfit map parse error. This means we changed a GUID or deleted an item. GUID : " + text);
				}
			}
		}
		return list;
	}

	private void ParseOutfitString(string newMapString)
	{
		List<OutfitSelectionToBody> list = ParseOutfitMapString(newMapString);
		OutfitSelectionToBody[] array = outfitSelections.ToArray();
		foreach (OutfitSelectionToBody outfitSelectionToBody in array)
		{
			if (!list.Contains(outfitSelectionToBody))
			{
				UnEquipOutfit(outfitSelectionToBody);
			}
		}
		foreach (OutfitSelectionToBody item in list)
		{
			if (item != null && item.selection != null && item.selection.outfitItem != null && !outfitSelections.Contains(item))
			{
				EquipOutfit(item);
			}
		}
		UpdateTeamOutfits();
	}

	private void UnEquipOutfit(OutfitSelectionToBody selectionToBody)
	{
		ThisPlayer.AnimateInOut.additionalRenderers.Remove(selectionToBody.selection.outfitItem.SkinnedMeshRenderer);
		if (outfitSelections.Contains(selectionToBody))
		{
			outfitSelections.Remove(selectionToBody);
		}
		OutfitTool outfitTool = OutfitTool.Find(ThisPlayer, selectionToBody.selection, OutfitTool.ToolPurpose.Drawer);
		if (outfitTool != null)
		{
			outfitTool.EquipState = OutfitTool.PlayerEquipState.NotWearing;
		}
		if (base.isLocal && ThisPlayer.IsInChangingRoom)
		{
			AnalyticsHelper.AvatarOutfitEquip(selectionToBody, false);
		}
		UnityEngine.Object.Destroy(selectionToBody.selection.outfitItem.gameObject);
	}

	private void EquipOutfit(OutfitSelectionToBody selectionToBody)
	{
		InstantiateNewOutfitPrefab(selectionToBody.selection, selectionToBody.bodyPart);
		OutfitTool outfitTool = OutfitTool.Find(ThisPlayer, selectionToBody.selection, OutfitTool.ToolPurpose.Drawer);
		if (outfitTool != null)
		{
			outfitTool.EquipState = OutfitTool.PlayerEquipState.Wearing;
		}
		outfitTool = OutfitTool.Find(ThisPlayer, selectionToBody.selection, OutfitTool.ToolPurpose.Doffing);
		if (outfitTool != null)
		{
			outfitTool.EquipState = OutfitTool.PlayerEquipState.Wearing;
		}
		if (base.isLocal && ThisPlayer.IsInChangingRoom)
		{
			AnalyticsHelper.AvatarOutfitEquip(selectionToBody, true);
		}
	}

	public OutfitItem InstantiateNewOutfitPrefab(OutfitSelection selection, Player.BodyPart bodyPart, bool ownedByPlayerOutfit = true)
	{
		OutfitItem outfitItem = UnityEngine.Object.Instantiate(selection.outfitItem);
		outfitItem.Type = selection.outfitItem.Type;
		if (selection.decal != null)
		{
			outfitItem.ApplyDecal(selection.decal);
		}
		if (selection.mask != null)
		{
			outfitItem.ApplyMask(selection.mask);
		}
		if (selection.colorSwatch != null)
		{
			outfitItem.ApplySwatch(selection.colorSwatch);
		}
		outfitItem.CurrentBodyPart = bodyPart;
		outfitItem.Owner = ThisPlayer;
		selection.outfitItem = outfitItem;
		OutfitSelectionToBody item = new OutfitSelectionToBody(selection, bodyPart);
		if (ownedByPlayerOutfit)
		{
			outfitSelections.Add(item);
			ThisPlayer.AnimateInOut.additionalRenderers.Add(outfitItem.SkinnedMeshRenderer);
		}
		SetOutfitTarget(outfitItem);
		SetOutFitLayer(outfitItem);
		if (!base.isLocal)
		{
			outfitItem.gameObject.SetActive(ThisPlayer.IsVisible);
		}
		ThisPlayer.SetParentPlayerRoot(outfitItem.transform);
		return outfitItem;
	}

	private void SetOutFitLayer(OutfitItem outfitItem)
	{
		Layers layer = Layers.RemotePlayerPhysics;
		if (base.isLocal)
		{
			layer = ((!outfitItem.CurrentBodyPart.IsHand()) ? Layers.HiddenInFirstPerson : Layers.LocalPlayerPhysics);
		}
		outfitItem.gameObject.SetLayerRecursively(layer);
	}

	private void SetOutfitTarget(OutfitItem newOutfit)
	{
		SkinnedMeshRenderer targetSkinnedMesh = null;
		switch (newOutfit.Type)
		{
		case OutfitManager.OutfitType.Hat:
		case OutfitManager.OutfitType.BackHead:
		case OutfitManager.OutfitType.Hair:
			targetSkinnedMesh = face;
			break;
		case OutfitManager.OutfitType.Eye:
			targetSkinnedMesh = eyes;
			break;
		case OutfitManager.OutfitType.Mouth:
			targetSkinnedMesh = mouth;
			break;
		case OutfitManager.OutfitType.Neck:
		case OutfitManager.OutfitType.Shirt:
		case OutfitManager.OutfitType.Belt:
		case OutfitManager.OutfitType.Pocket:
		case OutfitManager.OutfitType.TeamJersey:
			targetSkinnedMesh = torso;
			break;
		case OutfitManager.OutfitType.Wrist:
		case OutfitManager.OutfitType.Glove:
		case OutfitManager.OutfitType.Watch:
		case OutfitManager.OutfitType.TeamWrist:
			if (newOutfit.CurrentBodyPart == Player.BodyPart.LeftHand)
			{
				targetSkinnedMesh = leftHand;
				break;
			}
			if (newOutfit.CurrentBodyPart == Player.BodyPart.RightHand)
			{
				targetSkinnedMesh = rightHand;
				break;
			}
			Debug.LogError(string.Concat("An outfit (", newOutfit, ") with hand type is being placed to an incorrect body part. ", newOutfit.CurrentBodyPart));
			break;
		default:
			throw new Exception("PlayerOutfit.SetOutfitTarget can not set target for this outfit " + newOutfit);
		}
		newOutfit.SetTargetSkinnedMesh(targetSkinnedMesh);
	}

	public bool IsWearing(OutfitSelection selection)
	{
		foreach (OutfitSelectionToBody outfitSelection in outfitSelections)
		{
			if (outfitSelection.selection == selection)
			{
				return true;
			}
		}
		return false;
	}

	public void SnapShotSelection()
	{
		savedHairColorGuid = HairColor.guidString;
		savedSkinColorGuid = SkinColor.guidString;
		savedSelectionStr = SerializeOutfitSelections();
		snapShotReady = true;
	}

	public void RestoreSelection()
	{
		if (snapShotReady)
		{
			SetHairColor(savedHairColorGuid);
			SetSkinColor(savedSkinColorGuid);
			ParseOutfitString(savedSelectionStr);
			recnetLocalAvatarDirty = true;
		}
	}

	public void DebugEquipModeratorOutfits()
	{
		foreach (OutfitSelectionToBody moderatorOutfit in OutfitManager.Instance.ModeratorOutfits)
		{
			SetOutfit(moderatorOutfit);
		}
	}
}
