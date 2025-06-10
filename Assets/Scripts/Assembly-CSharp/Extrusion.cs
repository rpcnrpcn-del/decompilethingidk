using System;
using System.Collections.Generic;
using UnityEngine;

public class Extrusion : Tool
{
	[Header("Extrusion")]
	[SerializeField]
	private Material material;

	[SerializeField]
	private float radius = 0.033f;

	[SerializeField]
	private RecRoomAudioClip loopingExtrusionAudio;

	private bool canExtrudeMeshes = true;

	private List<Curve> curves = new List<Curve>();

	private ExtrudedMesh currentMesh;

	private GameObject currentMeshGameObject;

	private SFXAudioSource extrusionLoopAudioSource;

	private static Transform _extrusionRoot;

	private static Transform extrusionRoot
	{
		get
		{
			if (_extrusionRoot == null)
			{
				GameObject gameObject = new GameObject("Extrusion Root");
				gameObject.transform.SetParent(null, false);
				_extrusionRoot = gameObject.transform;
			}
			return _extrusionRoot;
		}
	}

	private Curve CurrentCurve
	{
		get
		{
			return (curves.Count <= 0) ? null : curves[curves.Count - 1];
		}
	}

	public Transform ExtrusionPoint { get; private set; }

	public bool IsExtrudingMesh
	{
		get
		{
			return currentMesh != null;
		}
	}

	public override bool OwnershipTransferAllowed
	{
		get
		{
			return !base.OnlyOwnerCanPickup;
		}
	}

	public event Action ExtrusionFinishedEvent;

	protected override void Awake()
	{
		base.Awake();
		base.transform.SetParent(extrusionRoot, true);
		canExtrudeMeshes = true;
		base.OnlyOwnerCanPickup = true;
		base.Rigidbody.isKinematic = true;
		supportsGravityPickup = false;
		supportsOffCenterGravityPickup = false;
		int viewID = (int)base.photonView.instantiationData[0];
		MeshExtruderTool component = PhotonView.Find(viewID).GetComponent<MeshExtruderTool>();
		ExtrusionPoint = component.ExtrusionPoint;
	}

	private void Update()
	{
		UpdatePreviewControlPoint();
	}

	protected override void OnReset(Vector3 position, Quaternion rotation, bool wasEnabled, Vector3 oldPosition, bool wasCleanedUp)
	{
		if (SingletonMonoBehaviour<ToolCleanupManager>.Instance != null)
		{
			SingletonMonoBehaviour<ToolCleanupManager>.Instance.RemoveTool(this);
		}
		if (base.hasAuthority)
		{
			base.Owner.ToolController.ReleaseTool(this);
			SynchronizedField.ClearAllFieldsForPhotonObject(this);
			PhotonNetwork.Destroy(base.gameObject);
		}
	}

	public void MasterStartExtrudingNewMesh()
	{
		if (base.hasAuthority && ExtrusionPoint != null)
		{
			base.photonView.RPC("RpcStartExtrudingNewMesh", PhotonTargets.All, ExtrusionPoint.position, ExtrusionPoint.rotation);
		}
	}

	public void MasterFinishExtrudingCurrentMesh()
	{
		if (base.hasAuthority && ExtrusionPoint != null)
		{
			base.photonView.RPC("RpcFinishExtrudingCurrentMesh", PhotonTargets.All, ExtrusionPoint.position);
		}
	}

	public void MasterFinishExtrusion()
	{
		if (base.hasAuthority)
		{
			base.photonView.RPC("RpcFinishExtrusion", PhotonTargets.All);
		}
	}

	public void MasterAddControlPoint()
	{
		if (base.hasAuthority && ExtrusionPoint != null)
		{
			base.photonView.RPC("RpcAddControlPoint", PhotonTargets.All, ExtrusionPoint.position);
		}
	}

	public void UpdatePreviewControlPoint()
	{
		if (IsExtrudingMesh && ExtrusionPoint != null)
		{
			Curve currentCurve = CurrentCurve;
			currentCurve.UpdateLatestControlPoint(ExtrusionPoint.position);
			currentMesh.AddTemporaryControlPointVertices(currentCurve.IncompleteControlPoints, currentCurve.LastFinishedControlPoint, radius);
			currentMesh.CopyChangesToGPU();
		}
	}

	public override void Pickup(Player player, Rigidbody target, Vector3 targetSpacePickupPosition, Quaternion targetSpacePickupRotation)
	{
		if (canExtrudeMeshes)
		{
			FinishExtrusion();
		}
		base.Pickup(player, target, targetSpacePickupPosition, targetSpacePickupRotation);
	}

	private void AddControlPoint(Vector3 position, bool isEnd = false)
	{
		if (!IsExtrudingMesh)
		{
			return;
		}
		Curve currentCurve = CurrentCurve;
		if (currentCurve.ControlPoints.Count == 0)
		{
			currentCurve.AddControlPoint(position);
		}
		else
		{
			currentCurve.UpdateLatestControlPoint(position);
			ControlPoint previousControlPoint = currentCurve.PreviousControlPoint;
			ControlPoint previousPreviousControlPoint = currentCurve.PreviousPreviousControlPoint;
			currentMesh.AddPermanentControlPointVertices(previousControlPoint, previousPreviousControlPoint, radius);
			float num = ((previousPreviousControlPoint != null) ? (previousControlPoint.Position - previousPreviousControlPoint.Position).magnitude : 0f);
			if (!(num <= 2f * radius))
			{
				AddNewCollider(previousControlPoint.Position);
			}
		}
		if (isEnd)
		{
			ControlPoint latestControlPoint = currentCurve.LatestControlPoint;
			currentMesh.AddPermanentControlPointVertices(latestControlPoint, currentCurve.PreviousControlPoint, radius, true);
			AddNewCollider(latestControlPoint.Position);
		}
		else
		{
			currentCurve.AddControlPoint(position);
			currentMesh.AddTemporaryControlPointVertices(currentCurve.IncompleteControlPoints, currentCurve.LastFinishedControlPoint, radius);
		}
		currentMesh.CopyChangesToGPU();
	}

	private void AddNewCollider(Vector3 position)
	{
		GameObject gameObject = new GameObject("Collider");
		gameObject.layer = 10;
		gameObject.transform.SetParent(currentMeshGameObject.transform);
		gameObject.transform.position = position;
		gameObject.AddComponent<SphereCollider>().radius = radius;
		AddToolCollider(gameObject.AddComponent<ToolCollider>());
	}

	private void StartExtrudingNewMesh(Vector3 position, Quaternion rotation)
	{
		currentMeshGameObject = new GameObject("Mesh");
		currentMeshGameObject.layer = 10;
		currentMeshGameObject.transform.SetParent(base.transform);
		currentMeshGameObject.transform.position = position;
		currentMeshGameObject.transform.rotation = rotation;
		currentMesh = new ExtrudedMesh(currentMeshGameObject.transform);
		currentMeshGameObject.AddComponent<MeshFilter>().mesh = currentMesh.Mesh;
		MeshRenderer meshRenderer = currentMeshGameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = material;
		base.ToolRenderer.SetupRenderer(meshRenderer);
		StopLoopingExtrusionAudio();
	}

	private void FinishExtrudingCurrentMesh()
	{
		if (IsExtrudingMesh)
		{
			currentMesh.Finish();
			currentMesh = null;
			currentMeshGameObject = null;
		}
		StopLoopingExtrusionAudio();
	}

	private void FinishExtrusion()
	{
		canExtrudeMeshes = false;
		base.OnlyOwnerCanPickup = false;
		base.Rigidbody.isKinematic = false;
		supportsGravityPickup = true;
		supportsOffCenterGravityPickup = true;
		if (SingletonMonoBehaviour<ToolCleanupManager>.Instance != null)
		{
			SingletonMonoBehaviour<ToolCleanupManager>.Instance.AddTool(this);
		}
		if (this.ExtrusionFinishedEvent != null)
		{
			this.ExtrusionFinishedEvent();
		}
		StopLoopingExtrusionAudio();
	}

	private void CreateFromControlPoints()
	{
		int count = curves.Count;
		for (int i = 0; i < count; i++)
		{
			Curve curve = curves[i];
			StartExtrudingNewMesh(curve.Position, curve.Rotation);
			List<ControlPoint> incompleteControlPoints = curve.IncompleteControlPoints;
			int num = curve.ControlPoints.Count - incompleteControlPoints.Count;
			for (int j = 0; j < num; j++)
			{
				ControlPoint controlPoint = curve.ControlPoints[j];
				ControlPoint lastPermanentControlPoint = ((j <= 0) ? null : curve.ControlPoints[j - 1]);
				bool isEnd = curve.Finished && j == num - 1;
				currentMesh.AddPermanentControlPointVertices(controlPoint, lastPermanentControlPoint, radius, isEnd);
				AddNewCollider(curve.ControlPoints[j].Position);
			}
			if (!curve.Finished)
			{
				currentMesh.AddTemporaryControlPointVertices(incompleteControlPoints, curve.LastFinishedControlPoint, radius);
				currentMesh.CopyChangesToGPU();
			}
			else
			{
				currentMesh.CopyChangesToGPU();
				FinishExtrudingCurrentMesh();
			}
		}
		if (!canExtrudeMeshes)
		{
			FinishExtrusion();
		}
	}

	private void StartLoopingExtrusionAudio()
	{
		if (extrusionLoopAudioSource == null)
		{
			extrusionLoopAudioSource = AudioManager.StartLooping3DSFX(loopingExtrusionAudio, ExtrusionPoint);
		}
	}

	private void StopLoopingExtrusionAudio()
	{
		if (extrusionLoopAudioSource != null)
		{
			AudioManager.StopLoopingSFX(extrusionLoopAudioSource);
			extrusionLoopAudioSource = null;
		}
	}

	public static Extrusion CreateNewExtrusion(MeshExtruderTool extruder)
	{
		object[] data = new object[1] { extruder.photonView.viewID };
		return PhotonNetwork.Instantiate("[Extrusion]", extruder.ExtrusionPoint.position, extruder.ExtrusionPoint.rotation, 0, data).GetComponent<Extrusion>();
	}

	[PunRPC]
	public void RpcStartExtrudingNewMesh(Vector3 position, Quaternion rotation)
	{
		curves.Add(new Curve(position, rotation));
		StartExtrudingNewMesh(position, rotation);
		AddControlPoint(position);
		StartLoopingExtrusionAudio();
	}

	[PunRPC]
	public void RpcFinishExtrudingCurrentMesh(Vector3 position)
	{
		AddControlPoint(position, true);
		CurrentCurve.Finish();
		FinishExtrudingCurrentMesh();
	}

	[PunRPC]
	public void RpcFinishExtrusion()
	{
		FinishExtrusion();
	}

	[PunRPC]
	public void RpcAddControlPoint(Vector3 position)
	{
		AddControlPoint(position);
	}

	[PunRPC]
	public void RpcCreateFromControlPoints(Vector3[] controlPointPositions, Vector3[] curveInitialPositions, Quaternion[] curveInitialRotations, int[] curveFinalIndices, bool canExtrude)
	{
		curves.Clear();
		int num = 0;
		int num2 = controlPointPositions.Length;
		int num3 = curveFinalIndices.Length;
		for (int i = 0; i < num3; i++)
		{
			Curve curve = new Curve(curveInitialPositions[i], curveInitialRotations[i]);
			int num4 = curveFinalIndices[i];
			bool flag = num4 >= 0;
			int num5 = ((!flag) ? (num2 - 1) : num4);
			while (num <= num5)
			{
				curve.AddControlPoint(controlPointPositions[num++]);
			}
			if (flag)
			{
				curve.Finish();
			}
			curves.Add(curve);
		}
		canExtrudeMeshes = canExtrude;
		CreateFromControlPoints();
	}

	private void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		int count = curves.Count;
		if (!base.hasAuthority || count <= 0)
		{
			return;
		}
		int num = 0;
		Vector3[] array = new Vector3[curves.Count];
		Quaternion[] array2 = new Quaternion[curves.Count];
		int[] array3 = new int[curves.Count];
		for (int i = 0; i < curves.Count; i++)
		{
			num += curves[i].ControlPoints.Count;
			if (curves[i].Finished)
			{
				array3[i] = num - 1;
			}
			else
			{
				array3[i] = -1;
			}
			array[i] = curves[i].Position;
			array2[i] = curves[i].Rotation;
		}
		Vector3[] array4 = new Vector3[num];
		int num2 = 0;
		for (int j = 0; j < curves.Count; j++)
		{
			for (int k = 0; k < curves[j].ControlPoints.Count; k++)
			{
				array4[num2] = curves[j].ControlPoints[k].Position;
				num2++;
			}
		}
		base.photonView.RPC("RpcCreateFromControlPoints", newPlayer, array4, array, array2, array3, canExtrudeMeshes);
	}

	protected override void OnPhotonPlayerDisconnected(PhotonPlayer oldPlayer)
	{
		if (base.hasAuthority && base.Owner.PhotonPlayer == oldPlayer)
		{
			if (IsExtrudingMesh)
			{
				RpcFinishExtrudingCurrentMesh(ExtrusionPoint.position);
			}
			RpcFinishExtrusion();
		}
		base.OnPhotonPlayerDisconnected(oldPlayer);
	}
}
