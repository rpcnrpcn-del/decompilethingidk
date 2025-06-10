using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ViveControllerInput : StandaloneInputModule
{
	public static ViveControllerInput Instance;

	[Header(" [Runtime variables]")]
	[Tooltip("Indicates whether or not the gui was hit by any controller this frame")]
	public bool GuiHit;

	[Tooltip("Indicates whether or not a button was used this frame")]
	public bool ButtonUsed;

	[Tooltip("Generated cursors")]
	public Transform[] Cursors;

	private GameObject[] CurrentPoint;

	private GameObject[] CurrentPressed;

	private GameObject[] CurrentDragging;

	private PointerEventData[] PointEvents;

	private bool Initialized;

	[Tooltip("Generated non rendering camera (used for raycasting ui)")]
	public Camera ControllerCamera;

	[SerializeField]
	private ControllerIO[] Controllers;

	private Vector3[] overriddenControllersPosition;

	private Vector3[] overriddenControllersForward;

	private bool[] isControllerCameraOverridden;

	[Tooltip("AG : max Ui interaction distance")]
	public float MaxUIDistance = 25f;

	[Tooltip("AG : max UI interaction angle from head look direction")]
	public float MaxUIHeadAngle = 40f;

	[Header("2D UI settings")]
	[SerializeField]
	private StandaloneInputModule standaloneInputModule;

	private bool hasFocus = true;

	private float lastProcessTime;

	public bool StandAloneInput
	{
		set
		{
			base.enabled = !value;
			standaloneInputModule.enabled = value;
			if (base.enabled)
			{
				standaloneInputModule.forceModuleActive = true;
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}
			else
			{
				ClearSelection();
				Process();
				base.forceModuleActive = true;
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		if (!Initialized)
		{
			Instance = this;
			ControllerCamera = new GameObject("Controller UI Camera").AddComponent<Camera>();
			Object.DontDestroyOnLoad(ControllerCamera);
			ControllerCamera.clearFlags = CameraClearFlags.Nothing;
			ControllerCamera.cullingMask = 0;
			ControllerCamera.stereoTargetEye = StereoTargetEyeMask.None;
			ControllerCamera.nearClipPlane = 0.01f;
			ControllerCamera.farClipPlane = MaxUIDistance;
			Cursors = new Transform[Controllers.Length];
			overriddenControllersPosition = new Vector3[Controllers.Length];
			overriddenControllersForward = new Vector3[Controllers.Length];
			isControllerCameraOverridden = new bool[Controllers.Length];
			for (int i = 0; i < Cursors.Length; i++)
			{
				GameObject gameObject = new GameObject("Cursor " + i);
				Object.DontDestroyOnLoad(gameObject);
				Cursors[i] = gameObject.GetComponent<Transform>();
			}
			CurrentPoint = new GameObject[Cursors.Length];
			CurrentPressed = new GameObject[Cursors.Length];
			CurrentDragging = new GameObject[Cursors.Length];
			PointEvents = new PointerEventData[Cursors.Length];
			SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
			Initialized = true;
		}
	}

	protected void Update()
	{
		if (hasFocus && Time.unscaledTime - lastProcessTime > 10f)
		{
			Debug.LogError("Controller Input did not process!");
			ResetModule();
		}
		if (!hasFocus)
		{
			Process();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		ResetModule();
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		this.hasFocus = hasFocus;
		if (hasFocus)
		{
			ResetModule();
		}
	}

	private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
	{
		ResetModule();
	}

	private void ResetModule()
	{
		base.forceModuleActive = false;
		base.forceModuleActive = true;
		lastProcessTime = Time.unscaledTime;
	}

	private bool GetLookPointerEventData(int index)
	{
		if (PointEvents[index] == null)
		{
			PointEvents[index] = new PointerEventData(base.eventSystem);
		}
		else
		{
			PointEvents[index].Reset();
		}
		PointEvents[index].delta = Vector2.zero;
		PointEvents[index].position = new Vector2(Screen.width / 2, Screen.height / 2);
		PointEvents[index].scrollDelta = Vector2.zero;
		bool flag = false;
		if (Controllers[index].UIInteractionEnabled)
		{
			base.eventSystem.RaycastAll(PointEvents[index], m_RaycastResultCache);
			PointEvents[index].pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache);
			if (PointEvents[index].pointerCurrentRaycast.gameObject != null)
			{
				RectTransform component = PointEvents[index].pointerCurrentRaycast.gameObject.GetComponent<RectTransform>();
				Vector3 worldPoint;
				if (RectTransformUtility.ScreenPointToWorldPointInRectangle(component, PointEvents[index].position, PointEvents[index].enterEventCamera, out worldPoint))
				{
					Vector3 vector = worldPoint - SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.position;
					Vector3 forward = SingletonMonoBehaviour<CameraRig>.Instance.CameraIO.transform.forward;
					float distance = PointEvents[index].pointerCurrentRaycast.distance;
					if (distance < MaxUIDistance && Vector3.Angle(vector, forward) < MaxUIHeadAngle && !Physics.Raycast(Controllers[index].transform.position, Controllers[index].transform.forward, distance - 0.1f, 226900992))
					{
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			GuiHit = true;
		}
		else
		{
			PointEvents[index].pointerCurrentRaycast = default(RaycastResult);
		}
		Controllers[index].UISelectable = false;
		if (PointEvents[index].pointerCurrentRaycast.isValid)
		{
			foreach (GameObject item in PointEvents[index].hovered)
			{
				if (!(item != null))
				{
					continue;
				}
				Selectable component2 = item.gameObject.GetComponent<Selectable>();
				if (component2 != null && component2.IsInteractable())
				{
					Controllers[index].UISelectable = true;
					Tooltip component3 = item.gameObject.GetComponent<Tooltip>();
					if (component3 != null)
					{
						component3.SetHoverPoint(Cursors[index]);
					}
				}
			}
		}
		m_RaycastResultCache.Clear();
		return true;
	}

	private void UpdateCursor(int index, PointerEventData pointData)
	{
		if (PointEvents[index].pointerCurrentRaycast.gameObject != null)
		{
			Cursors[index].gameObject.SetActive(true);
			if (pointData.pointerEnter != null)
			{
				RectTransform component = pointData.pointerEnter.GetComponent<RectTransform>();
				Vector3 worldPoint;
				if (RectTransformUtility.ScreenPointToWorldPointInRectangle(component, pointData.position, pointData.enterEventCamera, out worldPoint))
				{
					Cursors[index].position = worldPoint;
					Cursors[index].rotation = component.rotation;
					Controllers[index].UIRaycastPosition = worldPoint;
				}
			}
		}
		else
		{
			Cursors[index].gameObject.SetActive(false);
			Controllers[index].UIRaycastPosition = null;
		}
	}

	public new void ClearSelection()
	{
		if ((bool)base.eventSystem.currentSelectedGameObject)
		{
			base.eventSystem.SetSelectedGameObject(null);
		}
	}

	private void Select(GameObject go)
	{
		ClearSelection();
		if ((bool)ExecuteEvents.GetEventHandler<ISelectHandler>(go))
		{
			base.eventSystem.SetSelectedGameObject(go);
		}
	}

	private new bool SendUpdateEventToSelectedObject()
	{
		if (base.eventSystem.currentSelectedGameObject == null)
		{
			return false;
		}
		BaseEventData baseEventData = GetBaseEventData();
		ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
		return baseEventData.used;
	}

	public void OverrideControllerCameraPositions(ControllerIO controllerIO, Vector3 position, Vector3 forward)
	{
		for (int i = 0; i < Controllers.Length; i++)
		{
			if (Controllers[i] == controllerIO)
			{
				overriddenControllersPosition[i] = position;
				overriddenControllersForward[i] = forward;
				isControllerCameraOverridden[i] = true;
				break;
			}
		}
	}

	private void UpdateCameraPosition(int index)
	{
		if (isControllerCameraOverridden[index])
		{
			ControllerCamera.transform.position = overriddenControllersPosition[index];
			ControllerCamera.transform.forward = overriddenControllersForward[index];
		}
		else
		{
			ControllerCamera.transform.position = Controllers[index].transform.position;
			ControllerCamera.transform.forward = Controllers[index].transform.forward;
			Debug.DrawRay(ControllerCamera.transform.position, ControllerCamera.transform.forward, Color.cyan);
		}
		isControllerCameraOverridden[index] = false;
	}

	public override void Process()
	{
		lastProcessTime = Time.unscaledTime;
		GuiHit = false;
		ButtonUsed = false;
		SendUpdateEventToSelectedObject();
		for (int i = 0; i < Cursors.Length; i++)
		{
			if (!Controllers[i].gameObject.activeInHierarchy)
			{
				if (Cursors[i].gameObject.activeInHierarchy)
				{
					Cursors[i].gameObject.SetActive(false);
				}
				continue;
			}
			UpdateCameraPosition(i);
			if (!GetLookPointerEventData(i))
			{
				continue;
			}
			CurrentPoint[i] = PointEvents[i].pointerCurrentRaycast.gameObject;
			HandlePointerExitAndEnter(PointEvents[i], CurrentPoint[i]);
			UpdateCursor(i, PointEvents[i]);
			if (!(Controllers[i] != null))
			{
				continue;
			}
			if (ButtonDown(i))
			{
				ClearSelection();
				PointEvents[i].pressPosition = PointEvents[i].position;
				PointEvents[i].pointerPressRaycast = PointEvents[i].pointerCurrentRaycast;
				PointEvents[i].pointerPress = null;
				if (CurrentPoint[i] != null)
				{
					CurrentPressed[i] = CurrentPoint[i];
					GameObject gameObject = ExecuteEvents.ExecuteHierarchy(CurrentPressed[i], PointEvents[i], ExecuteEvents.pointerDownHandler);
					if (gameObject == null)
					{
						gameObject = ExecuteEvents.ExecuteHierarchy(CurrentPressed[i], PointEvents[i], ExecuteEvents.pointerClickHandler);
						if (gameObject != null)
						{
							CurrentPressed[i] = gameObject;
						}
					}
					else
					{
						CurrentPressed[i] = gameObject;
						ExecuteEvents.Execute(gameObject, PointEvents[i], ExecuteEvents.pointerClickHandler);
					}
					if (gameObject != null)
					{
						PointEvents[i].pointerPress = gameObject;
						CurrentPressed[i] = gameObject;
						ButtonUsed = true;
					}
					ExecuteEvents.Execute(CurrentPressed[i], PointEvents[i], ExecuteEvents.beginDragHandler);
					PointEvents[i].pointerDrag = CurrentPressed[i];
					CurrentDragging[i] = CurrentPressed[i];
				}
			}
			if (ButtonUp(i))
			{
				if ((bool)CurrentDragging[i])
				{
					ExecuteEvents.Execute(CurrentDragging[i], PointEvents[i], ExecuteEvents.endDragHandler);
					if (CurrentPoint[i] != null)
					{
						ExecuteEvents.ExecuteHierarchy(CurrentPoint[i], PointEvents[i], ExecuteEvents.dropHandler);
					}
					PointEvents[i].pointerDrag = null;
					CurrentDragging[i] = null;
				}
				if ((bool)CurrentPressed[i])
				{
					ExecuteEvents.Execute(CurrentPressed[i], PointEvents[i], ExecuteEvents.pointerUpHandler);
					PointEvents[i].rawPointerPress = null;
					PointEvents[i].pointerPress = null;
					CurrentPressed[i] = null;
				}
			}
			if (CurrentDragging[i] != null)
			{
				ExecuteEvents.Execute(CurrentDragging[i], PointEvents[i], ExecuteEvents.dragHandler);
			}
		}
	}

	private bool ButtonDown(int index)
	{
		return Controllers[index].HairTriggerButtonDown;
	}

	private bool ButtonUp(int index)
	{
		return Controllers[index].HairTriggerButtonUp;
	}
}
