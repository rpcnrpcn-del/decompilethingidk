using System;
using System.Collections.Generic;
using UnityEngine;

public class ParabolicPointer : MonoBehaviour
{
	public enum MeshMode
	{
		Valid = 0,
		Invalid = 1,
		Shortened = 2,
		Disabled = 3,
		Invisible = 4,
		FadeOut = 5
	}

	[Serializable]
	public class GraphicMaterials
	{
		public Material DefaultMaterial;

		public Material UIMaterial;
	}

	private const int MAX_PARABOLA_SEGMENTS = 20;

	[Header("Parabola Mesh Properties")]
	[SerializeField]
	private float parabolaSegmentLength = 1f;

	[SerializeField]
	private float meshDistanceBetweenVertices = 0.05f;

	[SerializeField]
	private float meshWidth = 0.04f;

	[SerializeField]
	private Vector2 uvScale = Vector2.one;

	[SerializeField]
	private float shortenedDistance = 0.66f;

	[SerializeField]
	private float reflectionOffset = 0.5f;

	public float PersonalSpaceFromLaser = 1f;

	public GraphicMaterials ValidTargetMaterials;

	public GraphicMaterials InvalidTargetMaterials;

	public GraphicMaterials FadeOutMaterials;

	public GraphicMaterials ShortenedTargetMaterials;

	[SerializeField]
	private FloatValueCurve laserSpeedCurve;

	private Mesh parabolaMesh;

	private List<Vector3> laserScratchSpace;

	private bool isLocal;

	private float teleportSpeed;

	private BezierPath bezierPath = new BezierPath();

	private TeleportationPortal _teleportationPortal;

	private RaycastHit lastHit = default(RaycastHit);

	public Vector3 InitialParabolaTangent { get; private set; }

	public Vector3 FinalParabolaTangent { get; private set; }

	public Vector3 FinalParabolaPoint { get; private set; }

	public bool Reflected { get; private set; }

	public Vector3 ReflectionPoint { get; private set; }

	public Vector3 ReflectionNormal { get; private set; }

	public float CurrentParabolaAngle { get; private set; }

	public MeshMode Mode { get; set; }

	public LaserTeleporter LaserTeleporter { get; set; }

	public bool StraightLaser { get; set; }

	public TeleportationPortal TeleportationPortal
	{
		get
		{
			return _teleportationPortal;
		}
		set
		{
			if (_teleportationPortal != value)
			{
				if (_teleportationPortal != null)
				{
					_teleportationPortal.Highlight = false;
				}
				if (value != null && value.IsValid && LaserTeleporter.Visible)
				{
					value.Highlight = true;
					_teleportationPortal = value;
				}
				else
				{
					_teleportationPortal = null;
				}
			}
		}
	}

	public Player RemotePlayer { get; set; }

	public Tool HitTool { get; set; }

	public Vector3 HitToolPosition { get; set; }

	public Vector3 HitToolNormal { get; set; }

	public GameTeleportRestriction TeleportRestrictions { get; set; }

	public bool HitSomething { get; private set; }

	public bool PointOnTeleportRegion { get; private set; }

	public bool PointOnNavMesh { get; internal set; }

	private Material LaserMaterial
	{
		get
		{
			GraphicMaterials graphicMaterials = null;
			switch (Mode)
			{
			case MeshMode.Valid:
				graphicMaterials = ValidTargetMaterials;
				break;
			case MeshMode.Invalid:
				graphicMaterials = InvalidTargetMaterials;
				break;
			case MeshMode.Shortened:
				graphicMaterials = ShortenedTargetMaterials;
				break;
			case MeshMode.FadeOut:
				graphicMaterials = FadeOutMaterials;
				break;
			default:
				graphicMaterials = InvalidTargetMaterials;
				break;
			}
			return graphicMaterials.DefaultMaterial;
		}
	}

	private void Start()
	{
		laserScratchSpace = new List<Vector3>(20);
		parabolaMesh = new Mesh();
		parabolaMesh.MarkDynamic();
		parabolaMesh.name = "Parabolic Pointer";
		parabolaMesh.vertices = new Vector3[0];
		parabolaMesh.triangles = new int[0];
		if (LaserTeleporter != null && LaserTeleporter.Hand != null)
		{
			isLocal = LaserTeleporter.Hand.isLocal;
		}
		Mode = MeshMode.Disabled;
	}

	private void Update()
	{
		if (Mode != MeshMode.Disabled && Mode != MeshMode.Invisible && LaserTeleporter.Visible)
		{
			bool targetingLocalPlayer = !isLocal && RemotePlayer != null && RemotePlayer.isLocal;
			bezierPath = GenerateBezierPath(targetingLocalPlayer, base.transform.forward);
			float num = laserSpeedCurve.Evaluate(teleportSpeed);
			GenerateMesh(ref parabolaMesh, bezierPath, meshDistanceBetweenVertices, base.transform.forward, Time.time % 1f * num);
			Graphics.DrawMesh(parabolaMesh, Matrix4x4.identity, LaserMaterial, 2);
		}
	}

	public void PerformParabolicRaycasts(float maxFloorDistance, float speed)
	{
		teleportSpeed = speed;
		ClearRaycast();
		laserScratchSpace.Clear();
		HitSomething = false;
		if (StraightLaser)
		{
			HitSomething = CalculateStraightLinePoints(maxFloorDistance);
		}
		else
		{
			Vector3 a = CalculateParabolaAcceleration(maxFloorDistance);
			HitSomething = CalculateParabolaPoints(a, parabolaSegmentLength, float.MaxValue);
		}
		Vector3 reflectionPoint = Vector3.zero;
		Vector3 reflectionPointTangent = Vector3.zero;
		Vector3 vector = Vector3.zero;
		bool flag = false;
		if (HitSomething && !PointOnTeleportRegion)
		{
			vector = base.transform.forward;
			if (HitSomething)
			{
				vector = lastHit.normal;
				if (Mathf.Abs(Vector3.Dot(vector, Vector3.up)) <= 1f - Mathf.Epsilon)
				{
					vector = Vector3.ProjectOnPlane(vector, Vector3.up).normalized;
				}
			}
			flag = CalculateReflection(vector, parabolaSegmentLength, out reflectionPoint, out reflectionPointTangent);
		}
		Vector3 normalized = (laserScratchSpace[1] - laserScratchSpace[0]).normalized;
		Vector3 vector2 = laserScratchSpace[laserScratchSpace.Count - 1];
		Vector3 normalized2 = (laserScratchSpace[laserScratchSpace.Count - 2] - vector2).normalized;
		if (flag && PointOnTeleportRegion)
		{
			SetParabolaPointsWithReflection(normalized, reflectionPointTangent, reflectionPoint, vector, vector2);
		}
		else
		{
			SetParabolaPoints(normalized, normalized2, vector2);
		}
	}

	public void ClearParabolicRaycasts()
	{
		ClearRaycast();
	}

	public void SetParabolaPoints(Vector3 initialTangent, Vector3 finalPointTangent, Vector3 finalPoint)
	{
		InitialParabolaTangent = initialTangent;
		FinalParabolaTangent = finalPointTangent;
		FinalParabolaPoint = finalPoint;
		Reflected = false;
		ReflectionPoint = Vector3.zero;
		ReflectionNormal = Vector3.zero;
	}

	public void SetParabolaPointsWithReflection(Vector3 initialTangent, Vector3 reflectionPointTangent, Vector3 reflectionPoint, Vector3 reflectionNormal, Vector3 finalPoint)
	{
		InitialParabolaTangent = initialTangent;
		FinalParabolaTangent = reflectionPointTangent;
		FinalParabolaPoint = finalPoint;
		Reflected = true;
		ReflectionPoint = reflectionPoint;
		ReflectionNormal = reflectionNormal;
	}

	private bool DoRaycast(Ray ray, float magnitude, out RaycastHit hit)
	{
		bool flag = false;
		hit = default(RaycastHit);
		if (!flag && Physics.Raycast(ray, out hit, magnitude, 19456, QueryTriggerInteraction.Collide))
		{
			flag = true;
			if (hit.collider.gameObject.layer == 14 || hit.collider.gameObject.CompareTag("TeleportRegion"))
			{
				TeleportRestrictions = hit.collider.gameObject.GetComponent<GameTeleportRestriction>();
				TeleportationPortal = hit.collider.gameObject.GetComponent<TeleportationPortal>();
				PointOnTeleportRegion = true;
			}
			else if (hit.collider.gameObject.IsInLayerMask(LayerMasks.AnyPlayerPhysics))
			{
				PlayerCollider component = hit.collider.gameObject.GetComponent<PlayerCollider>();
				if (component != null && component.ThisPlayer != null)
				{
					RemotePlayer = component.ThisPlayer;
				}
			}
			else if (hit.collider.gameObject.IsInLayerMask(LayerMasks.ToolPhysics))
			{
				Tool colliderTool = hit.collider.GetColliderTool();
				if (colliderTool == null || colliderTool.IsHeld)
				{
					flag = false;
				}
				else if (colliderTool.SupportsTeleportHits)
				{
					HitTool = colliderTool;
					HitToolPosition = hit.point;
					HitToolNormal = hit.normal;
				}
			}
		}
		return flag;
	}

	private void ClearRaycast()
	{
		TeleportationPortal = null;
		TeleportRestrictions = null;
		RemotePlayer = null;
		HitTool = null;
		HitToolPosition = Vector3.zero;
		HitToolNormal = Vector3.zero;
		PointOnTeleportRegion = false;
	}

	private static float ParabolicCurve(float p0, float v0, float a, float t)
	{
		return p0 + v0 * t + 0.5f * a * t * t;
	}

	private static float ParabolicCurveDeriv(float v0, float a, float t)
	{
		return v0 + a * t;
	}

	private static Vector3 ParabolicCurve(Vector3 p0, Vector3 v0, Vector3 a, float t)
	{
		Vector3 result = default(Vector3);
		for (int i = 0; i < 3; i++)
		{
			result[i] = ParabolicCurve(p0[i], v0[i], a[i], t);
		}
		return result;
	}

	private static Vector3 ParabolicCurveDeriv(Vector3 v0, Vector3 a, float t)
	{
		Vector3 result = default(Vector3);
		for (int i = 0; i < 3; i++)
		{
			result[i] = ParabolicCurveDeriv(v0[i], a[i], t);
		}
		return result;
	}

	private bool CalculateParabolaPoints(Vector3 a, float segmentLength, float maxHorizontalDistance)
	{
		bool result = false;
		Vector3 forward = base.transform.forward;
		Vector3 position = base.transform.position;
		laserScratchSpace.Add(position);
		Vector3 vector = position;
		float num = 0f;
		Ray ray = new Ray(position, base.transform.forward);
		Vector3 vector2 = position;
		RaycastHit hit = default(RaycastHit);
		float num2 = 0f;
		for (int i = 0; i < 20; i++)
		{
			num += segmentLength / ParabolicCurveDeriv(forward, a, num).magnitude;
			vector2 = ParabolicCurve(position, forward, a, num);
			Vector3 vector3 = vector2 - vector;
			float magnitude = Vector3.Project(vector3, base.transform.forward).magnitude;
			num2 += magnitude;
			if (num2 >= maxHorizontalDistance)
			{
				break;
			}
			ray = new Ray(vector, vector3.normalized);
			if (DoRaycast(ray, vector3.magnitude, out hit))
			{
				result = true;
				lastHit = hit;
				laserScratchSpace.Add(hit.point);
				break;
			}
			laserScratchSpace.Add(vector2);
			vector = vector2;
		}
		return result;
	}

	private bool CalculateStraightLinePoints(float maxDistance)
	{
		bool result = false;
		Vector3 forward = base.transform.forward;
		Vector3 position = base.transform.position;
		laserScratchSpace.Add(position);
		Ray ray = new Ray(position, forward);
		RaycastHit hit;
		if (DoRaycast(ray, maxDistance, out hit))
		{
			result = true;
			lastHit = hit;
			laserScratchSpace.Add(hit.point);
		}
		else
		{
			laserScratchSpace.Add(position + forward * maxDistance);
		}
		return result;
	}

	private bool CalculateReflection(Vector3 reflectionPointNormal, float segmentLength, out Vector3 reflectionPoint, out Vector3 reflectionPointTangent)
	{
		reflectionPoint = laserScratchSpace[laserScratchSpace.Count - 1];
		reflectionPointTangent = (laserScratchSpace[laserScratchSpace.Count - 2] - reflectionPoint).normalized;
		Vector3 vector = reflectionPoint + reflectionPointNormal * reflectionOffset;
		bool result = false;
		bool flag = false;
		while (laserScratchSpace.Count < 20)
		{
			Vector3 vector2 = vector - Vector3.up * segmentLength;
			Vector3 vector3 = vector2 - vector;
			Ray ray = new Ray(vector, vector3.normalized);
			RaycastHit hit;
			if (DoRaycast(ray, vector3.magnitude, out hit))
			{
				result = true;
				if (!flag)
				{
					flag = true;
					laserScratchSpace.Add(vector);
				}
				laserScratchSpace.Add(hit.point);
				break;
			}
			laserScratchSpace.Add(vector2);
			vector = vector2;
		}
		return result;
	}

	private BezierPath GenerateBezierPath(bool targetingLocalPlayer, Vector3 defaultTangent)
	{
		bezierPath.Clear();
		if (Mode == MeshMode.Shortened)
		{
			Vector3 vector = defaultTangent * shortenedDistance;
			Vector3 end = base.transform.position + vector;
			Vector3 vector2 = base.transform.position + 0.5f * vector;
			bezierPath.AddCurve(base.transform.position, vector2, vector2, end);
		}
		else if (targetingLocalPlayer)
		{
			Vector3 finalPoint = ((!Reflected) ? FinalParabolaPoint : ReflectionPoint);
			finalPoint += FinalParabolaTangent * PersonalSpaceFromLaser;
			AddBezierPath(base.transform.position, InitialParabolaTangent, finalPoint, FinalParabolaTangent);
		}
		else if (HitTool != null)
		{
			AddBezierPath(base.transform.position, InitialParabolaTangent, ReflectionPoint, FinalParabolaTangent);
		}
		else if (Reflected)
		{
			AddBezierPath(base.transform.position, InitialParabolaTangent, ReflectionPoint, FinalParabolaTangent);
			if (!StraightLaser)
			{
				Vector3 normalized = (FinalParabolaPoint - ReflectionPoint + 0.25f * ReflectionNormal).normalized;
				Vector3 initialControlPoint;
				Vector3 finalControlPoint;
				CalculateParabolaControlPoints(ReflectionPoint, normalized, FinalParabolaPoint, Vector3.up, out initialControlPoint, out finalControlPoint);
				bezierPath.AddCurve(initialControlPoint, finalControlPoint, FinalParabolaPoint);
			}
		}
		else
		{
			AddBezierPath(base.transform.position, InitialParabolaTangent, FinalParabolaPoint, FinalParabolaTangent);
		}
		return bezierPath;
	}

	private void AddBezierPath(Vector3 initialPoint, Vector3 initialTangent, Vector3 finalPoint, Vector3 finalTangent)
	{
		Vector3 initialControlPoint;
		Vector3 finalControlPoint;
		CalculateParabolaControlPoints(initialPoint, InitialParabolaTangent, finalPoint, FinalParabolaTangent, out initialControlPoint, out finalControlPoint);
		bezierPath.AddCurve(initialPoint, initialControlPoint, finalControlPoint, finalPoint);
	}

	private void GenerateMesh(ref Mesh mesh, BezierPath path, float sampleLength, Vector3 forward, float uvoffset)
	{
		float pathLength = path.PathLength;
		int num = Mathf.RoundToInt(pathLength / sampleLength);
		mesh.Clear();
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		if (path.CurveCount <= 0 || num <= 0)
		{
			return;
		}
		Vector3[] array = new Vector3[num * 2];
		Vector2[] array2 = new Vector2[num * 2];
		Vector2[] array3 = new Vector2[num * 2];
		float num2 = meshWidth / 2f;
		Vector3 normalized = Vector3.Cross(forward, Vector3.up).normalized;
		for (int i = 0; i < num; i++)
		{
			Vector3 vector3 = path.EvaluatePath((float)i / ((float)num - 1f));
			array[2 * i] = vector3 - normalized * num2;
			array[2 * i + 1] = vector3 + normalized * num2;
			float num3 = uvoffset;
			if (i == num - 1 && i > 1)
			{
				float magnitude = (vector2 - vector).magnitude;
				float magnitude2 = (vector3 - vector).magnitude;
				num3 += 1f - magnitude2 / magnitude;
			}
			array2[2 * i] = Vector2.Scale(new Vector2(0f, i), uvScale) - new Vector2(0f, num3);
			array2[2 * i + 1] = Vector2.Scale(new Vector2(1f, i), uvScale) - new Vector2(0f, num3);
			float y = Mathf.InverseLerp(0f, num - 1, i);
			array3[2 * i] = new Vector2(0f, y);
			array3[2 * i + 1] = new Vector2(1f, y);
			vector2 = vector;
			vector = vector3;
		}
		int[] array4 = new int[6 * (array.Length - 2)];
		for (int j = 0; j < array.Length / 2 - 1; j++)
		{
			int num4 = 2 * j;
			int num5 = 2 * j + 1;
			int num6 = 2 * j + 2;
			int num7 = 2 * j + 3;
			array4[12 * j] = num4;
			array4[12 * j + 1] = num5;
			array4[12 * j + 2] = num6;
			array4[12 * j + 3] = num6;
			array4[12 * j + 4] = num5;
			array4[12 * j + 5] = num7;
			array4[12 * j + 6] = num6;
			array4[12 * j + 7] = num5;
			array4[12 * j + 8] = num4;
			array4[12 * j + 9] = num7;
			array4[12 * j + 10] = num5;
			array4[12 * j + 11] = num6;
		}
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.uv2 = array3;
		mesh.triangles = array4;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
	}

	private void CalculateParabolaControlPoints(Vector3 initialPoint, Vector3 initialTangent, Vector3 finalPoint, Vector3 finalTangent, out Vector3 initialControlPoint, out Vector3 finalControlPoint)
	{
		float num = ((!(Vector3.Dot(initialTangent, finalTangent) >= 0.99f)) ? ((finalPoint - initialPoint).magnitude / (initialTangent - finalTangent).magnitude) : 0f);
		Vector3 vector = initialPoint + initialTangent * num;
		initialControlPoint = 0.33f * initialPoint + 0.67f * vector;
		finalControlPoint = 0.33f * finalPoint + 0.67f * vector;
	}

	private Vector3 CalculateParabolaAcceleration(float maxFloorDistance)
	{
		Vector3 normalized = Vector3.ProjectOnPlane(base.transform.forward, Vector3.up).normalized;
		Vector3 normalized2 = Vector3.Cross(normalized, Vector3.up).normalized;
		Vector3 normalized3 = (Quaternion.AngleAxis(45f, normalized2) * normalized).normalized;
		float num = Vector3.Dot(normalized3, normalized);
		float num2 = maxFloorDistance / num;
		Vector3 vector = Vector3.Project(normalized3, Vector3.up);
		return -2f * vector / num2;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		if (lastHit.collider != null)
		{
			Gizmos.DrawWireSphere(lastHit.point + lastHit.normal, 0.1f);
			Gizmos.DrawLine(lastHit.point, lastHit.point + lastHit.normal);
		}
		if (laserScratchSpace != null)
		{
			for (int i = 1; i < laserScratchSpace.Count; i++)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawSphere(laserScratchSpace[i], 0.02f);
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(laserScratchSpace[i - 1], laserScratchSpace[i]);
			}
		}
	}
}
