using System;
using System.Collections;
using System.Collections.Generic;
using Photon;
using UnityEngine;
using UnityEngine.Rendering;

public class DrawingSurface : Photon.MonoBehaviour
{
	public enum AntiAliasing
	{
		ONE = 1,
		TWO = 2,
		FOUR = 4,
		EIGHT = 8
	}

	[Serializable]
	private struct RandomWaitTimeConfig
	{
		[Tooltip("Minimum time to wait before sending")]
		public float initialWait;

		public float randomMin;

		public float randomMax;

		public float GetTime()
		{
			return initialWait + ((!Mathf.Approximately(randomMin, randomMax)) ? UnityEngine.Random.Range(randomMin, randomMax) : randomMin);
		}
	}

	public enum DrawingSurfaceType
	{
		None = 0,
		Whiteboard = 1
	}

	[Header("Surface Config")]
	[SerializeField]
	private DrawingSurfaceType drawingSurfaceType;

	[Header("Coordinate Mapping")]
	[SerializeField]
	private Transform drawingRectMin;

	[SerializeField]
	private Transform drawingRectMax;

	private Rect drawingBounds = default(Rect);

	[Header("Rendering")]
	[SerializeField]
	private Renderer surfaceRenderer;

	[SerializeField]
	private int resolution = 512;

	[SerializeField]
	private AntiAliasing antiAliasing = AntiAliasing.EIGHT;

	private RenderTexture renderTexture;

	private CommandBuffer commandBuffer;

	[Header("Texture Serialization")]
	[SerializeField]
	private int xChunks = 8;

	[SerializeField]
	private int yChunks = 8;

	private bool modified;

	private Texture2D incomingTex;

	private List<DrawingToolBase> drawingTools = new List<DrawingToolBase>();

	private Dictionary<int, byte[]> encodedChunks = new Dictionary<int, byte[]>();

	[Header("RPC Staggering")]
	[SerializeField]
	private RandomWaitTimeConfig staggerWaitTime = new RandomWaitTimeConfig
	{
		initialWait = 2f,
		randomMin = 0f,
		randomMax = 3f
	};

	[SerializeField]
	private RandomWaitTimeConfig perChunkWaitTime = new RandomWaitTimeConfig
	{
		initialWait = 0.1f,
		randomMin = 0f,
		randomMax = 0.1f
	};

	public float WHRation
	{
		get
		{
			return surfaceRenderer.transform.localScale.x / surfaceRenderer.transform.localScale.y;
		}
	}

	public int PhotonId
	{
		get
		{
			return base.photonView.viewID;
		}
	}

	public Vector3 Normal
	{
		get
		{
			return base.transform.forward;
		}
	}

	public void MarkModified()
	{
		modified = true;
		encodedChunks.Clear();
	}

	protected override void Awake()
	{
		if (surfaceRenderer == null)
		{
			throw new Exception("I need a surface renderer to not crash!");
		}
		base.Awake();
		renderTexture = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
		renderTexture.antiAliasing = (int)antiAliasing;
		Texture texture = surfaceRenderer.material.GetTexture("_DrawingTex");
		Graphics.Blit(texture, renderTexture);
		surfaceRenderer.material.SetTexture("_DrawingTex", renderTexture);
		if (drawingRectMin != null && drawingRectMax != null)
		{
			drawingBounds = Rect.MinMaxRect(drawingRectMax.localPosition.x, drawingRectMin.localPosition.y, drawingRectMin.localPosition.x, drawingRectMax.localPosition.y);
			return;
		}
		throw new Exception("Drawing Bounds need to be set for DrawingSurface to work!");
	}

	private void Start()
	{
		RebuildCommandBuffer();
	}

	private void RebuildCommandBuffer()
	{
		if (commandBuffer != null)
		{
			Camera.main.RemoveCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
			commandBuffer = null;
		}
		if (drawingTools.Count <= 0)
		{
			return;
		}
		commandBuffer = new CommandBuffer();
		commandBuffer.name = "Draw Mesh";
		commandBuffer.SetRenderTarget(renderTexture);
		foreach (DrawingToolBase drawingTool in drawingTools)
		{
			commandBuffer.DrawRenderer(drawingTool.DrawingRenderer, drawingTool.Material);
		}
		Camera.main.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
	}

	public void AddDrawingTool(DrawingToolBase drawingTool)
	{
		drawingTools.Add(drawingTool);
		RebuildCommandBuffer();
	}

	public void RemoveDrawingTool(DrawingToolBase drawingTool)
	{
		drawingTools.Remove(drawingTool);
		RebuildCommandBuffer();
	}

	private void OnDrawGizmos()
	{
		if (drawingRectMin != null && drawingRectMax != null)
		{
			Gizmos.color = Color.red.WithAlpha(0.5f);
			Gizmos.DrawLine(drawingRectMin.position, drawingRectMax.position);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RenderTexture.active = null;
	}

	public float DistanceToPlane(Vector3 worldPos)
	{
		return base.transform.InverseTransformPoint(worldPos).z;
	}

	public bool IsOverPlane(Vector3 position, out Vector2 onPlanePos, out float distanceToPlane)
	{
		Vector3 vector = base.transform.InverseTransformPoint(position);
		distanceToPlane = vector.z;
		vector.z = 0f;
		onPlanePos = vector;
		return drawingBounds.Contains(vector);
	}

	public Vector2 WorldToNormalizedBoardPos(Vector3 worldPos)
	{
		return NormalizedBoardSpace(WorldToBoardSpace(worldPos));
	}

	public Vector3 OnDrawingPlane(Vector3 worldPos)
	{
		Vector3 vector = worldPos - drawingRectMin.position;
		Vector3 vector2 = Vector3.Project(vector, Normal);
		return worldPos - vector2;
	}

	public Vector3 WorldToBoardSpace(Vector3 worldPos)
	{
		Vector3 position = OnDrawingPlane(worldPos);
		return base.transform.InverseTransformPoint(position);
	}

	public Vector2 NormalizedBoardSpace(Vector2 localPos)
	{
		Vector2 point = localPos * -1f;
		Vector2 vector = Rect.PointToNormalized(drawingBounds, point);
		return vector * 2f - new Vector2(1f, 1f);
	}

	public void OnSurfaceTriggerEnter(Collider other)
	{
		if (other.CompareTag("BrushTool"))
		{
			DrawingToolBase componentInParent = other.GetComponentInParent<DrawingToolBase>();
			if (componentInParent != null && (componentInParent.DrawingSurfaceType & drawingSurfaceType) != DrawingSurfaceType.None)
			{
				componentInParent.CurrentSurface = this;
				AddDrawingTool(componentInParent);
			}
		}
	}

	public void OnSurfaceTriggerExit(Collider other)
	{
		if (other.CompareTag("BrushTool"))
		{
			DrawingToolBase componentInParent = other.GetComponentInParent<DrawingToolBase>();
			if (componentInParent != null && (componentInParent.DrawingSurfaceType & drawingSurfaceType) != DrawingSurfaceType.None)
			{
				RemoveDrawingTool(componentInParent);
				componentInParent.CurrentSurface = null;
			}
		}
	}

	public void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		if (PhotonNetwork.isMasterClient && modified)
		{
			StartCoroutine(SendTextureRoutine(newPlayer));
		}
	}

	private static int IntKey(ushort x, ushort y)
	{
		return (x << 16) | y;
	}

	private IEnumerator SendTextureRoutine(PhotonPlayer newPlayer)
	{
		yield return new WaitForSecondsRealtime(staggerWaitTime.GetTime());
		int xWidth = resolution / xChunks;
		int yHeight = resolution / yChunks;
		Texture2D tex = new Texture2D(xWidth, yHeight);
		int totalBytes = 0;
		for (ushort x = 0; x < xChunks; x++)
		{
			for (ushort y = 0; y < yChunks; y++)
			{
				int key = IntKey(x, y);
				byte[] bytes;
				if (!encodedChunks.TryGetValue(key, out bytes))
				{
					RenderTexture active = RenderTexture.active;
					RenderTexture.active = renderTexture;
					tex.ReadPixels(new Rect(x * xWidth, y * yHeight, xWidth, yHeight), 0, 0);
					tex.Apply();
					RenderTexture.active = active;
					bytes = tex.EncodeToPNG();
					encodedChunks[key] = bytes;
				}
				totalBytes += bytes.Length;
				bool lastChunk = x == xChunks - 1 && y == yChunks - 1;
				base.photonView.RPC("RpcSetTexture", newPlayer, (int)x, (int)y, bytes, lastChunk);
				yield return new WaitForSecondsRealtime(perChunkWaitTime.GetTime());
			}
		}
		UnityEngine.Object.Destroy(tex);
		Debug.Log(string.Format("Texture of size {0}b sent", totalBytes));
	}

	[PunRPC]
	private void RpcSetTexture(int x, int y, byte[] bytes, bool lastChunk)
	{
		if (incomingTex == null)
		{
			incomingTex = new Texture2D(resolution, resolution);
		}
		int num = resolution / xChunks;
		int num2 = resolution / yChunks;
		Texture2D texture2D = new Texture2D(num, num2);
		texture2D.LoadImage(bytes);
		incomingTex.SetPixels(x * num, (yChunks - 1 - y) * num2, num, num2, texture2D.GetPixels());
		incomingTex.Apply();
		if (lastChunk)
		{
			Graphics.Blit(incomingTex, renderTexture);
			UnityEngine.Object.Destroy(incomingTex);
			UnityEngine.Object.Destroy(texture2D);
			incomingTex = null;
		}
	}
}
