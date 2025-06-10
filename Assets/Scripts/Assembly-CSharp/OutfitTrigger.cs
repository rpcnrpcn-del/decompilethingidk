using UnityEngine;

public class OutfitTrigger : MonoBehaviour
{
	private BoxCollider boxCollider;

	public OutfitItem OutfitItem { get; private set; }

	public Transform TargetBone { get; set; }

	public bool EnabledOutsideChangingRoom { get; set; }

	public void SetOutfitOwner(OutfitItem outfitItem)
	{
		OutfitItem = outfitItem;
		TargetBone = outfitItem.SkinnedMeshRenderer.bones[0];
		if (boxCollider != null)
		{
			Object.Destroy(boxCollider);
			boxCollider = null;
		}
		boxCollider = base.gameObject.AddComponent<BoxCollider>();
		boxCollider.size = outfitItem.SkinnedMeshRenderer.bounds.size;
		boxCollider.isTrigger = true;
		boxCollider.tag = "Outfit";
		base.gameObject.layer = 15;
		base.name = outfitItem.name + "_trigger";
	}

	private void LateUpdate()
	{
		if (OutfitItem != null && TargetBone != null && Player.LocalPlayer != null)
		{
			base.transform.position = OutfitItem.SkinnedMeshRenderer.bounds.center;
			base.transform.rotation = TargetBone.rotation;
		}
	}
}
