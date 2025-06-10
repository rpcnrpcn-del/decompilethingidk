using System;
using UnityEngine;

public class FrameBudgetMonitor : MonoBehaviour
{
	[Serializable]
	private class Property
	{
		public string Name;

		public float MeanLimit;

		public float PeakLimit;

		public string Suffix;

		[Tooltip("Optional multiplier to scale the raw profiling data by to get it into the measurement range (ideally between 0-10)")]
		public float Multiplier;

		[NonSerialized]
		public int Id;

		[NonSerialized]
		public float Mean;

		[NonSerialized]
		public float Peak;
	}

	[SerializeField]
	private Property[] cpuProperties = new Property[3]
	{
		new Property
		{
			Name = "Rendering",
			MeanLimit = 3f,
			PeakLimit = 5f,
			Suffix = " ms"
		},
		new Property
		{
			Name = "Scripts",
			MeanLimit = 3f,
			PeakLimit = 4f,
			Suffix = " ms"
		},
		new Property
		{
			Name = "Physics",
			MeanLimit = 0.6f,
			PeakLimit = 0.8f,
			Suffix = " ms"
		}
	};

	[SerializeField]
	private Property[] gpuProperties = new Property[6]
	{
		new Property
		{
			Name = "Shadows/Depth",
			MeanLimit = 3f,
			PeakLimit = 4f,
			Suffix = " ms"
		},
		new Property
		{
			Name = "Opaque",
			MeanLimit = 3f,
			PeakLimit = 4f,
			Suffix = " ms"
		},
		new Property
		{
			Name = "Transparent",
			MeanLimit = 0.5f,
			PeakLimit = 1f,
			Suffix = " ms"
		},
		new Property
		{
			Name = "PostProcess",
			MeanLimit = 0.7f,
			PeakLimit = 1.5f,
			Suffix = " ms"
		},
		new Property
		{
			Name = "Batches",
			MeanLimit = 1.5f,
			PeakLimit = 2f,
			Suffix = "K",
			Multiplier = 1000f
		},
		new Property
		{
			Name = "Vertices",
			MeanLimit = 8f,
			PeakLimit = 10f,
			Suffix = "M"
		}
	};
}
