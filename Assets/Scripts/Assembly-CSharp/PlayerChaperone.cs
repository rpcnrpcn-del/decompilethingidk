using System;
using System.Collections;
using UnityEngine;
using Valve.VR;

public class PlayerChaperone : MonoBehaviour
{
	public enum ChaperoneMode
	{
		SteamVR = 0,
		Oculus = 1
	}

	private delegate void CornerAssignmentCallback(Vector3[] corners);

	[SerializeField]
	private float lineThickness = 0.1f;

	[SerializeField]
	private Material material;

	[SerializeField]
	private MeshRenderer chaperoneBoundsRenderer;

	[SerializeField]
	private MeshRenderer chaperoneCenterRenderer;

	[SerializeField]
	private float centerRaycastDistance = 0.2f;

	private MeshFilter chaperoneMeshFilter;

	private Vector3[] vertices;

	private Player thisPlayer;

	private static ChaperoneMode? _currentChaperoneMode = null;

	private static RaycastHit[] chaperoneCenterRaycastResults = new RaycastHit[20];

	private HmdQuad_t playSpaceBounds = default(HmdQuad_t);

	public bool ShowCurrentBounds { get; set; }

	public static ChaperoneMode CurrentChaperoneMode
	{
		get
		{
			ChaperoneMode? currentChaperoneMode = _currentChaperoneMode;
			if (!currentChaperoneMode.HasValue)
			{
				_currentChaperoneMode = ChaperoneMode.SteamVR;
			}
			return _currentChaperoneMode.Value;
		}
	}

	private void Awake()
	{
		thisPlayer = GetComponentInParent<Player>();
		chaperoneMeshFilter = chaperoneBoundsRenderer.gameObject.GetComponent<MeshFilter>();
	}

	private void Update()
	{
		if (thisPlayer != null && thisPlayer.isLocal && (PlatformManager.Instance.CurrentTrackingMode == PlatformManager.TrackingMode.ONE_EIGHTY_DEGREE || SingletonMonoBehaviour<SettingsManager>.Instance.ShowRoomCenter))
		{
			Vector3 origin = base.transform.position + Vector3.up * centerRaycastDistance / 2f;
			int num = Physics.RaycastNonAlloc(origin, -Vector3.up, chaperoneCenterRaycastResults, centerRaycastDistance, 2048);
			if (num > 0)
			{
				Array.Sort(chaperoneCenterRaycastResults, (RaycastHit a, RaycastHit b) => b.point.y.CompareTo(a.point.y));
				chaperoneCenterRenderer.transform.position = chaperoneCenterRaycastResults[0].point;
			}
			else
			{
				chaperoneCenterRenderer.transform.position = base.transform.position;
			}
			chaperoneCenterRenderer.gameObject.SetActive(true);
		}
		else
		{
			chaperoneCenterRenderer.gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		if (Application.isPlaying && thisPlayer != null && thisPlayer.isLocal)
		{
			StartCoroutine(BuildMesh());
		}
	}

	private IEnumerator BuildMesh()
	{
		chaperoneMeshFilter.mesh = null;
		Vector3[] borderCorners = null;
		switch (CurrentChaperoneMode)
		{
		case ChaperoneMode.SteamVR:
			yield return GetBoundsSteamVR(delegate(Vector3[] c)
			{
				borderCorners = c;
			});
			break;
		case ChaperoneMode.Oculus:
			yield return GetBoundsOculus(delegate(Vector3[] c)
			{
				borderCorners = c;
			});
			break;
		}
		if (borderCorners != null)
		{
			BuildBorderMesh(borderCorners);
		}
	}

	private IEnumerator GetBoundsSteamVR(CornerAssignmentCallback assignCorners)
	{
		CVRChaperone chaperone = OpenVR.Chaperone;
		if (chaperone == null)
		{
			assignCorners(null);
			yield break;
		}
		while (chaperone.GetCalibrationState() != ChaperoneCalibrationState.OK)
		{
			yield return null;
		}
		bool initOpenVR = !SteamVR.active && !SteamVR.usingNativeSupport;
		if (initOpenVR)
		{
			EVRInitError peError = EVRInitError.None;
			OpenVR.Init(ref peError, EVRApplicationType.VRApplication_Other);
		}
		HmdQuad_t pRect = default(HmdQuad_t);
		bool success = chaperone != null && chaperone.GetPlayAreaRect(ref pRect);
		if (initOpenVR)
		{
			OpenVR.Shutdown();
		}
		if (!success)
		{
			Debug.LogWarning("Failed to get Calibrated Play Area bounds!  Make sure you have tracking first, and that your space is calibrated.");
			assignCorners(null);
			yield break;
		}
		assignCorners(new Vector3[4]
		{
			new Vector3(pRect.vCorners0.v0, pRect.vCorners0.v1, pRect.vCorners0.v2),
			new Vector3(pRect.vCorners1.v0, pRect.vCorners1.v1, pRect.vCorners1.v2),
			new Vector3(pRect.vCorners2.v0, pRect.vCorners2.v1, pRect.vCorners2.v2),
			new Vector3(pRect.vCorners3.v0, pRect.vCorners3.v1, pRect.vCorners3.v2)
		});
	}

	private IEnumerator GetBoundsOculus(CornerAssignmentCallback assignCorners)
	{
		OVRBoundary boundary = OVRManager.boundary;
		if (boundary == null || !boundary.GetConfigured())
		{
			assignCorners(null);
		}
		else
		{
			assignCorners(GetBoundaryBordersOculus(boundary));
		}
		yield break;
	}

	private Vector3[] GetBoundaryBordersOculus(OVRBoundary boundary)
	{
		Vector3 dimensions = boundary.GetDimensions(OVRBoundary.BoundaryType.PlayArea);
		return new Vector3[4]
		{
			new Vector3((0f - dimensions.x) / 2f, dimensions.y, (0f - dimensions.z) / 2f),
			new Vector3(dimensions.x / 2f, dimensions.y, (0f - dimensions.z) / 2f),
			new Vector3(dimensions.x / 2f, dimensions.y, dimensions.z / 2f),
			new Vector3((0f - dimensions.x) / 2f, dimensions.y, dimensions.z / 2f)
		};
	}

	private void BuildBorderMesh(Vector3[] borderCorners)
	{
		vertices = new Vector3[borderCorners.Length * 2];
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < borderCorners.Length; i++)
		{
			Vector3 vector = borderCorners[i];
			vertices[i] = new Vector3(vector.x, 0.01f, vector.z);
			zero += vertices[i];
		}
		zero /= (float)borderCorners.Length;
		chaperoneCenterRenderer.transform.localPosition = zero;
		if (lineThickness == 0f)
		{
			chaperoneMeshFilter.mesh = null;
			return;
		}
		for (int j = 0; j < borderCorners.Length; j++)
		{
			int num = (j + 1) % borderCorners.Length;
			int num2 = (j + borderCorners.Length - 1) % borderCorners.Length;
			Vector3 normalized = (vertices[num] - vertices[j]).normalized;
			Vector3 normalized2 = (vertices[num2] - vertices[j]).normalized;
			Vector3 vector2 = vertices[j];
			vector2 += Vector3.Cross(normalized, Vector3.up) * lineThickness;
			vector2 += Vector3.Cross(normalized2, Vector3.down) * lineThickness;
			vertices[borderCorners.Length + j] = vector2;
		}
		int[] triangles = new int[24]
		{
			7, 4, 0, 7, 0, 3, 6, 7, 3, 6,
			3, 2, 5, 6, 2, 5, 2, 1, 4, 5,
			1, 4, 1, 0
		};
		Vector2[] uv = new Vector2[8]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f)
		};
		Mesh mesh = new Mesh();
		chaperoneMeshFilter.mesh = mesh;
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		if (material != null)
		{
			chaperoneBoundsRenderer.material = material;
		}
		else
		{
			chaperoneBoundsRenderer.material = Resources.GetBuiltinResource<Material>("Sprites-Default.mat");
		}
	}

	public bool IsInBounds()
	{
		bool result = false;
		switch (CurrentChaperoneMode)
		{
		case ChaperoneMode.Oculus:
			result = IsInBoundsOculus();
			break;
		case ChaperoneMode.SteamVR:
			result = IsInBoundsSteamVR();
			break;
		}
		return result;
	}

	private bool IsInBoundsOculus()
	{
		OVRBoundary boundary = OVRManager.boundary;
		if (boundary != null && boundary.GetConfigured())
		{
			Vector3[] boundaryBordersOculus = GetBoundaryBordersOculus(boundary);
			for (int i = 0; i < boundaryBordersOculus.Length; i++)
			{
				boundaryBordersOculus[i] = HmdVector3ToHeadSpace(boundaryBordersOculus[i]);
			}
			return IsInConvexPolygon(thisPlayer.Head.transform.position, boundaryBordersOculus);
		}
		return false;
	}

	private bool IsInBoundsSteamVR()
	{
		CVRChaperone chaperone = OpenVR.Chaperone;
		if (chaperone != null && chaperone.GetCalibrationState() == ChaperoneCalibrationState.OK)
		{
			if (!chaperone.GetPlayAreaRect(ref playSpaceBounds))
			{
				return false;
			}
			Vector3[] polygon = new Vector3[4]
			{
				HmdVector3ToHeadSpace(playSpaceBounds.vCorners0),
				HmdVector3ToHeadSpace(playSpaceBounds.vCorners1),
				HmdVector3ToHeadSpace(playSpaceBounds.vCorners2),
				HmdVector3ToHeadSpace(playSpaceBounds.vCorners3)
			};
			return IsInConvexPolygon(thisPlayer.Head.transform.position, polygon);
		}
		return false;
	}

	private Vector3 HmdVector3ToHeadSpace(HmdVector3_t hmdVector)
	{
		return thisPlayer.transform.TransformPoint(new Vector3(hmdVector.v0, hmdVector.v1, hmdVector.v2));
	}

	private Vector3 HmdVector3ToHeadSpace(Vector3 hmdVector)
	{
		return thisPlayer.transform.TransformPoint(hmdVector);
	}

	private bool IsInConvexPolygon(Vector3 testPoint, Vector3[] polygon)
	{
		if (polygon.Length == 0)
		{
			return false;
		}
		if (polygon.Length == 1)
		{
			return polygon[0].x == testPoint.x && polygon[0].z == testPoint.z;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < polygon.Length; i++)
		{
			if (polygon[i].x == testPoint.x && polygon[i].z == testPoint.z)
			{
				return true;
			}
			float x = polygon[i].x;
			float z = polygon[i].z;
			int num3 = ((i < polygon.Length - 1) ? (i + 1) : 0);
			float x2 = polygon[num3].x;
			float z2 = polygon[num3].z;
			float x3 = testPoint.x;
			float z3 = testPoint.z;
			float num4 = (x3 - x) * (z2 - z) - (z3 - z) * (x2 - x);
			if (num4 > 0f)
			{
				num++;
			}
			if (num4 < 0f)
			{
				num2++;
			}
			if (num > 0 && num2 > 0)
			{
				return false;
			}
		}
		return true;
	}
}
