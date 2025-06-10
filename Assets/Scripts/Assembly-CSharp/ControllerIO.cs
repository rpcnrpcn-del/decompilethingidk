using System;
using System.Collections;
using UnityEngine;
using UnityEngine.VR;
using Valve.VR;

public class ControllerIO : MonoBehaviour
{
	[Serializable]
	private class DebugInputSettings
	{
		public Transform ControllerToHandspace;

		public int PickupMouseButton;

		public int TriggerMouseButton;

		public KeyCode MenuKey;

		public KeyCode TeleportKey;

		public KeyCode PushToTalkKey;

		public KeyCode RotateLeftKey;

		public KeyCode RotateRightKey;

		public KeyCode RotateDownKey;

		public KeyCode FlyModeModifierButton;
	}

	private class SteamVrInputSettings
	{
		public Transform ControllerToHandspace;

		public EVRButtonId PickupButton;

		public EVRButtonId TriggerButton;

		public EVRButtonId MenuButton;

		public EVRButtonId TeleportButton;

		public EVRButtonId PushToTalkButton;

		public EVRButtonId FlyModeModifierButton;

		public EVRButtonId RotateAxis;

		public float RotateAxisDeadzone;
	}

	[Serializable]
	private class SteamVrViveInputSettings : SteamVrInputSettings
	{
		public AxisSwipeButtons RotateAxisSwipeButtons;
	}

	[Serializable]
	private class SteamVrOculusInputSettings : SteamVrInputSettings
	{
		public AxisPressButtons RotateAxisPressButtons;
	}

	[Serializable]
	private class OvrInputSettings
	{
		public Transform ControllerToHandspace;

		public OVRInput.Axis1D PickupAxis;

		public OVRInput.Button PickupButton;

		public OVRInput.Axis1D TriggerAxis;

		public OVRInput.Button TriggerButton;

		public OVRInput.Button MenuButton;

		public OVRInput.Button TeleportButton;

		public OVRInput.Button PushToTalkButton;

		public OVRInput.Button FlyModeModifierButton;

		public OVRInput.Axis2D RotateAxis;

		public float RotateAxisDeadzone;

		public AxisPressButtons ThumbstickDirectionButtons;
	}

	public enum HandType
	{
		LeftHand = 0,
		RightHand = 1
	}

	public enum InputMode
	{
		Debug = 0,
		SteamVR_Vive = 1,
		SteamVR_Oculus = 2,
		Oculus = 3
	}

	[SerializeField]
	private HandType hand;

	[SerializeField]
	private DebugInputSettings debugInput = new DebugInputSettings();

	[SerializeField]
	private SteamVrViveInputSettings steamVrVive = new SteamVrViveInputSettings
	{
		PickupButton = EVRButtonId.k_EButton_Axis1,
		TriggerButton = EVRButtonId.k_EButton_Axis1,
		MenuButton = EVRButtonId.k_EButton_ApplicationMenu,
		TeleportButton = EVRButtonId.k_EButton_Axis0,
		PushToTalkButton = EVRButtonId.k_EButton_Grip,
		FlyModeModifierButton = EVRButtonId.k_EButton_Grip,
		RotateAxis = EVRButtonId.k_EButton_Axis0,
		RotateAxisDeadzone = 0.2f
	};

	[SerializeField]
	private SteamVrOculusInputSettings steamVrOculus = new SteamVrOculusInputSettings
	{
		PickupButton = EVRButtonId.k_EButton_Axis2,
		TriggerButton = EVRButtonId.k_EButton_Axis1,
		MenuButton = EVRButtonId.k_EButton_ApplicationMenu,
		TeleportButton = EVRButtonId.k_EButton_A,
		PushToTalkButton = EVRButtonId.k_EButton_Axis0,
		FlyModeModifierButton = EVRButtonId.k_EButton_Grip,
		RotateAxis = EVRButtonId.k_EButton_Axis0,
		RotateAxisDeadzone = 0.2f
	};

	[SerializeField]
	private OvrInputSettings oculus = new OvrInputSettings
	{
		PickupAxis = OVRInput.Axis1D.PrimaryHandTrigger,
		PickupButton = OVRInput.Button.PrimaryHandTrigger,
		TriggerAxis = OVRInput.Axis1D.PrimaryIndexTrigger,
		TriggerButton = OVRInput.Button.PrimaryIndexTrigger,
		MenuButton = OVRInput.Button.Two,
		TeleportButton = OVRInput.Button.One,
		PushToTalkButton = OVRInput.Button.PrimaryThumbstick,
		FlyModeModifierButton = OVRInput.Button.PrimaryHandTrigger,
		RotateAxis = OVRInput.Axis2D.PrimaryThumbstick,
		RotateAxisDeadzone = 0.2f
	};

	private const ushort OCULUS_MIN_HAPTIC_DURATION_MICROSECONDS = 3125;

	private OVRHapticsClip _oculusHapticsClip;

	private bool PickupButtonOverride;

	private bool hairPickupButtonDown;

	private bool hairPickupButtonUp;

	private bool hairTriggerButtonDown;

	private bool hairTriggerButtonUp;

	private uint steamVrDeviceIndex = uint.MaxValue;

	private Transform rawControllerTransform;

	private OVRHapticsClip OculusHapticsClip
	{
		get
		{
			if (_oculusHapticsClip == null)
			{
				_oculusHapticsClip = new OVRHapticsClip();
			}
			return _oculusHapticsClip;
		}
	}

	public HandType Hand
	{
		get
		{
			return hand;
		}
	}

	public bool MenuButtonPressed { get; private set; }

	public bool MenuButtonDown { get; private set; }

	public bool MenuButtonUp { get; private set; }

	public bool TeleportButtonPressed { get; private set; }

	public bool TeleportButtonDown { get; private set; }

	public bool TeleportButtonUp { get; private set; }

	public bool PushToTalkButtonPressed { get; private set; }

	public bool PushToTalkButtonDown { get; private set; }

	public bool PushToTalkButtonUp { get; private set; }

	public bool FlyModeModifierButtonPressed { get; private set; }

	public bool FlyModeModifierButtonDown { get; private set; }

	public bool FlyModeModifierButtonUp { get; private set; }

	public Vector2 TeleportRotateAxis { get; private set; }

	public bool TeleportRotateAxisInDeadZone { get; private set; }

	public bool RotateRightButtonPressed { get; private set; }

	public bool RotateRightButtonDown { get; private set; }

	public bool RotateRightButtonUp { get; private set; }

	public bool RotateLeftButtonPressed { get; private set; }

	public bool RotateLeftButtonDown { get; private set; }

	public bool RotateLeftButtonUp { get; private set; }

	public bool RotateDownButtonPressed { get; private set; }

	public bool RotateDownButtonDown { get; private set; }

	public bool RotateDownButtonUp { get; private set; }

	public float PickupAxis { get; private set; }

	public bool PickupButtonPressed { get; private set; }

	public bool PickupButtonDown { get; private set; }

	public bool PickupButtonUp { get; private set; }

	public float TriggerAxis { get; private set; }

	public bool TriggerButtonPressed { get; private set; }

	public bool TriggerButtonDown { get; private set; }

	public bool TriggerButtonUp { get; private set; }

	public bool HairPickupButtonDownConsumed { get; set; }

	public bool HairPickupButtonUpConsumed { get; set; }

	public bool HairPickupButtonPressed { get; private set; }

	public bool HairPickupButtonDown
	{
		get
		{
			return hairPickupButtonDown;
		}
		private set
		{
			hairPickupButtonDown = value;
			if (hairPickupButtonDown)
			{
				HairPickupButtonDownTime = Time.time;
				HairPickupButtonDownConsumed = false;
			}
		}
	}

	public bool HairPickupButtonUp
	{
		get
		{
			return hairPickupButtonUp && !HairPickupButtonUpConsumed;
		}
		private set
		{
			hairPickupButtonUp = value;
			if (hairPickupButtonUp)
			{
				HairPickupButtonUpTime = Time.time;
				HairPickupButtonUpConsumed = false;
			}
		}
	}

	public bool HairTriggerButtonDownConsumed { get; set; }

	public bool HairTriggerButtonUpConsumed { get; set; }

	public bool HairTriggerButtonPressed { get; private set; }

	public bool HairTriggerButtonDown
	{
		get
		{
			return hairTriggerButtonDown;
		}
		private set
		{
			hairTriggerButtonDown = value;
			if (hairTriggerButtonDown)
			{
				HairTriggerButtonDownTime = Time.time;
				HairTriggerButtonDownConsumed = false;
			}
		}
	}

	public bool HairTriggerButtonUp
	{
		get
		{
			return hairTriggerButtonUp && !HairTriggerButtonUpConsumed;
		}
		private set
		{
			hairTriggerButtonUp = value;
			if (hairTriggerButtonUp)
			{
				HairTriggerButtonUpTime = Time.time;
				HairTriggerButtonUpConsumed = false;
			}
		}
	}

	public float HairPickupButtonDownTime { get; private set; }

	public float HairPickupButtonUpTime { get; private set; }

	public float HairTriggerButtonDownTime { get; private set; }

	public float HairTriggerButtonUpTime { get; private set; }

	public bool IsConnected { get; private set; }

	public bool IsTracking { get; private set; }

	public bool ModelVisible { get; set; }

	public Vector3? UIRaycastPosition { get; set; }

	public bool UISelectable { get; set; }

	public bool UIInteractionEnabled { get; set; }

	public static InputMode CurrentInputMode
	{
		get
		{
			if (PlatformManager.Instance != null && PlatformManager.Instance.CurrentPlatform == PlatformManager.PlatformType.OCULUS)
			{
				return InputMode.Oculus;
			}
			if (PlatformManager.Instance != null && PlatformManager.Instance.CurrentHardwareType == PlatformManager.HardwareType.OCULUS)
			{
				return InputMode.SteamVR_Oculus;
			}
			if (Application.isEditor && !VRDevice.isPresent)
			{
				return InputMode.Debug;
			}
			return InputMode.SteamVR_Vive;
		}
	}

	public event Action TransformUpdated;

	private void Start()
	{
		if (PlatformManager.Instance != null && PlatformManager.Instance.CurrentPlatform == PlatformManager.PlatformType.STEAM)
		{
			SteamVR_Utils.Event.Listen("new_poses", OnNewSteamVrPoses);
		}
		ConfigureInputModeSpecificControllerToHandspaceTransform();
	}

	private void OnDestroy()
	{
		if (PlatformManager.Instance != null && PlatformManager.Instance.CurrentPlatform == PlatformManager.PlatformType.STEAM)
		{
			SteamVR_Utils.Event.Remove("new_poses", OnNewSteamVrPoses);
		}
	}

	private void Update()
	{
		switch (CurrentInputMode)
		{
		case InputMode.Debug:
			UpdateInputStateForEditor();
			break;
		case InputMode.SteamVR_Vive:
			UpdateInputStateForSteamVRVive();
			break;
		case InputMode.SteamVR_Oculus:
			UpdateInputStateForSteamVROculus();
			break;
		case InputMode.Oculus:
			UpdateInputStateForOculus();
			break;
		}
	}

	private void ConfigureInputModeSpecificControllerToHandspaceTransform()
	{
		Transform transform = null;
		switch (CurrentInputMode)
		{
		case InputMode.Debug:
			transform = debugInput.ControllerToHandspace;
			break;
		case InputMode.SteamVR_Vive:
			transform = steamVrVive.ControllerToHandspace;
			break;
		case InputMode.SteamVR_Oculus:
			transform = steamVrOculus.ControllerToHandspace;
			break;
		case InputMode.Oculus:
			transform = oculus.ControllerToHandspace;
			break;
		}
		rawControllerTransform = new GameObject(base.gameObject.name + "_raw").transform;
		rawControllerTransform.parent = base.transform.parent;
		base.transform.parent = rawControllerTransform;
		base.transform.localPosition = transform.localPosition;
		base.transform.localRotation = transform.localRotation;
		base.transform.localScale = transform.localScale;
	}

	private void UpdateInputStateForEditor()
	{
		IsTracking = true;
		IsConnected = true;
		rawControllerTransform.position = Camera.main.transform.position;
		rawControllerTransform.rotation = Camera.main.transform.rotation;
		if (this.TransformUpdated != null)
		{
			this.TransformUpdated();
		}
		MenuButtonPressed = Input.GetKey(debugInput.MenuKey);
		MenuButtonDown = Input.GetKeyDown(debugInput.MenuKey);
		MenuButtonUp = Input.GetKeyUp(debugInput.MenuKey);
		TeleportButtonPressed = Input.GetKey(debugInput.TeleportKey);
		TeleportButtonDown = Input.GetKeyDown(debugInput.TeleportKey);
		TeleportButtonUp = Input.GetKeyUp(debugInput.TeleportKey);
		RotateRightButtonPressed = Input.GetKey(debugInput.RotateRightKey);
		RotateRightButtonDown = Input.GetKeyDown(debugInput.RotateRightKey);
		RotateRightButtonUp = Input.GetKeyUp(debugInput.RotateRightKey);
		RotateLeftButtonPressed = Input.GetKey(debugInput.RotateLeftKey);
		RotateLeftButtonDown = Input.GetKeyDown(debugInput.RotateLeftKey);
		RotateLeftButtonUp = Input.GetKeyUp(debugInput.RotateLeftKey);
		RotateDownButtonPressed = Input.GetKey(debugInput.RotateDownKey);
		RotateDownButtonDown = Input.GetKeyDown(debugInput.RotateDownKey);
		RotateDownButtonUp = Input.GetKeyUp(debugInput.RotateDownKey);
		PushToTalkButtonPressed = Input.GetKey(debugInput.PushToTalkKey);
		PushToTalkButtonDown = Input.GetKeyDown(debugInput.PushToTalkKey);
		PushToTalkButtonUp = Input.GetKeyUp(debugInput.PushToTalkKey);
		FlyModeModifierButtonPressed = Input.GetKey(debugInput.FlyModeModifierButton);
		FlyModeModifierButtonDown = Input.GetKeyDown(debugInput.FlyModeModifierButton);
		FlyModeModifierButtonUp = Input.GetKeyUp(debugInput.FlyModeModifierButton);
		if (PickupButtonOverride)
		{
			PickupAxis = 1f;
		}
		else
		{
			PickupAxis = ((!Input.GetMouseButton(debugInput.PickupMouseButton)) ? 0f : 1f);
		}
		PickupButtonPressed = Input.GetMouseButton(debugInput.PickupMouseButton);
		PickupButtonDown = Input.GetMouseButtonDown(debugInput.PickupMouseButton);
		PickupButtonUp = Input.GetMouseButtonUp(debugInput.PickupMouseButton);
		if (Input.GetKeyUp(KeyCode.F))
		{
			PickupButtonOverride = !PickupButtonOverride;
		}
		TriggerAxis = ((!Input.GetMouseButton(debugInput.TriggerMouseButton)) ? 0f : 1f);
		TriggerButtonPressed = Input.GetMouseButton(debugInput.TriggerMouseButton);
		TriggerButtonDown = Input.GetMouseButtonDown(debugInput.TriggerMouseButton);
		TriggerButtonUp = Input.GetMouseButtonUp(debugInput.TriggerMouseButton);
		HairPickupButtonPressed = Input.GetMouseButton(debugInput.PickupMouseButton);
		HairPickupButtonDown = Input.GetMouseButtonDown(debugInput.PickupMouseButton);
		HairPickupButtonUp = Input.GetMouseButtonUp(debugInput.PickupMouseButton);
		HairTriggerButtonPressed = Input.GetMouseButton(debugInput.TriggerMouseButton);
		HairTriggerButtonDown = Input.GetMouseButtonDown(debugInput.TriggerMouseButton);
		HairTriggerButtonUp = Input.GetMouseButtonUp(debugInput.TriggerMouseButton);
	}

	private void UpdateInputStateForSteamVRVive()
	{
		if (IsConnected)
		{
			SteamVR_Controller.Device device = SteamVR_Controller.Input((int)steamVrDeviceIndex);
			MenuButtonPressed = device.GetPress(steamVrVive.MenuButton);
			MenuButtonDown = device.GetPressDown(steamVrVive.MenuButton);
			MenuButtonUp = device.GetPressUp(steamVrVive.MenuButton);
			TeleportButtonPressed = device.GetPress(steamVrVive.TeleportButton);
			TeleportButtonDown = device.GetPressDown(steamVrVive.TeleportButton);
			TeleportButtonUp = device.GetPressUp(steamVrVive.TeleportButton);
			TeleportRotateAxis = device.GetAxis(steamVrVive.RotateAxis);
			bool flag = !device.GetTouchUp(steamVrVive.RotateAxis) && (device.GetTouchDown(steamVrVive.RotateAxis) || device.GetTouch(steamVrVive.RotateAxis));
			bool flag2 = !device.GetPressUp(steamVrVive.RotateAxis) && (device.GetPressDown(steamVrVive.RotateAxis) || device.GetPress(steamVrVive.RotateAxis));
			TeleportRotateAxisInDeadZone = TeleportRotateAxis.sqrMagnitude <= steamVrVive.RotateAxisDeadzone * steamVrVive.RotateAxisDeadzone || !flag2;
			steamVrVive.RotateAxisSwipeButtons.Update(TeleportRotateAxis, !flag, !flag2);
			RotateRightButtonPressed = steamVrVive.RotateAxisSwipeButtons.GetPressed(AxisButtons.Direction.RIGHT);
			RotateRightButtonDown = steamVrVive.RotateAxisSwipeButtons.GetDown(AxisButtons.Direction.RIGHT);
			RotateRightButtonUp = steamVrVive.RotateAxisSwipeButtons.GetUp(AxisButtons.Direction.RIGHT);
			RotateLeftButtonPressed = steamVrVive.RotateAxisSwipeButtons.GetPressed(AxisButtons.Direction.LEFT);
			RotateLeftButtonDown = steamVrVive.RotateAxisSwipeButtons.GetDown(AxisButtons.Direction.LEFT);
			RotateLeftButtonUp = steamVrVive.RotateAxisSwipeButtons.GetUp(AxisButtons.Direction.LEFT);
			RotateDownButtonPressed = steamVrVive.RotateAxisSwipeButtons.GetPressed(AxisButtons.Direction.DOWN);
			RotateDownButtonDown = steamVrVive.RotateAxisSwipeButtons.GetDown(AxisButtons.Direction.DOWN);
			RotateDownButtonUp = steamVrVive.RotateAxisSwipeButtons.GetUp(AxisButtons.Direction.DOWN);
			if (RotateRightButtonDown || RotateDownButtonDown || RotateLeftButtonDown)
			{
				VibrateTouchpad(100, 1000);
			}
			PushToTalkButtonPressed = device.GetPress(steamVrVive.PushToTalkButton);
			PushToTalkButtonDown = device.GetPressDown(steamVrVive.PushToTalkButton);
			PushToTalkButtonUp = device.GetPressUp(steamVrVive.PushToTalkButton);
			FlyModeModifierButtonPressed = device.GetPress(steamVrVive.FlyModeModifierButton);
			FlyModeModifierButtonDown = device.GetPressDown(steamVrVive.FlyModeModifierButton);
			FlyModeModifierButtonUp = device.GetPressUp(steamVrVive.FlyModeModifierButton);
			PickupAxis = device.GetAxis(steamVrVive.PickupButton).x;
			PickupButtonPressed = device.GetPress(steamVrVive.PickupButton);
			PickupButtonDown = device.GetPressDown(steamVrVive.PickupButton);
			PickupButtonUp = device.GetPressUp(steamVrVive.PickupButton);
			TriggerAxis = device.GetAxis(steamVrVive.TriggerButton).x;
			TriggerButtonPressed = device.GetPress(steamVrVive.TriggerButton);
			TriggerButtonDown = device.GetPressDown(steamVrVive.TriggerButton);
			TriggerButtonUp = device.GetPressUp(steamVrVive.TriggerButton);
			HairPickupButtonPressed = device.GetHairTrigger();
			HairPickupButtonDown = device.GetHairTriggerDown();
			HairPickupButtonUp = device.GetHairTriggerUp();
			HairTriggerButtonPressed = device.GetHairTrigger();
			HairTriggerButtonDown = device.GetHairTriggerDown();
			HairTriggerButtonUp = device.GetHairTriggerUp();
		}
	}

	private void UpdateInputStateForSteamVROculus()
	{
		if (IsConnected)
		{
			SteamVR_Controller.Device device = SteamVR_Controller.Input((int)steamVrDeviceIndex);
			MenuButtonPressed = device.GetPress(steamVrOculus.MenuButton);
			MenuButtonDown = device.GetPressDown(steamVrOculus.MenuButton);
			MenuButtonUp = device.GetPressUp(steamVrOculus.MenuButton);
			TeleportButtonPressed = device.GetPress(steamVrOculus.TeleportButton);
			TeleportButtonDown = device.GetPressDown(steamVrOculus.TeleportButton);
			TeleportButtonUp = device.GetPressUp(steamVrOculus.TeleportButton);
			TeleportRotateAxis = device.GetAxis(steamVrOculus.RotateAxis);
			TeleportRotateAxisInDeadZone = TeleportRotateAxis.sqrMagnitude <= steamVrOculus.RotateAxisDeadzone * steamVrOculus.RotateAxisDeadzone;
			steamVrOculus.RotateAxisPressButtons.Update(TeleportRotateAxis, TeleportRotateAxisInDeadZone);
			RotateRightButtonPressed = steamVrOculus.RotateAxisPressButtons.GetPressed(AxisButtons.Direction.RIGHT);
			RotateRightButtonDown = steamVrOculus.RotateAxisPressButtons.GetDown(AxisButtons.Direction.RIGHT);
			RotateRightButtonUp = steamVrOculus.RotateAxisPressButtons.GetUp(AxisButtons.Direction.RIGHT);
			RotateLeftButtonPressed = steamVrOculus.RotateAxisPressButtons.GetPressed(AxisButtons.Direction.LEFT);
			RotateLeftButtonDown = steamVrOculus.RotateAxisPressButtons.GetDown(AxisButtons.Direction.LEFT);
			RotateLeftButtonUp = steamVrOculus.RotateAxisPressButtons.GetUp(AxisButtons.Direction.LEFT);
			RotateDownButtonPressed = steamVrOculus.RotateAxisPressButtons.GetPressed(AxisButtons.Direction.DOWN);
			RotateDownButtonDown = steamVrOculus.RotateAxisPressButtons.GetDown(AxisButtons.Direction.DOWN);
			RotateDownButtonUp = steamVrOculus.RotateAxisPressButtons.GetUp(AxisButtons.Direction.DOWN);
			PushToTalkButtonPressed = device.GetPress(steamVrOculus.PushToTalkButton);
			PushToTalkButtonDown = device.GetPressDown(steamVrOculus.PushToTalkButton);
			PushToTalkButtonUp = device.GetPressUp(steamVrOculus.PushToTalkButton);
			FlyModeModifierButtonPressed = device.GetPress(steamVrOculus.FlyModeModifierButton);
			FlyModeModifierButtonDown = device.GetPressDown(steamVrOculus.FlyModeModifierButton);
			FlyModeModifierButtonUp = device.GetPressUp(steamVrOculus.FlyModeModifierButton);
			PickupAxis = device.GetAxis(steamVrOculus.PickupButton).x;
			PickupButtonPressed = device.GetPress(steamVrOculus.PickupButton);
			PickupButtonDown = device.GetPressDown(steamVrOculus.PickupButton);
			PickupButtonUp = device.GetPressUp(steamVrOculus.PickupButton);
			TriggerAxis = device.GetAxis(steamVrOculus.TriggerButton).x;
			TriggerButtonPressed = device.GetPress(steamVrOculus.TriggerButton);
			TriggerButtonDown = device.GetPressDown(steamVrOculus.TriggerButton);
			TriggerButtonUp = device.GetPressUp(steamVrOculus.TriggerButton);
			HairPickupButtonPressed = PickupButtonPressed;
			HairPickupButtonDown = PickupButtonDown;
			HairPickupButtonUp = PickupButtonUp;
			HairTriggerButtonPressed = TriggerButtonPressed;
			HairTriggerButtonDown = TriggerButtonDown;
			HairTriggerButtonUp = TriggerButtonUp;
		}
	}

	private void UpdateInputStateForOculus()
	{
		OVRInput.Controller controller = ((hand == HandType.LeftHand) ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch);
		OVRPlugin.Node nodeId = ((hand != HandType.LeftHand) ? OVRPlugin.Node.HandRight : OVRPlugin.Node.HandLeft);
		IsConnected = OVRPlugin.GetNodePresent(nodeId);
		if (!IsConnected)
		{
			IsTracking = false;
		}
		else
		{
			IsTracking = OVRInput.GetControllerPositionTracked(controller) && OVRInput.GetControllerOrientationTracked(controller);
			if (IsTracking)
			{
				OVRPose oVRPose = OVRPlugin.GetNodePose(nodeId, false).ToOVRPose();
				rawControllerTransform.localPosition = oVRPose.position;
				rawControllerTransform.localRotation = oVRPose.orientation;
				if (this.TransformUpdated != null)
				{
					this.TransformUpdated();
				}
			}
		}
		if (IsConnected && OVRManager.hasVrFocus)
		{
			MenuButtonPressed = OVRInput.Get(oculus.MenuButton, controller);
			MenuButtonDown = OVRInput.GetDown(oculus.MenuButton, controller);
			MenuButtonUp = OVRInput.GetUp(oculus.MenuButton, controller);
			TeleportButtonPressed = OVRInput.Get(oculus.TeleportButton, controller);
			TeleportButtonDown = OVRInput.GetDown(oculus.TeleportButton, controller);
			TeleportButtonUp = OVRInput.GetUp(oculus.TeleportButton, controller);
			PushToTalkButtonPressed = OVRInput.Get(oculus.PushToTalkButton, controller);
			PushToTalkButtonDown = OVRInput.GetDown(oculus.PushToTalkButton, controller);
			PushToTalkButtonUp = OVRInput.GetUp(oculus.PushToTalkButton, controller);
			FlyModeModifierButtonPressed = OVRInput.Get(oculus.FlyModeModifierButton, controller);
			FlyModeModifierButtonDown = OVRInput.GetDown(oculus.FlyModeModifierButton, controller);
			FlyModeModifierButtonUp = OVRInput.GetUp(oculus.FlyModeModifierButton, controller);
			TeleportRotateAxis = OVRInput.Get(oculus.RotateAxis, controller);
			TeleportRotateAxisInDeadZone = TeleportRotateAxis.sqrMagnitude <= oculus.RotateAxisDeadzone * oculus.RotateAxisDeadzone;
			oculus.ThumbstickDirectionButtons.Update(TeleportRotateAxis, TeleportRotateAxisInDeadZone);
			RotateRightButtonPressed = oculus.ThumbstickDirectionButtons.GetPressed(AxisButtons.Direction.RIGHT);
			RotateRightButtonDown = oculus.ThumbstickDirectionButtons.GetDown(AxisButtons.Direction.RIGHT);
			RotateRightButtonUp = oculus.ThumbstickDirectionButtons.GetUp(AxisButtons.Direction.RIGHT);
			RotateLeftButtonPressed = oculus.ThumbstickDirectionButtons.GetPressed(AxisButtons.Direction.LEFT);
			RotateLeftButtonDown = oculus.ThumbstickDirectionButtons.GetDown(AxisButtons.Direction.LEFT);
			RotateLeftButtonUp = oculus.ThumbstickDirectionButtons.GetUp(AxisButtons.Direction.LEFT);
			RotateDownButtonPressed = oculus.ThumbstickDirectionButtons.GetPressed(AxisButtons.Direction.DOWN);
			RotateDownButtonDown = oculus.ThumbstickDirectionButtons.GetDown(AxisButtons.Direction.DOWN);
			RotateDownButtonUp = oculus.ThumbstickDirectionButtons.GetUp(AxisButtons.Direction.DOWN);
			PickupAxis = OVRInput.Get(oculus.PickupAxis, controller);
			PickupButtonPressed = OVRInput.Get(oculus.PickupButton, controller);
			PickupButtonDown = OVRInput.GetDown(oculus.PickupButton, controller);
			PickupButtonUp = OVRInput.GetUp(oculus.PickupButton, controller);
			TriggerAxis = OVRInput.Get(oculus.TriggerAxis, controller);
			TriggerButtonPressed = OVRInput.Get(oculus.TriggerButton, controller);
			TriggerButtonDown = OVRInput.GetDown(oculus.TriggerButton, controller);
			TriggerButtonUp = OVRInput.GetUp(oculus.TriggerButton, controller);
			HairPickupButtonPressed = PickupButtonPressed;
			HairPickupButtonDown = PickupButtonDown;
			HairPickupButtonUp = PickupButtonUp;
			HairTriggerButtonPressed = TriggerButtonPressed;
			HairTriggerButtonDown = TriggerButtonDown;
			HairTriggerButtonUp = TriggerButtonUp;
		}
	}

	private void OnNewSteamVrPoses(params object[] args)
	{
		TrackedDevicePose_t[] array = (TrackedDevicePose_t[])args[0];
		ETrackedControllerRole unDeviceType = ((hand == HandType.LeftHand) ? ETrackedControllerRole.LeftHand : ETrackedControllerRole.RightHand);
		CVRSystem system = OpenVR.System;
		steamVrDeviceIndex = ((system == null) ? uint.MaxValue : system.GetTrackedDeviceIndexForControllerRole(unDeviceType));
		if (steamVrDeviceIndex >= array.Length)
		{
			IsConnected = false;
			IsTracking = false;
			return;
		}
		IsConnected = array[steamVrDeviceIndex].bDeviceIsConnected;
		IsTracking = array[steamVrDeviceIndex].bPoseIsValid;
		if (IsConnected && IsTracking)
		{
			SteamVR_Utils.RigidTransform rigidTransform = new SteamVR_Utils.RigidTransform(array[steamVrDeviceIndex].mDeviceToAbsoluteTracking);
			rawControllerTransform.localPosition = rigidTransform.pos;
			rawControllerTransform.localRotation = rigidTransform.rot;
			if (this.TransformUpdated != null)
			{
				this.TransformUpdated();
			}
		}
	}

	public Coroutine VibrateContinuous(ushort intensity = 1000)
	{
		return StartCoroutine(VibrateContinuousCoroutine(intensity));
	}

	public Coroutine Vibrate(int milliseconds = 1, ushort intensity = 1000)
	{
		return StartCoroutine(VibrateCoroutine(milliseconds, intensity));
	}

	private Coroutine VibrateTouchpad(int milliseconds = 1, ushort intensity = 1000)
	{
		return StartCoroutine(VibrateCoroutine(milliseconds, intensity, true));
	}

	private IEnumerator VibrateCoroutine(int milliseconds, ushort intensity, bool useTouchpad = false)
	{
		float endTime = Time.unscaledTime + (float)milliseconds / 1000f;
		while (Time.unscaledTime < endTime)
		{
			PulseHaptics(intensity, useTouchpad);
			yield return null;
		}
	}

	private IEnumerator VibrateContinuousCoroutine(ushort intensity, bool useTouchpad = false)
	{
		while (true)
		{
			PulseHaptics(intensity, useTouchpad);
			yield return null;
		}
	}

	private void PulseHaptics(ushort durationMicroseconds, bool useTouchpad)
	{
		switch (CurrentInputMode)
		{
		case InputMode.Debug:
			break;
		case InputMode.SteamVR_Vive:
		case InputMode.SteamVR_Oculus:
			if (IsConnected)
			{
				SteamVR_Controller.Device device = SteamVR_Controller.Input((int)steamVrDeviceIndex);
				if (useTouchpad)
				{
					device.TriggerHapticPulse(durationMicroseconds);
				}
				else
				{
					device.TriggerHapticPulse(durationMicroseconds);
				}
			}
			break;
		case InputMode.Oculus:
			if (IsConnected)
			{
				int num = Mathf.Max(1, durationMicroseconds / 3125);
				if (OculusHapticsClip.Count > num)
				{
					OculusHapticsClip.Reset();
				}
				while (OculusHapticsClip.Count < num)
				{
					OculusHapticsClip.WriteSample(byte.MaxValue);
				}
				OVRHaptics.OVRHapticsChannel oVRHapticsChannel = ((hand != HandType.LeftHand) ? OVRHaptics.RightChannel : OVRHaptics.LeftChannel);
				oVRHapticsChannel.Preempt(OculusHapticsClip);
			}
			break;
		}
	}
}
