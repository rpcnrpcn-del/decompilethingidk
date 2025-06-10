using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CinematicEffects;

public class HandheldCameraTool : Tool
{
	[Header("Camera")]
	[SerializeField]
	private Camera camera;

	[SerializeField]
	private Transform lensTransform;

	[SerializeField]
	private Vector2 lowResolution = new Vector2(320f, 180f);

	[SerializeField]
	private Vector2 highResolution = new Vector2(640f, 360f);

	[SerializeField]
	private bool hideOwner = true;

	[Header("Screens")]
	[SerializeField]
	private Renderer[] screenRenderers;

	[SerializeField]
	private Renderer externalScreenRenderer;

	[SerializeField]
	private Material screenRenderMaterial;

	[SerializeField]
	private Material defaultScreenMaterial;

	[Header("Smoothing")]
	[SerializeField]
	private float positionSmoothingTime = 0.25f;

	[Header("Capture Feedback")]
	[SerializeField]
	private GameObject recordingFeedback;

	[SerializeField]
	private Slider recordingProgressFeedback;

	[SerializeField]
	private GameObject exportingFeedback;

	[SerializeField]
	private Text savedFeedback;

	private RenderTexture renderTexture;

	private Vector3 velocity = Vector3.zero;

	private Vector3 rotationVelocity = Vector3.zero;

	private bool? usingHighResolution;

	private float aspect = 1f;

	private VideoCapture videoCapture;

	private Material screenRenderMaterialCopy;

	private Animator animator;

	private static readonly int triggerId = Animator.StringToHash("Trigger");

	private static readonly int screenAnimationId = Animator.StringToHash("ScreenOpen");

	private float triggerAmount;

	private bool screenOpen;

	[NonSerialized]
	public bool RemoteControl;

	protected override void Awake()
	{
		base.Awake();
		aspect = lowResolution.x / lowResolution.y;
		videoCapture = GetComponent<VideoCapture>();
		if (videoCapture != null)
		{
			videoCapture.RecordingCamera = camera;
			videoCapture.CaptureStartedEvent += OnCaptureStarted;
			videoCapture.ExportStartedEvent += OnExportStarted;
			videoCapture.ExportFinishedEvent += OnExportFinished;
		}
		exportingFeedback.SetActive(false);
		recordingFeedback.SetActive(false);
		recordingProgressFeedback.gameObject.SetActive(false);
		savedFeedback.gameObject.SetActive(false);
		screenRenderMaterialCopy = new Material(screenRenderMaterial);
		animator = GetComponentInChildren<Animator>();
	}

	protected override void Start()
	{
		base.Start();
		camera.aspect = aspect;
		TonemappingColorGrading.LUTSettings lut = Camera.main.GetComponent<TonemappingColorGrading>().lut;
		camera.GetComponent<TonemappingColorGrading>().lut = lut;
		camera.gameObject.name = base.gameObject.name + "_Camera";
		camera.transform.SetParent(null, true);
		SwitchTextureResolution(false);
		if (externalScreenRenderer != null)
		{
			externalScreenRenderer.material = screenRenderMaterialCopy;
		}
		OpenScreen(false);
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

	public override void Pickup(Player player, Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		base.Pickup(player, target, targetSpacePickupPosition, targetSpacePickupRotation);
		if (hideOwner && player == Player.LocalPlayer)
		{
			camera.cullingMask &= -257;
		}
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		base.Release(player, linearVelocity, angularVelocity);
		if (hideOwner && player == Player.LocalPlayer)
		{
			camera.cullingMask |= 256;
		}
		if (base.hasAuthority)
		{
			FinishCapturing();
			OpenScreen(false);
		}
	}

	public override void OnInputDown()
	{
		if (base.hasAuthority)
		{
			StartCapturing();
		}
	}

	public override void OnInputUp()
	{
		if (base.hasAuthority)
		{
			FinishCapturing();
		}
	}

	protected override void OnLock()
	{
		OpenScreen(true);
	}

	protected override void OnUnlock()
	{
		OpenScreen(false);
		if (base.hasAuthority)
		{
			FinishCapturing();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (videoCapture != null)
		{
			videoCapture.CaptureStartedEvent -= OnCaptureStarted;
			videoCapture.ExportStartedEvent -= OnExportStarted;
			videoCapture.ExportFinishedEvent -= OnExportFinished;
		}
		if (renderTexture != null)
		{
			UnityEngine.Object.Destroy(renderTexture);
		}
		if (camera != null)
		{
			UnityEngine.Object.Destroy(camera.gameObject);
		}
	}

	protected override void OnPhotonPlayerDisconnected(PhotonPlayer oldPlayer)
	{
		if (base.hasAuthority && (base.Owner == null || base.Owner.PhotonPlayer == oldPlayer))
		{
			FinishCapturing();
		}
		base.OnPhotonPlayerDisconnected(oldPlayer);
	}

	private void Update()
	{
		if (!RemoteControl)
		{
			camera.transform.position = Vector3.SmoothDamp(camera.transform.position, lensTransform.position, ref velocity, positionSmoothingTime);
			camera.transform.rotation = UnityExtensions.SmoothDamp(camera.transform.rotation, lensTransform.rotation, ref rotationVelocity, positionSmoothingTime);
		}
		else
		{
			camera.transform.position = lensTransform.position;
			camera.transform.rotation = lensTransform.rotation;
		}
		if (base.hasAuthority)
		{
			SetTrigger(InputAmount);
			UpdateCapture();
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Graphics.Blit(renderTexture, dest);
	}

	public void StartCapturing()
	{
		SwitchTextureResolution(true);
		if (videoCapture != null && !videoCapture.IsCapturing)
		{
			videoCapture.StartCapture();
		}
	}

	public void FinishCapturing()
	{
		if (videoCapture != null && videoCapture.IsCapturing)
		{
			videoCapture.FinishCapture();
		}
		SwitchTextureResolution(false);
	}

	private void UpdateCapture()
	{
		if (!(videoCapture != null) || !videoCapture.IsCapturing)
		{
			return;
		}
		if (!RemoteControl && !base.IsHeld)
		{
			FinishCapturing();
			OpenScreen(false);
			return;
		}
		float num = videoCapture.UpdateFrame(renderTexture);
		recordingProgressFeedback.value = num;
		if (num >= 1f)
		{
			FinishCapturing();
		}
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

	public void OpenScreen(bool open)
	{
		if (animator != null && animator.isActiveAndEnabled)
		{
			animator.SetBool(screenAnimationId, open);
		}
		if (externalScreenRenderer == null)
		{
			camera.enabled = open;
		}
		if (screenOpen != open)
		{
			screenOpen = open;
			Renderer[] array = screenRenderers;
			foreach (Renderer renderer in array)
			{
				renderer.material = ((!open) ? defaultScreenMaterial : screenRenderMaterialCopy);
			}
		}
	}

	private void SetTrigger(float trigger)
	{
		triggerAmount = trigger;
		if (animator != null && animator.isActiveAndEnabled)
		{
			animator.SetFloat(triggerId, triggerAmount);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isReading)
		{
			float trigger = (float)stream.ReceiveNext();
			SetTrigger(trigger);
			bool open = (bool)stream.ReceiveNext();
			OpenScreen(open);
		}
		else
		{
			stream.SendNext(triggerAmount);
			stream.SendNext(screenOpen);
		}
	}
}
