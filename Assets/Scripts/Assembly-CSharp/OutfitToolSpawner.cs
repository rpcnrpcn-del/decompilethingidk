using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OutfitToolSpawner : MonoBehaviour
{
	public OutfitDrawer[] Drawers;

	[Header("UI")]
	public Transform DrawersMenu;

	[Header("Audio")]
	public RecRoomAudioClip openDrawer;

	public RecRoomAudioClip pagingSound;

	private Dictionary<OutfitManager.OutfitType, Dictionary<string, OutfitDrawer>> drawerMap = new Dictionary<OutfitManager.OutfitType, Dictionary<string, OutfitDrawer>>();

	private HashSet<string> pendingSpawns = new HashSet<string>();

	private bool waitingOnSpawns;

	private void Awake()
	{
		Canvas componentInChildren = GetComponentInChildren<Canvas>(true);
		if (componentInChildren != null && ViveControllerInput.Instance != null)
		{
			componentInChildren.worldCamera = ViveControllerInput.Instance.ControllerCamera;
		}
	}

	private IEnumerator Start()
	{
		InitializeDrawerMap();
		while (Player.LocalPlayer == null)
		{
			yield return null;
		}
		StartCoroutine(RunInitializeOutfitTools());
		Player.LocalPlayer.IsInChangingRoomUpdated += LocalPlayer_IsInChangingRoomUpdated;
		OutfitDrawer[] drawers = Drawers;
		foreach (OutfitDrawer outfitDrawer in drawers)
		{
			if (outfitDrawer != null)
			{
				outfitDrawer.SelectButton.interactable = true;
				outfitDrawer.PagingSound = pagingSound;
			}
		}
	}

	private void OnDestroy()
	{
		if (OutfitManager.Instance != null)
		{
			OutfitManager.Instance.OutfitToolSpawned -= OutfitManager_ToolSpawned;
			OutfitManager.Instance.OutfitSelectionUnlocked -= OutfitManager_OutfitSelectionUnlocked;
		}
	}

	private void OutfitManager_OutfitSelectionUnlocked(OutfitSelection selection)
	{
		OutfitTool outfitTool = OutfitTool.Find(Player.LocalPlayer, selection, OutfitTool.ToolPurpose.Drawer);
		if (outfitTool == null)
		{
			OutfitManager.Instance.LocalSpawnOutfitTool(selection, OutfitTool.ToolPurpose.Drawer);
		}
	}

	private void LocalPlayer_IsInChangingRoomUpdated(bool inChangingRoom)
	{
		if (inChangingRoom)
		{
			return;
		}
		OutfitDrawer[] drawers = Drawers;
		foreach (OutfitDrawer outfitDrawer in drawers)
		{
			if (outfitDrawer != null)
			{
				outfitDrawer.IsSelected = false;
			}
		}
	}

	private IEnumerator RunInitializeOutfitTools()
	{
		while (!OutfitManager.IsInitialized || Player.LocalPlayer == null)
		{
			yield return null;
		}
		pendingSpawns.Clear();
		waitingOnSpawns = true;
		OutfitManager.Instance.OutfitToolSpawned += OutfitManager_ToolSpawned;
		OutfitManager.Instance.OutfitSelectionUnlocked += OutfitManager_OutfitSelectionUnlocked;
		foreach (OutfitSelection unlockedOutfitSelection in OutfitManager.Instance.UnlockedOutfitSelections)
		{
			if (unlockedOutfitSelection != null && unlockedOutfitSelection.outfitItem != null && unlockedOutfitSelection.outfitItem.DisplayOnRack)
			{
				string item = OutfitManager.Instance.LocalSpawnOutfitTool(unlockedOutfitSelection, OutfitTool.ToolPurpose.Drawer);
				pendingSpawns.Add(item);
			}
		}
	}

	private void OutfitManager_ToolSpawned(OutfitTool outfitTool, string guid)
	{
		if (pendingSpawns.Contains(guid))
		{
			pendingSpawns.Remove(guid);
		}
		if (outfitTool.Purpose == OutfitTool.ToolPurpose.Drawer)
		{
			OutfitDrawer rack = GetRack(outfitTool.OutfitTarget.outfitItem);
			if (rack != null)
			{
				rack.AddSelection(outfitTool);
				if (!waitingOnSpawns)
				{
					rack.UpdateOutfitPlacement();
				}
			}
			else
			{
				Debug.LogWarning("Null Drawer for outfitTool:" + outfitTool);
			}
		}
		if (waitingOnSpawns && pendingSpawns.Count == 0)
		{
			waitingOnSpawns = false;
			Debug.Log("Pending spawns complete. Updating all drawers at once");
			StartCoroutine(UpdateAllDrawerPlacements());
		}
	}

	private IEnumerator UpdateAllDrawerPlacements()
	{
		OutfitDrawer[] drawers = Drawers;
		foreach (OutfitDrawer d in drawers)
		{
			d.UpdateOutfitPlacement();
			yield return null;
		}
	}

	private void InitializeDrawerMap()
	{
		for (int i = 0; i < Drawers.Length; i++)
		{
			if (drawerMap.ContainsKey(Drawers[i].OutfitType))
			{
				Dictionary<string, OutfitDrawer> dictionary = drawerMap[Drawers[i].OutfitType];
				if (string.IsNullOrEmpty(Drawers[i].Name) || dictionary.ContainsKey(Drawers[i].Name))
				{
					Drawers[i].Name = Guid.NewGuid().ToString();
				}
				dictionary.Add(Drawers[i].Name, Drawers[i]);
				continue;
			}
			Dictionary<string, OutfitDrawer> dictionary2 = new Dictionary<string, OutfitDrawer>();
			if (string.IsNullOrEmpty(Drawers[i].Name))
			{
				Drawers[i].Name = Guid.NewGuid().ToString();
			}
			dictionary2.Add(Drawers[i].Name, Drawers[i]);
			drawerMap.Add(Drawers[i].OutfitType, dictionary2);
		}
	}

	private OutfitDrawer GetRack(OutfitItem outfitPrefab)
	{
		if (!drawerMap.ContainsKey(outfitPrefab.Type))
		{
			return null;
		}
		Dictionary<string, OutfitDrawer> dictionary = drawerMap[outfitPrefab.Type];
		if (dictionary.ContainsKey(outfitPrefab.PreferredRackName))
		{
			return dictionary[outfitPrefab.PreferredRackName];
		}
		if (!string.IsNullOrEmpty(outfitPrefab.PreferredRackName))
		{
			OutfitDrawer[] drawers = Drawers;
			foreach (OutfitDrawer outfitDrawer in drawers)
			{
				if (outfitPrefab.PreferredRackName == outfitDrawer.Name)
				{
					return outfitDrawer;
				}
			}
		}
		if (dictionary.Count > 0)
		{
			return dictionary.Values.First();
		}
		return null;
	}

	public void Button_RackSelected(Button button)
	{
		OutfitDrawer[] drawers = Drawers;
		foreach (OutfitDrawer outfitDrawer in drawers)
		{
			if (outfitDrawer != null && outfitDrawer.SelectButton == button)
			{
				AudioManager.Play3DSFX(openDrawer, base.transform.position);
				outfitDrawer.IsSelected = !outfitDrawer.IsSelected;
				break;
			}
		}
	}
}
