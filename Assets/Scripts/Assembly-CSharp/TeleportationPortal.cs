using System;
using UnityEngine;

public class TeleportationPortal : MonoBehaviour
{
	public Transform Target;

	[Header("Interactions")]
	[SerializeField]
	private bool supportsHandInteraction = true;

	[Header("Target Activity")]
	[SerializeField]
	private bool supportsActivityLoading;

	[SerializeField]
	private string destinationActivity = string.Empty;

	[Tooltip("If the user can teleport with a tool in hand?")]
	public bool CanCarryTools;

	public Renderer HighlightRenderer;

	[SerializeField]
	private int highlightRendererMaterialIndex;

	[SerializeField]
	private Color highlightColor = Color.cyan;

	private Material[] highlightMaterials;

	private Material[] defaultMaterials;

	[HideInInspector]
	public Vector3? TargetPositionOverride;

	[HideInInspector]
	public Quaternion? TargetRotationOverride;

	private bool _highlight;

	public Vector3 DestinationPosition
	{
		get
		{
			if (TargetPositionOverride.HasValue)
			{
				return TargetPositionOverride.Value;
			}
			if (Target != null)
			{
				return Target.transform.position;
			}
			return base.transform.position;
		}
	}

	public Vector3 DestinationForward
	{
		get
		{
			Quaternion quaternion = base.transform.rotation;
			if (TargetRotationOverride.HasValue)
			{
				quaternion = TargetRotationOverride.Value;
			}
			else if (Target != null)
			{
				quaternion = Target.transform.rotation;
			}
			return quaternion * Vector3.forward;
		}
	}

	public virtual bool SupportsPlayerRespawn
	{
		get
		{
			return false;
		}
	}

	public bool SupportsActivityLoading
	{
		get
		{
			return supportsActivityLoading;
		}
	}

	public bool SupportsHandInteraction
	{
		get
		{
			return supportsHandInteraction;
		}
	}

	public string DestinationActivity
	{
		get
		{
			return destinationActivity;
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
			if (HighlightRenderer != null && _highlight != value)
			{
				_highlight = value;
				HighlightRenderer.materials = ((!_highlight) ? defaultMaterials : highlightMaterials);
			}
		}
	}

	public virtual bool IsValid
	{
		get
		{
			return base.isActiveAndEnabled && (Target != null || SupportsActivityLoading || SupportsPlayerRespawn || TargetPositionOverride.HasValue);
		}
	}

	public event Action<Player> PlayerUseEvent;

	public void FirePlayerUseEvent(Player player)
	{
		if (this.PlayerUseEvent != null)
		{
			this.PlayerUseEvent(player);
		}
	}

	private void Awake()
	{
		if (!(HighlightRenderer != null))
		{
			return;
		}
		defaultMaterials = HighlightRenderer.materials;
		highlightMaterials = new Material[defaultMaterials.Length];
		for (int i = 0; i < highlightMaterials.Length; i++)
		{
			highlightMaterials[i] = new Material(defaultMaterials[i]);
			if (i == highlightRendererMaterialIndex)
			{
				highlightMaterials[i].EnableKeyword("_EMISSION");
				highlightMaterials[i].SetColor("_EmissionColor", highlightColor);
			}
		}
	}

	private void OnDisable()
	{
		if (Highlight)
		{
			Highlight = false;
		}
	}
}
