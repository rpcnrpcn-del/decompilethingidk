using System;
using UnityEngine;

public class UIPlayerCamera : MonoBehaviour
{
	public enum FollowMode
	{
		POV = 0,
		Selfie = 1,
		BirdsEye = 2,
		ThirdPerson = 3
	}

	[Header("Camera")]
	[SerializeField]
	private Transform lensTransform;

	public Vector2 Resolution = new Vector2(1280f, 1080f);

	[SerializeField]
	private float fieldOfView = 60f;

	[SerializeField]
	private float nearClip = 0.05f;

	[SerializeField]
	private float farClip = 100f;

	[SerializeField]
	private LayerMask layerMask = -65537;

	[Header("Smoothing")]
	[SerializeField]
	private float positionSmoothingTime = 0.05f;

	[Header("Follow")]
	[SerializeField]
	private FollowMode currentFollowMode = FollowMode.Selfie;

	[SerializeField]
	private Texture emptyTexture;

	private Vector3 velocity = Vector3.zero;

	private Vector3 rotationVelocity = Vector3.zero;

	private float aspect = 1f;

	private GameManager gameManager;

	private Renderer _targetRenderer;

	public Camera Camera { get; private set; }

	public RenderTexture RenderTexture { get; private set; }

	public Player Player { get; private set; }

	public Renderer TargetRenderer
	{
		get
		{
			return _targetRenderer;
		}
		private set
		{
			if (_targetRenderer != value && _targetRenderer != null)
			{
				_targetRenderer.material.mainTexture = emptyTexture;
			}
			_targetRenderer = value;
		}
	}

	public TeleportationPortal PlayerTeleportPortal { get; private set; }

	public Transform UIRoot { get; private set; }

	public Transform FixedPoint { get; set; }

	public bool IsActive
	{
		get
		{
			return TargetRenderer != null && (Player != null || FixedPoint != null);
		}
	}

	protected void Start()
	{
		gameManager = RecRoomSceneManager.Instance.GameManager;
		aspect = Resolution.x / Resolution.y;
		RenderTexture = new RenderTexture((int)Resolution.x, (int)Resolution.y, 24);
		GameObject gameObject = new GameObject();
		gameObject.name = base.gameObject.name + "_FollowCamera";
		gameObject.tag = "Untagged";
		gameObject.transform.SetParent(base.transform.parent, true);
		Camera = gameObject.AddComponent<Camera>();
		Camera.cullingMask = layerMask;
		Camera.stereoTargetEye = StereoTargetEyeMask.None;
		Camera.targetTexture = RenderTexture;
		Camera.aspect = aspect;
		Camera.fieldOfView = fieldOfView;
		Camera.nearClipPlane = nearClip;
		Camera.farClipPlane = farClip;
	}

	protected void OnDestroy()
	{
		if (RenderTexture != null)
		{
			UnityEngine.Object.Destroy(RenderTexture);
		}
		if (Camera != null)
		{
			UnityEngine.Object.Destroy(Camera.gameObject);
		}
	}

	private void OnDisable()
	{
		if (Camera != null)
		{
			Camera.enabled = false;
		}
	}

	private void OnEnable()
	{
		if (Camera != null)
		{
			Camera.enabled = true;
			Camera.transform.position = lensTransform.position;
			Camera.transform.rotation = lensTransform.rotation;
		}
	}

	private void Update()
	{
		bool isActive = IsActive;
		Camera.gameObject.SetActive(isActive);
		if (isActive && Player != null && Player.Head != null)
		{
			Camera.transform.position = Vector3.SmoothDamp(Camera.transform.position, lensTransform.position, ref velocity, positionSmoothingTime);
			Camera.transform.rotation = UnityExtensions.SmoothDamp(Camera.transform.rotation, lensTransform.rotation, ref rotationVelocity, positionSmoothingTime);
			Transform transform = Player.Head.transform;
			TargetRenderer.material.mainTexture = RenderTexture;
			Vector3 vector = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f) * Vector3.forward;
			if (PlayerTeleportPortal != null)
			{
				PlayerTeleportPortal.TargetPositionOverride = Player.CurrentFloorPosition - vector * UnityEngine.Random.Range(1f, 1.5f);
				PlayerTeleportPortal.TargetRotationOverride = Quaternion.LookRotation(transform.position - PlayerTeleportPortal.TargetPositionOverride.Value);
			}
			if (FixedPoint != null)
			{
				base.transform.position = FixedPoint.position;
				base.transform.rotation = FixedPoint.rotation;
			}
			else
			{
				switch (currentFollowMode)
				{
				case FollowMode.POV:
					base.transform.position = transform.position + transform.forward * 0.1f;
					base.transform.rotation = transform.rotation;
					break;
				case FollowMode.Selfie:
					base.transform.position = transform.position + vector + Vector3.up * 0.25f;
					base.transform.rotation = Quaternion.LookRotation(transform.position - base.transform.position);
					break;
				case FollowMode.BirdsEye:
					base.transform.position = transform.position + Vector3.up * 5f;
					base.transform.rotation = Quaternion.LookRotation(transform.position - base.transform.position);
					break;
				case FollowMode.ThirdPerson:
					base.transform.position = transform.position - vector + Vector3.up * 0.3f;
					base.transform.rotation = Quaternion.LookRotation(transform.position - base.transform.position);
					break;
				}
			}
		}
		else if (TargetRenderer != null)
		{
			TargetRenderer.material.mainTexture = emptyTexture;
		}
		if (PlayerTeleportPortal != null)
		{
			PlayerTeleportPortal.gameObject.SetActive(isActive && !Player.isLocal && (gameManager == null || gameManager.CurrentState != GameStates.GAME_RUNNING));
		}
		if (UIRoot != null)
		{
			UIRoot.gameObject.SetActive(Player != null);
		}
	}

	public void Setup(Player targetPlayer, Renderer targetRenderer = null, TeleportationPortal teleportPortal = null, Transform uiRoot = null)
	{
		Player = targetPlayer;
		TargetRenderer = targetRenderer;
		FixedPoint = null;
		if (PlayerTeleportPortal != null && PlayerTeleportPortal.Highlight && teleportPortal == null)
		{
			PlayerTeleportPortal.Highlight = false;
		}
		PlayerTeleportPortal = teleportPortal;
		UIRoot = uiRoot;
	}

	public void CycleFollowMode()
	{
		currentFollowMode = (FollowMode)((int)(currentFollowMode + 1) % Enum.GetNames(typeof(FollowMode)).Length);
	}
}
