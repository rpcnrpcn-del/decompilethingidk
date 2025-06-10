using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;
using UnityStandardAssets.CinematicEffects;

public class TutorialCameraTool : Tool
{
	[Header("Camera")]
	[SerializeField]
	private Camera camera;

	[SerializeField]
	private VideoCapture videoCapture;

	[SerializeField]
	private Vector2 lowResolution = new Vector2(320f, 180f);

	[SerializeField]
	private Vector2 highResolution = new Vector2(640f, 360f);

	[Header("Screens")]
	[SerializeField]
	private Renderer[] screenRenderers;

	[SerializeField]
	private Material screenRenderMaterial;

	[Header("Capture Feedback")]
	[SerializeField]
	private GameObject recordingFeedback;

	[SerializeField]
	private Slider recordingProgressFeedback;

	[SerializeField]
	private GameObject exportingFeedback;

	[SerializeField]
	private Text savedFeedback;

	private bool? usingHighResolution;

	private RenderTexture renderTexture;

	private Material screenRenderMaterialCopy;

	private static readonly int screenAnimationId = Animator.StringToHash("ScreenOpen");

	public bool IsRecording
	{
		get
		{
			return videoCapture.IsCapturing;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		videoCapture.RecordingCamera = camera;
		videoCapture.CaptureStartedEvent += OnCaptureStarted;
		videoCapture.ExportStartedEvent += OnExportStarted;
		videoCapture.ExportFinishedEvent += OnExportFinished;
		exportingFeedback.SetActive(false);
		recordingFeedback.SetActive(false);
		recordingProgressFeedback.gameObject.SetActive(false);
		savedFeedback.gameObject.SetActive(false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		videoCapture.CaptureStartedEvent -= OnCaptureStarted;
		videoCapture.ExportStartedEvent -= OnExportStarted;
		videoCapture.ExportFinishedEvent -= OnExportFinished;
	}

	protected override void Start()
	{
		base.Start();
		screenRenderMaterialCopy = new Material(screenRenderMaterial);
		Renderer[] array = screenRenderers;
		foreach (Renderer renderer in array)
		{
			renderer.material = screenRenderMaterialCopy;
		}
		float aspect = lowResolution.x / lowResolution.y;
		camera.aspect = aspect;
		TonemappingColorGrading.LUTSettings lut = Camera.main.GetComponent<TonemappingColorGrading>().lut;
		camera.GetComponent<TonemappingColorGrading>().lut = lut;
		SwitchTextureResolution(false);
		GetComponentInChildren<Animator>().SetBool(screenAnimationId, true);
	}

	private void SwitchTextureResolution(bool useHighResolution)
	{
		if (!usingHighResolution.HasValue || usingHighResolution.Value != useHighResolution)
		{
			usingHighResolution = useHighResolution;
			Vector2 vector = ((!useHighResolution) ? lowResolution : highResolution);
			renderTexture = new RenderTexture((int)vector.x, (int)vector.y, 24);
			renderTexture.antiAliasing = ((QualitySettings.antiAliasing <= 0) ? 1 : QualitySettings.antiAliasing);
			if (camera != null)
			{
				camera.targetTexture = renderTexture;
			}
			screenRenderMaterialCopy.mainTexture = renderTexture;
		}
	}

	private void Update()
	{
		if (IsRecording)
		{
			float num = videoCapture.UpdateFrame(renderTexture);
			recordingProgressFeedback.value = num;
			if (num >= 1f)
			{
				FinishCapturing();
			}
		}
	}

	private void StartCapturing()
	{
		SwitchTextureResolution(true);
		VRSettings.renderScale = 0.5f;
		if (!IsRecording)
		{
			videoCapture.StartCapture();
		}
	}

	private void FinishCapturing()
	{
		if (IsRecording)
		{
			videoCapture.FinishCapture();
		}
		SwitchTextureResolution(false);
		VRSettings.renderScale = 1f;
	}

	public void ToggleRecording()
	{
		if (!IsRecording)
		{
			StartCapturing();
		}
		else
		{
			FinishCapturing();
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Graphics.Blit(renderTexture, dest);
	}

	private void OnCaptureStarted()
	{
		recordingFeedback.gameObject.SetActive(true);
		recordingProgressFeedback.gameObject.SetActive(true);
	}

	private void OnExportStarted()
	{
		recordingFeedback.gameObject.SetActive(false);
		recordingProgressFeedback.gameObject.SetActive(false);
		exportingFeedback.SetActive(true);
	}

	private void OnExportFinished(string filename)
	{
		exportingFeedback.SetActive(false);
		StartCoroutine(DisplayFileSaveFeedbackCoroutine(filename));
	}

	private IEnumerator DisplayFileSaveFeedbackCoroutine(string filename)
	{
		filename = Path.GetFileName(filename);
		savedFeedback.gameObject.SetActive(true);
		savedFeedback.text = filename + " Saved!";
		yield return new WaitForSeconds(2f);
		savedFeedback.gameObject.SetActive(false);
	}
}
