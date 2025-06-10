using System;
using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Ultimate Game Tools/Colliders/Concave Collider")]
public class ConcaveCollider : MonoBehaviour
{
	public enum EAlgorithm
	{
		Normal = 0,
		Fast = 1,
		Legacy = 2
	}

	public delegate void LogDelegate([MarshalAs(UnmanagedType.LPStr)] string message);

	public delegate void ProgressDelegate([MarshalAs(UnmanagedType.LPStr)] string message, float fPercent);

	private struct SConvexDecompositionInfoInOut
	{
		public uint uMaxHullVertices;

		public uint uMaxHulls;

		public float fPrecision;

		public float fBackFaceDistanceFactor;

		public uint uLegacyDepth;

		public uint uNormalizeInputMesh;

		public uint uUseFastVersion;

		public uint uTriangleCount;

		public uint uVertexCount;

		public int nHullsOut;
	}

	private struct SConvexDecompositionHullInfo
	{
		public int nVertexCount;

		public int nTriangleCount;
	}

	public EAlgorithm Algorithm = EAlgorithm.Fast;

	public int MaxHullVertices = 64;

	public int MaxHulls = 128;

	public float InternalScale = 10f;

	public float Precision = 0.8f;

	public bool CreateMeshAssets;

	public bool CreateHullMesh;

	public bool DebugLog;

	public int LegacyDepth = 6;

	public bool ShowAdvancedOptions;

	public float MinHullVolume = 1E-05f;

	public float BackFaceDistanceFactor = 0.2f;

	public bool NormalizeInputMesh;

	public bool ForceNoMultithreading;

	public PhysicMaterial PhysMaterial;

	public bool IsTrigger;

	public GameObject[] m_aGoHulls;

	[SerializeField]
	private PhysicMaterial LastMaterial;

	[SerializeField]
	private bool LastIsTrigger;

	[SerializeField]
	private int LargestHullVertices;

	[SerializeField]
	private int LargestHullFaces;

	private void OnDestroy()
	{
		DestroyHulls();
	}

	private void Reset()
	{
		DestroyHulls();
	}

	private void Update()
	{
		if (PhysMaterial != LastMaterial)
		{
			GameObject[] aGoHulls = m_aGoHulls;
			foreach (GameObject gameObject in aGoHulls)
			{
				if ((bool)gameObject)
				{
					Collider component = gameObject.GetComponent<Collider>();
					if ((bool)component)
					{
						component.material = PhysMaterial;
						LastMaterial = PhysMaterial;
					}
				}
			}
		}
		if (IsTrigger == LastIsTrigger)
		{
			return;
		}
		GameObject[] aGoHulls2 = m_aGoHulls;
		foreach (GameObject gameObject2 in aGoHulls2)
		{
			if ((bool)gameObject2)
			{
				Collider component2 = gameObject2.GetComponent<Collider>();
				if ((bool)component2)
				{
					component2.isTrigger = IsTrigger;
					LastIsTrigger = IsTrigger;
				}
			}
		}
	}

	public void DestroyHulls()
	{
		LargestHullVertices = 0;
		LargestHullFaces = 0;
		if (m_aGoHulls == null)
		{
			return;
		}
		if (Application.isEditor && !Application.isPlaying)
		{
			GameObject[] aGoHulls = m_aGoHulls;
			foreach (GameObject gameObject in aGoHulls)
			{
				if ((bool)gameObject)
				{
					UnityEngine.Object.DestroyImmediate(gameObject);
				}
			}
		}
		else
		{
			GameObject[] aGoHulls2 = m_aGoHulls;
			foreach (GameObject gameObject2 in aGoHulls2)
			{
				if ((bool)gameObject2)
				{
					UnityEngine.Object.Destroy(gameObject2);
				}
			}
		}
		m_aGoHulls = null;
	}

	public void CancelComputation()
	{
		CancelConvexDecomposition();
	}

	public int GetLargestHullVertices()
	{
		return LargestHullVertices;
	}

	public int GetLargestHullFaces()
	{
		return LargestHullFaces;
	}

	[DllImport("ConvexDecompositionDll")]
	private static extern void DllInit(bool bUseMultithreading);

	[DllImport("ConvexDecompositionDll")]
	private static extern void DllClose();

	[DllImport("ConvexDecompositionDll")]
	private static extern void SetLogFunctionPointer(IntPtr pfnUnity3DLog);

	[DllImport("ConvexDecompositionDll")]
	private static extern void SetProgressFunctionPointer(IntPtr pfnUnity3DProgress);

	[DllImport("ConvexDecompositionDll")]
	private static extern void CancelConvexDecomposition();

	[DllImport("ConvexDecompositionDll")]
	private static extern bool DoConvexDecomposition(ref SConvexDecompositionInfoInOut infoInOut, Vector3[] pfVertices, int[] puIndices);

	[DllImport("ConvexDecompositionDll")]
	private static extern bool GetHullInfo(uint uHullIndex, ref SConvexDecompositionHullInfo infoOut);

	[DllImport("ConvexDecompositionDll")]
	private static extern bool FillHullMeshData(uint uHullIndex, ref float pfVolumeOut, int[] pnIndicesOut, Vector3[] pfVerticesOut);
}
