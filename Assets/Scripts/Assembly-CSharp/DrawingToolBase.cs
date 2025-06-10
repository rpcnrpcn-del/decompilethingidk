using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class DrawingToolBase : Tool
{
	[Header("Brush Properties")]
	[SerializeField]
	private DrawingSurface.DrawingSurfaceType _drawingSurfaceType;

	[SerializeField]
	private Transform _brushTip;

	[SerializeField]
	protected float maxDrawingOffset = 0.1f;

	[SerializeField]
	protected float minControlPointDistance = 0.001f;

	[SerializeField]
	private AnimationCurve vibrationCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	[SerializeField]
	private float minVelocityForVibrate = 0.001f;

	[SerializeField]
	private Material _material;

	private Vector2 lastPosition = Vector2.zero;

	private Vector2 planePos = Vector2.zero;

	private Vector2 normalizedPlanePos = Vector2.zero;

	protected float distanceToPlane = float.MaxValue;

	private TimestampedRollingBufferVector3 velocityBuffer = new TimestampedRollingBufferVector3();

	private bool _isDrawing;

	private GameObject drawingObject;

	private MeshFilter drawingMeshFilter;

	protected Mesh drawingMesh;

	private float timeSinceLastPulse = float.MaxValue;

	private float pulseInterval;

	[SerializeField]
	private float soundVelocityThreshold;

	[SerializeField]
	private RecRoomAudioClip brushDownClip;

	[SerializeField]
	private RecRoomAudioClip brushLoopClip;

	private SFXAudioSource loopingSource;

	public DrawingSurface CurrentSurface { get; set; }

	public DrawingSurface.DrawingSurfaceType DrawingSurfaceType
	{
		get
		{
			return _drawingSurfaceType;
		}
	}

	public Transform BrushTip
	{
		get
		{
			return _brushTip;
		}
	}

	public Material Material
	{
		get
		{
			return _material;
		}
	}

	public Vector2 Velocity { get; private set; }

	public Vector2 OnPlanePos
	{
		get
		{
			return planePos;
		}
	}

	public Vector2 NormalizedPlanePos
	{
		get
		{
			return normalizedPlanePos;
		}
	}

	public bool IsDrawing
	{
		get
		{
			return _isDrawing;
		}
		set
		{
			if (_isDrawing != value)
			{
				_isDrawing = value;
				if (_isDrawing)
				{
					PenDown();
				}
				else
				{
					PenUp();
				}
			}
		}
	}

	public MeshRenderer DrawingRenderer { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		drawingMesh = new Mesh();
		drawingObject = new GameObject(base.gameObject.name + "_Drawing");
		drawingMeshFilter = drawingObject.AddComponent<MeshFilter>();
		drawingMeshFilter.mesh = drawingMesh;
		DrawingRenderer = drawingObject.AddComponent<MeshRenderer>();
		DrawingRenderer.shadowCastingMode = ShadowCastingMode.Off;
		DrawingRenderer.receiveShadows = false;
		DrawingRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
		DrawingRenderer.materials = new Material[0];
		DrawingRenderer.lightProbeUsage = LightProbeUsage.Off;
		DrawingRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		drawingObject.transform.SetParent(base.transform);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red.WithAlpha(0.5f);
		Gizmos.DrawRay(base.transform.position, base.transform.up * 0.25f);
		Gizmos.color = Color.blue.WithAlpha(0.5f);
		Gizmos.DrawRay(base.transform.position, base.transform.forward * 0.25f);
	}

	protected virtual void Update()
	{
		bool isDrawing = false;
		if (CurrentSurface != null && CurrentSurface.IsOverPlane(BrushTip.transform.position, out planePos, out distanceToPlane))
		{
			normalizedPlanePos = CurrentSurface.NormalizedBoardSpace(planePos);
			if (distanceToPlane < maxDrawingOffset)
			{
				Vector2 vector = planePos - lastPosition;
				velocityBuffer.Add(Time.time, vector);
				Vector3 value = vector;
				if (velocityBuffer.TryGetAverageValueOverTime(Time.time - 0.1f, Time.time, out value))
				{
					Velocity = value;
				}
				lastPosition = planePos;
				isDrawing = true;
				AuthorityAddControlPoint(CurrentSurface);
			}
		}
		IsDrawing = isDrawing;
		VibrationUpdate();
		SoundUpdate();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Object.Destroy(drawingObject);
	}

	protected abstract void AuthorityAddControlPoint(DrawingSurface currentSurface);

	protected abstract void AuthorityFinishStroke();

	private void VibrationUpdate()
	{
		if (!(base.Owner != null) || !base.Owner.isLocal || !IsDrawing)
		{
			return;
		}
		pulseInterval = ((!(Velocity.magnitude > minVelocityForVibrate)) ? float.MaxValue : vibrationCurve.Evaluate(Velocity.magnitude));
		if (timeSinceLastPulse >= pulseInterval)
		{
			if (base.HolderHand != null && base.HolderHand.ControllerIO != null)
			{
				base.HolderHand.ControllerIO.Vibrate(1, 500);
			}
			timeSinceLastPulse = 0f;
		}
		timeSinceLastPulse += Time.deltaTime;
	}

	public void PenDown()
	{
		if (brushDownClip != null)
		{
			AudioManager.Play3DSFX(brushDownClip, base.transform);
		}
		if (base.Owner.isLocal && base.HolderHand != null && base.HolderHand.ControllerIO != null)
		{
			base.HolderHand.ControllerIO.Vibrate(1, 1000);
		}
	}

	public void PenUp()
	{
		if (base.Owner.isLocal && base.HolderHand != null && base.HolderHand.ControllerIO != null)
		{
			base.HolderHand.ControllerIO.Vibrate(1, 1000);
		}
	}

	protected virtual void SoundUpdate()
	{
		if (brushLoopClip == null || !(brushLoopClip.audioClip != null))
		{
			return;
		}
		if (IsDrawing && Velocity.magnitude > soundVelocityThreshold)
		{
			if (loopingSource == null)
			{
				loopingSource = AudioManager.Play3DSFX(brushLoopClip, base.transform);
				loopingSource.Loop();
			}
		}
		else if (loopingSource != null)
		{
			loopingSource.Stop();
			loopingSource = null;
		}
	}

	public override void Release(Player player, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		base.Release(player, linearVelocity, angularVelocity);
		PenUp();
	}
}
public abstract class DrawingToolBase<ControlPointType> : DrawingToolBase where ControlPointType : class, IDrawingControlPoint
{
	public class Stroke
	{
		public readonly int SurfaceId;

		public readonly List<ControlPointType> controlPoints = new List<ControlPointType>();

		public Stroke(int surfaceId, ControlPointType startPoint)
		{
			SurfaceId = surfaceId;
			controlPoints.Add(startPoint);
		}
	}

	protected Stroke currentStroke;

	protected ControlPointType lastControlPoint;

	protected List<Vector3> vertices = new List<Vector3>();

	protected List<Vector2> uvs = new List<Vector2>();

	protected List<int> tris = new List<int>();

	private bool meshDirty;

	protected override void Update()
	{
		base.Update();
		if (!base.IsDrawing && currentStroke != null)
		{
			AuthorityFinishStroke();
		}
		if (meshDirty)
		{
			CleanAndUpdateMesh();
		}
	}

	protected abstract Stroke CreateNewStroke(DrawingSurface drawingSurface, ControlPointType firstControlPoint);

	protected abstract ControlPointType CreateNewControlPoint();

	public void StartStroke(int surfaceId, ControlPointType firstPoint)
	{
		DrawingSurface component = PhotonView.Find(surfaceId).GetComponent<DrawingSurface>();
		StartStroke(component, firstPoint);
	}

	private void StartStroke(DrawingSurface surface, ControlPointType firstPoint)
	{
		base.CurrentSurface = surface;
		base.CurrentSurface.MarkModified();
		currentStroke = CreateNewStroke(base.CurrentSurface, firstPoint);
		MarkDirty();
	}

	public void FinishStroke()
	{
		ClearMesh();
		currentStroke = null;
		lastControlPoint = (ControlPointType)null;
	}

	private void MarkDirty()
	{
		meshDirty = true;
		if (base.CurrentSurface != null)
		{
			base.CurrentSurface.MarkModified();
		}
	}

	public void AddControlPoint(DrawingSurface surface, ControlPointType newCP)
	{
		if (currentStroke == null)
		{
			StartStroke(surface, newCP);
		}
		else
		{
			currentStroke.controlPoints.Add(newCP);
			lastControlPoint = newCP;
		}
		MarkDirty();
	}

	private void CleanAndUpdateMesh()
	{
		ClearMesh();
		if (!UpdateMesh())
		{
		}
	}

	protected abstract bool UpdateMesh();

	private void ClearMesh()
	{
		vertices.Clear();
		uvs.Clear();
		tris.Clear();
		if (drawingMesh != null)
		{
			drawingMesh.Clear();
		}
		meshDirty = false;
	}

	protected override void AuthorityAddControlPoint(DrawingSurface surface)
	{
		if (base.hasAuthority && (lastControlPoint == null || Vector2.Distance(base.NormalizedPlanePos, lastControlPoint.Position) > minControlPointDistance))
		{
			ControlPointType val = CreateNewControlPoint();
			base.photonView.RPC("RpcAddControlPoint", PhotonTargets.All, surface.photonView.viewID, val);
		}
	}

	protected override void AuthorityFinishStroke()
	{
		if (base.hasAuthority)
		{
			base.photonView.RPC("RpcFinishStroke", PhotonTargets.All);
		}
	}

	[PunRPC]
	public void RpcAddControlPoint(int surfaceId, ControlPointType controlPoint)
	{
		DrawingSurface component = PhotonView.Find(surfaceId).GetComponent<DrawingSurface>();
		AddControlPoint(component, controlPoint);
	}

	[PunRPC]
	public void RpcFinishStroke()
	{
		FinishStroke();
	}
}
