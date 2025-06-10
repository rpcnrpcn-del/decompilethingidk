using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Mirror : MonoBehaviour
{
	private class ReflectionData
	{
		public RenderTexture texture;

		public MaterialPropertyBlock propertyBlock;
	}

	[SerializeField]
	private bool disablePixelLights = true;

	[SerializeField]
	[Tooltip("Mirror will use up to the anti-aliasing level specified in QualitySettings, without exceeding this value (1, 2, 4, or 8)")]
	private int maxAntiAliasing = 1;

	private Dictionary<Camera, ReflectionData> m_Reflections = new Dictionary<Camera, ReflectionData>();

	private Camera mirrorCamera;

	private Skybox mirrorSkybox;

	private static bool s_InsideRendering = false;

	private static int TexturePropertyID;

	private static int CameraPositionPropertyID;

	private static readonly Rect LeftEyeRect = new Rect(0f, 0f, 0.5f, 1f);

	private static readonly Rect RightEyeRect = new Rect(0.5f, 0f, 0.5f, 1f);

	private static readonly Rect DefaultRect = new Rect(0f, 0f, 1f, 1f);

	private void OnValidate()
	{
		if (maxAntiAliasing != 1 && maxAntiAliasing != 2 && maxAntiAliasing != 4 && maxAntiAliasing != 8)
		{
			maxAntiAliasing = 1;
		}
	}

	private void Awake()
	{
		TexturePropertyID = Shader.PropertyToID("_ReflectionTex");
		CameraPositionPropertyID = Shader.PropertyToID("_CameraPosition");
	}

	public void OnWillRenderObject()
	{
		Renderer component = GetComponent<Renderer>();
		if (!base.enabled || !component || !component.enabled)
		{
			return;
		}
		Camera current = Camera.current;
		if (!current || current == mirrorCamera || s_InsideRendering)
		{
			return;
		}
		s_InsideRendering = true;
		ReflectionData reflectionData = GetReflectionData(current);
		int pixelLightCount = QualitySettings.pixelLightCount;
		if (disablePixelLights)
		{
			QualitySettings.pixelLightCount = 0;
		}
		UpdateCameraModes(current);
		if (current.stereoEnabled)
		{
			if (current.stereoTargetEye == StereoTargetEyeMask.Both || current.stereoTargetEye == StereoTargetEyeMask.Left)
			{
				Matrix4x4 stereoProjectionMatrix = current.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
				Matrix4x4 stereoViewMatrix = current.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
				RenderMirror(reflectionData.texture, stereoViewMatrix, stereoProjectionMatrix, LeftEyeRect);
			}
			if (current.stereoTargetEye == StereoTargetEyeMask.Both || current.stereoTargetEye == StereoTargetEyeMask.Right)
			{
				Matrix4x4 stereoViewMatrix2 = current.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
				Matrix4x4 stereoProjectionMatrix2 = current.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
				RenderMirror(reflectionData.texture, stereoViewMatrix2, stereoProjectionMatrix2, RightEyeRect);
			}
		}
		else
		{
			RenderMirror(reflectionData.texture, current.worldToCameraMatrix, current.projectionMatrix, DefaultRect);
		}
		reflectionData.propertyBlock.SetVector(CameraPositionPropertyID, current.transform.position);
		component.SetPropertyBlock(reflectionData.propertyBlock);
		if (disablePixelLights)
		{
			QualitySettings.pixelLightCount = pixelLightCount;
		}
		s_InsideRendering = false;
	}

	private void RenderMirror(RenderTexture targetTexture, Matrix4x4 camWorldToLocalMatrix, Matrix4x4 camProjectionMatrix, Rect camViewport)
	{
		mirrorCamera.worldToCameraMatrix = camWorldToLocalMatrix;
		mirrorCamera.projectionMatrix = camProjectionMatrix;
		mirrorCamera.targetTexture = targetTexture;
		mirrorCamera.rect = camViewport;
		Vector3 position = base.transform.position;
		Vector3 normal = -base.transform.up;
		Vector4 plane = Plane(position, normal);
		mirrorCamera.worldToCameraMatrix *= CalculateReflectionMatrix(plane);
		Vector4 clipPlane = CameraSpacePlane(mirrorCamera, position, normal);
		mirrorCamera.projectionMatrix = mirrorCamera.CalculateObliqueMatrix(clipPlane);
		mirrorCamera.transform.position = mirrorCamera.cameraToWorldMatrix.GetPosition();
		mirrorCamera.transform.rotation = mirrorCamera.cameraToWorldMatrix.GetRotation();
		bool invertCulling = GL.invertCulling;
		GL.invertCulling = !invertCulling;
		mirrorCamera.Render();
		GL.invertCulling = invertCulling;
	}

	private void OnDisable()
	{
		if ((bool)mirrorCamera)
		{
			Object.DestroyImmediate(mirrorCamera.gameObject);
			mirrorCamera = null;
		}
		foreach (ReflectionData value in m_Reflections.Values)
		{
			Object.DestroyImmediate(value.texture);
		}
		m_Reflections.Clear();
	}

	private void UpdateCameraModes(Camera src)
	{
		if (!mirrorCamera)
		{
			GameObject gameObject = new GameObject("MirrorCam" + base.gameObject.name, typeof(Camera), typeof(Skybox), typeof(FlushOnPostRender));
			gameObject.hideFlags = HideFlags.HideAndDontSave;
			mirrorSkybox = gameObject.GetComponent<Skybox>();
			mirrorCamera = gameObject.GetComponent<Camera>();
			mirrorCamera.enabled = false;
			mirrorCamera.cullingMask = -4194305;
		}
		mirrorCamera.useOcclusionCulling = false;
		mirrorCamera.clearFlags = src.clearFlags;
		mirrorCamera.backgroundColor = src.backgroundColor;
		if (src.clearFlags == CameraClearFlags.Skybox)
		{
			Skybox skybox = src.GetComponent(typeof(Skybox)) as Skybox;
			if (!skybox || !skybox.material)
			{
				mirrorSkybox.enabled = false;
			}
			else
			{
				mirrorSkybox.enabled = true;
				mirrorSkybox.material = skybox.material;
			}
		}
		mirrorCamera.farClipPlane = src.farClipPlane;
		mirrorCamera.nearClipPlane = src.nearClipPlane;
		mirrorCamera.orthographic = src.orthographic;
		mirrorCamera.fieldOfView = src.fieldOfView;
		mirrorCamera.aspect = src.aspect;
		mirrorCamera.orthographicSize = src.orthographicSize;
	}

	private ReflectionData GetReflectionData(Camera currentCamera)
	{
		ReflectionData value = null;
		if (!m_Reflections.TryGetValue(currentCamera, out value))
		{
			value = new ReflectionData();
			value.propertyBlock = new MaterialPropertyBlock();
			m_Reflections[currentCamera] = value;
		}
		int num = currentCamera.pixelWidth;
		int pixelHeight = currentCamera.pixelHeight;
		int b = Mathf.Min(QualitySettings.antiAliasing, maxAntiAliasing);
		b = Mathf.Max(1, b);
		if (currentCamera.stereoEnabled)
		{
			num *= 2;
		}
		if (!value.texture || value.texture.width != num || value.texture.height != pixelHeight || value.texture.antiAliasing != b)
		{
			if ((bool)value.texture)
			{
				Object.DestroyImmediate(value.texture);
			}
			value.texture = new RenderTexture(num, pixelHeight, 24);
			value.texture.antiAliasing = b;
			value.texture.hideFlags = HideFlags.DontSave;
			value.propertyBlock.SetTexture(TexturePropertyID, value.texture);
		}
		return value;
	}

	private Vector4 Plane(Vector3 pos, Vector3 normal)
	{
		return new Vector4(normal.x, normal.y, normal.z, 0f - Vector3.Dot(pos, normal));
	}

	private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal)
	{
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Vector3 pos2 = worldToCameraMatrix.MultiplyPoint(pos);
		Vector3 normalized = worldToCameraMatrix.MultiplyVector(normal).normalized;
		return Plane(pos2, normalized);
	}

	private static Matrix4x4 CalculateReflectionMatrix(Vector4 plane)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		identity.m00 = 1f - 2f * plane[0] * plane[0];
		identity.m01 = -2f * plane[0] * plane[1];
		identity.m02 = -2f * plane[0] * plane[2];
		identity.m03 = -2f * plane[3] * plane[0];
		identity.m10 = -2f * plane[1] * plane[0];
		identity.m11 = 1f - 2f * plane[1] * plane[1];
		identity.m12 = -2f * plane[1] * plane[2];
		identity.m13 = -2f * plane[3] * plane[1];
		identity.m20 = -2f * plane[2] * plane[0];
		identity.m21 = -2f * plane[2] * plane[1];
		identity.m22 = 1f - 2f * plane[2] * plane[2];
		identity.m23 = -2f * plane[3] * plane[2];
		identity.m30 = 0f;
		identity.m31 = 0f;
		identity.m32 = 0f;
		identity.m33 = 1f;
		return identity;
	}
}
