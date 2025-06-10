using System.Collections;
using System.Threading;
using UnityEngine;

namespace UTJ
{
	[AddComponentMenu("UTJ/Misc/FixDeltaTime")]
	public class FixDeltaTime : MonoBehaviour
	{
		public float m_targetFrameRate = 30f;

		public IEnumerator Wait()
		{
			yield return new WaitForEndOfFrame();
			float wt = Time.maximumDeltaTime;
			while (Time.realtimeSinceStartup - Time.unscaledTime < wt)
			{
				Thread.Sleep(1);
			}
		}

		public void ApplyFrameRate()
		{
			Time.maximumDeltaTime = 1f / m_targetFrameRate;
		}

		private void OnEnable()
		{
			ApplyFrameRate();
		}

		private void Update()
		{
			ApplyFrameRate();
			StartCoroutine(Wait());
		}
	}
}
