using System.Runtime.InteropServices;
using UnityEngine;
using Valve.VR;

public class FlushOnPostRender : MonoBehaviour
{
	private static int s_numFlushesThisFrame;

	private static int s_lastFrameCount;

	private void OnPostRender()
	{
		if (OpenVR.Compositor != null)
		{
			Compositor_FrameTiming pTiming = new Compositor_FrameTiming
			{
				m_nSize = (uint)Marshal.SizeOf(typeof(Compositor_FrameTiming))
			};
			OpenVR.Compositor.GetFrameTiming(ref pTiming, 0u);
			if (pTiming.m_nNumFramePresents > 1)
			{
				return;
			}
		}
		if (s_lastFrameCount != Time.frameCount)
		{
			s_lastFrameCount = Time.frameCount;
			s_numFlushesThisFrame = 0;
		}
		if (++s_numFlushesThisFrame <= 3)
		{
			GL.Flush();
		}
	}
}
