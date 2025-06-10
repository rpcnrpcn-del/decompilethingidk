using System;
using UnityEngine;
using UnityStandardAssets.CinematicEffects;

public class CameraRig : SingletonMonoBehaviour<CameraRig>
{
	[SerializeField]
	private Camera cameraIO;

	[SerializeField]
	private ControllerIO leftHandControllerIO;

	[SerializeField]
	private ControllerIO rightHandControllerIO;

	[SerializeField]
	private HeadGestureDetect headGestureDetect;

	[Header("Post Process Effects")]
	[SerializeField]
	private TonemappingColorGrading tonemapping;

	[Header("Standing Scale Fade")]
	[SerializeField]
	private float standingScaleStartAngle = 60f;

	[SerializeField]
	private float standingScaleEndAngle = 90f;

	[NonSerialized]
	public bool LinearMaskEnabled;

	private Texture defaultTonemappingLUT;

	private PostEffectsMaterial postEffectQuad;

	private WorldSpaceCameraBlinders blinders;

	private FadeImageEffect fadeImageEffect;

	public Camera CameraIO
	{
		get
		{
			return cameraIO;
		}
	}

	public ControllerIO LeftHandControllerIO
	{
		get
		{
			return leftHandControllerIO;
		}
	}

	public ControllerIO RightHandControllerIO
	{
		get
		{
			return rightHandControllerIO;
		}
	}

	public HeadGestureDetect HeadGestureDetect
	{
		get
		{
			return headGestureDetect;
		}
	}

	public TonemappingColorGrading Tonemapping
	{
		get
		{
			return tonemapping;
		}
	}

	public VignetteImageEffect VignetteImageEffect { get; private set; }

	public MonochromeImageEffect MonochromeImageEffect { get; private set; }

	public bool Blind
	{
		get
		{
			return fadeImageEffect.Blind;
		}
		set
		{
			fadeImageEffect.Blind = value;
		}
	}

	public Vector3 CameraFloorOffsetLocalSpace
	{
		get
		{
			Vector3 vector = CameraIO.transform.position - base.transform.position;
			Vector3 vector2 = Vector3.ProjectOnPlane(vector, Vector3.up);
			return base.transform.InverseTransformVector(vector2);
		}
	}

	private void Awake()
	{
		SingletonMonoBehaviour<CameraRig>.Instance = this;
		postEffectQuad = GetComponentInChildren<PostEffectsMaterial>();
		postEffectQuad.Camera = cameraIO;
		fadeImageEffect = GetComponentInChildren<FadeImageEffect>();
		VignetteImageEffect = GetComponentInChildren<VignetteImageEffect>();
		MonochromeImageEffect = GetComponentInChildren<MonochromeImageEffect>();
		blinders = GetComponentInChildren<WorldSpaceCameraBlinders>();
		blinders.DesiredForwardTransform = base.transform;
	}

	private void Start()
	{
		defaultTonemappingLUT = Tonemapping.lut.texture;
	}

	private void Update()
	{
		blinders.BlindersEnabled = PlatformManager.Instance.CurrentTrackingMode == PlatformManager.TrackingMode.ONE_EIGHTY_DEGREE;
	}

	public void SwapHands()
	{
		ControllerIO controllerIO = leftHandControllerIO;
		leftHandControllerIO = rightHandControllerIO;
		rightHandControllerIO = controllerIO;
		if (Player.LocalPlayer != null)
		{
			Player.LocalPlayer.SetupControllerIOForHands();
		}
	}

	public void SetTonemappingLUT(Texture lutTexture)
	{
		if (lutTexture == null)
		{
			lutTexture = defaultTonemappingLUT;
		}
		TonemappingColorGrading[] array = UnityEngine.Object.FindObjectsOfType<TonemappingColorGrading>();
		foreach (TonemappingColorGrading tonemappingColorGrading in array)
		{
			TonemappingColorGrading.LUTSettings lut = tonemappingColorGrading.lut;
			lut.texture = lutTexture;
			tonemappingColorGrading.lut = lut;
		}
	}
}
