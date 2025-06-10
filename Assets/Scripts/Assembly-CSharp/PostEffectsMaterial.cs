using System;
using UnityEngine;
using UnityEngine.Rendering;

public class PostEffectsMaterial : MonoBehaviour
{
	private int vignetteRadiusId;

	private int vignetteSoftnessId;

	private int gradientId;

	private int fadeId;

	private Renderer renderer;

	[NonSerialized]
	public Camera Camera;

	private float _vignetteIntensity;

	private float _vignetteSoftness;

	private Vector4 _gradient = new Vector4(0f, 1f, 1f, 1f);

	private float _fade = 1f;

	public float VignetteIntensity
	{
		get
		{
			return _vignetteIntensity;
		}
		set
		{
			_vignetteIntensity = value;
			renderer.material.SetFloat(vignetteRadiusId, 1f - _vignetteIntensity);
		}
	}

	public float VignetteSoftness
	{
		get
		{
			return _vignetteSoftness;
		}
		set
		{
			_vignetteSoftness = value;
			renderer.material.SetFloat(vignetteSoftnessId, _vignetteSoftness);
		}
	}

	public float Fade
	{
		get
		{
			return _fade;
		}
		set
		{
			_fade = value;
			renderer.material.SetFloat(fadeId, _fade);
		}
	}

	public void SetGradient(float startTexCoord, float endTexCoord, float startFade, float endFade)
	{
		_gradient.x = startTexCoord;
		_gradient.y = endTexCoord;
		_gradient.z = startFade;
		_gradient.w = endFade;
		renderer.material.SetVector(gradientId, _gradient);
	}

	private void Awake()
	{
		renderer = GetComponent<Renderer>();
		renderer.enabled = false;
		vignetteRadiusId = Shader.PropertyToID("_VignetteRadius");
		vignetteSoftnessId = Shader.PropertyToID("_VignetteSoftness");
		gradientId = Shader.PropertyToID("_GradientVector");
		fadeId = Shader.PropertyToID("_Fade");
		SetGradient(0f, 1f, 1f, 1f);
		VignetteIntensity = 0f;
		Fade = 0f;
	}

	private void Start()
	{
		if (Camera == null)
		{
			Debug.LogError("PostEffectsMaterial missing reference to a camera.");
		}
		float num = 2f * Mathf.Tan(Camera.fieldOfView * 0.5f * ((float)Math.PI / 180f));
		float num2 = num * Camera.aspect;
		float num3 = Mathf.Sqrt(num * num + num2 * num2);
		float num4 = ((!(Camera.aspect > 1f)) ? (num3 / num2) : (num3 / num));
		num *= num4;
		num2 *= num4;
		base.transform.localScale = new Vector3(num2, num, 1f);
		CommandBuffer commandBuffer = new CommandBuffer();
		commandBuffer.name = "Draw Post Effect Quad";
		commandBuffer.DrawRenderer(renderer, renderer.material);
		Camera.AddCommandBuffer(CameraEvent.AfterImageEffects, commandBuffer);
	}

	private void LateUpdate()
	{
		if (Camera != null)
		{
			base.transform.rotation = Quaternion.LookRotation(Camera.transform.forward, Vector3.up);
		}
	}
}
