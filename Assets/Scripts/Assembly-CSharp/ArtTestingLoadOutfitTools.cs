using System.Collections;
using UnityEngine;

public class ArtTestingLoadOutfitTools : MonoBehaviour
{
	private OutfitDrawer drawer;

	private void Awake()
	{
		VerifySingleCopyOfScript();
		StartCoroutine(RunClearScene());
	}

	private IEnumerator RunClearScene()
	{
		OutfitItem[] outfits = Object.FindObjectsOfType<OutfitItem>();
		OutfitItem[] array = outfits;
		foreach (OutfitItem outfitItem in array)
		{
			outfitItem.gameObject.SetActive(false);
			Object.DestroyImmediate(outfitItem.gameObject);
		}
		while (OutfitManager.Instance == null || Player.LocalPlayer == null || !Player.LocalPlayer.IsInitialized)
		{
			yield return null;
		}
		drawer = base.gameObject.AddComponent<OutfitDrawer>();
		drawer.IsSelected = true;
		Vector3 startPosition = new Vector3(0f, 1.5f, 0f);
		for (int j = 0; j < OutfitManager.Instance.OutfitPrefabs.Count; j++)
		{
			OutfitItem outfit = OutfitManager.Instance.OutfitPrefabs[j];
			CreateCombinations(outfit, startPosition + j * Vector3.back * 0.3f);
		}
	}

	private void CreateCombinations(OutfitItem outfit, Vector3 startPosition)
	{
		CreateOutfitTool(new OutfitSelection(outfit), startPosition);
		int num = 1;
		if (outfit.SelectionGroups == null)
		{
			return;
		}
		for (int i = 0; i < outfit.SelectionGroups.Length; i++)
		{
			OutfitItem.OutfitSelectionGroup outfitSelectionGroup = outfit.SelectionGroups[i];
			if (!outfitSelectionGroup.IsValid)
			{
				continue;
			}
			for (int j = 0; j <= outfitSelectionGroup.decals.Count; j++)
			{
				for (int k = 0; k < outfitSelectionGroup.masks.Count; k++)
				{
					for (int l = 0; l < outfitSelectionGroup.swatches.Count; l++)
					{
						CreateOutfitTool(new OutfitSelection(outfit, (l >= outfitSelectionGroup.swatches.Count) ? null : outfitSelectionGroup.swatches[l], (k >= outfitSelectionGroup.masks.Count) ? null : outfitSelectionGroup.masks[k], (j >= outfitSelectionGroup.decals.Count) ? null : outfitSelectionGroup.decals[j]), startPosition + num * Vector3.left * ((outfit.Type != OutfitManager.OutfitType.Shirt) ? 0.25f : 0.5f));
						num++;
					}
				}
			}
		}
	}

	private void CreateOutfitTool(OutfitSelection selection, Vector3 position)
	{
		OutfitTool outfitTool = OutfitTool.Find(PhotonNetwork.player, selection, OutfitTool.ToolPurpose.Drawer);
		if (outfitTool == null)
		{
			OutfitManager.Instance.LocalSpawnOutfitTool(selection, OutfitTool.ToolPurpose.Drawer);
			outfitTool = OutfitTool.Find(PhotonNetwork.player, selection, OutfitTool.ToolPurpose.Drawer);
			if (outfitTool != null)
			{
				drawer.AddSelection(outfitTool);
				outfitTool.transform.position = position;
				outfitTool.DrawerPosition = position;
				outfitTool.CurrentState = OutfitTool.OutfitToolState.InOpenDrawer;
			}
		}
	}

	private void VerifySingleCopyOfScript()
	{
		ArtTestingLoadOutfitTools[] array = Object.FindObjectsOfType<ArtTestingLoadOutfitTools>();
		if (array.Length > 1)
		{
			Debug.LogError("Found two ArtTestingLoadOutfitTools in scene. This will break expected behavior.");
			ArtTestingLoadOutfitTools[] array2 = array;
			foreach (ArtTestingLoadOutfitTools artTestingLoadOutfitTools in array2)
			{
				Debug.LogError("ArtTestingLoadOutfitTools on gameObject:" + artTestingLoadOutfitTools.gameObject, artTestingLoadOutfitTools.gameObject);
			}
		}
	}
}
