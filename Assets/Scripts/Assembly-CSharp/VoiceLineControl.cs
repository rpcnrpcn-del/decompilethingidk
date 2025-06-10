using System;
using System.Collections;
using UnityEngine;

public class VoiceLineControl : MonoBehaviour
{
	[Serializable]
	public class VoiceLineControlConfig
	{
		public float zTranslationWrapLimit = 0.1f;

		public float zTranslationRate = 0.01f;

		public float jitterFPS = 20f;

		public float TTL = 0.1f;
	}

	private Vector3 initialLocalPosition;

	private Vector3 initialScale;

	private float endOfLife;

	private Vector3 minJitter;

	private Vector3 maxJitter;

	private Vector3 jitterScale;

	private float zTrans;

	[HideInInspector]
	public VoiceLineControlConfig Config;

	private Coroutine jitterCoroutine;

	private void Awake()
	{
		initialLocalPosition = base.transform.localPosition;
		initialScale = base.transform.localScale;
		minJitter = new Vector3(initialScale.x - initialScale.x / 10f, initialScale.y - initialScale.y / 10f, initialScale.z - initialScale.z / 10f);
		maxJitter = new Vector3(initialScale.x + initialScale.x / 10f, initialScale.y + initialScale.y / 10f, initialScale.z + initialScale.z / 10f);
		SetRandomJitterScale();
	}

	private void OnEnable()
	{
		zTrans = 0f;
		base.transform.localPosition = initialLocalPosition;
		base.transform.localScale = initialScale;
		if (jitterCoroutine != null)
		{
			StopCoroutine(jitterCoroutine);
		}
		StartCoroutine(JitterCoroutine());
	}

	private void SetRandomJitterScale()
	{
		float x = UnityEngine.Random.Range(minJitter.x, maxJitter.x);
		float y = UnityEngine.Random.Range(minJitter.y, maxJitter.y);
		float z = UnityEngine.Random.Range(minJitter.z, maxJitter.z);
		jitterScale = new Vector3(x, y, z);
	}

	private IEnumerator JitterCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(1f / Config.jitterFPS);
			SetRandomJitterScale();
		}
	}

	private void Update()
	{
		zTrans += Config.zTranslationRate * Time.deltaTime;
		zTrans %= Config.zTranslationWrapLimit;
		Vector3 localPosition = initialLocalPosition;
		localPosition.z += zTrans;
		base.transform.localPosition = localPosition;
		base.transform.localScale = jitterScale;
		if (Time.time >= endOfLife)
		{
			base.gameObject.SetActive(false);
		}
	}

	public void SetLifespan(float endOfLife)
	{
		this.endOfLife = endOfLife;
	}
}
