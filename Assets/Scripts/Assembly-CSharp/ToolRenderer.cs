using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ToolRenderer : MonoBehaviour
{
	public enum HighlightMode
	{
		None = 0,
		Physical = 1,
		Magnet = 2,
		SmartTool = 3,
		Locked = 4
	}

	protected static readonly Color noHighlightColor = new Color(0f, 0f, 0f, 0f);

	protected static readonly Color physicalPickupHighlightColor = new Color(0.75f, 0.25f, 0.25f, 1f);

	protected static readonly Color magnetPickupHighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

	protected static readonly Color smartToolPickupHighlightColor = new Color(0f, 0.2f, 0.7f, 1f);

	protected static readonly Color lockedToolPickupHighlightColor = new Color(0.36f, 0.75f, 0f, 1f);

	[SerializeField]
	protected bool visibleWhileRemoteVisible;

	[SerializeField]
	protected List<Renderer> ignoredRenderers = new List<Renderer>();

	[SerializeField]
	protected Transform[] ignoredRendererRoots;

	[Header("Accent Renderers")]
	[SerializeField]
	protected bool useTeamColorAsAccentColor = true;

	[SerializeField]
	protected ModelMaterial[] accentRenderers;

	[SerializeField]
	protected Color defaultAccentColor;

	[Header("Highlight")]
	[SerializeField]
	protected bool supportsLockedHightlight = true;

	protected Tool thisTool;

	protected Color color = Color.white;

	protected HighlightMode mode;

	protected static int emissionColorId;

	protected Color accentColor;

	protected Dictionary<Renderer, Material> defaultMaterials;

	protected bool ghosted;

	public Color AccentColor
	{
		get
		{
			return accentColor;
		}
		set
		{
			accentColor = value;
			if (accentRenderers != null)
			{
				ModelMaterial[] array = accentRenderers;
				for (int i = 0; i < array.Length; i++)
				{
					ModelMaterial modelMaterial = array[i];
					modelMaterial.Renderer.materials[modelMaterial.MaterialIndex].color = accentColor;
				}
			}
			if (this.AccentColorUpdateEvent != null)
			{
				this.AccentColorUpdateEvent();
			}
		}
	}

	public Color Color
	{
		set
		{
			color = value;
			ForAllMaterialsInAllRenderers(delegate(Material mat)
			{
				mat.color = color;
			});
		}
	}

	public virtual HighlightMode Mode
	{
		get
		{
			return mode;
		}
		set
		{
			if (mode != value)
			{
				mode = value;
				Color highlightColor = magnetPickupHighlightColor;
				if (mode == HighlightMode.Physical)
				{
					highlightColor = physicalPickupHighlightColor;
				}
				else if (mode == HighlightMode.Magnet)
				{
					highlightColor = magnetPickupHighlightColor;
				}
				else if (mode == HighlightMode.SmartTool)
				{
					highlightColor = smartToolPickupHighlightColor;
				}
				else if (mode == HighlightMode.Locked && supportsLockedHightlight)
				{
					highlightColor = lockedToolPickupHighlightColor;
				}
				else
				{
					highlightColor = noHighlightColor;
				}
				ForAllMaterialsInAllRenderers(delegate(Material mat)
				{
					mat.SetColor(emissionColorId, highlightColor);
				});
			}
		}
	}

	public List<Renderer> Renderers { get; private set; }

	public Bounds Bounds
	{
		get
		{
			Bounds bounds;
			UnityExtensions.GetCombinedRendererBounds(Renderers, out bounds);
			return bounds;
		}
	}

	public bool Visible
	{
		get
		{
			return Renderers != null && Renderers[0].enabled && Renderers[0].gameObject.activeSelf;
		}
		private set
		{
			if (Renderers == null)
			{
				return;
			}
			foreach (Renderer renderer in Renderers)
			{
				renderer.enabled = value;
			}
			foreach (Renderer ignoredRenderer in ignoredRenderers)
			{
				ignoredRenderer.enabled = value;
			}
			if (this.VisibleChangeEvent != null)
			{
				this.VisibleChangeEvent();
			}
		}
	}

	public event Action AccentColorUpdateEvent;

	public event Action VisibleChangeEvent;

	public void SetVisible(bool toolVisible, bool remoteVisible = false)
	{
		Visible = toolVisible && (visibleWhileRemoteVisible || !remoteVisible);
	}

	protected virtual void Awake()
	{
		InitializeRenderer();
		ResetAccentColor();
	}

	protected virtual void Start()
	{
		thisTool = GetComponent<Tool>();
		thisTool.PickupEvent += OnPickup;
		thisTool.ResetEvent += OnReset;
		thisTool.OwnerRoleUpdateEvent += OnOwnerRoleUpdated;
		AddInstantiatedIgnoredRenderers();
	}

	protected virtual void Update()
	{
		UpdateGhostedVisuals();
	}

	protected virtual void OnDestroy()
	{
		if (thisTool != null)
		{
			thisTool.PickupEvent -= OnPickup;
			thisTool.ResetEvent -= OnReset;
		}
	}

	public void ResetAccentColor()
	{
		AccentColor = defaultAccentColor;
	}

	private void InitializeRenderer()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(true);
		defaultMaterials = new Dictionary<Renderer, Material>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (!ignoredRenderers.Contains(renderer))
			{
				AddRenderer(renderer);
			}
			if (renderer as ParticleSystemRenderer == null)
			{
				defaultMaterials.Add(renderer, renderer.material);
			}
		}
	}

	private void AddInstantiatedIgnoredRenderers()
	{
		if (ignoredRendererRoots == null || ignoredRendererRoots.Length <= 0)
		{
			return;
		}
		List<Renderer> list = new List<Renderer>();
		for (int i = 0; i < ignoredRendererRoots.Length; i++)
		{
			list.AddRange(ignoredRendererRoots[i].GetComponentsInChildren<Renderer>());
		}
		List<int> list2 = new List<int>();
		for (int j = 0; j < Renderers.Count; j++)
		{
			if (list.Contains(Renderers[j]))
			{
				list2.Add(j);
			}
		}
		for (int k = 0; k < list2.Count; k++)
		{
			Renderers.RemoveAt(list2[k]);
		}
		ignoredRenderers.AddRange(list);
	}

	private void AddRenderer(Renderer renderer, Material highlightMaterialOverride = null)
	{
		if (Renderers == null)
		{
			Renderers = new List<Renderer>();
			emissionColorId = Shader.PropertyToID("_EmissionColor");
		}
		Material[] materials = renderer.materials;
		foreach (Material material in materials)
		{
			material.EnableKeyword("_EMISSION");
		}
		if (!Renderers.Contains(renderer))
		{
			Renderers.Add(renderer);
		}
	}

	public void SetupRenderer(Renderer newMeshRenderer, Material highlightMaterialOverride = null)
	{
		AddRenderer(newMeshRenderer, highlightMaterialOverride);
	}

	protected void ForAllMaterialsInAllRenderers(Action<Material> matAction)
	{
		ForAllMaterialsInRenderers(Renderers, matAction);
	}

	protected void ForAllMaterialsInRenderers(List<Renderer> renderers, Action<Material> matAction)
	{
		if (renderers == null)
		{
			return;
		}
		foreach (Renderer renderer in renderers)
		{
			Material[] materials = renderer.materials;
			foreach (Material obj in materials)
			{
				matAction(obj);
			}
		}
	}

	protected void OnPickup(Tool tool)
	{
		UpdateTeamAccentColor();
	}

	protected void OnReset(Tool tool, Vector3 position, Quaternion rotation)
	{
		if (useTeamColorAsAccentColor)
		{
			ResetAccentColor();
		}
	}

	protected void OnOwnerRoleUpdated(Tool tool)
	{
		UpdateTeamAccentColor();
	}

	public void UpdateTeamAccentColor()
	{
		if (useTeamColorAsAccentColor)
		{
			GameTeam gameTeam = ((!(thisTool.Owner != null)) ? GameTeam.INVALID : thisTool.Owner.Team);
			if (gameTeam != GameTeam.INVALID)
			{
				AccentColor = GameTeamSettings.GetTeamColor(gameTeam);
			}
			else
			{
				ResetAccentColor();
			}
		}
	}

	protected virtual void UpdateGhostedVisuals()
	{
		if (!ghosted)
		{
			if (!(Player.LocalPlayer != null) || !Player.LocalPlayer.SituationPulse.Active || !((Player.LocalPlayer.CurrentFloorPosition - base.transform.position).sqrMagnitude < Player.LocalPlayer.SituationPulse.Radius * Player.LocalPlayer.SituationPulse.Radius))
			{
				return;
			}
			foreach (Renderer key in defaultMaterials.Keys)
			{
				key.material = Player.LocalPlayer.SituationPulse.GhostMaterial;
			}
			ghosted = true;
		}
		else
		{
			if (!(Player.LocalPlayer != null) || (Player.LocalPlayer.SituationPulse.Active && !((Player.LocalPlayer.CurrentFloorPosition - base.transform.position).sqrMagnitude >= Player.LocalPlayer.SituationPulse.Radius * Player.LocalPlayer.SituationPulse.Radius)))
			{
				return;
			}
			foreach (Renderer key2 in defaultMaterials.Keys)
			{
				key2.material = defaultMaterials[key2];
			}
			ghosted = false;
		}
	}
}
