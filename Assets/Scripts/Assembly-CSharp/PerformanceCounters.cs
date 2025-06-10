using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PerformanceCounters : MonoBehaviour
{
	[Serializable]
	public class HistogramData
	{
		public string name;

		public Vector2 bounds;

		public bool InBounds(float value)
		{
			return bounds.WithinBounds(value);
		}
	}

	[SerializeField]
	private float updateInterval = 1f;

	[SerializeField]
	private float fpsLogInterval = 120f;

	[SerializeField]
	private float rttLogInterval = 120f;

	[SerializeField]
	[Tooltip("Round trip time to Photon Servers")]
	private HistogramData[] rttHistograms;

	[SerializeField]
	private HistogramData[] fpsHistograms;

	private int frameCount;

	private float nextUpdate;

	private float nextRttLogTime;

	private float nextFpsLogTime;

	private float accumulatedFPS;

	private float accumulatedPing;

	public float FPS { get; private set; }

	public float RountTripTime { get; private set; }

	private void Start()
	{
		Reset();
		SceneManager.sceneLoaded += SceneManager_sceneLoaded;
	}

	private void Update()
	{
		frameCount++;
		accumulatedFPS += Time.timeScale / Time.deltaTime;
		accumulatedPing += PhotonNetwork.GetPing();
		if (Time.time > nextUpdate && frameCount > 0)
		{
			FPS = accumulatedFPS / (float)frameCount;
			RountTripTime = accumulatedPing / (float)frameCount;
			accumulatedPing = 0f;
			accumulatedFPS = 0f;
			nextUpdate = Time.time + updateInterval;
			frameCount = 0;
			if (Time.time > nextRttLogTime)
			{
				AnalyticsHelper.ActivityServerRoundTripTime(RountTripTime, GetHistogramLabel(RountTripTime, rttHistograms));
				nextRttLogTime = Time.time + rttLogInterval;
			}
			if (Time.time > nextFpsLogTime)
			{
				AnalyticsHelper.ActivityFrameRate(FPS, GetHistogramLabel(FPS, fpsHistograms));
				nextFpsLogTime = Time.time + fpsLogInterval;
			}
		}
	}

	private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		Reset();
	}

	private void Reset()
	{
		FPS = 0f;
		RountTripTime = 0f;
		frameCount = 0;
		accumulatedPing = 0f;
		accumulatedFPS = 0f;
		nextUpdate = Time.time + updateInterval;
		nextRttLogTime = Time.time + rttLogInterval;
		nextFpsLogTime = Time.time + fpsLogInterval;
	}

	private string GetHistogramLabel(float value, HistogramData[] histograms)
	{
		for (int i = 0; i < histograms.Length; i++)
		{
			if (histograms[i].InBounds(value))
			{
				return histograms[i].name;
			}
		}
		return string.Empty;
	}
}
