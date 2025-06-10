using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DesaturateEffect : MonoBehaviour
{
	public enum Mode
	{
		FullScreen = 0,
		PartialScreen = 1,
		TargetedToOverlayUI = 2
	}

	[SerializeField]
	private Shader desaturateEffectShader;

	[SerializeField]
	private Mode mode;

	[Header("PartialScreen Mode Config")]
	[SerializeField]
	private Vector2 partialScreenBottomLeft = new Vector2(0f, 0.2f);

	[SerializeField]
	private Vector2 partialScreenTopRight = new Vector2(1f, 0.4f);

	[SerializeField]
	[Range(0f, 1f)]
	private float partialScreenEdgeFade = 0.02f;

	[Header("TargetedToOverlayUI Mode Config")]
	[SerializeField]
	private int maskTextureSize = 256;

	[SerializeField]
	private int expandIterations = 4;

	[SerializeField]
	private int blurIterations = 2;

	[Header("Desaturation Config")]
	[SerializeField]
	[Range(0f, 1f)]
	private float maxDesaturation = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float desaturatedBlackLevel;

	[SerializeField]
	[Range(0f, 1f)]
	private float desaturatedWhiteLevel = 1f;

	private Camera camera;

	private Camera desaturateMaskGenerator;

	private RenderTexture desaturateMaskTexture;

	private Material desaturateMaterial;

	private float currentDesaturation;

	private static int SampleOffsetPropId;

	private static int RegionParamsPropId;

	private static int ScreenEdgeFadePropId;

	private static int DesaturationAmountPropId;

	private static int ColorMappingParamsPropId;

	private static int DesaturationMaskPropId;

	public float DesaturationLevel
	{
		get
		{
			return currentDesaturation;
		}
		set
		{
			currentDesaturation = value;
			base.enabled = currentDesaturation > 0f || mode == Mode.TargetedToOverlayUI;
		}
	}

	public Mode CurrentMode
	{
		get
		{
			return mode;
		}
		set
		{
			mode = value;
			base.enabled = currentDesaturation > 0f || mode == Mode.TargetedToOverlayUI;
		}
	}

	private void Awake()
	{
		SampleOffsetPropId = Shader.PropertyToID("_SampleOffset");
		RegionParamsPropId = Shader.PropertyToID("_RegionParams");
		ScreenEdgeFadePropId = Shader.PropertyToID("_ScreenEdgeFade");
		DesaturationAmountPropId = Shader.PropertyToID("_DesaturationAmount");
		ColorMappingParamsPropId = Shader.PropertyToID("_ColorMappingParams");
		DesaturationMaskPropId = Shader.PropertyToID("_DesaturationMask");
		camera = GetComponent<Camera>();
		int num = maskTextureSize;
		int height = maskTextureSize;
		if (camera.stereoEnabled)
		{
			num *= 2;
		}
		desaturateMaskTexture = new RenderTexture(num, height, 0);
		GameObject gameObject = new GameObject("DesatMaskGenerator", typeof(Camera));
		desaturateMaskGenerator = gameObject.GetComponent<Camera>();
		desaturateMaskGenerator.transform.parent = base.transform;
		desaturateMaskGenerator.cullingMask = 4194304;
		desaturateMaskGenerator.clearFlags = CameraClearFlags.Color;
		desaturateMaskGenerator.backgroundColor = Color.clear;
		desaturateMaskGenerator.targetTexture = desaturateMaskTexture;
		desaturateMaskGenerator.enabled = false;
		desaturateMaterial = new Material(desaturateEffectShader);
	}

	private void OnPreRender()
	{
		if (mode == Mode.TargetedToOverlayUI)
		{
			GenerateDesatMask();
		}
	}

	private void GenerateDesatMask()
	{
		if (camera.stereoEnabled)
		{
			desaturateMaskGenerator.worldToCameraMatrix = camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
			desaturateMaskGenerator.projectionMatrix = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
			desaturateMaskGenerator.rect = new Rect(0f, 0f, 0.5f, 1f);
			desaturateMaskGenerator.Render();
			desaturateMaskGenerator.worldToCameraMatrix = camera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
			desaturateMaskGenerator.projectionMatrix = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
			desaturateMaskGenerator.rect = new Rect(0.5f, 0f, 0.5f, 1f);
			desaturateMaskGenerator.Render();
		}
		else
		{
			desaturateMaskGenerator.worldToCameraMatrix = camera.worldToCameraMatrix;
			desaturateMaskGenerator.projectionMatrix = camera.projectionMatrix;
			desaturateMaskGenerator.rect = new Rect(0f, 0f, 1f, 1f);
			desaturateMaskGenerator.Render();
		}
		int width = desaturateMaskTexture.width / 4;
		int height = desaturateMaskTexture.height / 4;
		RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0);
		desaturateMaterial.EnableKeyword("COMBINE_MAX");
		desaturateMaterial.SetFloat(SampleOffsetPropId, 0.5f);
		Graphics.Blit(desaturateMaskTexture, renderTexture, desaturateMaterial, 0);
		desaturateMaterial.SetFloat(SampleOffsetPropId, 1f);
		for (int i = 0; i < expandIterations; i++)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0);
			Graphics.Blit(renderTexture, temporary, desaturateMaterial, 0);
			RenderTexture.ReleaseTemporary(renderTexture);
			renderTexture = temporary;
		}
		desaturateMaterial.DisableKeyword("COMBINE_MAX");
		for (int j = 0; j < blurIterations; j++)
		{
			desaturateMaterial.SetFloat(SampleOffsetPropId, 0.5f + (float)j * 0.6f);
			RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, 0);
			Graphics.Blit(renderTexture, temporary2, desaturateMaterial, 0);
			RenderTexture.ReleaseTemporary(renderTexture);
			renderTexture = temporary2;
		}
		Graphics.Blit(renderTexture, desaturateMaskTexture);
		RenderTexture.ReleaseTemporary(renderTexture);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		desaturateMaterial.SetKeywordEnabled("FULL_SCREEN_DESATURATION", mode == Mode.FullScreen);
		desaturateMaterial.SetKeywordEnabled("PARTIAL_SCREEN_DESATURATION", mode == Mode.PartialScreen);
		desaturateMaterial.SetKeywordEnabled("USE_DESATURATION_MASK", mode == Mode.TargetedToOverlayUI);
		desaturateMaterial.SetVector(ColorMappingParamsPropId, new Vector4(desaturatedWhiteLevel - desaturatedBlackLevel, desaturatedBlackLevel, maxDesaturation, 0f));
		if (mode == Mode.TargetedToOverlayUI)
		{
			desaturateMaterial.SetTexture(DesaturationMaskPropId, desaturateMaskTexture);
		}
		else
		{
			desaturateMaterial.SetVector(RegionParamsPropId, new Vector4(partialScreenBottomLeft.x, partialScreenBottomLeft.y, partialScreenTopRight.x, partialScreenTopRight.y));
			desaturateMaterial.SetFloat(ScreenEdgeFadePropId, partialScreenEdgeFade);
			desaturateMaterial.SetFloat(DesaturationAmountPropId, currentDesaturation);
		}
		Graphics.Blit(source, destination);
	}
}
