using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimateInOut : MonoBehaviour
{
	[Header("Prefab")]
	[SerializeField]
	private ParticleSystem effectPrefab;

	[Header("Bounds")]
	[SerializeField]
	private Collider boundingCollider;

	[Header("Timing")]
	[SerializeField]
	private float animInStartOffset = 0.2f;

	[SerializeField]
	private float animOutStartOffset = 0.2f;

	[Header("Effect Support")]
	[SerializeField]
	private bool supportsAnimateIn = true;

	[SerializeField]
	private bool supportsAnimateOut = true;

	public bool SuppressAnimation;

	private ParticleSystem latestEffect;

	private bool updatePositionWhilePlaying = true;

	private List<Renderer> renderers;

	public List<Renderer> ignoredRenderers = new List<Renderer>();

	public List<Renderer> additionalRenderers = new List<Renderer>();

	private void Awake()
	{
		SceneManager.sceneUnloaded += OnSceneUnloaded;
	}

	private void OnEnable()
	{
		if (!SuppressAnimation && !AnimateInOutManager.SuppressAnimations && supportsAnimateIn)
		{
			PlayEffect(true);
		}
	}

	private void OnDisable()
	{
		if (!SuppressAnimation && !AnimateInOutManager.SuppressAnimations && supportsAnimateOut)
		{
			PlayEffect(false);
		}
	}

	private void Update()
	{
		if ((bool)latestEffect && latestEffect.isPlaying && updatePositionWhilePlaying)
		{
			UpdateEffectPosition(base.transform.position);
		}
	}

	public void PlayEffect(bool animIn, Vector3? position = null)
	{
		if (AnimateInOutManager.Instance != null)
		{
			if (effectPrefab == null)
			{
				effectPrefab = AnimateInOutManager.Instance.DefaultEffectPrefab;
			}
			latestEffect = Object.Instantiate(effectPrefab);
			latestEffect.transform.parent = AnimateInOutManager.Instance.transform;
			UpdateEffectPosition((!position.HasValue) ? base.transform.position : position.Value);
			latestEffect.Simulate((!animIn) ? animOutStartOffset : animInStartOffset);
			latestEffect.Play();
			updatePositionWhilePlaying = animIn && !position.HasValue;
		}
	}

	public void StopEffect()
	{
		if ((bool)latestEffect)
		{
			Object.Destroy(latestEffect.gameObject);
			latestEffect = null;
		}
	}

	private void UpdateEffectPosition(Vector3 position)
	{
		Bounds bounds;
		if (GetEffectBounds(out bounds))
		{
			latestEffect.transform.position = bounds.center - base.transform.position + position;
			ParticleSystem.ShapeModule shape = latestEffect.shape;
			shape.box = bounds.size * 1.3f;
			float num = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
			ParticleSystem.MainModule main = latestEffect.main;
			main.startSize = num * 0.3f;
		}
		else
		{
			latestEffect.transform.position = position;
		}
	}

	private bool GetEffectBounds(out Bounds bounds)
	{
		if (boundingCollider != null)
		{
			bounds = boundingCollider.bounds;
			return true;
		}
		if (renderers == null)
		{
			renderers = GetComponentsInChildren<Renderer>().ToList();
			renderers.RemoveAll((Renderer r) => ignoredRenderers.Contains(r));
			renderers.RemoveAll((Renderer r) => r.gameObject.GetComponent<ImpactDecal>() != null);
		}
		return UnityExtensions.GetCombinedRendererBounds(renderers, out bounds);
	}

	private void OnSceneUnloaded(Scene scene)
	{
		SceneManager.sceneUnloaded -= OnSceneUnloaded;
		if (latestEffect != null)
		{
			Object.DestroyImmediate(latestEffect.gameObject);
		}
	}
}
