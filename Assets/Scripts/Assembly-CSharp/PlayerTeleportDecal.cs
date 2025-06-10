using System.Collections.Generic;
using UnityEngine;

public class PlayerTeleportDecal : MonoBehaviour
{
	public enum DecalVisibilityMode
	{
		None = 0,
		Ground = 1,
		AvatarAndGround = 2,
		ToolHit = 3,
		AvatarAndGroundAndArrow = 4
	}

	[Header("Avatar")]
	[SerializeField]
	private Transform head;

	[SerializeField]
	private Transform body;

	[SerializeField]
	private Transform ground;

	[SerializeField]
	private Transform arrow;

	[Header("Tool Hit Visual")]
	[SerializeField]
	private TeleportChargeVisual toolHitVisual;

	[Header("Materials")]
	[SerializeField]
	private Material validMaterial;

	[SerializeField]
	private Material invalidMaterial;

	[HideInInspector]
	public List<Renderer> Renderers = new List<Renderer>();

	private DecalVisibilityMode visibilityMode;

	private bool isValidPosition;

	public DecalVisibilityMode VisibilityMode
	{
		get
		{
			return visibilityMode;
		}
		set
		{
			visibilityMode = value;
			bool active = visibilityMode == DecalVisibilityMode.AvatarAndGround || visibilityMode == DecalVisibilityMode.AvatarAndGroundAndArrow;
			head.gameObject.SetActive(active);
			body.gameObject.SetActive(active);
			bool active2 = visibilityMode == DecalVisibilityMode.Ground || visibilityMode == DecalVisibilityMode.AvatarAndGround || visibilityMode == DecalVisibilityMode.AvatarAndGroundAndArrow;
			ground.gameObject.SetActive(active2);
			bool active3 = visibilityMode == DecalVisibilityMode.ToolHit;
			toolHitVisual.gameObject.SetActive(active3);
			bool active4 = visibilityMode == DecalVisibilityMode.AvatarAndGroundAndArrow;
			arrow.gameObject.SetActive(active4);
		}
	}

	public bool IsValidPosition
	{
		get
		{
			return isValidPosition;
		}
		set
		{
			if (isValidPosition != value)
			{
				isValidPosition = value;
				UpdateMaterials();
			}
		}
	}

	private void Awake()
	{
		Renderers.AddRange(GetComponentsInChildren<Renderer>(true));
		Renderer[] componentsInChildren = arrow.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer item in componentsInChildren)
		{
			Renderers.Remove(item);
		}
		VisibilityMode = DecalVisibilityMode.None;
		UpdateMaterials();
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void UpdateAvatarDecal(Player player, Vector3 groundPoint, bool isRotating, Vector3 rotationForward)
	{
		ground.position = groundPoint;
		arrow.position = groundPoint;
		arrow.rotation = ((!isRotating) ? Quaternion.identity : Quaternion.LookRotation(rotationForward, Vector3.up));
		Quaternion quaternion = Quaternion.identity;
		if (isRotating)
		{
			Vector3 fromDirection = player.transform.forward;
			if (PlatformManager.Instance.CurrentTrackingMode == PlatformManager.TrackingMode.THREE_SIXTY_DEGREE)
			{
				fromDirection = Vector3.ProjectOnPlane(player.Head.transform.forward, Vector3.up).normalized;
			}
			quaternion = Quaternion.FromToRotation(fromDirection, rotationForward);
		}
		head.position = groundPoint + player.Head.HeightOffset;
		head.rotation = player.Head.transform.rotation * quaternion;
		body.position = head.position - player.Body.ToHead;
		body.rotation = player.Body.transform.rotation * quaternion;
	}

	public void UpdateToolHitDecal(bool enabled, float charge, Vector3 toolHitPosition, Vector3 toolHitNormal)
	{
		if (enabled)
		{
			VisibilityMode = DecalVisibilityMode.ToolHit;
			toolHitVisual.Charge = charge;
			toolHitVisual.transform.position = toolHitPosition;
			toolHitVisual.transform.rotation = Quaternion.LookRotation(toolHitNormal, Vector3.up);
		}
	}

	private void UpdateMaterials()
	{
		foreach (Renderer renderer in Renderers)
		{
			renderer.material = ((!IsValidPosition) ? invalidMaterial : validMaterial);
		}
	}
}
